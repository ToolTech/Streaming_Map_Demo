//******************************************************************************
// File			: Context.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzContext class
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
        public class Context : Reference
        {
            public Context(IntPtr nativeReference) : base(nativeReference) { }

            public Context() : base(Context_create()) { }
            
            public static void InitializeFactory()
            {
                AddFactory(new Context());
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Context(nativeReference) as Reference;
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Context_create();
           
            #endregion
        }
    }
}
