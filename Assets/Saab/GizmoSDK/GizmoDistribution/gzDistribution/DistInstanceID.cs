//******************************************************************************
// File			: DistInstanceID.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistInstanceID class
// Author		: Anders Modén		
// Product		: GizmoDistribution 2.10.4
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
// AMO	181023	Created file 	
//
//******************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;


namespace GizmoSDK
{
    namespace GizmoDistribution
    {

        public class DistInstanceID : Reference
        {
            public DistInstanceID(IntPtr nativeReference) : base(nativeReference) { }


            public override string ToString()
            {
                return Marshal.PtrToStringUni(DistInstanceID_asString(GetNativeReference()));
            }

            public static bool operator ==(DistInstanceID obj1, DistInstanceID obj2)
            {
                if (ReferenceEquals(obj1, obj2))
                {
                    return true;
                }

                if (ReferenceEquals(obj1, null))
                {
                    return false;
                }
                if (ReferenceEquals(obj2, null))
                {
                    return false;
                }

                return DistInstanceID_equal(obj1.GetNativeReference(), obj2.GetNativeReference());
            }

            public static bool operator !=(DistInstanceID obj1, DistInstanceID obj2)
            {
                return !(obj1 == obj2);
            }

            public bool Equals(DistInstanceID other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }
                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return DistInstanceID_equal(GetNativeReference(), other.GetNativeReference());
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == GetType() && Equals((DistInstanceID)obj);
            }

            public override int GetHashCode()
            {
                return (int)DistInstanceID_hashCode(GetNativeReference());
            }


            #region --------------------------- private ----------------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistInstanceID_asString(IntPtr id_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistInstanceID_equal(IntPtr id_reference1, IntPtr id_reference2);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt64 DistInstanceID_hashCode(IntPtr id_reference);


            #endregion
        }
    }
}
