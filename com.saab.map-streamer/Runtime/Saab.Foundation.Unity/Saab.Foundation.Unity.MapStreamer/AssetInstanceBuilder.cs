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
// File			: NodeGeometryHelper.cs
// Module		:
// Description	: Helper class for vertice updates
// Author		: Anders Modén
// Product		: Gizmo3D 2.12.184
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
// ZJP	240827	Created file                                        (2.12.171)
//
//******************************************************************************

using GizmoSDK.Gizmo3D;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer
{
    /// <summary>
    /// Internal builder used for RefNode (shared assets), during asset traversal geometry nodes are registered
    /// as prefabs using AddAssetPrefab() and SceneManager will use the AssetInstanceBuilder when traversing RefNode
    /// scenegraphs, if the geometry node has been registered the mesh and material will be reused and the NodeHandle
    /// will be marked as shared.
    /// </summary>
    internal class AssetInstanceBuilder : INodeBuilder
    {
        public PoolObjectFeature Feature => throw new NotSupportedException();

        // we create instances across multiple frames by default to reduce the running time of our main render method
        public BuildPriority Priority => BuildPriority.Low;

        // maps a native node address to the managed handle
        private readonly Dictionary<IntPtr, NodeHandle> _assetPrefabs = new Dictionary<IntPtr, NodeHandle>();

        /// <summary>
        /// Maps a Geometry node to a built gameobject
        /// </summary>
        /// <param name="geo"></param>
        /// <param name="nodeHandle"></param>
        public void AddAssetPrefab(Geometry geo, NodeHandle nodeHandle)
        {
            _assetPrefabs.Add(geo.GetNativeReference(), nodeHandle);
        }
        
        public bool Build(NodeHandle nodeHandle, NodeHandle activeStateNode)
        {
            if (!_assetPrefabs.TryGetValue(nodeHandle.node.GetNativeReference(), out NodeHandle assetPrefab))
            {
                Debug.LogError("no asset prefab found for geometry node during instancing pass");
                return false;
            }

            System.Diagnostics.Debug.Assert(nodeHandle.node.GetNativeReference() == assetPrefab.node.GetNativeReference());
            CreateInstanceFromPrefab(nodeHandle, assetPrefab);
            return true;
        }

        public void BuiltObjectReturnedToPool(GameObject gameObject, bool sharedAsset)
        {
            System.Diagnostics.Debug.Assert(sharedAsset);

            if (gameObject.TryGetComponent<MeshRenderer>(out var renderer))
            {
                renderer.enabled = false;
                renderer.sharedMaterial = null;
            }
            if (gameObject.TryGetComponent<MeshFilter>(out var filter))
                filter.sharedMesh = null;
        }

        public void InitPoolObject(GameObject gameObject)
        {
            gameObject.AddComponent<MeshRenderer>();
            gameObject.AddComponent<MeshFilter>();
        }

        public bool CanBuild(Node node, TraversalState traversalState, IntersectMaskValue intersectMask)
        {
            // SceneManager will manually control when this builder runs
            throw new NotSupportedException();
        }

        public void SetTextureManager(TextureManager textureManager)
        {
            // nop
        }

        private void CreateInstanceFromPrefab(NodeHandle clone, NodeHandle src)
        {
            // copy mesh renderer and assign shared material instance
            CopyMeshRenderer(src.gameObject, clone.gameObject);

            // copy mesh filter and assign shared mesh instance
            CopyMeshFilter(src.gameObject, clone.gameObject);
        }

        private static void CopyMeshRenderer(GameObject from, GameObject to)
        {
            if (!from.TryGetComponent<MeshRenderer>(out var src))
                return;

            if (!to.TryGetComponent<MeshRenderer>(out var dst))
                dst = to.AddComponent<MeshRenderer>();

            dst.sharedMaterial = src.sharedMaterial;
            dst.enabled = true;
        }

        private static void CopyMeshFilter(GameObject from, GameObject to)
        {
            if (!from.TryGetComponent<MeshFilter>(out var src))
                return;

            if (!to.TryGetComponent<MeshFilter>(out var dst))
                dst = to.AddComponent<MeshFilter>();

            dst.sharedMesh = src.sharedMesh;
        }
    }
}
