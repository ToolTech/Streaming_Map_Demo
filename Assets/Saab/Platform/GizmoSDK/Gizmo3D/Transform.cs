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
// File			: Transform.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzTransform class
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
        public class Transform : Group
        {
            public Transform(IntPtr nativeReference) : base(nativeReference) { }

            public Transform(string name="") : base(Transform_create(name)) { }

            public new static void InitializeFactory()
            {
                AddFactory(new Transform());
            }

            public new static void UninitializeFactory()
            {
                RemoveFactory("gzTransform");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Transform(nativeReference) as Reference;
            }

            public bool HasTranslation()
            {
                return Transform_hasTranslation(GetNativeReference());
            }

            public bool GetTranslation(out Vec3 translation)
            {
                translation = new Vec3();
                return Transform_getTranslation(GetNativeReference(),ref translation);
            }

            public bool IsActive()
            {
                return Transform_isActive(GetNativeReference());
            }

            public void GetTransform(out Matrix4 transform)
            {
                transform = new Matrix4();
                Transform_getTransform(GetNativeReference(), ref transform);
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Transform_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Transform_hasTranslation(IntPtr transform_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Transform_getTranslation(IntPtr transform_reference,ref Vec3 translation);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Transform_isActive(IntPtr transform_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Transform_getTransform(IntPtr transform_reference, ref Matrix4 translation);

            #endregion
        }
    }
}
