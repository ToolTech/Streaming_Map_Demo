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
        public static class Time
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

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CallingConvention = CallingConvention.Cdecl)]
            private static extern double Time_systemSeconds();

            [DllImport(Platform.BRIDGE, CallingConvention = CallingConvention.Cdecl)]
            private static extern double Time_now();

            [DllImport(Platform.BRIDGE, CallingConvention = CallingConvention.Cdecl)]
            private static extern double Time_getSystemTimeOffset();

            [DllImport(Platform.BRIDGE, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Time_setSystemTimeOffset(double offset);

            #endregion
        }
    }
}

