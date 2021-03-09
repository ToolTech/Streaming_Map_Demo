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
// File			: NodeAction.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzNodeAction class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.7
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
        public enum NodeActionEvent
        {
            // Subscribeable options
            BEFORE_PRE_TRAVERSE,
            AFTER_PRE_TRAVERSE,
            BEFORE_POST_TRAVERSE,
            AFTER_POST_TRAVERSE,
            SHADER_UPDATE,
            BEFORE_RENDER,
            AFTER_RENDER,

            BEFORE_SCENE_RENDER,
            AFTER_SCENE_RENDER,

            BEFORE_SCENE_UPDATE_NODE_DATA,
            AFTER_SCENE_UPDATE_NODE_DATA,

            BEFORE_NODE_DATA_UPDATE,
            AFTER_NODE_DATA_UPDATE,

            REF,
            UNREF,
            NODE_ID_CHANGE,
            IS_TRAVERSABLE,
            IS_NOT_TRAVERSABLE,


            // Internal action , no use
            ACTION_COUNT,

            // Default Actions, always active
            ADD,
            REMOVE  // You get trigger native ptr in userdata
        };

        public class NodeActionProvider : GizmoSDK.GizmoBase.Object 
        {
            public NodeActionProvider(IntPtr nativeReference) : base(nativeReference) {}

            public void AddActionInterface(NodeActionInterface receiver, NodeActionEvent action, IntPtr userdata=default(IntPtr))
            {
                NodeActionProvider_addActionInterface(GetNativeReference(), receiver.GetNativeReference(), action, userdata);
            }

            // We added NodeLock to Release to allow GC to be locked by edit or render
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

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void NodeActionProvider_addActionInterface(IntPtr nodeact_reference, IntPtr recv_ref, NodeActionEvent action,IntPtr userdata);

            #endregion
        }
    }
}
