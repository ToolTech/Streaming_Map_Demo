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
// File			: RoiNode.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzRoiNode class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.5
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

            public bool RoiTranslation
            {
                get
                {
                    return RoiNode_getRoiTranslation(GetNativeReference());
                }

                set
                {
                    RoiNode_setRoiTranslation(GetNativeReference(), value);
                }
            }

            public bool OrigoAtRoiNodePosition
            {
                get
                {
                    return RoiNode_getOrigoAtRoiNodePosition(GetNativeReference());
                }

                set
                {
                    RoiNode_setOrigoAtRoiNodePosition(GetNativeReference(), value);
                }
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

            public double LoadDistance
            {
                get
                {
                    return RoiNode_getLoadDistance(GetNativeReference());
                }

                set
                {
                    RoiNode_setLoadDistance(GetNativeReference(), value);
                }
            }

            public double PurgeDistance
            {
                get
                {
                    return RoiNode_getPurgeDistance(GetNativeReference());
                }

                set
                {
                    RoiNode_setPurgeDistance(GetNativeReference(), value);
                }
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr RoiNode_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void RoiNode_getPosition(IntPtr roinode_reference, ref Vec3D position);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void RoiNode_setPosition(IntPtr roinode_reference, ref Vec3D position);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double RoiNode_getLoadDistance(IntPtr roinode_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void RoiNode_setLoadDistance(IntPtr roinode_reference, double distance);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void RoiNode_setPurgeDistance(IntPtr roinode_reference, double distance);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double RoiNode_getPurgeDistance(IntPtr roinode_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void RoiNode_setOrigoAtRoiNodePosition(IntPtr roinode_reference, bool origo);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool RoiNode_getOrigoAtRoiNodePosition(IntPtr roinode_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void RoiNode_setRoiTranslation(IntPtr roinode_reference, bool origo);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool RoiNode_getRoiTranslation(IntPtr roinode_reference);
            #endregion
        }
    }
}
