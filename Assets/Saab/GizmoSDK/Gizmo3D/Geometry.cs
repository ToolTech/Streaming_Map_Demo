//******************************************************************************
// File			: Geometry.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzGeometry class
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
// AMO	180301	Created file 	
//
//******************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;


namespace GizmoSDK
{
    namespace Gizmo3D
    {
        public class Geometry : Node
        {
            public Geometry(IntPtr nativeReference) : base(nativeReference) { }

            public Geometry(string name="") : base(Geometry_create(name)) { }

            public new static void InitializeFactory()
            {
                AddFactory(new Geometry());
            }

            public new static void UninitializeFactory()
            {
                RemoveFactory("gzGeometry");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Geometry(nativeReference) as Reference;
            }

            public bool  GetVertexData(out float [] vertice_data, out int[] indice_data)
            {
                UInt32 vertices=0;
                UInt32 indices=0;

                IntPtr native_vertice_data = IntPtr.Zero;
                IntPtr native_indice_data = IntPtr.Zero;

                if ( Geometry_getVertexData(GetNativeReference(), ref vertices, ref native_vertice_data, ref indices,ref native_indice_data) )
                {
                    vertice_data = new float[vertices*3];
                    indice_data = new int[indices];

                    Marshal.Copy(native_vertice_data, vertice_data, (int)0, (int)vertices * 3);
                    Marshal.Copy(native_indice_data, indice_data, (int)0, (int)indices);

                    Geometry_freeVertexData(GetNativeReference(), native_vertice_data, native_indice_data);

                    return true;
                }


                vertice_data = null;
                indice_data = null;

                return false;
            }

            public bool GetColorData(out float[] color_data)
            {
                UInt32 colors = 0;

                IntPtr native_color_data = IntPtr.Zero;

                if (Geometry_getColorData(GetNativeReference(), ref colors, ref native_color_data))
                {
                    color_data = new float[colors * 4];

                    Marshal.Copy(native_color_data, color_data, (int)0, (int)colors * 4);

                    return true;
                }

                color_data = null;

                return false;
            }

            public bool GetNormalData(out float[] normal_data)
            {
                UInt32 normals = 0;

                IntPtr native_normal_data = IntPtr.Zero;

                if (Geometry_getNormalData(GetNativeReference(), ref normals, ref native_normal_data))
                {
                    normal_data = new float[normals * 3];

                    Marshal.Copy(native_normal_data, normal_data, (int)0, (int)normals * 3);

                    return true;
                }

                normal_data = null;

                return false;
            }

            public UInt32 GetTextureUnits()
            {
                return Geometry_getTextureUnits(GetNativeReference());
            }

            public bool GetTexCoordData(out float[] uv_data,UInt32 texture_unit=0)
            {
                UInt32 texcoord = 0;

                IntPtr native_textcoord_data = IntPtr.Zero;

                if (Geometry_getTexCoordData(GetNativeReference(), ref texcoord, ref native_textcoord_data, texture_unit))
                {
                    uv_data = new float[texcoord * 2];

                    Marshal.Copy(native_textcoord_data, uv_data, (int)0, (int)texcoord * 2);

                    return true;
                }

                uv_data = null;

                return false;
            }


            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Geometry_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Geometry_getVertexData(IntPtr geometry_reference,ref UInt32 vertices,ref IntPtr native_vertice_data,ref UInt32 indices,ref IntPtr native_indice_data);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Geometry_freeVertexData(IntPtr geometry_reference,IntPtr native_vertice_data, IntPtr native_indice_data);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Geometry_getColorData(IntPtr geometry_reference, ref UInt32 colors, ref IntPtr native_color_data);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Geometry_getNormalData(IntPtr geometry_reference, ref UInt32 normals, ref IntPtr native_normal_data);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 Geometry_getTextureUnits(IntPtr geometry_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Geometry_getTexCoordData(IntPtr geometry_reference, ref UInt32 texcoords, ref IntPtr native_texcoord_data, UInt32 texture_unit);

            #endregion
        }
    }
}
