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
// File			: NodeHandle.cs
// Module		:
// Description	: Handle to native Gizmo3D nodes
// Author		: Anders Modén
// Product		: Gizmo3D 2.10.1
//
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows (Win32) and IRIX® for  
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

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************

// Unity Managed classes
using UnityEngine;

// Gizmo Managed classes
using GizmoSDK.GizmoBase;
using GizmoSDK.Gizmo3D;

// Fix some conflicts between unity and Gizmo namespaces
using gzTransform = GizmoSDK.Gizmo3D.Transform;
using System.Collections.Generic;
using Assets.Crossboard;

public struct CrossboardDataset
{
    public Vector3[] POSITION;
    public Vector2[] UV0;
    public Vector2[] UV1;

    // Opaque shader
    public List<Vector2> UV0List;
    public List<Vector3> UV1List;
    public List<Vector3> UV2List;
    public List<Vector3> UV3List;

    // ************* Opaque shader compute *************
    public List<Vector4> UV0ListComp;
    public List<Vector4> UV1ListComp;

    public Color[] COLOR;
}

namespace Saab.Foundation.Unity.MapStreamer
{

    // The NodeHandle component of a game object stores a Node reference to the corresponding Gizmo item on the native side
    public class NodeHandle : MonoBehaviour
    {
        //public CrossboardRenderer Renderer { private get; set; }
        // Handle to native gizmo node
        internal Node node;

        // True if we have added this object as a lookup table object
        internal bool inObjectDict = false;

        // True if we have added this object as a node update object
        internal bool inNodeUpdateList = false;

        // Set to true if we shall continiously update our transform
        internal bool updateTransform = false;

        // Set to our material if we shall activate it on out geometry
        internal Material currentMaterial;

        // ComputeShader for culling + furstum
        internal ComputeShader ComputeShader;

        private readonly string ID = "Saab.Foundation.Unity.MapStreamer.NodeHandle";

        // We need to release all existing objects in a locked mode
        void OnDestroy()
        {

            // Basically all nodes in the GameObject scene should already be release by callbacks but there might be some nodes left that needs this behaviour
            if (node != null)
            {
                if (node.IsValid())
                {
                    NodeLock.WaitLockEdit();
                    node.Dispose();
                    NodeLock.UnLock();
                }
            }
        }

