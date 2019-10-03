//******************************************************************************
// File			: RoiNode.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzRoiNode class
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
        public class RoiNode : Transform
        {
            public RoiNode(IntPtr nativeReference) : base(nativeReference) { }

            public RoiNode(string name="") : base(RoiNode_create(name)) { }

            public new static void InitializeFactory()
            {
                AddFactory(new RoiNode());
            }

            public static new void UninitializeFactory()
            {
                RemoveFactory("gzRoiNode");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new RoiNode(nativeReference) as Reference;
            }

            public Vec3D Position
            {
                get
                {
                    Vec3D result = new Vec3D();

                    RoiNode_getPosition(GetNativeReference(), ref result);

                    return result;
                }

                set
                {
                    RoiNode_setPosition(GetNativeReference(), ref value);
                }
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr RoiNode_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void RoiNode_getPosition(IntPtr roi_reference, ref Vec3D position);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void RoiNode_setPosition(IntPtr roi_reference, ref Vec3D position);
            #endregion
        }
    }
}
