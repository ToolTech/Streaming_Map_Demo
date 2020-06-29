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
// File			: CrossboardRenderer.cs
// Module		:
// Description	: Shader Code
// Author		: ALBNI
// Product		: BTA
//
//
// Revision History...
//
// Who	Date	Description
//
//
//******************************************************************************

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************

using UnityEngine;

namespace Assets.Crossboard
{
    public class CrossboardRenderer : MonoBehaviour
    {
        public bool OpaqueCrossboard = false;
        public bool OpaqueCrossboardCompute = false;
        public Material DefaultMaterial;

        public virtual void SetCrossboardDataset(CrossboardDataset dataset, Material material)
        {
            var mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = dataset.POSITION;

            if (OpaqueCrossboard)
            {
                mesh.SetUVs(0, dataset.UV0List);
                mesh.SetUVs(1, dataset.UV1List);
                mesh.SetUVs(2, dataset.UV2List);
                mesh.SetUVs(3, dataset.UV3List);
            }
            else if (OpaqueCrossboardCompute)
            {
                mesh.SetUVs(0, dataset.UV0ListComp);
                mesh.SetUVs(1, dataset.UV1ListComp);
            }
            else
            {
                mesh.uv = dataset.UV0;
                mesh.uv2 = dataset.UV1;
            }

            mesh.colors = dataset.COLOR;

            var n = dataset.POSITION.Length;
            var indices = new int[n];
            for (var i = 0; i < n; ++i)
            {
                indices[i] = i;
            }

            mesh.SetIndices(indices, MeshTopology.Points, 0);


            //mesh.bounds = new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f));

            var meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
            
            meshFilter.sharedMesh = mesh;

            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshRenderer.sharedMaterial = material ?? DefaultMaterial;
        }
    }
}
