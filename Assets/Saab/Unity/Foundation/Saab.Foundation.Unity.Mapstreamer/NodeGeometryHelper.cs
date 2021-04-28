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
using System;

// Unity
using UnityEngine;

// GizmoSDK
using GizmoSDK.Gizmo3D;
using GizmoSDK.GizmoBase;

namespace Saab.Foundation.Unity.MapStreamer
{
    public static class GeometryHelper
    {
        [ThreadStatic] private static float[] _float_data;
        [ThreadStatic] private static int[] _indices;

        [ThreadStatic] private static Vector3[] _positions;
        [ThreadStatic] private static Vector3[] _normals;
        [ThreadStatic] private static Color[] _colors;
        [ThreadStatic] private static Vector2[] _texCoords;

        public static bool Build(Geometry geom, out Mesh output, MeshRenderer renderer)
        {
            try
            {
                Performance.Enter("GeometryBuilder.Build");

                return BuildInternal(geom, out output,renderer);
            }
            finally
            {
                Performance.Leave();
            }
        }

        private static bool CopyPositionAndIndices(Geometry geom, Mesh mesh)
        {
            uint numVertices = 0;
            uint numIndices = 0;
            if (!geom.GetVertexData(ref _float_data, ref numVertices, ref _indices, ref numIndices))
                return false;

            if (_positions == null || _positions.Length < numVertices)
                _positions = new Vector3[numVertices];

            var float_index = 0;
            for (var i = 0; i < _positions.Length; i++)
            {
                _positions[i].x = _float_data[float_index++];
                _positions[i].y = _float_data[float_index++];
                _positions[i].z = _float_data[float_index++];
            }

            mesh.SetVertices(_positions, 0, (int)numVertices);
            mesh.SetIndices(_indices, 0, (int)numIndices, MeshTopology.Triangles, 0);

            return true;
        }

        private static bool CopyColors(Geometry geom, Mesh mesh,MeshRenderer renderer)
        {
            var numVertices = mesh.vertexCount;

            uint numColors = 0;

            // Right now we are not able to set an overall color of the geometry or per primitive color so we just skip it

            if (!geom.GetColorData(ref _float_data, ref numColors))
                return false;

            if (_colors == null || _colors.Length < numColors)
                _colors = new Color[numColors];

            var float_index = 0;
            for (var i = 0; i < _colors.Length; i++)
            {
                _colors[i].r = _float_data[float_index++];
                _colors[i].g = _float_data[float_index++];
                _colors[i].b = _float_data[float_index++];
                _colors[i].a = _float_data[float_index++];
            }

            if (numColors == 1)    // Overall color, not per vertex
            {
                renderer.material.color = _colors[0];
                return true;
            }
            else if (numColors != numVertices)
                return false;

            mesh.SetColors(_colors, 0, numVertices);

            return true;
        }

        private static bool CopyNormals(Geometry geom, Mesh mesh)
        {
            var numVertices = mesh.vertexCount;

            uint numNormals = 0;
            if (!geom.GetNormalData(ref _float_data, ref numNormals) || numNormals != numVertices)
                return false;

            if (_normals == null || _normals.Length < numNormals)
                _normals = new Vector3[numNormals];

            var float_index = 0;
            for (var i = 0; i < _normals.Length; i++)
            {
                _normals[i].x = _float_data[float_index++];
                _normals[i].y = _float_data[float_index++];
                _normals[i].z = _float_data[float_index++];
            }

            mesh.SetNormals(_normals, 0, numVertices);

            return true;
        }

        private static void GenerateNormals(Mesh mesh)
        {
            var numVertices = mesh.vertexCount;

            if (_normals == null || _normals.Length < numVertices)
                _normals = new Vector3[numVertices];

            for (var i = 0; i < _normals.Length; i++)
            {
                _normals[i].x = 0;
                _normals[i].y = 1;
                _normals[i].z = 0;
            }

            mesh.SetNormals(_normals, 0, numVertices);
        }

        private static int CopyTexcoords(Geometry geom, Mesh mesh)
        {
            var numVertices = mesh.vertexCount;

            var texture_units = geom.GetTextureUnits();

            var totalTexCoords = numVertices * texture_units;

            if (_texCoords == null || _texCoords.Length < totalTexCoords)
                _texCoords = new Vector2[totalTexCoords];

            var offset = 0;
            for (var ch = 0; ch < texture_units; ++ch)
            {
                uint numCoords = 0;
                if (geom.GetTexCoordData(ref _float_data, ref numCoords, (uint)ch) && numCoords == numVertices)
                {
                    var float_index = 0;
                    for (var i = offset; i < (offset + numCoords); i++)
                    {
                        _texCoords[i].x = _float_data[float_index++];
                        _texCoords[i].y = _float_data[float_index++];
                    }

                    mesh.SetUVs(ch, _texCoords, offset, numVertices);
                }

                offset += numVertices;
            }

            return (int)texture_units;
        }

        private static bool BuildInternal(Geometry geom, out Mesh mesh,MeshRenderer renderer)
        {
            //output = default;

            //TODO: Lets try to get native data directly in the future
            //      Try NativeArray<> ??
            // NativeArray<T> ConvertExistingDataToNativeArray(void* dataPointer, int length, Unity.Collections.Allocator allocator); 

            //IntPtr native_vertice_data=IntPtr.Zero;
            //IntPtr native_indice_data=IntPtr.Zero;
            //
            //if (geom.GetVertexData(ref native_vertice_data, ref len, ref native_indice_data, ref indice_len))
            //{
            //    unsafe
            //    {
            //        NativeArray<Vector3> _vec3 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3>(native_vertice_data.ToPointer(), (int)/en, /Allocator.None);
            //    }
            //
            //}

            // Further: Unity offset SetVertexDataParams, SetVertexData API, allowing us to define a vertex structure
            // NativeArray<VertexStructure>
            //
            // This could be used to improve the SetX functions of the mesh

            mesh = new Mesh();

            if (!CopyPositionAndIndices(geom, mesh))
                return false;

            CopyColors(geom, mesh,renderer);

            if (!CopyNormals(geom, mesh))
                GenerateNormals(mesh);
            
            CopyTexcoords(geom, mesh);

            return true;
        }
    }
}