        public bool BuildGameObject()
        {
            if (node == null)
                return false;

            if (!node.IsValid())
                return false;

            // ---------------------------- Crossboard check -----------------------------------

            Crossboard cb = node as Crossboard;


            if (cb != null)
            {
                if (currentMaterial == null)    // No available material
                {
                    Message.Send(ID, MessageLevel.WARNING, $"Missing material in {node.GetName()}");
                    return false;
                }

                float[] position_data;
                float[] object_data;

                if (cb.GetObjectPositions(out position_data) && cb.GetObjectData(out object_data))
                {
                    int objects = position_data.Length / 3; // Number of objects

                    CrossboardDataset dataset = new CrossboardDataset();
                    dataset.POSITION = new Vector3[objects];
                    dataset.UV0ListComp = new List<Vector4>(objects);
                    dataset.UV1ListComp = new List<Vector4>(objects);
                    dataset.COLOR = new Color[objects];

                    CrossboardRenderer_ComputeShader Renderer = gameObject.AddComponent<CrossboardRenderer_ComputeShader>();
                    Renderer.OpaqueCrossboardCompute = true;

                    Renderer._computeShader = Instantiate(ComputeShader);

                    //Message.Send("NodeHandle", MessageLevel.DEBUG, "Instantiate ComputeShader");

                    int float3_index = 0;
                    int float4_index = 0;

                    for (int i = 0; i < objects; i++)
                    {
                        dataset.POSITION[i] = new Vector3(position_data[float3_index], position_data[float3_index + 1], position_data[float3_index + 2]);

                        // size, heading, pitch, roll 
                        dataset.UV0ListComp.Add(new Vector4(object_data[float4_index] * 2f, object_data[float4_index + 1], object_data[float4_index + 2], object_data[float4_index + 3]));
                        // postion offset (x - its up normal), planes offset ( xyz - in there normal direction)
                        dataset.UV1ListComp.Add(new Vector4(-0.02f, 0, 0, 0));

                        float3_index += 3;
                        float4_index += 4;
                    }

                    if (cb.UseColors)
                    {
                        float[] color_data;

                        if (cb.GetColorData(out color_data))
                        {
                            float4_index = 0;

                            Color[] colors = new Color[objects];

                            for (int i = 0; i < objects; i++)
                            {
                                colors[i] = new Color(color_data[float4_index], color_data[float4_index + 1], color_data[float4_index + 2], color_data[float4_index + 3]);
                                float4_index += 4;
                            }

                            dataset.COLOR = colors;
                        }
                    }

                    //Debug.Log("Instantiate mat");

                    Renderer.Material = Instantiate(currentMaterial);
                    Renderer.SetCrossboardDataset(dataset);
                }

                return true;

            }

            // ---------------------------- Geometry check -------------------------------------

            Geometry geom = node as Geometry;

            if (geom != null)
            {
                float[] float_data;
                int[] indices;

                if (geom.GetVertexData(out float_data, out indices))
                {
                    MeshFilter filter = gameObject.AddComponent<MeshFilter>();
                    MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

                    Mesh mesh = new Mesh();

                    Vector3[] vertices = new Vector3[float_data.Length / 3];

                    int float_index = 0;

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = new Vector3(float_data[float_index], float_data[float_index + 1], float_data[float_index + 2]);

                        float_index += 3;
                    }

                    mesh.vertices = vertices;
                    mesh.triangles = indices;


                    if (geom.GetColorData(out float_data))
                    {
                        if (float_data.Length / 4 == vertices.Length)
                        {
                            float_index = 0;

                            Color[] cols = new Color[vertices.Length];

                            for (int i = 0; i < vertices.Length; i++)
                            {
                                cols[i] = new Color(float_data[float_index], float_data[float_index + 1], float_data[float_index + 2], float_data[float_index + 3]);
                                float_index += 4;
                            }

                            mesh.colors = cols;
                        }
                    }

                    if (geom.GetNormalData(out float_data))
                    {
                        if (float_data.Length / 3 == vertices.Length)
                        {
                            float_index = 0;

                            Vector3[] normals = new Vector3[vertices.Length];

                            for (int i = 0; i < vertices.Length; i++)
                            {
                                normals[i] = new Vector3(float_data[float_index], float_data[float_index + 1], float_data[float_index + 2]);
                                float_index += 3;
                            }

                            mesh.normals = normals;
                        }
                    }
                    else
                        mesh.RecalculateNormals();

                    uint texture_units = geom.GetTextureUnits();

                    if (texture_units > 0)
                    {
                        if (geom.GetTexCoordData(out float_data, 0))
                        {
                            if (float_data.Length / 2 == vertices.Length)
                            {
                                float_index = 0;

                                Vector2[] tex_coords = new Vector2[vertices.Length];

                                for (int i = 0; i < vertices.Length; i++)
                                {
                                    tex_coords[i] = new Vector2(float_data[float_index], float_data[float_index + 1]);
                                    float_index += 2;
                                }

                                mesh.uv = tex_coords;
                            }
                        }

                        if ((texture_units > 1) && geom.GetTexCoordData(out float_data, 1))
                        {
                            if (float_data.Length / 2 == vertices.Length)
                            {
                                float_index = 0;

                                Vector2[] tex_coords = new Vector2[vertices.Length];

                                for (int i = 0; i < vertices.Length; i++)
                                {
                                    tex_coords[i] = new Vector2(float_data[float_index], float_data[float_index + 1]);
                                    float_index += 2;
                                }

                                mesh.uv2 = tex_coords;
                            }
                        }

                        if ((texture_units > 2) && geom.GetTexCoordData(out float_data, 2))
                        {
                            if (float_data.Length / 2 == vertices.Length)
                            {
                                float_index = 0;

                                Vector2[] tex_coords = new Vector2[vertices.Length];

                                for (int i = 0; i < vertices.Length; i++)
                                {
                                    tex_coords[i] = new Vector2(float_data[float_index], float_data[float_index + 1]);
                                    float_index += 2;
                                }

                                mesh.uv3 = tex_coords;
                            }
                        }

                        if ((texture_units > 3) && geom.GetTexCoordData(out float_data, 3))
                        {
                            if (float_data.Length / 2 == vertices.Length)
                            {
                                float_index = 0;

                                Vector2[] tex_coords = new Vector2[vertices.Length];

                                for (int i = 0; i < vertices.Length; i++)
                                {
                                    tex_coords[i] = new Vector2(float_data[float_index], float_data[float_index + 1]);
                                    float_index += 2;
                                }

                                mesh.uv4 = tex_coords;
                            }
                        }
                    }

                    filter.sharedMesh = mesh;

                    renderer.sharedMaterial = currentMaterial;

                }
                return true;

            }

            return true;
        }

        public void UpdateNodeInternals()
        {
            if (updateTransform)
            {
                gzTransform tr = node as gzTransform;

                if (tr != null)
                {
                    Vec3 translation;

                    if (tr.GetTranslation(out translation))
                    {
                        Vector3 trans = new Vector3(translation.x, translation.y, translation.z);
                        transform.localPosition = trans;
                    }
                }
            }
        }
    }

}
