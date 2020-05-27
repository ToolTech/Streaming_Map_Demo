//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s)
// herein belong to SAAB AB. The program(s) may be used and/or
// copied only with the written permission of SAAB AB, or in
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
// File			: Geometry.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzGeometry class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.6
//		
//
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

            public bool  GetVertexData(ref float [] vertice_data, ref UInt32 vertices,ref int[] indice_data,ref UInt32 indices)
            {
                IntPtr native_vertice_data = IntPtr.Zero;
                IntPtr native_indice_data = IntPtr.Zero;

                if ( Geometry_getVertexData(GetNativeReference(), ref vertices, ref native_vertice_data, ref indices,ref native_indice_data) )
                {
                    if(vertice_data==null || vertice_data.Length < vertices * 3)
                        vertice_data = new float[vertices*3];

                    if (indice_data == null || indice_data.Length < indices)
                        indice_data = new int[indices];

                    Marshal.Copy(native_vertice_data, vertice_data, (int)0, (int)vertices * 3);
                    Marshal.Copy(native_indice_data, indice_data, (int)0, (int)indices);
                      
                    return true;
                }

                return false;
            }

            public bool GetVertexData(ref IntPtr native_vertice_data, ref UInt32 vertices, ref IntPtr native_indice_data, ref UInt32 indices)
            {
                return Geometry_getVertexData(GetNativeReference(), ref vertices, ref native_vertice_data, ref indices, ref native_indice_data);
            }

            public bool GetColorData(ref float[] color_data,ref UInt32 colors)
            {
                IntPtr native_color_data = IntPtr.Zero;

                if (Geometry_getColorData(GetNativeReference(), ref colors, ref native_color_data))
                {
                    if(color_data==null || color_data.Length<colors*4)
                        color_data = new float[colors * 4];

                    Marshal.Copy(native_color_data, color_data, (int)0, (int)colors * 4);

                    return true;
                }

                return false;
            }

            public bool GetNormalData(ref float[] normal_data, ref UInt32 normals)
            {
                IntPtr native_normal_data = IntPtr.Zero;

                if (Geometry_getNormalData(GetNativeReference(), ref normals, ref native_normal_data))
                {
                    if (normal_data == null || normal_data.Length < normals * 3)
                        normal_data = new float[normals * 3];

                    Marshal.Copy(native_normal_data, normal_data, (int)0, (int)normals * 3);

                    return true;
                }

                return false;
            }

            public UInt32 GetTextureUnits()
            {
                return Geometry_getTextureUnits(GetNativeReference());
            }

            public bool GetTexCoordData(ref float[] uv_data, ref UInt32 texcoord,  UInt32 texture_unit=0)
            {
                IntPtr native_textcoord_data = IntPtr.Zero;

                if (Geometry_getTexCoordData(GetNativeReference(), ref texcoord, ref native_textcoord_data, texture_unit))
                {
                    if (uv_data == null || uv_data.Length < texcoord * 2)
                        uv_data = new float[texcoord * 2];

                    Marshal.Copy(native_textcoord_data, uv_data, (int)0, (int)texcoord * 2);

                    return true;
                }

                return false;
            }


            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Geometry_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Geometry_getVertexData(IntPtr geometry_reference,ref UInt32 vertices,ref IntPtr native_vertice_data,ref UInt32 indices,ref IntPtr native_indice_data);
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
