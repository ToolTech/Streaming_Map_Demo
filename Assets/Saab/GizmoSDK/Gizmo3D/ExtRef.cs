//******************************************************************************
// File			: ExtRef.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzExtRef class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.4
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
// AMO	180620	Created file 	
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
        public class ExtRef : Node
        {
            public ExtRef(IntPtr nativeReference) : base(nativeReference) { }

            public ExtRef(string name = "") : base(ExtRef_create(name)) { }

            public new static void InitializeFactory()
            {
                AddFactory(new ExtRef());
            }

            public static new void UninitializeFactory()
            {
                RemoveFactory("gzExtRef");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new ExtRef(nativeReference) as Reference;
            }

            public string ResourceURL
            {
                get
                {
                    return Marshal.PtrToStringUni(ExtRef_getResourceURL(GetNativeReference()));
                }
                set
                {
                    ExtRef_setResourceURL(GetNativeReference(), value);
                }
            }

            public string ObjectID
            {
                get
                {
                    return Marshal.PtrToStringUni(ExtRef_getObjectID(GetNativeReference()));
                }
                set
                {
                    ExtRef_setObjectID(GetNativeReference(), value);
                }
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr ExtRef_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr ExtRef_getResourceURL(IntPtr extref_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void ExtRef_setResourceURL(IntPtr extref_reference,string url);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr ExtRef_getObjectID(IntPtr extref_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void ExtRef_setObjectID(IntPtr extref_reference, string id);

            #endregion
        }
    }
}
