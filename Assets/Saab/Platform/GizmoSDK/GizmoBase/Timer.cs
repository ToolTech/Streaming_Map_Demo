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
// Product		: GizmoBase 2.10.4
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
        public class Timer : Reference
        {
            public Timer() : base(Timer_create()) { }

            public double GetTime()
            {
                return Timer_getTime(GetNativeReference());
            }

            public double GetFrequency(double samples=1.0)
            {
                double time = GetTime();

                if (time != 0.0)
                    return samples / time;

                return 0.0;
            }

            public override string ToString()
            {
                return $"Timer:{GetTime()} Frequency:{GetFrequency()}";
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Timer_create();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern double Timer_getTime(IntPtr timer_ref);
         


            #endregion
        }
    }
}

