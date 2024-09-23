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
// AMO  230113  Added Feature Maps from texture 1                   (2.12.43)
//
//******************************************************************************

// Unity
using UnityEngine;

// GizmoSDK
using GizmoSDK.Gizmo3D;

namespace Saab.Foundation.Unity.MapStreamer
{
    public abstract class NodeBuilderBase : MonoBehaviour, INodeBuilder
    {
        [SerializeField]
        private BuildPriority _mode = BuildPriority.Immediate;

        protected TextureManager _textureManager;

        public abstract PoolObjectFeature Feature { get; }

        public BuildPriority Priority => _mode;

        public abstract bool Build(NodeHandle nodeHandle, NodeHandle activeStateNode);

        public abstract void BuiltObjectReturnedToPool(GameObject gameObject, bool sharedAsset);
        public abstract void InitPoolObject(GameObject gameObject);

        public abstract bool CanBuild(Node node, TraversalState traversalState, IntersectMaskValue intersectMask);

        public void SetTextureManager(TextureManager textureManager)
        {
            _textureManager = textureManager;
        }

        public virtual void Reset()
        {
            // NOP
        }
    }

    public abstract class GeometryBuilderBase : NodeBuilderBase
    {
        [SerializeField]
        [Tooltip("Material to use when state is missing")]
        protected Material _fallbackMaterial;

        [SerializeField]
        [Tooltip("Material to use when building geometry")]
        protected Material _material;

        [SerializeField]
        [Tooltip("Only nodes with a matching intersection mask will be processed by this builder")]
        protected IntersectMaskValue _mask;

        [SerializeField]
        [Tooltip("Ignore state information and always use the fallback material")]
        private bool _forceFallbackMaterial;


        public override bool CanBuild(Node node, TraversalState traversalState, IntersectMaskValue intersectMask)
        {
            return (intersectMask & _mask) != IntersectMaskValue.NOTHING && node is Geometry;
        }

        public override bool Build(NodeHandle nodeHandle, NodeHandle activeStateNode)
        {
            var geo = (Geometry)nodeHandle.node;
            
            var go = nodeHandle.gameObject;
            
            // MeshRenderer component
            if (!go.TryGetComponent<MeshRenderer>(out var meshRenderer))
                meshRenderer = go.AddComponent<MeshRenderer>();

            // MeshFilter component
            if (!go.TryGetComponent<MeshFilter>(out var meshFilter))
                meshFilter = go.AddComponent<MeshFilter>();

            if (!GeometryHelper.Build(geo, out Mesh mesh, out Color uniformColor))
            {
                Debug.LogError("failed to generate mesh, no geometry was built");

                Destroy(mesh);
                meshFilter.sharedMesh = null;
                meshRenderer.enabled = false;
                return false;
            }

            Material material;
            if (_forceFallbackMaterial)
            {
                material = Instantiate(_fallbackMaterial);
            }
            else
            {
                if (activeStateNode != null)
                {
                    if (CreateStateNodeResources(activeStateNode))
                    {
                        material = CreateMaterialFromState(activeStateNode);
                        material.color = uniformColor;
                    }
                    else
                    {
                        Debug.LogError("failed to create resources from state, using fallback material");
                        material = Instantiate(_fallbackMaterial);
                    }
                }
                else
                {
                    Debug.LogError("missing state, using fallback material");
                    material = Instantiate(_fallbackMaterial);
                }
            }

            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = material;
            meshRenderer.enabled = true;

            return true;
        }

        public override void BuiltObjectReturnedToPool(GameObject gameObject, bool sharedAsset)
        {
            if (sharedAsset)
            {
                if (gameObject.TryGetComponent<MeshRenderer>(out var renderer))
                {
                    renderer.enabled = false;
                    renderer.sharedMaterial = null;
                }
                if (gameObject.TryGetComponent<MeshFilter>(out var meshFilter))
                    meshFilter.sharedMesh = null;
            }
            else
            {
                // release material & mesh resources
                if (gameObject.TryGetComponent<MeshRenderer>(out var renderer))
                {
                    Destroy(renderer.sharedMaterial);
                    renderer.sharedMaterial = null;
                    renderer.enabled = false;
                }
                if (gameObject.TryGetComponent<MeshFilter>(out var meshFilter))
                {
                    Destroy(meshFilter.sharedMesh);
                    meshFilter.sharedMesh = null;
                }
            }
        }

        public override void InitPoolObject(GameObject gameObject)
        {
            gameObject.AddComponent<MeshRenderer>();
            gameObject.AddComponent<MeshFilter>();
        }

        /// <summary>
        /// Loads resources from a state and stores on the node for future reference
        /// </summary>
        /// <param name="stateNode">scenegraph rendering state</param>
        /// <returns>true if resources was loaded from the state correctly</returns>
        protected abstract bool CreateStateNodeResources(NodeHandle stateNode);

        /// <summary>
        /// Creates and setups a material instance from a scenegraph rendering state, uses fallback material if state is not valid
        /// </summary>
        /// <param name="stateNode">scenegraph rendering state</param>
        /// <returns>new material instance</returns>
        protected virtual Material CreateMaterialFromState(NodeHandle stateNode)
        {
            if (!stateNode.texture)
                return Instantiate(_fallbackMaterial);

            var material = Instantiate(_material);

            material.mainTexture = stateNode.texture;

            return material;
        }
    }
}
