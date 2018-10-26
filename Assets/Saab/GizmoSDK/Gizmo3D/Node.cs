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
        public class Node : NodeActionProvider, INameInterface, IDebugInterface
        {
            public Node(IntPtr nativeReference) : base(nativeReference) { }

            public Node(string name = "") : base(Node_create(name)) { }

            public new static void InitializeFactory()
            {
                AddFactory(new Node());
            }

            public static void UnInitializeFactory()
            {
                RemoveFactory("gzNode");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Node(nativeReference) as Reference;
            }

            public string GetName()
            {
                return Marshal.PtrToStringUni(Node_getName(GetNativeReference()));
            }

            public void SetName(string name)
            {
                Node_setName(GetNativeReference(), name);
            }

            public bool HasState()
            {
                return Node_hasState(GetNativeReference());
            }

            public State State
            {
                get
                {
                    return new State(Node_getState(GetNativeReference()));
                }

                set
                {
                    Node_setState(GetNativeReference(), value.GetNativeReference());
                }
            }

            public float BoundaryRadius
            {
                get
                {
                    return Node_getBoundaryRadius(GetNativeReference());
                }
            }

            public Vec3 BoundaryCenter
            {
                get
                {
                    Vec3 res;
                    Node_getBoundaryCenter(GetNativeReference(), out res);
                    return res;
                }
            }

            public virtual void Debug(DebugFlags features = DebugFlags.SHOW_ALL)
            {
                Node_debug(GetNativeReference(), features);
            }

            public bool HasDirtySaveData()
            {
                return Node_hasDirtySaveData(GetNativeReference());
            }

            public void SetDirtySaveData(bool on=true)
            {
                Node_setDirtySaveData(GetNativeReference(), on);
            }

            public bool SaveDirtyData(string url="")
            {
                return Node_saveDirtyData(GetNativeReference(), url);
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Node_create(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Node_setName(IntPtr node_reference,string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Node_getName(IntPtr node_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Node_debug(IntPtr node_reference, DebugFlags features);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Node_hasState(IntPtr node_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Node_getState(IntPtr node_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Node_setState(IntPtr node_reference, IntPtr state_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Node_hasDirtySaveData(IntPtr node_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Node_setDirtySaveData(IntPtr node_reference,bool on);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Node_saveDirtyData(IntPtr node_reference, string url);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern float Node_getBoundaryRadius(IntPtr node_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Node_getBoundaryCenter(IntPtr node_reference, out Vec3 center);

            #endregion
        }
    }
}
