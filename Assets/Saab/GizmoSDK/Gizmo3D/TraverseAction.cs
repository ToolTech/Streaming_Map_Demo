//******************************************************************************
// File			: Node.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzNode class
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
        public abstract class TraverseAction : Reference
        {
            public TraverseAction(IntPtr nativeReference) : base(nativeReference) { }
                       
           
            //#region Native dll interface ----------------------------------
            //[DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            //private static extern IntPtr TraverseAction_create();

            //#endregion
        }

        public class CullTraverseAction : TraverseAction
        {
            public CullTraverseAction(IntPtr nativeReference) : base(nativeReference) { }

            public CullTraverseAction() : base(CullTraverseAction_create()) { }

            public static void InitializeFactory()
            {
                AddFactory(new CullTraverseAction());
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new CullTraverseAction(nativeReference) as Reference;
            }


            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr CullTraverseAction_create();
            #endregion
        }
    }
}
