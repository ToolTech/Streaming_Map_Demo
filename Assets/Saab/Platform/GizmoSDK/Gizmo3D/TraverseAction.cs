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
// File			: Node.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzNode class
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

            override public void Release()
            {
                if (IsValid())
                {
                    try
                    {
                        NodeLock.WaitLockEdit();

                        base.Release();
                    }
                    finally
                    {

                        NodeLock.UnLock();
                    }
                }
            }

            virtual public void ReleaseInRender()
            {
                if (IsValid())
                {
                    try
                    {
                        NodeLock.WaitLockRender();

                        base.Release();
                    }
                    finally
                    {
                        NodeLock.UnLock();
                    }
                }
            }
        }

        public class CullTraverseAction : TraverseAction
        {
            public CullTraverseAction(IntPtr nativeReference) : base(nativeReference) { }

            public CullTraverseAction() : base(CullTraverseAction_create()) { }

            public static void InitializeFactory()
            {
                AddFactory(new CullTraverseAction());
            }

            public static void UninitializeFactory()
            {
                RemoveFactory("gzCullTraverseAction");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new CullTraverseAction(nativeReference) as Reference;
            }

            public void SetOmniTraverser(bool omni)
            {
                CullTraverseAction_setOmniTraverser(GetNativeReference(), omni);
            }


            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr CullTraverseAction_create();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void CullTraverseAction_setOmniTraverser(IntPtr traverse_ref,bool omni);
            #endregion
        }
    }
}
