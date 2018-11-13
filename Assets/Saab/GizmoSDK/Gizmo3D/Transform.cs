//******************************************************************************
// File			: Transform.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzTransform class
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
        public class Transform : Group
        {
            public Transform(IntPtr nativeReference) : base(nativeReference) { }

            public Transform(string name="") : base(Transform_create(name)) { }

            public new static void InitializeFactory()
            {
                AddFactory(new Transform());
            }

            public new static void UnInitializeFactory()
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

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Transform_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Transform_hasTranslation(IntPtr transform_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Transform_getTranslation(IntPtr transform_reference,ref Vec3 translation);
            #endregion
        }
    }
}
