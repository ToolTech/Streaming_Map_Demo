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
// File			: TextureManager.cs
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
// ZJP	240902	Created file                                        (2.12.179)
//
//******************************************************************************

// Framework
using System;
using System.Collections.Generic;


// Unity
using UnityEngine;
using UnityEngine.Experimental.Rendering;

// GizmoSDK

namespace Saab.Foundation.Unity.MapStreamer
{
    public class TextureManager
    {
        private struct TextureCacheItem
        {
            public Texture2D Texture;
            public TextureImageInfo Info;
            public int RefCount;
        }

        private readonly Dictionary<IntPtr, TextureCacheItem> _textureCache = new Dictionary<IntPtr, TextureCacheItem>();
        private readonly Dictionary<Texture2D, IntPtr> _lookup = new Dictionary<Texture2D, IntPtr>();

        public bool TryAdd(IntPtr key, Texture2D value, TextureImageInfo info)
        {
            if (_textureCache.TryAdd(key, new TextureCacheItem() { Texture = value, Info = info, RefCount = 1 }))
            {
                // add a reverse lookup to support the free operation
                _lookup.Add(value, key);
                return true;
            }

            // resource already existed, no operation performed
            return false;
        }

        public bool TryGet(IntPtr key, out Texture value, out TextureImageInfo info)
        {
            if (_textureCache.TryGetValue(key, out TextureCacheItem item))
            {
                // item existed in the cache, increase the ref count and return the resource
                item.RefCount++;
                _textureCache[key] = item;

                value = item.Texture;
                info = item.Info;
                return true;
            }

            // failed to find the given resource
            value = null;
            info = null;
            return false;
        }

        public bool Free(Texture2D texture)
        {
            if (_lookup.TryGetValue(texture, out IntPtr key))
            {
                TextureCacheItem item = _textureCache[key];
                item.RefCount--;

                // check if this was the last reference
                if (item.RefCount > 0)
                {
                    // simply update the ref counter
                    _textureCache[key] = item;
                    return true;
                }

                // this was the last reference for the texture, we should release it
                _lookup.Remove(texture);
                _textureCache.Remove(key);

                Texture2DCache.Free(texture);

                return true;
            }

            // failed to find the given resource, no operation performed
            return false;
        }

        public void Clear()
        {
            foreach (var kvp in _lookup)
                Texture2DCache.Free(kvp.Key);

            _lookup.Clear();
            _textureCache.Clear();
        }
    }

    public static class Texture2DCache
    {
        private static readonly Dictionary<ulong, Stack<Texture2D>> _textures = new Dictionary<ulong, Stack<Texture2D>>();

        private static readonly Dictionary<ulong, ulong> _singleTextureSize = new Dictionary<ulong, ulong>();

        // estimate of current texture cache size
        private static ulong _estimatedCacheSizeInBytes;

        // maximum allowed texture cache size (256 MB)
        private static ulong _maxCacheSizeInBytes = 256 * 1024 * 1024;

        // maximum allowed texture size, to prevent caching large textures (2048x2048)
        private static int _maxSize = 2048;

        private static int _texturesCreated;
        private static int _texturesDestroyed;

        public struct TexturePoolInfo
        {
            public int Width;
            public int Height;
            public TextureFormat Format;
            public bool MipChain;
            public int TextureCount;
            public ulong EstimatedSizeInBytes;
        }

        public struct CacheInfo
        {
            public ulong EstimatedSizeInBytes;
            public ulong MaxMemory;
            public int MaxSize;
            public int TexturesInCache;
            public int TexturesCreated;
            public int TexturesDestroyed;
        }

        public static List<TexturePoolInfo> GetDetailInfo(out CacheInfo cacheInfo)
        {
            int inCache = 0;

            var result = new List<TexturePoolInfo>();

            foreach (var kvp in _textures)
            {
                Stack<Texture2D> textures = kvp.Value;
                if (textures.Count == 0)
                    continue;

                Texture2D texture = textures.Peek();

                inCache += textures.Count;

                result.Add(new TexturePoolInfo()
                {
                    EstimatedSizeInBytes = (ulong)textures.Count * _singleTextureSize[kvp.Key],
                    Width = texture.width,
                    Height = texture.height,
                    Format = texture.format,
                    MipChain = texture.mipmapCount > 1,
                    TextureCount = textures.Count,
                });
            }

            cacheInfo = new CacheInfo()
            {
                EstimatedSizeInBytes = _estimatedCacheSizeInBytes,
                MaxMemory = _maxCacheSizeInBytes,
                MaxSize = _maxSize,
                TexturesCreated = _texturesCreated,
                TexturesDestroyed = _texturesDestroyed,
                TexturesInCache = inCache,
            };

            return result;
        }

