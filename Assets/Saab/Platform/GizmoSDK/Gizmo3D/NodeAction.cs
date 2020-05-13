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
        public class NodeAction : NodeActionInterface, INameInterface 
        {
            public delegate void EventHandler_OnAction(NodeAction sender, NodeActionEvent action, Context context, NodeActionProvider trigger, TraverseAction traverser, IntPtr userdata);
            public event EventHandler_OnAction OnAction;

            public NodeAction(IntPtr nativeReference) : base(nativeReference) { ReferenceDictionary<NodeAction>.AddObject(this); }

            public NodeAction(string name="") : base(NodeAction_create(name)) { ReferenceDictionary<NodeAction>.AddObject(this); }

            override public void Release()
            {
               ReferenceDictionary<NodeAction>.RemoveObject(this);
               base.Release();
            }
            
            override public void ReleaseInRender()
            {
                ReferenceDictionary<NodeAction>.RemoveObject(this);
                base.ReleaseInRender();
            }

            public void Attach(Node node)
            {
                NodeAction_attach(GetNativeReference(), node.GetNativeReference());
            }

            public void Deattach(Node node)
            {
                NodeAction_deattach(GetNativeReference(), node.GetNativeReference());
            }

            static public void InitializeFactory()
            {
                AddFactory(new NodeAction());
            }

            public static void UninitializeFactory()
            {
                RemoveFactory("gzNodeAction");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new NodeAction(nativeReference) as Reference;
            }

            public string GetName()
            {
                return Marshal.PtrToStringUni(NodeAction_getName(GetNativeReference()));
            }

            public void SetName(string name)
            {
                NodeAction_setName(GetNativeReference(), name);
            }

            static public void Initialize()
            {
                if (s_class_init == null)
                    s_class_init = new Initializer();
            }

            static public void Uninitialize()
            {
                if (s_class_init != null)
                    s_class_init = null;
            }

            #region ---------------- Private functions ------------------------


            private sealed class Initializer
            {
                public Initializer()
                {
                    if (s_dispatcher_OnAction == null)
                    {
                        s_dispatcher_OnAction = new NodeAction_OnAction_Callback(OnAction_callback);
                        NodeAction_SetCallback_OnAction(s_dispatcher_OnAction);
                    }
                }

                ~Initializer()
                {
                    if (s_dispatcher_OnAction != null)
                    {
                        NodeAction_SetCallback_OnAction(null);
                        s_dispatcher_OnAction = null;
                    }
                }
            }

            static private Initializer s_class_init = new Initializer();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void NodeAction_OnAction_Callback(IntPtr instance, NodeActionEvent action, IntPtr native_context_ref, IntPtr native_trigger_ref, IntPtr native_traverser_ref, IntPtr userdata);


            static private NodeAction_OnAction_Callback s_dispatcher_OnAction;

            [MonoPInvokeCallback(typeof(NodeAction_OnAction_Callback))]
            static private void OnAction_callback(IntPtr instance,NodeActionEvent action, IntPtr native_context_ref, IntPtr native_trigger_ref, IntPtr native_traverser_ref, IntPtr userdata)
            {
                NodeAction na = ReferenceDictionary<NodeAction>.GetObject(instance);

                if (na != null)
                {
                    if (action != NodeActionEvent.REMOVE)
                        na.OnAction?.Invoke(na, action, CreateObject(native_context_ref) as Context, CreateObject(native_trigger_ref) as NodeActionProvider, CreateObject(native_traverser_ref) as TraverseAction, userdata);
                    else
                        na.OnAction?.Invoke(na, action, CreateObject(native_context_ref) as Context, null, CreateObject(native_traverser_ref) as TraverseAction, native_trigger_ref);
                }
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void NodeAction_SetCallback_OnAction(NodeAction_OnAction_Callback fn);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr NodeAction_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void NodeAction_setName(IntPtr nodeact_reference,string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr NodeAction_getName(IntPtr nodeact_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr NodeAction_attach(IntPtr nodeact_reference,IntPtr node_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr NodeAction_deattach(IntPtr nodeact_reference, IntPtr node_ref);




            #endregion

            #endregion
        }
    }
}
