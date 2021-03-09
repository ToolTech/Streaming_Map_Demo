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
// File			: DynamicTypeError.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to DynamicTypeError class
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
       
        public class DynamicTypeError : Reference 
        {
            public static implicit operator DynamicType(DynamicTypeError error)
            {
                if (error == null)
                    return null;

                return new DynamicType(error.GetNativeReference());
            }

            public static implicit operator DynamicTypeError(DynamicType data)
            {
                if (data == null)
                    return null;

                return new DynamicTypeError(data);
            }

            public static implicit operator DynamicTypeError(string error)
            {
                return new DynamicTypeError(error);
            }

            
            public static implicit operator string(DynamicTypeError error)
            {
                if(error == null)
                    throw (new Exception("DynamicTypeError is null"));

                return error.GetError();
            }
                      


            public DynamicTypeError(string error) : base(DynamicTypeError_create(error)) { }

            public DynamicTypeError(DynamicType data) : base(data?.GetNativeReference() ?? IntPtr.Zero)
            {
                if (data == null)
                    throw (new Exception("DynamicType is null"));

                if (!data.Is(DynamicType.Type.ERROR))
                    throw (new Exception("DynamicType is not an DynamicTypeError"));
            }

            public string GetError()
            {
                return Marshal.PtrToStringUni(DynamicTypeError_getError(GetNativeReference()));
            }

            public bool IsError()
            {
                return DynamicTypeError_isError(GetNativeReference());
            }

            public override string ToString()
            {
                return ((DynamicType)(this)).AsString();
            }


            #region ---------------------- private -------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeError_create(string error);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeError_getError(IntPtr dynamic_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DynamicTypeError_isError(IntPtr dynamic_reference);



            #endregion


        }
    }
}

