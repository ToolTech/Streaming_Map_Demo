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

            public void AddNode(Node node)
            {
                Group_addNode(GetNativeReference(), node.GetNativeReference());
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

            public override Reference Create(IntPtr nativeReference)
            {
                return new Group(nativeReference) as Reference;
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Group_addNode(IntPtr groupref,IntPtr noderef);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Group_create(string name);
           
            #endregion
        }
    }
}
