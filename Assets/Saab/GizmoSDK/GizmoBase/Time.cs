//******************************************************************************
// File			: Time.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzTime class
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

namespace GizmoSDK
{
    namespace GizmoBase
    {
        public class Time : Reference
        {
            public static double SystemSeconds
            {
                get { return Time_systemSeconds(); }
            }

            public static double SystemSecondsOffset
            {
                get { return Time_getSystemTimeOffset(); }
                set { Time_setSystemTimeOffset(value); }
            }

            public static double Now
            {
                get { return Time_now(); }
            }

            public Time() : base(Time_create()) { }
            public Time(double seconds) : base(Time_create_seconds(seconds)) { }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double Time_systemSeconds();

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double Time_now();

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double Time_getSystemTimeOffset();

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Time_setSystemTimeOffset(double offset);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Time_create();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Time_create_seconds(double seconds);

            #endregion
        }
    }
}

