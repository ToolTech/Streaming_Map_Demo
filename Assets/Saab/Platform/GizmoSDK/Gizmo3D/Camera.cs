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
// File			: Camera.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzCamera classes
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.5
//		
//
//			
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows (Win32) and Android for  
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
        public abstract class Camera : GizmoBase.Object, INameInterface
        {
            // Camera is locked in ref/unref by own instance mutex
            // The camera must be able to use ref/unref in both render/edit mode

            public Camera(IntPtr nativeReference) : base(nativeReference) { }
                        
            public void Debug(Context context,bool on=true)
            {
                Camera_debug(GetNativeReference(), context.GetNativeReference(),on);
            }

            public void DebugRefresh()
            {
                Camera_debug_refresh(GetNativeReference());
            }

            public string GetName()
            {
                return Marshal.PtrToStringUni(Camera_getName(GetNativeReference()));
            }

            public void SetName(string name)
            {
                Camera_setName(GetNativeReference(), name);
            }

            public void Render(Context context,UInt32 size_x,UInt32 size_y,UInt32 width,TraverseAction action)
            {
                Camera_render(GetNativeReference(),context.GetNativeReference(), size_x, size_y, width, action.GetNativeReference());
            }

            public void GetScreenVectors(int x, int y, uint size_x, uint size_y, out Vec3D position, out Vec3 direction)
            {
                position = new Vec3D();
                direction = new Vec3();

                Camera_getScreenVectors(GetNativeReference(), x, y, size_x, size_y, ref position, ref direction);
            }

            public Matrix4 Transform
            {
                get
                {
                    Matrix4 result = new Matrix4();

                    Camera_getTransform(GetNativeReference(), ref result);

                    return result;
                }

                set
                {
                    Camera_setTransform(GetNativeReference(), ref value);
                }
            }

            public bool RoiPosition
            {
                get
                {
                    return Camera_useRoiPosition(GetNativeReference());
                }

                set
                {
                    Camera_setUseRoiPosition(GetNativeReference(),value);
                }
            }

            public float FarClipPlane
            {
                get
                {
                    return Camera_getFarClipPlane(GetNativeReference());
                }

                set
                {
                    Camera_setFarClipPlane(GetNativeReference(),value);
                }
            }

            public float NearClipPlane
            {
                get
                {
                    return Camera_getNearClipPlane(GetNativeReference());
                }

                set
                {
                    Camera_setNearClipPlane(GetNativeReference(), value);
                }
            }


            public Scene Scene
            {
                get { return CreateObject(Camera_getScene(GetNativeReference())) as Scene; }
                set { Camera_setScene(GetNativeReference(), value.GetNativeReference()); }
            }

            public Vec3D Position
            {
                get { Vec3D pos = new Vec3D(); Camera_getPosition(GetNativeReference(), ref pos); return pos; }
                set { Camera_setPosition(GetNativeReference(), ref value); }
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_setName(IntPtr camera_reference, string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_setTransform(IntPtr camera_reference, ref Matrix4 matrix);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_getTransform(IntPtr camera_reference, ref Matrix4 matrix);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Camera_getName(IntPtr camera_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_setScene(IntPtr camera_reference, IntPtr scene_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Camera_getScene(IntPtr camera_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_setPosition(IntPtr camera_reference, ref Vec3D position);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_getPosition(IntPtr camera_reference, ref Vec3D position);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_render(IntPtr camera_reference, IntPtr context_reference , UInt32 size_x,UInt32 size_y,UInt32 width, IntPtr action_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_debug(IntPtr camera_reference,IntPtr context_reference,bool on);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_debug_refresh(IntPtr camera_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern float Camera_getNearClipPlane(IntPtr camera_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern float Camera_getFarClipPlane(IntPtr camera_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_setNearClipPlane(IntPtr camera_reference,float near);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_setFarClipPlane(IntPtr camera_reference,float far);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Camera_useRoiPosition(IntPtr camera_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_setUseRoiPosition(IntPtr camera_reference,bool use);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Camera_getScreenVectors(IntPtr camera_reference,Int32 x, Int32 y, UInt32 size_x, UInt32 size_y, ref Vec3D position, ref Vec3 direction);

            #endregion
        }

        public class PerspCamera : Camera
        {
            public PerspCamera(IntPtr nativeReference) : base(nativeReference) { }

            public PerspCamera(string name="") : base(PerspCamera_create(name)) { }


            public float VerticalFOV
            {
                get
                {
                    return PerspCamera_getVerticalFOV(GetNativeReference());
                }

                set
                {
                    PerspCamera_setVerticalFOV(GetNativeReference(), value);
                }
            }

            public float HorizontalFOV
            {
                get
                {
                    return PerspCamera_getHorizontalFOV(GetNativeReference());
                }

                set
                {
                    PerspCamera_setHorizontalFOV(GetNativeReference(), value);
                }
            }
            static public new void InitializeFactory()
            {
                AddFactory(new PerspCamera());
            }

            public static void UninitializeFactory()
            {
                RemoveFactory("gzPerspCamera");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new PerspCamera(nativeReference) as Reference;
            }

            #region Native dll interface ---------------------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr PerspCamera_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PerspCamera_setVerticalFOV(IntPtr camera_reference,float fov);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void PerspCamera_setHorizontalFOV(IntPtr camera_reference, float fov);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern float PerspCamera_getVerticalFOV(IntPtr camera_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern float PerspCamera_getHorizontalFOV(IntPtr camera_reference);
            #endregion
        }
    }
}
