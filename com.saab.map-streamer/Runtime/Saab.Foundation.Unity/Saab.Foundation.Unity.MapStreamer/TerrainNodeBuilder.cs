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
// ZJP	240821	Created file                                        (2.12.171)
//
//******************************************************************************

using GizmoSDK.Gizmo3D;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer
{
    public class TerrainNodeBuilder : GeometryBuilderBase
    {
        public override PoolObjectFeature Feature => PoolObjectFeature.Terrain;

        public override bool CanBuild(Node node, TraversalState traversalState, IntersectMaskValue intersectMask)
        {
            return (traversalState == TraversalState.None) && 
                base.CanBuild(node, traversalState, intersectMask);
        }

        protected override bool CreateStateNodeResources(NodeHandle stateNode)
        {
            // check if the state has already been loaded
            if (stateNode.stateLoadInfo.HasFlag(StateLoadInfo.Texture))
                return true;

            var state = stateNode.node.State;

            if (!StateHelper.Build(state, out StateBuildOutput buildOutput, _textureManager))
            {
                state.ReleaseAlreadyLocked();
                return false;
            }

            stateNode.stateLoadInfo |= StateLoadInfo.Texture;
            stateNode.texture = buildOutput.Texture;
            stateNode.feature = buildOutput.Feature;
            stateNode.surfaceHeight = buildOutput.SurfaceHeight;
            stateNode.featureInfo = buildOutput.Feature_homography;

            state.ReleaseAlreadyLocked();
            return true;
        }

        protected override Material CreateMaterialFromState(NodeHandle stateNode)
        {
            // use fallback if no texture is available
            if (!stateNode.texture)
                return Instantiate(_fallbackMaterial);

            var material = Instantiate(_material);

            // main texture
            material.mainTexture = stateNode.texture;

            // feature
            if (stateNode.feature != null)
            {
                material.SetTexture("feature", stateNode.feature);
                material.EnableKeyword("feature");
            }
            else
            {
                material.DisableKeyword("feature");
            }

            return material;
        }
    }
}
