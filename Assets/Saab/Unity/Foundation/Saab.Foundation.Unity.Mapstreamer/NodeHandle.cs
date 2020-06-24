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
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

// Gizmo Managed classes
using GizmoSDK.GizmoBase;
using GizmoSDK.Gizmo3D;

// Fix some conflicts between unity and Gizmo namespaces
using gzTransform = GizmoSDK.Gizmo3D.Transform;
using System.Collections.Generic;
using Saab.Utility.Unity.NodeUtils;
using Assets.Crossboard;
using System;

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

        // True if we have added this object as a node update object
        internal bool inNodeUtilsRegistry = false;

        // True if we have added this object as a node update object
        internal bool inNodeUpdateList = false;

        // Set to true if we shall continiously update our transform
        internal bool updateTransform = false;

        // Set to our material if we shall activate it on out geometry
        internal Material currentMaterial;

        // ComputeShader for culling + furstum
        internal ComputeShader ComputeShader;

        private readonly string ID = "Saab.Foundation.Unity.MapStreamer.NodeHandle";

        [System.ThreadStatic] private static float[] _float_data;
        [System.ThreadStatic] private static int[] _indices;
        [System.ThreadStatic] private static Vector3[] _vertices;
        [System.ThreadStatic] private static Color[] _cols;
        [System.ThreadStatic] private static Vector2[] _tex_coords;

        // We need to release all existing objects in a locked mode
        void OnDestroy()
        {

            // Basically all nodes in the GameObject scene should already be release by callbacks but there might be some nodes left that needs this behaviour
            if (node != null)
            {
                if (inNodeUtilsRegistry)
                {
                    NodeUtils.RemoveGameObjectReference(node.GetNativeReference(), gameObject);
                    inNodeUtilsRegistry = false;
                }


                if (node.IsValid())
                {
                    NodeLock.WaitLockEdit();
                    node.Dispose();
                    NodeLock.UnLock();
                }
            }
        }

        // Only called from one thread
        public bool BuildGameObject()
        {
            try
            {
                Performance.Enter("NodeHandle.BuildGameObject");

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
                    uint len=0;

                    uint tris = 0;

                    uint indice_len=0;

                    //TODO: Lets try to get native data directly in the future
                    //      Try NativeArray<> ??
                    // NativeArray<T> ConvertExistingDataToNativeArray(void* dataPointer, int length, Unity.Collections.Allocator allocator); 

                    //IntPtr native_vertice_data=IntPtr.Zero;
                    //IntPtr native_indice_data=IntPtr.Zero;

                    //if (geom.GetVertexData(ref native_vertice_data, ref len, ref native_indice_data, ref indice_len))
                    //{
                    //    unsafe
                    //    {
                    //        NativeArray<Vector3> _vec3 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3>(native_vertice_data.ToPointer(), (int)len, Allocator.None);
                    //    }
                
                    //}

                    if (geom.GetVertexData(ref _float_data, ref len, ref _indices,ref indice_len))
                    {
                        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
                        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

                        Mesh mesh = new Mesh();

                        if (_vertices == null || _vertices.Length < len)
                            _vertices = new Vector3[len];

                        int float_index = 0;

                        for (int i = 0; i < len; i++)
                        {
                            _vertices[i].x = _float_data[float_index++];
                            _vertices[i].y = _float_data[float_index++];
                            _vertices[i].z = _float_data[float_index++];
                        }

                        mesh.SetVertices(_vertices, 0, (int)len);

                        mesh.SetTriangles(_indices, 0, (int)indice_len,0);

                        tris = len;

                        if (geom.GetColorData(ref _float_data,ref len))
                        {
                            if (len == tris)
                            {
                                float_index = 0;

                                if (_cols == null || _cols.Length < len)
                                    _cols = new Color[len];

                                for (int i = 0; i < len; i++)
                                {
                                    _cols[i].r = _float_data[float_index++];
                                    _cols[i].g = _float_data[float_index++];
                                    _cols[i].b = _float_data[float_index++];
                                    _cols[i].a = _float_data[float_index++];
                                }

                                mesh.SetColors(_cols, 0, (int)len);
                            }
                        }

                        if (geom.GetNormalData(ref _float_data,ref len))
                        {
                            if (len == tris)
                            {
                                float_index = 0;

                                if (_vertices == null || _vertices.Length < len)
                                    _vertices = new Vector3[len];

                                for (int i = 0; i < len; i++)
                                {
                                    _vertices[i].x = _float_data[float_index++];
                                    _vertices[i].y = _float_data[float_index++];
                                    _vertices[i].z = _float_data[float_index++];
                                }

                                mesh.SetNormals(_vertices, 0, (int)len);
                            }
                        }
                        else
                        {
                            //mesh.RecalculateNormals();

                            if (_vertices == null || _vertices.Length < tris)
                                _vertices = new Vector3[tris];

                            for (int i = 0; i < tris; i++)
                            {
                                _vertices[i].x = 0;
                                _vertices[i].y = 1;
                                _vertices[i].z = 0;
                            }

                            mesh.SetNormals(_vertices, 0, (int)tris);

                            //Vector3[] normals = new Vector3[1];       // Obviously this doesnt work. Shame!

                            //normals[0] = new Vector3(0, 1, 0);

                            //mesh.normals = normals;

                        }

                        uint texture_units = geom.GetTextureUnits();

                        if (texture_units > 0)
                        {
                            if (geom.GetTexCoordData(ref _float_data, ref len, 0))
                            {
                                if (len == tris)
                                {
                                    float_index = 0;

                                    if (_tex_coords == null || _tex_coords.Length < len)
                                        _tex_coords = new Vector2[len];

                                    for (int i = 0; i < len; i++)
                                    {
                                        _tex_coords[i].x = _float_data[float_index++];
                                        _tex_coords[i].y = _float_data[float_index++];
                                    }

                                    mesh.SetUVs(0, _tex_coords, 0, (int)len);
                                }
                            }

                            if ((texture_units > 1) && geom.GetTexCoordData(ref _float_data, ref len,1))
                            {
                                if (len == tris)
                                {
                                    float_index = 0;

                                    if (_tex_coords == null || _tex_coords.Length < len)
                                        _tex_coords = new Vector2[len];


                                    for (int i = 0; i < len; i++)
                                    {
                                        _tex_coords[i].x = _float_data[float_index++];
                                        _tex_coords[i].y = _float_data[float_index++];
                                    }

                                    mesh.SetUVs(1, _tex_coords, 0, (int)len);
                                }
                            }

                            if ((texture_units > 2) && geom.GetTexCoordData(ref _float_data, ref len,2))
                            {
                                if (len == tris)
                                {
                                    float_index = 0;

                                    if (_tex_coords == null || _tex_coords.Length < len)
                                        _tex_coords = new Vector2[len];


                                    for (int i = 0; i < len; i++)
                                    {
                                        _tex_coords[i].x = _float_data[float_index++];
                                        _tex_coords[i].y = _float_data[float_index++];
                                    }

                                    mesh.SetUVs(2, _tex_coords, 0, (int)len);

                                }
                            }

                            if ((texture_units > 3) && geom.GetTexCoordData(ref _float_data, ref len ,3))
                            {
                                if (len == tris)
                                {
                                    float_index = 0;

                                    if (_tex_coords == null || _tex_coords.Length < len)
                                        _tex_coords = new Vector2[len];


                                    for (int i = 0; i < len; i++)
                                    {
                                        _tex_coords[i].x = _float_data[float_index++];
                                        _tex_coords[i].y = _float_data[float_index++];
                                    }

                                    mesh.SetUVs(3, _tex_coords, 0, (int)len);

                                }
                            }
                        }

                        filter.sharedMesh = mesh;
                        renderer.sharedMaterial = currentMaterial;
                    }
                    else
                        Message.Send(ID, MessageLevel.WARNING, $"Failed to load geometry {geom.GetName()}");

                    return true;
                }
                return true;
            }
            finally
            {
                Performance.Leave();
            }
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
