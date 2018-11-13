//******************************************************************************
// File			: DynamicEvent.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzDynamicEvent classes
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
                m_dispatcher_OnEvent = new DynamicEventReceiver_OnEvent_Callback(OnEvent_callback);
                DynamicEventReceiver_SetCallback_OnEvent(GetNativeReference(), m_dispatcher_OnEvent);
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            private delegate void DynamicEventReceiver_OnEvent_Callback(IntPtr sender,ulong event_id, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9);

            private DynamicEventReceiver_OnEvent_Callback m_dispatcher_OnEvent;
            private void OnEvent_callback(IntPtr sender,UInt64 event_id, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4,IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9)
            {
                OnEvent?.Invoke(this, Reference.CreateObject(sender) as DynamicEventInterface,event_id,new DynamicType(a0), new DynamicType(a1), new DynamicType(a2), new DynamicType(a3), new DynamicType(a4), new DynamicType(a5), new DynamicType(a6), new DynamicType(a7), new DynamicType(a8), new DynamicType(a9));
            }

            public IntPtr GetNativeInterface()
            {
                return DynamicEventReceiver_getNativeInterface(GetNativeReference());
            }

            #region // --------------------- Native calls -----------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicEventReceiver_SetCallback_OnEvent(IntPtr client, DynamicEventReceiver_OnEvent_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicEventReceiver_create();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicEventReceiver_getNativeInterface(IntPtr native_reference);


            #endregion
        }






    }
}

