//******************************************************************************
// File			: DistClientID.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistClientID class
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
                            
        public class DistClientID : Reference
        {
            public DistClientID(IntPtr nativeReference) : base(nativeReference){}

            public DistInstanceID InstanceID
            {
                get { return new DistInstanceID(DistClientID_instanceID(GetNativeReference()));  }
            }
                        
            public override string ToString()
            {
                return Marshal.PtrToStringUni(DistClientID_asString(GetNativeReference()));
            }

            public static bool operator ==(DistClientID obj1, DistClientID obj2)
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

                return DistClientID_equal(obj1.GetNativeReference(), obj2.GetNativeReference());
            }

            public static bool operator !=(DistClientID obj1, DistClientID obj2)
            {
                return !(obj1 == obj2);
            }

            public bool Equals(DistClientID other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }
                if (ReferenceEquals(this, other))
                {
                    return true;
                }
                
                return DistClientID_equal(GetNativeReference(), other.GetNativeReference());
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

                return obj.GetType() == GetType() && Equals((DistClientID)obj);
            }

            public override int GetHashCode()
            {
                return (int)DistClientID_hashCode(GetNativeReference());
            }

            #region --------------------------- private ----------------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistClientID_asString(IntPtr id_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistClientID_instanceID(IntPtr id_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClientID_equal(IntPtr id_reference1, IntPtr id_reference2);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 DistClientID_hashCode(IntPtr id_reference);



            #endregion
        }
    }
}
