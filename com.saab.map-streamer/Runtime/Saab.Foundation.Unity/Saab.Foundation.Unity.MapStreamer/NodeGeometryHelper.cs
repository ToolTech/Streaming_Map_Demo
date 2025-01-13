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
// Product		: Gizmo3D 2.12.201
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

// Framework
using System;

// Unity
using UnityEngine;

// GizmoSDK
using GizmoSDK.Gizmo3D;
using GizmoSDK.GizmoBase;

using ProfilerMarker = global::Unity.Profiling.ProfilerMarker;
using ProfilerCategory = global::Unity.Profiling.ProfilerCategory;

namespace Saab.Foundation.Unity.MapStreamer
{
    public static class GeometryHelper
    {
        [ThreadStatic] private static int[] _indices;

        [ThreadStatic] private static Vector3[] _positions;
        [ThreadStatic] private static Vector3[] _normals;
        [ThreadStatic] private static Color[] _colors;
        [ThreadStatic] private static Vector2[] _texCoords;

        private static readonly ProfilerMarker _profilerMarker = new ProfilerMarker(ProfilerCategory.Render, "SM-BuildGeometry");

        public static bool Build(Geometry geom, out Mesh output, out Color uniformColor)
        {
            _profilerMarker.Begin();
            
            var res = BuildInternal(geom, out output, out uniformColor);
            
            _profilerMarker.End();
            return res;
        }

        private static bool CopyPositionAndIndices(Geometry geom, Mesh mesh)
        {
            uint numVertices = 0;
            uint numIndices = 0;

            if (!geom.GetTrinagleVertexData<Vector3>(ref _positions, ref numVertices, ref _indices, ref numIndices))
                return false;

            if (numVertices < 3 || numIndices < 3)
                return false;
            
            mesh.SetVertices(_positions, 0, (int)numVertices);
            mesh.SetIndices(_indices, 0, (int)numIndices, MeshTopology.Triangles, 0);

            return true;
        }

        private static bool CopyColors(Geometry geom, Mesh mesh, out Color uniformColor)
        {
            uniformColor = Color.white;
            var numVertices = mesh.vertexCount;

            uint numColors = 0;

            // Right now we are not able to set an overall color of the geometry or per primitive color so we just skip it

            if (!geom.GetColorData<Color>(ref _colors, ref numColors))
                return false;

            if (numColors == 1)    // Overall color, not per vertex
            {
                uniformColor = _colors[0];
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

            if (!geom.GetNormalData<Vector3>(ref _normals, ref numNormals) /*|| numNormals != numVertices*/)
                return false;

            mesh.SetNormals(_normals, 0, numVertices);

            return true;
        }

        private static void GenerateNormals(Mesh mesh)
        {
            var numVertices = mesh.vertexCount;

            if (Geometry.GenerateNormalData<Vector3>(ref _normals, (uint)numVertices, new Vec3(0,1,0)))
            {
                mesh.SetNormals(_normals, 0, numVertices);
            }
        }

        private static bool CopyTexcoords(Geometry geom, Mesh mesh)
        {
            var texture_units = geom.GetTextureUnits();

            uint numTexCoords=0;

            for (uint ch = 0; ch < texture_units; ++ch)
            {
                if (geom.GetTexCoordData<Vector2>(ref _texCoords, ref numTexCoords, ch))
                {
                    mesh.SetUVs((int)ch, _texCoords, 0, (int)numTexCoords);
                }
                else
                    return false;
            }

            return true;
        }

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

        private static bool BuildInternal(Geometry geom, out Mesh mesh, out Color uniformColor)
        {
            uniformColor = Color.white;

            mesh = new Mesh();

            if (!CopyPositionAndIndices(geom, mesh))
                return false;

            CopyColors(geom, mesh, out uniformColor);

            if (!CopyNormals(geom, mesh))
                GenerateNormals(mesh);          // Todo: 221205 AMO This must be changed if we have an overall normal ! AMO

            if (!CopyTexcoords(geom, mesh))
                return false;

            return true;
        }
    }
}
