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
// File			: DynamicTypeTimeTagged.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzDynamicTypeTimeTagged class
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
       
        public class DynamicTypeTimeTagged : Reference 
        {
            public static implicit operator DynamicType(DynamicTypeTimeTagged ttag)
            {
                if (ttag == null)
                    return null;

                return new DynamicType(ttag.GetNativeReference());
            }

            public static implicit operator DynamicTypeTimeTagged(DynamicType data)
            {
                if (data == null)
                    return null;

                return new DynamicTypeTimeTagged(data);
            }

            public DynamicTypeTimeTagged(double time,DynamicType data) : base(DynamicTypeTimeTagged_create_timetag(time,data?.GetNativeReference() ?? IntPtr.Zero )) { }

            public DynamicTypeTimeTagged(DynamicType data) : base(data?.GetNativeReference() ?? IntPtr.Zero)
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data), "DynamicType is null");

                if (!data.Is(DynamicType.Type.TIME_TAGGED))
                    throw new ArgumentException("DynamicType is not a TIME_TAGGED", nameof(data));
            }

            public double GetTimeTag()
            {
                return DynamicTypeTimeTagged_getTimeTag(GetNativeReference());
            }

            public DynamicType GetData()
            {
                return new DynamicType(DynamicTypeTimeTagged_getData(GetNativeReference()));
            }

            public override string ToString()
            {
                return ((DynamicType)(this)).AsString(false, true, "TT");
            }

            #region ---------------------- private -------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeTimeTagged_create_timetag(double time,IntPtr dynamic_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double DynamicTypeTimeTagged_getTimeTag(IntPtr dynamic_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicTypeTimeTagged_getData(IntPtr dynamic_reference);


            #endregion


        }
    }
}

