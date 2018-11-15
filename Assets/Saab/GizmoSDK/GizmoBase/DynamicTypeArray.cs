//******************************************************************************
// File			: DynamicTypeArray.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzDynamicTypeArray class
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
        public class DynamicTypeArrayIterator : IEnumerator<DynamicType>
        {
            public DynamicTypeArrayIterator(DynamicTypeArray array)
            {
                m_index = -1;
                m_array = array;
            }

            DynamicTypeArray m_array;
            int m_index;

            public DynamicType Current => m_array[m_index];

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                m_array = null;
            }

            public bool MoveNext()
            {
                m_index++;

                if (m_index >= m_array.Count)
                {
                    m_index = -1;
                    return false;
                }

                return true;
            }

            public void Reset()
            {
                m_index = -1;
            }
        }
        public class DynamicTypeArray : Reference , IList<DynamicType>
        {
            public int Count => (int)DynamicTypeArray_getSize(GetNativeReference());

            public bool IsReadOnly => false;

            public DynamicType this[int index]
            {
                get { return new DynamicType(DynamicTypeArray_get(GetNativeReference(), (UInt32)index)); }
                set { DynamicTypeArray_set(GetNativeReference(), (UInt32)index, value.GetNativeReference()); }
            }

            public static implicit operator DynamicType(DynamicTypeArray cont)
            {
                if (cont == null)
                    return null;

                return new DynamicType(DynamicTypeArray_pack_array(cont.GetNativeReference()));
            }

            public static implicit operator DynamicTypeArray(DynamicType data)
            {
                if (data == null)
                    return null;

                return new DynamicTypeArray(data);
            }

            public DynamicTypeArray() : base(DynamicTypeArray_create_array()) { }

            public DynamicTypeArray(DynamicType data) : base(DynamicTypeArray_unpack_array(data?.GetNativeReference() ?? IntPtr.Zero))
            {
                if (data == null)
                    throw (new Exception("DynamicType is null"));

                if (GetNativeReference() == IntPtr.Zero)
                    throw (new Exception("DynamicType is not an ARRAY"));
            }

           
            public int IndexOf(DynamicType item)
            {
                return DynamicTypeArray_index_of(GetNativeReference(),item.GetNativeReference());
            }

            public void Insert(int index, DynamicType item)
            {
                if (item == null)
                    throw (new Exception("Insert DynamicType is null"));

                DynamicTypeArray_insert_at(GetNativeReference(),(UInt32)index,item.GetNativeReference());
            }

            public void RemoveAt(int index)
            {
                if(index>=0)
                    DynamicTypeArray_remove_at(GetNativeReference(), (UInt32)index);
            }

            public void Add(DynamicType item)
            {
                if (item == null)
                    throw (new Exception("Add DynamicType is null"));

                DynamicTypeArray_add(GetNativeReference(), item.GetNativeReference());
            }

            public void Clear()
            {
                DynamicTypeArray_clear(GetNativeReference());
            }

            public bool Contains(DynamicType item)
            {
                if (item == null)
                    return false;

                return DynamicTypeArray_index_of(GetNativeReference(), item.GetNativeReference())!=-1;
            }

            public void CopyTo(DynamicType[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(DynamicType item)
            {
                if (item == null)
                    throw (new Exception("Remove DynamicType is null"));

                int index = IndexOf(item);

                if (index >= 0)
                {
                    RemoveAt(index);
                    return true;
                }

                return false;
            }

            public IEnumerator<DynamicType> GetEnumerator()
            {
                return new DynamicTypeArrayIterator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public override string ToString()
            {
                return ((DynamicType)(this)).AsString();
            }

            #region ---------------------- private -------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeArray_create_array();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeArray_unpack_array(IntPtr dyn_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeArray_pack_array(IntPtr array_ref);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 DynamicTypeArray_getSize(IntPtr array_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicTypeArray_add(IntPtr array_ref, IntPtr dyn_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicTypeArray_set(IntPtr array_ref, UInt32 index, IntPtr dyn_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeArray_get(IntPtr array_ref, UInt32 index);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicTypeArray_clear(IntPtr array_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern Int32 DynamicTypeArray_index_of(IntPtr array_ref, IntPtr dyn_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicTypeArray_insert_at(IntPtr array_ref, UInt32 index, IntPtr dyn_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicTypeArray_remove_at(IntPtr array_ref, UInt32 index);

            #endregion


        }
    }
}

