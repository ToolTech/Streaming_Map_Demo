//******************************************************************************
// File			: DynamicTypeContainer.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzDynamicType class
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.1
//		
// Copyright © 2003- Saab Training Systems AB, Sweden
//			
// NOTE:	GizmoBase is a platform abstraction utility layer for C++. It contains 
//			design patterns and C++ solutions for the advanced programmer.
//
//
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	180301	Created file 	
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Collections;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        public class DynamicTypeContainerIterator : Reference, IEnumerator<KeyValuePair<string,DynamicType>>
        {
            public DynamicTypeContainerIterator(DynamicTypeContainer cont) : base(DynamicTypeContainerIterator_create(cont.GetNativeReference())) { }

            public KeyValuePair<string, DynamicType> Current => m_current;

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (DynamicTypeContainerIterator_iterate(GetNativeReference()))
                {
                    m_current = new KeyValuePair<string, DynamicType>(Marshal.PtrToStringUni(DynamicTypeContainerIterator_current_name(GetNativeReference())), new DynamicType(DynamicTypeContainerIterator_current(GetNativeReference())));

 
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                DynamicTypeContainerIterator_reset(GetNativeReference());
            }

            KeyValuePair<string, DynamicType> m_current;

 
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeContainerIterator_create(IntPtr container_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DynamicTypeContainerIterator_iterate(IntPtr iterator_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeContainerIterator_current(IntPtr iterator_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeContainerIterator_current_name(IntPtr iterator_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicTypeContainerIterator_reset(IntPtr iterator_ref);
        }

        public class DynamicTypeContainer : Reference, IEnumerable<KeyValuePair<string, DynamicType>>
        {
            public static implicit operator DynamicType(DynamicTypeContainer cont)
            {
                if (cont == null)
                    return null;

                return new DynamicType(DynamicTypeContainer_pack_cont(cont.GetNativeReference()));
            }

            public static implicit operator DynamicTypeContainer(DynamicType data)
            {
                if (data == null)
                    return null;

                return new DynamicTypeContainer(data);
            }

            public DynamicTypeContainer() : base(DynamicTypeContainer_create_cont()) { }
            public  DynamicTypeContainer(DynamicType data) : base(DynamicTypeContainer_unpack_cont(data?.GetNativeReference() ?? IntPtr.Zero))
            {
                if (data == null)
                    throw (new Exception("DynamicType is null"));

                if (GetNativeReference()==IntPtr.Zero)
                    throw (new Exception("DynamicType is not a CONTAINER"));
            }

            public void SetAttribute(string name, DynamicType value)
            {
                if (name == null)
                    throw (new Exception("SetAttribute name is null"));

                if (value == null)
                    throw (new Exception("SetAttribute DynamicType is null"));

                DynamicTypeContainer_setAttribute(GetNativeReference(), name, value.GetNativeReference());
            }

            public DynamicType GetAttribute(string name)
            {
                if (name == null)
                    throw (new Exception("GetAttribute name is null"));

                return new DynamicType(DynamicTypeContainer_getAttribute(GetNativeReference(), name));
            }

            public bool HasAttribute(string name)
            {
                if (name == null)
                    throw (new Exception("HasAttribute name is null"));

                return DynamicTypeContainer_hasAttribute(GetNativeReference(), name);
            }

            public override string ToString()
            {
                return ((DynamicType)(this)).AsString();
            }

            #region ---------------------- private -------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeContainer_create_cont();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeContainer_unpack_cont(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeContainer_pack_cont(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicTypeContainer_setAttribute(IntPtr cont_reference, string name, IntPtr dynamic_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeContainer_getAttribute(IntPtr cont_reference, string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DynamicTypeContainer_hasAttribute(IntPtr cont_reference, string name);

            public IEnumerator<KeyValuePair<string, DynamicType>> GetEnumerator()
            {
                return new DynamicTypeContainerIterator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }
    }
}

