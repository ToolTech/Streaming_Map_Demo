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
// File			: DynamicEvent.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzDynamicEvent classes
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
// AMO	180927	Created file 	
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        public interface DynamicEventInterface
        {
            void SendEvent(UInt64 event_id, DynamicType a0 = default(DynamicType),
                                            DynamicType a1 = default(DynamicType),
                                            DynamicType a2 = default(DynamicType),
                                            DynamicType a3 = default(DynamicType),
                                            DynamicType a4 = default(DynamicType),
                                            DynamicType a5 = default(DynamicType),
                                            DynamicType a6 = default(DynamicType),
                                            DynamicType a7 = default(DynamicType),
                                            DynamicType a8 = default(DynamicType),
                                            DynamicType a9 = default(DynamicType));

            void AddSubscriber(DynamicEventNotifyInterface subscriber);
            void RemoveSubscriber(DynamicEventNotifyInterface subscriber);
        }

        public interface DynamicEventNotifyInterface
        {
            IntPtr GetNativeInterface();
        }

        public class DynamicEventReceiver : Reference , DynamicEventNotifyInterface
        {
            public delegate void DynamicEventReceiver_OnEvent(DynamicEventReceiver receiver,DynamicEventInterface sender,UInt64 event_id, DynamicType a0, DynamicType a1 , DynamicType a2 , DynamicType a3,DynamicType a4 , DynamicType a5, DynamicType a6 , DynamicType a7 , DynamicType a8 , DynamicType a9);
            public event DynamicEventReceiver_OnEvent OnEvent;

            
            public DynamicEventReceiver() : base(DynamicEventReceiver_create())
            {
                ReferenceDictionary<DynamicEventReceiver>.AddObject(this);

            }

            override public void Release()
            {
                ReferenceDictionary<DynamicEventReceiver>.RemoveObject(this);
                base.Release();
            }

            public IntPtr GetNativeInterface()
            {
                return DynamicEventReceiver_getNativeInterface(GetNativeReference());
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
                    if (s_dispatcher_OnEvent == null)
                    {
                        s_dispatcher_OnEvent = new DynamicEventReceiver_OnEvent_Callback(OnEvent_callback);
                        DynamicEventReceiver_SetCallback_OnEvent(s_dispatcher_OnEvent);
                    }
                }

                ~Initializer()
                {
                    if (s_dispatcher_OnEvent != null)
                    {
                        DynamicEventReceiver_SetCallback_OnEvent(null);
                        s_dispatcher_OnEvent = null;
                    }
                }
            }

            static private Initializer s_class_init = new Initializer();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            private delegate void DynamicEventReceiver_OnEvent_Callback(IntPtr instance,IntPtr sender, ulong event_id, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9);

            static private DynamicEventReceiver_OnEvent_Callback s_dispatcher_OnEvent;

            [MonoPInvokeCallback(typeof(DynamicEventReceiver_OnEvent_Callback))]
            static private void OnEvent_callback(IntPtr instance, IntPtr sender, UInt64 event_id, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9)
            {
                DynamicEventReceiver recv = ReferenceDictionary<DynamicEventReceiver>.GetObject(instance);

                if (recv != null)
                    recv.OnEvent?.Invoke(recv, Reference.CreateObject(sender) as DynamicEventInterface, event_id, new DynamicType(a0), new DynamicType(a1), new DynamicType(a2), new DynamicType(a3), new DynamicType(a4), new DynamicType(a5), new DynamicType(a6), new DynamicType(a7), new DynamicType(a8), new DynamicType(a9));
            }

            #endregion

            #region // --------------------- Native calls -----------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicEventReceiver_SetCallback_OnEvent(DynamicEventReceiver_OnEvent_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicEventReceiver_create();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicEventReceiver_getNativeInterface(IntPtr native_reference);


            #endregion
        }






    }
}

