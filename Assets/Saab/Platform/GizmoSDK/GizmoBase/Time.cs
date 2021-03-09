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
// File			: Time.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzTime class
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

            public double Seconds
            {
                get { return Time_seconds(GetNativeReference()); }
            }

            public double DaySeconds
            {
                get { return Time_daySeconds(GetNativeReference()); }
            }

            public UInt16 Minute
            {
                get { return Time_minute(GetNativeReference()); }
            }

            public double MinuteSeconds
            {
                get { return Time_minuteSeconds(GetNativeReference()); }
            }


            public UInt16 Hour
            {
                get { return Time_hour(GetNativeReference()); }
            }

            public UInt16 Day
            {
                get { return Time_day(GetNativeReference()); }
            }

            public UInt16 Month
            {
                get { return Time_month(GetNativeReference()); }
            }

            public UInt32 Year
            {
                get { return Time_year(GetNativeReference()); }
            }

            public static bool IsSystemSynchronized
            {
                get { return Time_isSystemSynchronized(); }
            }

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
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double Time_seconds(IntPtr time_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double Time_daySeconds(IntPtr time_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double Time_minuteSeconds(IntPtr time_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt16 Time_minute(IntPtr time_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt16 Time_hour(IntPtr time_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt16 Time_day(IntPtr time_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt16 Time_month(IntPtr time_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 Time_year(IntPtr time_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Time_isSystemSynchronized();

            #endregion
        }
    }
}

