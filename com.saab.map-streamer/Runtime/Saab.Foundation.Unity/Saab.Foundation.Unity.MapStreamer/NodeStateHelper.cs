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
// Product		: Gizmo3D 2.12.185
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
using System.Collections.Generic;

// Unity
using UnityEngine;

// GizmoSDK
using GizmoSDK.GizmoBase;
using GizmoSDK.Gizmo3D;

using gzTexture = GizmoSDK.Gizmo3D.Texture;
using Texture = UnityEngine.Texture;
using ProfilerMarker = global::Unity.Profiling.ProfilerMarker;
using ProfilerCategory = global::Unity.Profiling.ProfilerCategory;

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

    public static class StateHelper
    {
        private static readonly ProfilerMarker _profilerMarker = new ProfilerMarker(ProfilerCategory.Render, "SM-ReadTexture");

        private static readonly Dictionary<TextureFormat, bool> _supportedFormats = new Dictionary<TextureFormat, bool>();

        public static bool Build(State state, out StateBuildOutput output, TextureManager textureCache = null)
        {
            output = default;

            if (!ReadTextureFromState(state, out Texture2D texture, textureCache))
                return false;

            output.Texture = texture;

            if (ReadTextureFromState(state, out Texture2D feature, textureCache, out TextureImageInfo info, 1, false))       // Features always singletons and no mipmap force
            {
                if (info != null && info.homography.Is("gzImageHomography"))
                {
                    output.Feature = feature;
                    output.Feature_homography = ((ImageHomography)info.homography);
                }
                else
                    output.Feature = null;
            }

            if (ReadTextureFromState(state, out Texture2D surface, textureCache, out info, 2, false))       // Features always singletons and no mipmap force
            {
                if (info != null && info.homography.Is("gzImageHomography"))
                {
                    output.SurfaceHeight = surface;
                    output.SurfaceHeight_homography = ((ImageHomography)info.homography);
                }
                else
                    output.SurfaceHeight = null;
            }

            return true;
        }

        public static bool Build(State state, out Texture2D output, TextureManager textureCache = null)
        {
            output = default;

            if (!ReadTextureFromState(state, out Texture2D texture, textureCache))
                return false;

            output = texture;

            return true;
        }

        private static bool ReadTextureFromState(State state, out Texture2D result, TextureManager textureCache, out TextureImageInfo info, uint unit=0,bool useMipMap=true)
        {
            result = null;
            info = null;

            if (!state.HasTexture(unit) || state.GetMode(StateMode.TEXTURE) != StateModeActivation.ON)
                return false;

            using (var texture = state.GetTexture(unit))
            {
                try
                {
                    if (textureCache != null)
                    {
                        var ptr = texture.GetNativeReference();

                        if (textureCache.TryGet(ptr, out Texture cachedTexture, out TextureImageInfo cachedInfo))
                        {
                            result = (Texture2D)cachedTexture;
                            info = cachedInfo;
                            return true;
                        }

                        info = new TextureImageInfo();

                        if (!CopyTexture(texture, out result, info, useMipMap))
                            return false;

                        return textureCache.TryAdd(ptr, result, info);
                    }

                    info = new TextureImageInfo();

                    if (!CopyTexture(texture, out result, info, useMipMap))
                        return false;
                }
                finally
                {
                    // prevent dispose from locking object by releasing it manually here
                    texture.ReleaseAlreadyLocked();
                }
            }

            return true;
        }

        private static bool ReadTextureFromState(State state, out Texture2D result, TextureManager textureCache, uint unit = 0, bool useMipMap = true)
        {
            _profilerMarker.Begin();

            var res = ReadTextureFromStateInternal(state, out result, textureCache, unit, useMipMap);

            _profilerMarker.End();

            return res;
        }

        private static bool ReadTextureFromStateInternal(State state, out Texture2D result, TextureManager textureCache, uint unit = 0, bool useMipMap = true)
        {
            result = null;

            if (!state.HasTexture(unit) || state.GetMode(StateMode.TEXTURE) != StateModeActivation.ON)
                return false;

            using (var texture = state.GetTexture(unit))
            {
                try
                {
                    if (textureCache != null)
                    {
                        var ptr = texture.GetNativeReference();

                        if (textureCache.TryGet(ptr, out Texture cachedTexture, out _))
                        {
                            result = (Texture2D)cachedTexture;
                            return true;
                        }

                        if (!CopyTexture(texture, out result, null, useMipMap))
                            return false;

                        return textureCache.TryAdd(ptr, result, null);
                    }

                    if (!CopyTexture(texture, out result, null, useMipMap))
                        return false;
                }
                finally
                {
                    // prevent dispose from locking object by releasing it manually here
                    texture.ReleaseAlreadyLocked();
                }
            }

            return true;
        }

        private static bool CopyTexture(gzTexture gzTexture, out Texture2D result, TextureImageInfo info, bool mipChain = true)
        {
            result = null;

            if (!gzTexture.HasImage())
                return false;

            using (var image = gzTexture.GetImage())
            {
                var imageFormat = image.Format;

                var componentType = image.ComponentType;

                var components = image.Components;

                var textureFormat = GetUnityTextureFormat(imageFormat,componentType,components);

                var uncompress = !IsTextureFormatSupported(textureFormat);

                var nativePtr = IntPtr.Zero;

                if (!gzTexture.GetMipMapImageArray(ref nativePtr, out uint size, out _, out _, out _,
                    out uint width, out uint height, out uint depth, mipChain, uncompress))
                    return false;
                 
                if (depth != 1)
                    return false;

                result = new Texture2D((int)width, (int)height, textureFormat, mipChain);
                result.name = "SM - NodeTexture";

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

                if (info != null)
                {
                    info.homography = image.GetAttribute("UserDataImInfo", "ImI-Wrld-Hom");

                    info.border_x = image.GetAttribute("UserDataImInfo", "ImI-Pixel-X-border");
                    info.border_y = image.GetAttribute("UserDataImInfo", "ImI-Pixel-Y-border");
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
