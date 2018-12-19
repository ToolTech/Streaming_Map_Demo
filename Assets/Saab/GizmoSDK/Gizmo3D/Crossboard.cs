//******************************************************************************
// File			: Crossboard.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzCrossboard class
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
// AMO	181211	Created file 	
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
        [StructLayout(LayoutKind.Sequential)]
        public struct CrossboardLodData
        {
            public float near_dist;      // This structure must be 4 floats size
            public float near_fade;
            public float far_dist;
            public float far_fade;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct CrossboardObjectData
        {
            public float size;
            public float heading;
            public float pitch;
            public float roll;
        };

        public class Crossboard : Node
        {
            public Crossboard(IntPtr nativeReference) : base(nativeReference) { }

            public Crossboard(string name="") : base(Crossboard_create(name)) { }

            public new static void InitializeFactory()
            {
                AddFactory(new Crossboard());
            }

            public new static void UninitializeFactory()
            {
                RemoveFactory("gzCrossboard");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Crossboard(nativeReference) as Reference;
            }
            /// <summary>
            /// You get the positions of Crossboards as float x,y,z in one large array for performance sake
            /// </summary>
            /// <param name="vertice_data"></param>
            /// <returns></returns>
            public bool GetObjectPositions(out float[] vertice_data)
            {
                UInt32 vertices = 0;

                IntPtr native_vertice_data = IntPtr.Zero;

                if (Crossboard_getObjectPositions(GetNativeReference(), ref vertices, ref native_vertice_data))
                {
                    vertice_data = new float[vertices * 3];

                    Marshal.Copy(native_vertice_data, vertice_data, (int)0, (int)vertices * 3);

                    return true;
                }

                vertice_data = null;

                return false;
            }

            public bool GetObjectData(out float[] object_data)
            {
                UInt32 objects = 0;

                IntPtr native_object_data = IntPtr.Zero;

                if (Crossboard_getObjectData(GetNativeReference(), ref objects, ref native_object_data))
                {
                    object_data = new float[objects * 4];

                    Marshal.Copy(native_object_data, object_data, (int)0, (int)objects * 4);

                    return true;
                }

                object_data = null;

                return false;
            }

            public bool GetColorData(out float[] color_data)
            {
                UInt32 objects = 0;

                IntPtr native_color_data = IntPtr.Zero;

                if (Crossboard_getColorData(GetNativeReference(), ref objects, ref native_color_data))
                {
                    color_data = new float[objects * 4];

                    Marshal.Copy(native_color_data, color_data, (int)0, (int)objects * 4);

                    return true;
                }

                color_data = null;

                return false;
            }

            public bool UseColors
            {
                get { return Crossboard_getUseColors(GetNativeReference()); }
                set { Crossboard_setUseColors(GetNativeReference(), value); }
            }

            public float Near
            {
                get { return Crossboard_getNear(GetNativeReference()); }
                set { Crossboard_setNear(GetNativeReference(), value); }
            }

            public float NearFade
            {
                get { return Crossboard_getNearFade(GetNativeReference()); }
                set { Crossboard_setNearFade(GetNativeReference(), value); }
            }

            public float Far
            {
                get { return Crossboard_getFar(GetNativeReference()); }
                set { Crossboard_setFar(GetNativeReference(), value); }
            }

            public float FarFade
            {
                get { return Crossboard_getFarFade(GetNativeReference()); }
                set { Crossboard_setFarFade(GetNativeReference(), value); }
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Crossboard_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Crossboard_getObjectPositions(IntPtr crossboard_ref, ref UInt32 vertices, ref IntPtr native_vertice_data);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Crossboard_getObjectData(IntPtr crossboard_ref, ref UInt32 objects, ref IntPtr native_object_data);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Crossboard_getColorData(IntPtr crossboard_ref, ref UInt32 objects, ref IntPtr native_color_data);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Crossboard_getUseColors(IntPtr crossboard_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Crossboard_setUseColors(IntPtr crossboard_ref,bool on);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern float Crossboard_getNear(IntPtr crossboard_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Crossboard_setNear(IntPtr crossboard_ref, float distance);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern float Crossboard_getNearFade(IntPtr crossboard_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Crossboard_setNearFade(IntPtr crossboard_ref, float distance);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern float Crossboard_getFar(IntPtr crossboard_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Crossboard_setFar(IntPtr crossboard_ref, float distance);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern float Crossboard_getFarFade(IntPtr crossboard_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Crossboard_setFarFade(IntPtr crossboard_ref, float distance);

            #endregion
        }
    }
}
