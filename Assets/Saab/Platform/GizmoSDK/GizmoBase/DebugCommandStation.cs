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
// File			: DebugCommandStation.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzDebugCommandStation class
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.5
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
        
        public class DebugCommandStation : Reference
        {
            public const string DEFAULT_URL = "udp::45456?blocking=no";

            public delegate bool DebugCommandStationEventHandler_OnExec(string exec_message);
            public event DebugCommandStationEventHandler_OnExec OnExec;

            public DebugCommandStation(string url= DEFAULT_URL, bool echo=false) : base(DebugCommandStation_create(url,echo))
            {
                ReferenceDictionary<DebugCommandStation>.AddObject(this);
            }

            override public void Release()
            {
                ReferenceDictionary<DebugCommandStation>.RemoveObject(this);
                base.Release();
            }

            public bool Exec()
            {
                return DebugCommandStation_exec(GetNativeReference());
            }

            static public void Initialize()
            {
                if (s_class_init == null)
                    s_class_init = new Initializer();
            }

            static public void Uninitialize()
            {
                if (s_class_init != null)
                    s_class_init = null;
            }

            #region ---------------- Private functions ------------------------


            private sealed class Initializer
            {
                public Initializer()
                {
                    if (s_dispatcher_OnExec == null)
                    {
                        s_dispatcher_OnExec = new DebugCommandStationEventHandler_OnExec_Callback(OnExec_callback);
                        DebugCommandStation_SetCallback(s_dispatcher_OnExec);
                    }
                }

                ~Initializer()
                {
                    if (s_dispatcher_OnExec != null)
                    {
                        DebugCommandStation_SetCallback(null);
                        s_dispatcher_OnExec = null;
                    }
                }
            }
            
            static private Initializer s_class_init = new Initializer();

            // ----------------------------- OnExec ------------------------------------------------

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate bool DebugCommandStationEventHandler_OnExec_Callback(IntPtr instance,IntPtr message);

            private static DebugCommandStationEventHandler_OnExec_Callback s_dispatcher_OnExec;

            [MonoPInvokeCallback(typeof(DebugCommandStationEventHandler_OnExec_Callback))]
            static private bool OnExec_callback(IntPtr instance,IntPtr message)
            {
                DebugCommandStation client = ReferenceDictionary<DebugCommandStation>.GetObject(instance);

                if(client!=null && client.OnExec!=null)
                    return client.OnExec.Invoke(Marshal.PtrToStringUni(message));
                
                return true;
            }


            #endregion

            #region // --------------------- Native calls -----------------------
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DebugCommandStation_create(string url,bool echo);
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode,CallingConvention =CallingConvention.Cdecl)]
            private static extern void DebugCommandStation_SetCallback(DebugCommandStationEventHandler_OnExec_Callback fn);
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DebugCommandStation_exec(IntPtr instance);
            #endregion
        }






    }
}

