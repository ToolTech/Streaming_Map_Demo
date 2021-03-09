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
// File			: NativeString.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzString class
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
// AMO	201006	Created file                         (2.10.6)	
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;

namespace GizmoSDK
{
    namespace GizmoBase
    {
              
        public class NativeString : Reference , IEquatable<NativeString>
        {

            #region ---------------------- implicits --------------------

            public static implicit operator NativeString(string value)
            {
                return new NativeString(value);
            }

            public static implicit operator string(NativeString item)
            {
                return item.ToString();
            }

            #endregion

            public NativeString(string str,bool checkUnique=false,ushort uniqueID=0) : base(NativeString_create(str)) 
            {
                if (checkUnique)
                    NativeString_checkUnique(GetNativeReference());
                else if (uniqueID != 0)
                    NativeString_makeUnique(GetNativeReference(), uniqueID);
            }
            public NativeString(IntPtr nativeReference) : base(nativeReference) { }

            public static bool operator == (NativeString left, NativeString right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(NativeString left, NativeString right)
            {
                return !left.Equals(right);
            }

            public override bool Equals(System.Object obj)
            {
                if (obj is String)
                    return ToString().Equals(obj as String);

                return this.Equals(obj as NativeString);
            }

            public bool Equals(NativeString right)
            {
                // If parameter is null, return false.
                if (System.Object.ReferenceEquals(right, null))
                {
                    return false;
                }

                // If parameter is null, return false.
                if (!IsValid() || !right.IsValid())
                {
                    return false;
                }

                // Optimization for a common success case.
                if (System.Object.ReferenceEquals(this, right))
                {
                    return true;
                }

                // Optimization for a common success case.
                if (GetNativeReference() == right.GetNativeReference())
                {
                    return true;
                }

                return NativeString_equals(GetNativeReference(), right.GetNativeReference());
            }

            public override int GetHashCode()
            {
                return NativeString_hash(GetNativeReference());
            }

            public bool IsUnique()
            {
                return NativeString_isUnique(GetNativeReference());
            }
            
            public bool CheckUnique()
            {
                return NativeString_checkUnique(GetNativeReference());
            }

            public bool MakeUnique(UInt16 uniqueID)
            {
                return NativeString_makeUnique(GetNativeReference(),uniqueID);
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(NativeString_getString(GetNativeReference()));
            }

            #region // --------------------- Native calls -----------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr NativeString_create(string str);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr NativeString_getString(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool NativeString_equals(IntPtr reference1,IntPtr reference2);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern Int32 NativeString_hash(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool NativeString_isUnique(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool NativeString_makeUnique(IntPtr reference,UInt16 uniqueID);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool NativeString_checkUnique(IntPtr reference);

            #endregion
        }

    }
}

