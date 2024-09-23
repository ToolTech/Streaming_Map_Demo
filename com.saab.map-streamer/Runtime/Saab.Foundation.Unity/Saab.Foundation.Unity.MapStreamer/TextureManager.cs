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
// ZJP	240902	Created file                                        (2.12.179)
//
//******************************************************************************

// Framework
using System;
using System.Collections.Generic;

// Unity
using UnityEngine;

// GizmoSDK

namespace Saab.Foundation.Unity.MapStreamer
{
    public class TextureManager
    {
        private struct TextureCacheItem
        {
            public Texture Texture;
            public TextureImageInfo Info;
            public int RefCount;
        }

        private readonly Dictionary<IntPtr, TextureCacheItem> _textureCache = new Dictionary<IntPtr, TextureCacheItem>();
        private readonly Dictionary<Texture, IntPtr> _lookup = new Dictionary<Texture, IntPtr>();

        public bool TryAdd(IntPtr key, Texture value, TextureImageInfo info)
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

        public bool Free(Texture texture)
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
                GameObject.Destroy(texture);
                return true;
            }

            // failed to find the given resource, no operation performed
            return false;
        }

        public void Clear()
        {
            foreach (var kvp in _lookup)
                GameObject.Destroy(kvp.Key);
            
            _lookup.Clear();
            _textureCache.Clear();
        }
    }
}
