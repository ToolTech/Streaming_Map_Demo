//******************************************************************************
// File			: DynamicTypeInt64.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to DynamicTypeInt64 class
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.4
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
       
        public class DynamicTypeInt64 : Reference 
        {
            public static implicit operator DynamicType(DynamicTypeInt64 i64)
            {
                if (i64 == null)
                    return null;

                return new DynamicType(i64.GetNativeReference());
            }

            public static implicit operator DynamicTypeInt64(DynamicType data)
            {
                if (data == null)
                    return null;

                return new DynamicTypeInt64(data);
            }

            public static implicit operator DynamicTypeInt64(Int64 data)
            {
                return new DynamicTypeInt64(data);
            }

            public static implicit operator DynamicTypeInt64(UInt64 data)
            {
                return new DynamicTypeInt64(data);
            }

            public static implicit operator Int64(DynamicTypeInt64 i64)
            {
                if(i64==null)
                    throw (new Exception("DynamicTypeInt64 is null"));

                return i64.GetInt64();
            }

            public static implicit operator UInt64(DynamicTypeInt64 i64)
            {
                if (i64 == null)
                    throw (new Exception("DynamicTypeInt64 is null"));

                return i64.GetUInt64();
            }



            public DynamicTypeInt64(Int64 value) : base(DynamicTypeInt64_create(value)) { }

            public DynamicTypeInt64(UInt64 value) : base(DynamicTypeInt64_create((Int64)value)) { }

            public DynamicTypeInt64(DynamicType data) : base(data?.GetNativeReference() ?? IntPtr.Zero)
            {
                if (data == null)
                    throw (new Exception("DynamicType is null"));

                if (!data.Is(DynamicType.Type.INT64))
                    throw (new Exception("DynamicType is not a INT64"));
            }

            public Int64 GetInt64()
            {
                return DynamicTypeInt64_getInt64(GetNativeReference());
            }

            public UInt64 GetUInt64()
            {
                return (UInt64)DynamicTypeInt64_getInt64(GetNativeReference());
            }

            public override string ToString()
            {
                return ((DynamicType)(this)).AsString();
            }


            #region ---------------------- private -------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeInt64_create(Int64 value);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern Int64 DynamicTypeInt64_getInt64(IntPtr dynamic_reference);


            #endregion


        }
    }
}

