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
// Product		: Gizmo3D 2.10.6
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

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************
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
    }

    public interface ICache<in TKey, TValue>
    {
        bool TryGet(TKey key, out TValue value);
        bool TryAdd(TKey key, TValue value);
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
            return _cache.TryAdd(key, value);
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

                if (!ReadTextureFromState(state, out Texture2D texture, textureCache))
                    return false;

                output.Texture = texture;
            }
            finally
            {
                Performance.Leave();
            }

            return true;
        }

        private static bool ReadTextureFromState(State state, out Texture2D result, ICache<IntPtr, Texture2D> textureCache)
        {
            result = null;

            if (!state.HasTexture(0) || state.GetMode(StateMode.TEXTURE) != StateModeActivation.ON)
                return false;

            using (var texture = state.GetTexture(0))
            {

                if (textureCache != null && textureCache.TryGet(texture.GetNativeReference(), out result))
                    return true;

                if (!CopyTexture(texture, out result))
                    return false;

                if (textureCache != null)
                    textureCache.TryAdd(texture.GetNativeReference(), result);
            }

            return true;
        }

        private static bool CopyTexture(gzTexture gzTexture, out Texture2D result, bool mipChain = true)
        {
            result = null;

            if (!gzTexture.HasImage())
                return false;

            using (var image = gzTexture.GetImage())
            {
                var imageFormat = image.GetFormat();

                var textureFormat = GetUnityTextureFormat(imageFormat);

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
            }

            return true;
        }

        private static TextureFormat GetUnityTextureFormat(ImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case ImageFormat.RGBA:
                    return TextureFormat.RGBA32;

                case ImageFormat.RGB:
                    return TextureFormat.RGB24;

                case ImageFormat.COMPRESSED_RGBA8_ETC2:
                    return TextureFormat.ETC2_RGBA8;

                case ImageFormat.COMPRESSED_RGB8_ETC2:
                    return TextureFormat.ETC2_RGB;

                case ImageFormat.COMPRESSED_RGBA_S3TC_DXT1:
                case ImageFormat.COMPRESSED_RGB_S3TC_DXT1:
                    return TextureFormat.DXT1;

                case ImageFormat.COMPRESSED_RGBA_S3TC_DXT5:
                    return TextureFormat.DXT5;

                default:
                    throw new NotSupportedException();
            }
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
