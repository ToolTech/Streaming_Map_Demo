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
// File			: GeometryBuilder.cs
// Module		:
// Description	: Special class for geometry builder
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

// Unity
using UnityEngine;

// GizmoSDK
using GizmoSDK.Gizmo3D;

namespace Saab.Foundation.Unity.MapStreamer
{
    public class DefaultGeometryNodeBuilder : INodeBuilder
    {
        public BuildPriority Priority { get; private set; }

        public PoolObjectFeature Feature => PoolObjectFeature.StaticMesh;

        // lifetime data
        private readonly Material _defaultMaterial;

        // per-frame data

        public DefaultGeometryNodeBuilder(Shader shader, BuildPriority priority = BuildPriority.Immediate)
        {
            Priority = priority;

            _defaultMaterial = new Material(shader);
        }

        public bool CanBuild(Node node)
        {
            return node is Geometry;
        }

        public bool Build(NodeHandle nodeHandle, GameObject gameObject, NodeHandle activeStateNode)
        {
            var node = nodeHandle.node;

            var geo = node as Geometry;
            if (geo == null)
                return false;

            // Renderer, state and material ---------------------------------

            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshRenderer.sharedMaterial = _defaultMaterial;

            // check if the texture is loaded for this state, otherwise load it
            if (activeStateNode != null)
            {
                if ((activeStateNode.stateLoadInfo & StateLoadInfo.Texture) == StateLoadInfo.None)
                {
                    var state = activeStateNode.node.State;

                    if (!StateHelper.Build(state, out StateBuildOutput buildOutput, null))
                        return false;

                    activeStateNode.stateLoadInfo |= StateLoadInfo.Texture;
                    activeStateNode.texture = buildOutput.Texture;
                }

                meshRenderer.material.mainTexture = activeStateNode.texture;
            }

            // --------- Mesh ---------------------------------------------------------

            if (!GeometryHelper.Build(geo, out Mesh mesh, meshRenderer))
                return false;

            // ------- Filter ---------------------------------------------------------

            var meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            meshFilter.sharedMesh = mesh;

            

            return true;
        }

        public void BuiltObjectReturnedToPool(GameObject gameObject)
        {
            // NOP
        }
    }
}
