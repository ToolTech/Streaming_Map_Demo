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
// File			: NodeBuilder.cs
// Module		:
// Description	: Generic Builder Interface
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

using UnityEngine;
using GizmoSDK.Gizmo3D;

namespace Saab.Foundation.Unity.MapStreamer
{
    public enum BuildPriority
    {
        Immediate,
        Low,
    }

    public interface INodeBuilder
    {
        PoolObjectFeature Feature { get; }

         BuildPriority Priority { get; }

        /// <summary>
        /// Invoked by the SceneManager to determine if the provided node together with additional info
        /// is a valid build target for this builder
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="traversalState">Current traversal state</param>
        /// <param name="intersectMask">Current intersector mask</param>
        /// <returns></returns>
        bool CanBuild(Node node, TraversalState traversalState, IntersectMaskValue intersectMask);

        /// <summary>
        /// Builds the object from a native node using the provided state information
        /// </summary>
        /// <param name="nodeHandle">NodeHandle</param>
        /// <param name="activeStateNode">Current active state, such as texture and color information</param>
        /// <returns></returns>
        bool Build(NodeHandle nodeHandle, NodeHandle activeStateNode);

        /// <summary>
        /// Invoked when an object built by this builder is recycled
        /// </summary>
        /// <param name="gameObject">object that was recycled</param>
        /// <param name="sharedAsset">true if the object was was sharing resources</param>
        void BuiltObjectReturnedToPool(GameObject gameObject, bool sharedAsset);

        /// <summary>
        /// Invoked when allocating new objects for this builder instance, allowing the builder
        /// to add all required components
        /// </summary>
        /// <param name="gameObject">GameObject to decorate</param>
        void InitPoolObject(GameObject gameObject);

        /// <summary>
        /// Assigns the texture manager instance that the builder should use when working with textures
        /// </summary>
        /// <param name="textureManager">Assigned TextureManager instance</param>
        void SetTextureManager(TextureManager textureManager);

        /// <summary>
        /// Assigns the Material manager instance that the builder should use when working with Materials
        /// </summary>
        /// <param name="materialManager">Assigned MaterialManager instance</param>
        void SetMaterialManager(MaterialManager materialManager);

        /// <summary>
        /// Invoked as a result of reset, builder should clear any state
        /// </summary>
        void Reset();
    }
}
