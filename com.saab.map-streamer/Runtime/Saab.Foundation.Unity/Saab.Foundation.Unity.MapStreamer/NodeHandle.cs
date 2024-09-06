﻿//******************************************************************************
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
// File			: NodeHandle.cs
// Module		:
// Description	: Handle to native Gizmo3D nodes
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
// AMO	180607	Created file                                        (2.9.1)
// AMO  181212  Added support for crossboards                       (2.10.1)
//
//******************************************************************************

// Framework
using GizmoSDK.Gizmo3D;
// Gizmo Managed classes
using GizmoSDK.GizmoBase;
// Helper for working with GizmoSDK in unity
using Saab.Unity.Extensions;
using System;
// Unity Managed classes
using UnityEngine;

// Fix some conflicts between unity and Gizmo namespaces
using gzTransform = GizmoSDK.Gizmo3D.Transform;

namespace Saab.Foundation.Unity.MapStreamer
{

    [Flags]
    public enum StateLoadInfo : byte
    {
        None = 0,

        /// <summary>
        /// Indicates that the main texture is loaded
        /// </summary>
        Texture = 1 << 0,
    }

    [Flags]
    public enum NodeStateFlags : byte
    {
        None = 0,

        /// <summary>
        /// we have added this object as a node update object
        /// </summary>
        InRegistry = 1 << 0,

        /// <summary>
        /// we have added this object as a node update object
        /// </summary>
        InUpdateList = 1 << 1,

        /// <summary>
        /// we shall continiously update our transform
        /// </summary>
        UpdateTransform = 1 << 2,

        /// <summary>
        /// node is referencing an asset prefab
        /// </summary>
        AssetInstance = 1 << 3,
    }

    // The NodeHandle component of a game object stores a Node reference to the corresponding Gizmo item on the native side
    public class NodeHandle : MonoBehaviour
    {
        // Handle to native gizmo node
        public Node node { get; internal set; }

        // Tracks internal state of the node
        [NonSerialized]
        internal NodeStateFlags stateFlags;

        // Tracks state status
        [NonSerialized]
        internal StateLoadInfo stateLoadInfo;

        // Pooling key associated with this handle
        [SerializeField]
        internal PoolObjectFeature featureKey;

        // builder associated with this node
        [NonSerialized]
        internal INodeBuilder builder;

        // used during delayed build to detect recycled nodes
        [NonSerialized]
        internal byte version;

        // state-data (for now only single texture)
        [NonSerialized]
        public Texture2D texture;

        // state-data-feature (for now only single texture)
        [NonSerialized]
        public Texture2D feature;

        // state-data-height (for now only single texture)
        [NonSerialized]
        public Texture2D surfaceHeight;

        // resolution in meters for feature map
        [NonSerialized]
        public Matrix3D featureInfo;

        private const string ID = "Saab.Foundation.Unity.MapStreamer.NodeHandle";
        
        internal bool inNodeUtilsRegistry
        {
            get { return (stateFlags & NodeStateFlags.InRegistry) != NodeStateFlags.None; }
            set { if (value) stateFlags |= NodeStateFlags.InRegistry; else stateFlags &= ~NodeStateFlags.InRegistry; }
        }

        internal bool inNodeUpdateList
        {
            get { return (stateFlags & NodeStateFlags.InUpdateList) != NodeStateFlags.None; }
            set { if (value) stateFlags |= NodeStateFlags.InUpdateList; else stateFlags &= ~NodeStateFlags.InUpdateList; }
        }

        internal bool updateTransform
        {
            get { return (stateFlags & NodeStateFlags.UpdateTransform) != NodeStateFlags.None; }
            set { if (value) stateFlags |= NodeStateFlags.UpdateTransform; else stateFlags &= ~NodeStateFlags.UpdateTransform; }
        }

        // [ZJP] NOTE
        // This is handled by the pooling system now

        // We need to release all existing objects in a locked mode
        //void OnDestroy()
        //{
        //    if (node == null)
        //        return;
        //
        //    // Basically all nodes in the GameObject scene should already be release by callbacks but there might be some nodes left that needs this /behaviour
        //    if (inNodeUtilsRegistry)
        //    {
        //        NodeUtils.RemoveGameObjectReference(node.GetNativeReference(), gameObject);
        //        inNodeUtilsRegistry = false;
        //    }
        //
        //    if (node.IsValid())
        //    {
        //        NodeLock.WaitLockEdit();
        //        node.Dispose();
        //        NodeLock.UnLock();
        //    }
        //}

        // Only called from one thread


        public void UpdateNodeInternals()
        {
            // Roi and RoiNodes are updated here as they have a relative position to the ROI camera position
            // They are the ones that move the large parts of a map

            if (!updateTransform)
                return;

            var tr = node as gzTransform;
            if (tr == null)
                return;

            if (tr.GetTranslation(out Vec3 translation))
            {
                transform.localPosition = new Vector3(translation.x, translation.y, translation.z);
                return;
            }

            tr.GetTransform(out Matrix4 mat4);

            transform.localPosition = mat4.Translation().ToVector3();
            transform.localScale = mat4.Scale().ToVector3();
            transform.localRotation = mat4.Quaternion().ToQuaternion();
        }

        internal void Recycle(TextureManager textureManager)
        {
            if (node != null)
                node.ReleaseAlreadyLocked();
            
            node = null;
            builder = null;
            stateLoadInfo = StateLoadInfo.None;
            stateFlags = NodeStateFlags.None;
            if (texture)
            {
                textureManager.Free(texture);
                texture = null;
            }
            if (feature)
            {
                textureManager.Free(feature);
                feature = null;
            }
            if (surfaceHeight)
            {
                textureManager.Free(surfaceHeight);
                surfaceHeight = null;
            }
            featureInfo = default;
            version++;
        }
    }
}
