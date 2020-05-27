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
// File			: NodeLock.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzNodeLock class
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
        public class NodeLock 
        {
            public static void WaitLockRender()
            {
                NodeLock_waitLockRender();
            }

            public static void WaitLockEdit()
            {
                NodeLock_waitLockEdit();
            }

            public static bool TryLockRender(UInt32 wait=10)
            {
                return NodeLock_tryLockRender(wait);
            }

            public static bool TryLockEdit(UInt32 wait = 10)
            {
                return NodeLock_tryLockEdit(wait);
            }

            public static void UnLock()
            {
                NodeLock_unLock();
            }

            public static bool IsLockedEdit()
            {
                return NodeLock_isLockedEdit();
            }

            public static bool IsLockedRender()
            {
                return NodeLock_isLockedRender();
            }

            public static bool IsLocked()
            {
                return NodeLock_isLocked();
            }

            public static bool IsLockedByMe()
            {
                return NodeLock_isLockedByMe();
            }



            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void NodeLock_waitLockRender();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void NodeLock_waitLockEdit();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool NodeLock_tryLockRender(UInt32 wait);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool NodeLock_tryLockEdit(UInt32 wait);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void NodeLock_unLock();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool NodeLock_isLockedEdit();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool NodeLock_isLockedRender();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool NodeLock_isLocked();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool NodeLock_isLockedByMe();






            #endregion
        }
    }
}
