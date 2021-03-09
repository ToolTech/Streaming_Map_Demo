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
        public class NodeIterator : Reference , IEnumerator<Node>
        {
            public NodeIterator(Group group) : base(NodeIterator_create(group.GetNativeReference())) { }

            public Node Current => m_current;

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (NodeIterator_iterate(GetNativeReference()))
                {
                    m_current = CreateObject(NodeIterator_current(GetNativeReference())) as Node;

                    return true;
                }

                return false;
            }

            public void Reset()
            {
                NodeIterator_reset(GetNativeReference());
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr NodeIterator_create(IntPtr event_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool NodeIterator_iterate(IntPtr iterator_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr NodeIterator_current(IntPtr iterator_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void NodeIterator_reset(IntPtr iterator_ref);

            private Node m_current;
        }

        public class Group : Node , IEnumerable<Node>
        {
            public Group(IntPtr nativeReference) : base(nativeReference) { }

            public Group(string name="") : base(Group_create(name)) { }

            public void AddNode(Node node,Int32 index=-1)
            {
                Group_addNode(GetNativeReference(), node.GetNativeReference(),index);
            }

            public IEnumerator<Node> GetEnumerator()
            {
                return new NodeIterator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }


            public new static void InitializeFactory()
            {
                AddFactory(new Group());
            }

            public new static void UninitializeFactory()
            {
                RemoveFactory("gzGroup");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Group(nativeReference) as Reference;
            }


            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Group_addNode(IntPtr groupref,IntPtr noderef,Int32 index);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Group_create(string name);

            #endregion
        }
    }
}
