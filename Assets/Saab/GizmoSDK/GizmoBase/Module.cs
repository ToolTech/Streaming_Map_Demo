//******************************************************************************
// File			: Module.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzModule class
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
        public class Module : Reference , DynamicEventInterface
        {
            public Module(IntPtr nativeReference) : base(nativeReference)
            {
                
            }

            public Module() : base(Module_create())
            {

            }

            static public Module GetModule(string moduleName)
            {
               return Reference.CreateObject(Module_getModule(moduleName)) as Module;
            }

            public static void InitializeFactory()
            {
                AddFactory(new Module());
            }

            public static void UninitializeFactory()
            {
                RemoveFactory("gzModule");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Module(nativeReference) as Reference;
            }

            public DynamicType InvokeMethod(string IIDS_method, DynamicType a0 = null, DynamicType a1 = null, DynamicType a2 = null, DynamicType a3 = null, DynamicType a4 = null, DynamicType a5 = null, DynamicType a6 = null, DynamicType a7 = null, DynamicType a8 = null, DynamicType a9 = null)
            {
                return new DynamicType( Module_invokeMethod(GetNativeReference(), IIDS_method, a0?.GetNativeReference() ?? IntPtr.Zero, a1?.GetNativeReference() ?? IntPtr.Zero, a2?.GetNativeReference() ?? IntPtr.Zero, a3?.GetNativeReference() ?? IntPtr.Zero, a4?.GetNativeReference() ?? IntPtr.Zero, a5?.GetNativeReference() ?? IntPtr.Zero, a6?.GetNativeReference() ?? IntPtr.Zero, a7?.GetNativeReference() ?? IntPtr.Zero, a8?.GetNativeReference() ?? IntPtr.Zero, a9?.GetNativeReference() ?? IntPtr.Zero));
            }

                       
            public static DynamicType InvokeModuleMethod(string moduleName,string method)
            {
                return new DynamicType(Module_invokeModuleMethod(moduleName,method));
            }

            public void SendEvent(ulong event_id, DynamicType a0 = null, DynamicType a1 = null, DynamicType a2 = null, DynamicType a3 = null,DynamicType a4 = null, DynamicType a5 = null, DynamicType a6 = null, DynamicType a7 = null, DynamicType a8 = null, DynamicType a9 = null)
            {
                Module_sendEvent(GetNativeReference(), event_id, a0?.GetNativeReference() ?? IntPtr.Zero, a1?.GetNativeReference() ?? IntPtr.Zero, a2?.GetNativeReference() ?? IntPtr.Zero, a3?.GetNativeReference() ?? IntPtr.Zero, a4?.GetNativeReference() ?? IntPtr.Zero, a5?.GetNativeReference() ?? IntPtr.Zero, a6?.GetNativeReference() ?? IntPtr.Zero, a7?.GetNativeReference() ?? IntPtr.Zero, a8?.GetNativeReference() ?? IntPtr.Zero, a9?.GetNativeReference() ?? IntPtr.Zero);
            }

            public void AddSubscriber(DynamicEventNotifyInterface subscriber)
            {
                Module_addSubscriber(GetNativeReference(), subscriber.GetNativeInterface());
            }

            public void RemoveSubscriber(DynamicEventNotifyInterface subscriber)
            {
                Module_removeSubscriber(GetNativeReference(), subscriber.GetNativeInterface());
            }

            #region // --------------------- Native calls -----------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode,CallingConvention =CallingConvention.Cdecl)]
            private static extern IntPtr Module_getModule(string moduleName);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Module_create();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Module_invokeModuleMethod(string moduleName,string method);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Module_sendEvent(IntPtr module_reference,ulong event_id,IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Module_addSubscriber(IntPtr module_reference,IntPtr iface);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Module_removeSubscriber(IntPtr module_reference, IntPtr iface);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Module_invokeMethod(IntPtr module_reference, string IIDS_method, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9);

            #endregion
        }






    }
}

