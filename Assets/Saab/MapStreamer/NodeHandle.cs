//*****************************************************************************
// File			: NodeHandle.cs
// Module		:
// Description	: Handle to native Gizmo3D nodes
// Author		: Anders Modén
// Product		: Gizmo3D 2.10.1
//
// Copyright © 2003- Saab Training Systems AB, Sweden
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

// Unity Managed classes
using UnityEngine;

// Gizmo Managed classes
using GizmoSDK.GizmoBase;
using GizmoSDK.Gizmo3D;

// Fix some conflicts between unity and Gizmo namespaces
using gzTransform = GizmoSDK.Gizmo3D.Transform;

namespace Saab.Unity.MapStreamer
{
    // The NodeHandle component of a game object stores a Node reference to the corresponding Gizmo item on the native side
    public class NodeHandle : MonoBehaviour
    {

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
                float[] position_data;
                float[] object_data;

                if (cb.GetObjectPositions(out position_data) && cb.GetObjectData(out object_data) )
                {
                    int objects = position_data.Length / 3; // Number of objects

                    MeshFilter filter = gameObject.AddComponent<MeshFilter>();
                    MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

                    Mesh mesh = new Mesh();


                    Vector3[] vertices = new Vector3[objects];
                
                    int[] subIndices = new int[objects];

                    Vector2[] uv = new Vector2[objects];
                    Vector2[] uv2 = new Vector2[objects];

                    int float3_index = 0;
                    int float4_index = 0;

                    for (int i = 0; i < objects; i++)
                    {
                        vertices[i] = new Vector3(position_data[float3_index], position_data[float3_index + 1], position_data[float3_index + 2]);

                        subIndices[i] = i;

                        uv[i] = new Vector2(object_data[float4_index], object_data[float4_index + 1]);
                        uv2[i] = new Vector2(object_data[float4_index+2], object_data[float4_index + 3]);

                        float3_index += 3;
                        float4_index += 4;
                    }


                    mesh.vertices = vertices;
                    mesh.uv = uv;
                    mesh.uv2 = uv2;

                    if(cb.UseColors)
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

                            mesh.colors = colors;
                        }
                    }
                   
                    mesh.SetIndices(subIndices, MeshTopology.Points, 0);

                    filter.sharedMesh = mesh;

                    renderer.sharedMaterial = currentMaterial;

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