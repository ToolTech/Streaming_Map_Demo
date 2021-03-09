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
// File			: DynamicTypeUByteArray.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to DynamicTypeUByteArray class
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.7
//		
//
//			
// NOTE:	GizmoBase is a platform abstraction utility layer for C++. It contains 
//			design patterns and C++ solutions for the advanced programmer.
//
//
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	200827	Created file 	(2.10.6)
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
       
        public class DynamicTypeUByteArray : Reference 
        {
            public static implicit operator DynamicType(DynamicTypeUByteArray array)
            {
                if (array == null)
                    return null;

                return new DynamicType(array.GetNativeReference());
            }

            public static implicit operator DynamicTypeUByteArray(DynamicType data)
            {
                if (data == null)
                    return null;

                return new DynamicTypeUByteArray(data);
            }

            public static implicit operator DynamicTypeUByteArray(byte [] data)
            {
                return new DynamicTypeUByteArray(data);
            }

            public static implicit operator byte [] (DynamicTypeUByteArray array)
            {
                if(array==null)
                    throw (new Exception("DynamicTypeUByteArray is null"));

                byte[] result=null;

                _ = array.GetArray(ref result, out uint size);

                return result;
            }


            public DynamicTypeUByteArray(byte[] data=null) : base(DynamicTypeUByteArray_create()) 
            {
                if(data != null && data.Length>0)
                    SetArray(data);
            }

            public DynamicTypeUByteArray(DynamicType data) : base(data?.GetNativeReference() ?? IntPtr.Zero)
            {
                if (data == null)
                    throw (new Exception("DynamicType is null"));

                if (!data.Is(DynamicType.Type.UBYTE_ARRAY))
                    throw (new Exception("DynamicType is not an UByte Array"));
            }

            public void SetArray(byte[] data)
            {
                IntPtr native_data = Marshal.AllocHGlobal(data.Length);

                Marshal.Copy(data, 0, native_data, data.Length); // Transfer to unmanaged memory

                DynamicTypeUByteArray_setArray(GetNativeReference(), native_data, (UInt32)data.Length);
    
                Marshal.FreeHGlobal(native_data);
            }

            public bool GetArray(ref byte[] data, out UInt32 size)
            {
                size = 0;

                IntPtr native_data = IntPtr.Zero;

                if (DynamicTypeUByteArray_getArray(GetNativeReference(), ref native_data, ref size))
                {
                    if (data == null || data.Length < size)
                        data = new byte[size];

                    Marshal.Copy(native_data, data, (int)0, (int)size);

                    return true;
                }

                return false;
            }

            public override string ToString()
            {
                return ((DynamicType)(this)).AsString();
            }


            #region ---------------------- private -------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeUByteArray_create();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DynamicTypeUByteArray_getArray(IntPtr dynamic_reference,ref IntPtr native_data,ref UInt32 size);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicTypeUByteArray_setArray(IntPtr dynamic_reference, IntPtr native_data, UInt32 size);


            #endregion


        }
    }
}

