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
// File			: DistNotificationSet.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;



namespace GizmoSDK
{
    namespace GizmoDistribution
    {
        public class DistNotificationSetIterator : Reference, IEnumerator<DistAttribute>
        {
            public DistNotificationSetIterator(DistNotificationSet e) : base(DistNotificationSetIterator_create(e.GetNativeReference())) { }

            public DistAttribute Current => m_current;

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (DistNotificationSetIterator_iterate(GetNativeReference()))
                {
                    m_current = new DistAttribute(DistNotificationSetIterator_current(GetNativeReference()));

                    return true;
                }

                return false;
            }

            public void Reset()
            {
                DistNotificationSetIterator_reset(GetNativeReference());
            }

            #region --------------------------- private ----------------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistNotificationSetIterator_create(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistNotificationSetIterator_iterate(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistNotificationSetIterator_current(IntPtr reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistNotificationSetIterator_reset(IntPtr reference);

            private DistAttribute m_current;

            #endregion
        }

        public class DistNotificationSet : Reference , IEnumerable<DistAttribute>
        {

            public DistNotificationSet(IntPtr nativeReference) : base(nativeReference) { }

            public DynamicType GetAttributeValue(NativeString name)
            {
                var res = DistNotificationSet_getAttributeValue(GetNativeReference(), name.GetNativeReference());
                return res == IntPtr.Zero ? null : new DynamicType(res);
            }

            public bool HasAttribute(NativeString name)
            {
                return DistNotificationSet_hasAttribute(GetNativeReference(), name.GetNativeReference());
            }

            #region --------------------------- private ----------------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistNotificationSet_getAttributeValue(IntPtr event_reference, IntPtr name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistNotificationSet_hasAttribute(IntPtr event_reference, IntPtr name);

            public IEnumerator<DistAttribute> GetEnumerator()
            {
                return new DistNotificationSetIterator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

        }
    }
}