        public static void SetMaximumTextureMemory(int sizeInMegabytes)
        {
            _maxCacheSizeInBytes = (ulong)sizeInMegabytes * (1024 * 1024);
        }

        public static void SetMaximumTextureSize(int size)
        {
            _maxSize = size;
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        public static void Clear()
        {
            foreach (var kvp in _textures)
            {
                Stack<Texture2D> textures = kvp.Value;
                while (textures.Count > 0)
                {
                    GameObject.Destroy(textures.Pop());
                    _texturesDestroyed++;
                }
            }

            _estimatedCacheSizeInBytes = 0;
        }

        /// <summary>
        /// Allocates a new texture or reuses a texture from the cache
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="format">Graphics format</param>
        /// <param name="mipChain">True to use mip maps</param>
        /// <param name="canBeCached">True if the texture setting was valid for caching</param>
        /// <returns>New or recycled texture</returns>
        public static Texture2D GetOrCreateTexture(int width, int height, GraphicsFormat format,
            bool mipChain, out bool canBeCached)
        {
            canBeCached = UseCache(width, height, format);

            if (canBeCached)
            {
                ulong key = CreateGraphicsKey(width, height, format, mipChain);

                // check if cache contains a reusable texture
                if (_textures.TryGetValue(key, out Stack<Texture2D> textures) && textures.Count > 0)
                {
                    _estimatedCacheSizeInBytes -= _singleTextureSize[key];
                    return textures.Pop();
                }
            }

            _texturesCreated++;

            return new Texture2D(
                     width,
                     height,
                     format,
                     mipChain ? TextureCreationFlags.MipChain : TextureCreationFlags.None
                 );
        }

        /// <summary>
        /// Frees a texture by adding it to the cache for later recycling, or destroys the texture
        /// if cache was full or the texture was not valid for recycling, such as wrong format or size.
        /// </summary>
        /// <param name="texture">Texture to free</param>
        public static void Free(Texture2D texture)
        {
            var width = texture.width;
            var height = texture.height;
            var format = texture.graphicsFormat;
            var mipChain = texture.mipmapCount > 1;

            if (!UseCache(width, height, format))
            {
                // this type of texture should never be recycled
                GameObject.Destroy(texture);
                _texturesDestroyed++;
                return;
            }

            ulong key = CreateGraphicsKey(width, height, format, mipChain);

            if (!_textures.TryGetValue(key, out Stack<Texture2D> textures))
            {
                // create new pool for this texture setting
                textures = new Stack<Texture2D>();
                _textures.Add(key, textures);

                // create new estimated size entry for this texture setting
                _singleTextureSize.Add(key, EstimateTextureSizeInBytes(width, height, format, mipChain));
            }

            ulong newEstimatedCacheSizeInBytes = _estimatedCacheSizeInBytes + _singleTextureSize[key];
            if (newEstimatedCacheSizeInBytes > _maxCacheSizeInBytes)
            {
                // texture did not fit in cache, destroy it
                GameObject.Destroy(texture);
                _texturesDestroyed++;
            }
            else
            {
                // add texture to our cache
                _estimatedCacheSizeInBytes = newEstimatedCacheSizeInBytes;
                textures.Push(texture);
            }
        }

        private static ulong CreateGraphicsKey(int width, int height, GraphicsFormat format, bool mipChain)
        {
            ulong key = 0;
            key |= (ulong)(ushort)width;                         // bits 0–15
            key |= (ulong)(ushort)height << 16;                  // bits 16–31
            key |= (ulong)(ushort)format << 32;                  // bits 32–47
            key |= mipChain ? (1UL << 48) : 0;                   // bit 48
            return key;
        }

        private static ulong EstimateTextureSizeInBytes(int width, int height, GraphicsFormat format,
            bool mipChain)
        {
            // Approximate bytes per pixel for common formats
            int bytesPerPixel = format switch
            {
                GraphicsFormat.R8_UNorm => 1,

                GraphicsFormat.R8G8_UNorm => 2,

                GraphicsFormat.R8G8B8A8_UNorm => 4,
                GraphicsFormat.R8G8B8A8_SRGB => 4,

                GraphicsFormat.R16G16_SFloat => 4,     // 16-bit float * 2 channels

                GraphicsFormat.R32_SFloat => 4,
                GraphicsFormat.R32G32_SFloat => 8,
                GraphicsFormat.R32G32B32A32_SFloat => 16,

                // many formats do not require special handling here
                _ => 4 // fallback (Unity uses this estimate internally too)
            };

            // Handle compressed formats separately
            if (GraphicsFormatIsCompressed(format))
            {
                int blockSize = GetBlockSize(format); // 8, 16 or ASTC-specific
                int blockWidth = GetBlockWidth(format);
                int blockHeight = GetBlockHeight(format);

                ulong totalSize = 0;

                int w = width;
                int h = height;

                while (true)
                {
                    int blocksWide = (w + blockWidth - 1) / blockWidth;
                    int blocksHigh = (h + blockHeight - 1) / blockHeight;

                    totalSize += (ulong)(blocksWide * blocksHigh * blockSize);

                    if (!mipChain || (w == 1 && h == 1))
                        break;

                    w = Mathf.Max(1, w / 2);
                    h = Mathf.Max(1, h / 2);
                }

                return totalSize;
            }
            else
            {
                ulong baseLevel = (ulong)width * (ulong)height * (ulong)bytesPerPixel;

                if (!mipChain)
                    return baseLevel;

                // Approximate mipchain overhead: 1.33x the base level size
                return (baseLevel * 4) / 3;
            }
        }

        private static int GetBlockSize(GraphicsFormat format)
        {
            switch (format)
            {
                case GraphicsFormat.RGBA_DXT1_UNorm:
                case GraphicsFormat.RGB_ETC2_UNorm:
                    return 8;

                case GraphicsFormat.RGBA_DXT5_UNorm:
                case GraphicsFormat.RGBA_BC7_UNorm:
                case GraphicsFormat.RGBA_ETC2_UNorm:
                    return 16;

                // ASTC uses fixed 16-byte blocks regardless of block dimension
                case GraphicsFormat.RGBA_ASTC4X4_UNorm:
                case GraphicsFormat.RGBA_ASTC5X5_UNorm:
                case GraphicsFormat.RGBA_ASTC6X6_UNorm:
                case GraphicsFormat.RGBA_ASTC8X8_UNorm:
                case GraphicsFormat.RGBA_ASTC10X10_UNorm:
                case GraphicsFormat.RGBA_ASTC12X12_UNorm:
                    return 16;

                default:
                    return 16;
            }
        }

        private static int GetBlockWidth(GraphicsFormat format)
        {
            return format switch
            {
                GraphicsFormat.RGBA_ASTC4X4_UNorm => 4,
                GraphicsFormat.RGBA_ASTC5X5_UNorm => 5,
                GraphicsFormat.RGBA_ASTC6X6_UNorm => 6,
                GraphicsFormat.RGBA_ASTC8X8_UNorm => 8,
                GraphicsFormat.RGBA_ASTC10X10_UNorm => 10,
                GraphicsFormat.RGBA_ASTC12X12_UNorm => 12,
                _ => 4 // BC and ETC always use 4×4
            };
        }

        private static int GetBlockHeight(GraphicsFormat format)
        {
            return format switch
            {
                GraphicsFormat.RGBA_ASTC4X4_UNorm => 4,
                GraphicsFormat.RGBA_ASTC5X5_UNorm => 5,
                GraphicsFormat.RGBA_ASTC6X6_UNorm => 6,
                GraphicsFormat.RGBA_ASTC8X8_UNorm => 8,
                GraphicsFormat.RGBA_ASTC10X10_UNorm => 10,
                GraphicsFormat.RGBA_ASTC12X12_UNorm => 12,
                _ => 4
            };
        }

        private static bool GraphicsFormatIsCompressed(GraphicsFormat format)
        {
            switch (format)
            {
                case GraphicsFormat.RGBA_DXT1_UNorm:
                case GraphicsFormat.RGBA_DXT5_UNorm:
                case GraphicsFormat.RGBA_BC7_UNorm:

                case GraphicsFormat.RGB_ETC2_UNorm:
                case GraphicsFormat.RGBA_ETC2_UNorm:

                case GraphicsFormat.RGBA_ASTC4X4_UNorm:
                case GraphicsFormat.RGBA_ASTC5X5_UNorm:
                case GraphicsFormat.RGBA_ASTC6X6_UNorm:
                case GraphicsFormat.RGBA_ASTC8X8_UNorm:
                case GraphicsFormat.RGBA_ASTC10X10_UNorm:
                case GraphicsFormat.RGBA_ASTC12X12_UNorm:
                    return true;

                default:
                    return false;
            }
        }

        private static bool UseCache(int width, int height, GraphicsFormat format)
        {
            if (width > _maxSize || height > _maxSize)
                return false;

            switch (format)
            {
                case GraphicsFormat.R8G8B8A8_UNorm:
                case GraphicsFormat.R8_UNorm:
                case GraphicsFormat.R16G16_SFloat:
                case GraphicsFormat.R32G32_SFloat:
                case GraphicsFormat.RGBA_DXT1_UNorm:
                case GraphicsFormat.RGBA_DXT5_UNorm:
                    return true;

                default:
                    return false;
            }
        }

    }
}
