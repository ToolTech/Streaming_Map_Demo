//******************************************************************************
// File			: Performance.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to performance utilities
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.4
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
             
        public class Monitor
        {
            static public void Enter(string monitor)
            {
                Monitor_enter(monitor);
            }

            static public void Leave(string monitor)
            {
                Monitor_leave(monitor);
            }

            static public void InstallMonitor(string url="udp::45454?nic=127.0.0.1")
            {
                Monitor_install(url);
            }

            static public void AddValue(string monitor,DynamicType value,double time=-1,UInt32 instanceID=0)
            {
                Monitor_addValue(monitor,value.GetNativeReference(),time,instanceID);
            }

            static public void AddValueOpt(bool addValue,string monitor,DynamicType value, double time = -1, UInt32 instanceID = 0)
            {
                if (addValue)
                    AddValue(monitor, value, time, instanceID);
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Monitor_enter(string monitor);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Monitor_leave(string monitor);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Monitor_install(string url);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Monitor_addValue(string monitor,IntPtr dynamic_reference,double time,UInt32 instanceID);

            #endregion
        }


    }
}

