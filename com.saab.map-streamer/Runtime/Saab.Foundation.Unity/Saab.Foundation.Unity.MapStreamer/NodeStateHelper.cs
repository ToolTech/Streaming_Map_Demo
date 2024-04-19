//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s) 
// herein belong to Saab AB. The program(s) may be used and/or
// copied only with the written permission of Saab AB, or in
// accordance with the terms and conditions stipulated in the
// agreement/contract under which the program(s) have been
// supplied. 
//
//
// Information Class:	COMPANY UNCLASSIFIED
// Defence Secrecy:		NOT CLASSIFIED
// Export Control:		NOT EXPORT CONTROLLED
//
//
// File			: NodeStateHelper.cs
// Module		:
// Description	: Helper for texture and state uploads
// Author		: Anders Modén
// Product		: Gizmo3D 2.12.144
//
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows, Android, iOS and HoloLens for  
//			usage in Game or VisSim development.
//
//
// Revision History...
//
// Who	Date	Description
//
// ZJP	200625	Created file                                        (2.10.6)
//
//******************************************************************************

// Framework
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

// Unity
using UnityEngine;

// GizmoSDK
using GizmoSDK.GizmoBase;
using GizmoSDK.Gizmo3D;

using gzTexture = GizmoSDK.Gizmo3D.Texture;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace Saab.Foundation.Unity.MapStreamer
{
    public struct StateBuildOutput
    {
        public Texture2D Texture;
        public Texture2D Feature;
        public Texture2D SurfaceHeight;

        public Matrix3D Feature_homography;
        public Matrix3D SurfaceHeight_homography;
    }

    public class TextureImageInfo
    {
        public DynamicType homography;

        public DynamicType border_x;
        public DynamicType border_y;
    }

    public interface ICache<in TKey, TValue>
    {
        bool TryGet(TKey key, out TValue value);
        bool TryAdd(TKey key, TValue value);
        void CleanUp();
    }

    // Basic threadsafe implementation of a texture cache
    public class TextureCache : ICache<IntPtr, Texture2D>
    {
        private readonly ConcurrentDictionary<IntPtr, Texture2D> _cache = new ConcurrentDictionary<IntPtr, Texture2D>();
        public bool TryGet(IntPtr key, out Texture2D value)
        {
            return _cache.TryGetValue(key, out value);
        }
        public bool TryAdd(IntPtr key, Texture2D value)
        {
            if (_cache.TryAdd(key, value))
            {
                Reference.Ref(key);
                return true;
            }

            return false;
        }

        public void CleanUp()
        {
            foreach (var item in _cache)
            {
                var count = Reference.GetRef(item.Key);

                if(count<=2)    // Possibly One native (+1) and one managed (1)
                {
                    // Move dealloc to NodeLocked by secondary thread
                    ThreadEditLockedDeallocator.Dealloc(item.Key);

                    Reference.UnRef(item.Key);

                    Texture2D tex;

                    _cache.TryRemove(item.Key, out tex);
                        
                }
            }
        }
    }

    public static class StateHelper
    {
        private static readonly Dictionary<TextureFormat, bool> _supportedFormats = new Dictionary<TextureFormat, bool>();

        public static bool Build(State state, out StateBuildOutput output, ICache<IntPtr, Texture2D> textureCache = null)
        {
            output = default;

            try
            {
                Performance.Enter("StateBuilder.Build");

                if (!ReadTextureFromState(state, out Texture2D texture,textureCache,null))
                    return false;

                output.Texture = texture;

                TextureImageInfo info=new TextureImageInfo();

                if (ReadTextureFromState(state, out Texture2D feature, null, info, 1, false))       // Features always singletons and no mipmap force
                {
                    output.Feature = feature;
                    
                    if(info.homography.Is("gzImageHomography"))
                        output.Feature_homography = ((ImageHomography)info.homography);
                }

                info = new TextureImageInfo();

                if (ReadTextureFromState(state, out Texture2D surface, null, info, 2, false))       // Features always singletons and no mipmap force
                {
                    output.SurfaceHeight = surface;
                                        
                    if (info.homography.Is("gzImageHomography"))
                        output.SurfaceHeight_homography = ((ImageHomography)info.homography);
                }
            }
            finally
            {
                Performance.Leave();
            }

            return true;
        }

        private static bool ReadTextureFromState(State state, out Texture2D result, ICache<IntPtr, Texture2D> textureCache, TextureImageInfo tex_image_info, uint unit=0,bool useMipMap=true)
        {
            result = null;

            if (!state.HasTexture(unit) || state.GetMode(StateMode.TEXTURE) != StateModeActivation.ON)
                return false;

            using (var texture = state.GetTexture(unit))
            {
                if (textureCache != null)
                {
                    // Check possible shared states

                    var stateShare = state.GetReferenceCount();

                    if(stateShare>2)             // Share: 1 native, 1 managed, extra shared>2
                    {
                        if (textureCache.TryGet(state.GetNativeReference(), out result))
                        {
                            texture.ReleaseAlreadyLocked();
                            return true;
                        }

                        if (!CopyTexture(texture, out result, tex_image_info, useMipMap))
                            return false;

                        // Add a reference to data

                        textureCache.TryAdd(state.GetNativeReference(), result);

                        texture.ReleaseAlreadyLocked();

                        return true;
                    }

                    var texShare = texture.GetReferenceCount();

                    if (texShare > 2)             // Share: 1 native, 1 managed, extra shared>2
                    {
                        if (textureCache.TryGet(texture.GetNativeReference(), out result))
                        {
                            texture.ReleaseAlreadyLocked();
                            return true;
                        }

                        if (!CopyTexture(texture, out result, tex_image_info, useMipMap))
                            return false;

                        textureCache.TryAdd(texture.GetNativeReference(), result);

                        texture.ReleaseAlreadyLocked();

                        return true;
                    }
                }

                if (!CopyTexture(texture, out result, tex_image_info, useMipMap))
                    return false;

                texture.ReleaseAlreadyLocked();
            }

            return true;
        }

        private static bool CopyTexture(gzTexture gzTexture, out Texture2D result, TextureImageInfo tex_image_info, bool mipChain = true)
        {
            result = null;

            if (!gzTexture.HasImage())
                return false;

            using (var image = gzTexture.GetImage())
            {
                var imageFormat = image.GetFormat();

                var componentType = image.GetComponentType();

                var components = image.GetComponents();

                var textureFormat = GetUnityTextureFormat(imageFormat,componentType,components);

                var uncompress = !IsTextureFormatSupported(textureFormat);

                var nativePtr = IntPtr.Zero;

                if (!gzTexture.GetMipMapImageArray(ref nativePtr, out uint size, out _, out _, out _,
                    out uint width, out uint height, out uint depth, mipChain, uncompress))
                    return false;
                 
                if (depth != 1)
                    return false;

                result = new Texture2D((int)width, (int)height, textureFormat, mipChain);

#if false
                unsafe
                {
                    void* pointer = nativePtr.ToPointer();

                    NativeArray<byte> _image_data = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(pointer, (int)size, Allocator.None);

                    result.LoadRawTextureData(_image_data);
                }

#else

                result.LoadRawTextureData(nativePtr, (int)size);

#endif
                
                switch (gzTexture.MinFilter)
                {
                    case gzTexture.TextureMinFilter.LINEAR:
                    case gzTexture.TextureMinFilter.LINEAR_MIPMAP_NEAREST:
                        result.filterMode = FilterMode.Bilinear;
                        break;

                    case gzTexture.TextureMinFilter.LINEAR_MIPMAP_LINEAR:
                        result.filterMode = FilterMode.Trilinear;
                        break;

                    default:
                        result.filterMode = FilterMode.Point;
                        break;
                }

                result.Apply(mipChain, true);

                if (tex_image_info != null)
                {
                    tex_image_info.homography = image.GetAttribute("UserDataImInfo", "ImI-Wrld-Hom");

                    tex_image_info.border_x = image.GetAttribute("UserDataImInfo", "ImI-Pixel-X-border");
                    tex_image_info.border_y = image.GetAttribute("UserDataImInfo", "ImI-Pixel-Y-border");
                }
            }

            return true;
        }

        private static TextureFormat GetUnityTextureFormat(ImageFormat imageFormat , ComponentType compType, byte components)
        {
            switch (imageFormat)
            {
                case ImageFormat.RGBA:
                    if (compType == ComponentType.UNSIGNED_BYTE)
                        return TextureFormat.RGBA32;
                    if (compType == ComponentType.FLOAT)
                        return TextureFormat.RGBA32;
                    if (compType == ComponentType.HALF_FLOAT)
                        return TextureFormat.RGBAHalf;
                    break;

                case ImageFormat.RGB:
                    if (compType == ComponentType.UNSIGNED_BYTE)
                        return TextureFormat.RGB24;
                    break;

                case ImageFormat.COMPRESSED_RGBA8_ETC2:
                    return TextureFormat.ETC2_RGBA8;

                case ImageFormat.COMPRESSED_RGB8_ETC2:
                    return TextureFormat.ETC2_RGB;

                case ImageFormat.COMPRESSED_RGBA_S3TC_DXT1:
                case ImageFormat.COMPRESSED_RGB_S3TC_DXT1:
                    return TextureFormat.DXT1;

                case ImageFormat.COMPRESSED_RGBA_S3TC_DXT5:
                    return TextureFormat.DXT5;

                case ImageFormat.LUMINANCE:
                    if(compType==ComponentType.UNSIGNED_BYTE)
                        return TextureFormat.R8;
                    if (compType == ComponentType.FLOAT)
                        return TextureFormat.RFloat;
                    break;

                case ImageFormat.LUMINANCE_ALPHA:
                    if (compType == ComponentType.UNSIGNED_BYTE)
                        return TextureFormat.RG16;
                    if (compType == ComponentType.FLOAT)
                        return TextureFormat.RGFloat;
                    break;
            }

            throw new NotSupportedException();
        }

        private static bool IsTextureFormatSupported(TextureFormat format)
        {
            if (!_supportedFormats.TryGetValue(format, out bool supported))
            {
                // SystemInfo.SupportsTextureFormat is a very slow operation, so
                // we will cache the result for future queries.
                supported = SystemInfo.SupportsTextureFormat(format);
                _supportedFormats.Add(format, supported);
            }

            if (!supported)
                Message.Send("StateBuilder", MessageLevel.WARNING, $"{format} was not a supported format!");

            return supported;
        }
    }
}
