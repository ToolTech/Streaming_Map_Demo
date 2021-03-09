//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s)
// herein belong to SAAB AB. The program(s) may be used and/or
// copied only with the written permission of Saab AB, or in
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
// File			: DistTransaction.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistTransaction class
// Author		: Anders Modén		
// Product		: GizmoDistribution 2.10.7
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

using System;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;

 

namespace GizmoSDK
{
    namespace GizmoDistribution
    {
        public class DistTransaction : Reference
        {

            public DistTransaction(IntPtr nativeReference) : base(nativeReference) { }

            public DistTransaction() : base(DistTransaction_createDefaultTransaction()) { }

            public bool SetAttributeValue(NativeString name, DynamicType value)
            {
                return DistTransaction_setAttributeValue(GetNativeReference(), name.GetNativeReference(), value.GetNativeReference());
            }

            public DynamicType GetAttributeValue(NativeString name)
            {
                return new DynamicType(DistTransaction_getAttributeValue(GetNativeReference(), name.GetNativeReference()));
            }

            public bool HasAttribute(NativeString name)
            {
                return DistTransaction_hasAttribute(GetNativeReference(), name.GetNativeReference());
            }

            public void NewTransaction()
            {
                Reset(DistTransaction_createDefaultTransaction());
            }

            public bool IsEditable()
            {
                return DistTransaction_isEditable(GetNativeReference());
            }

            #region --------------------------- private ----------------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistTransaction_createDefaultTransaction();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistTransaction_setAttributeValue(IntPtr event_reference, IntPtr name, IntPtr dynamic_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistTransaction_getAttributeValue(IntPtr event_reference, IntPtr name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistTransaction_hasAttribute(IntPtr event_reference, IntPtr name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistTransaction_isEditable(IntPtr event_reference);

            #endregion

        }
    }
}
