//******************************************************************************
// File			: NodeAction.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzNodeAction class
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
                    NodeLock.WaitLockEdit();

                    base.Release(); 

                    NodeLock.UnLock();
                }
            }

            
            virtual public void ReleaseInRender()
            {
                if (IsValid())
                {
                    NodeLock.WaitLockRender();

                    base.Release();

                    NodeLock.UnLock();
                }
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void NodeActionProvider_addActionInterface(IntPtr nodeact_reference, IntPtr recv_ref, NodeActionEvent action,IntPtr userdata);

            #endregion
        }
    }
}
