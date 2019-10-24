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
// File			: gzDistManager.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistManager class
// Author		: Anders Modén		
// Product		: GizmoDistribution 2.10.4
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;




namespace GizmoSDK
{
    namespace GizmoDistribution
    {
        public enum ServerPriority
        {
            PRIO_NEVER = 0, //!< Will never be activated
            PRIO_VERY_LOW,  //!< Very low priority
            PRIO_LOW,       //!< Low priority
            PRIO_NORMAL,        //!< The default priority
            PRIO_HIGH,      //!< High priority
            PRIO_VERY_HIGH, //!< Very high priority
            PRIO_MAX         //!< The maximum priority
        }

        public class DistManager : Reference
        {
            public const string DEFAULT_MANAGER = "Def";

            private readonly HashSet<Type> _typeRegistry = new HashSet<Type>();

            public event EventHandler OnPostProcess;

            public DistManager(IntPtr nativeReference) : base(nativeReference)
            {
            }

            static public DistManager GetManager(bool create, string name)
            {
                IntPtr nativeReference = DistManager_getManager(create, name);

                return new DistManager(nativeReference);
            }

            static public DistManager GetManager(bool create)
            {
                IntPtr nativeReference = DistManager_getManager(create, DEFAULT_MANAGER);

                if (nativeReference == IntPtr.Zero)
                    return null;

                return new DistManager(nativeReference);
            }

            public T GetObject<T>(string objectName) where T : DistObject
            {
                return GetObject(objectName, typeof(T).Name) as T;
            }

            public void RegisterObject<T>() where T : DistObject
            {
                RegisterObjectHierarchy(typeof(T));
            }

            public void RegisterObject(Type type)
            {
                RegisterObjectHierarchy(type);
            }

            private void RegisterObjectHierarchy(Type objectType)
            {
                // stop recurse if parent is already registered
                if (_typeRegistry.Contains(objectType))
                {
                    return;
                }

                _typeRegistry.Add(objectType);

                var baseType = objectType.BaseType;
                if (baseType == typeof(DistObject))
                {
                    RegisterObjectPrototype(objectType, "gzDistObject");
                    return;
                }

                // recurse down, gizmo requires that we register in a bottom up order, otherwise type relations will not work
                RegisterObjectHierarchy(baseType);

                RegisterObjectPrototype(objectType, baseType.Name);
            }

            private void RegisterObjectPrototype(Type objectType, string nativeBaseTypename)
            {
                const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance |
                                                                System.Reflection.BindingFlags.NonPublic |
                                                                System.Reflection.BindingFlags.CreateInstance |
                                                                System.Reflection.BindingFlags.OptionalParamBinding;


                // manually construct native dist object to use as factory object
                var factoryObj = new DistObject($"{objectType.Name}_factory");
                var nativeRef = factoryObj.GetNativeReference();

                // create managed implementation, supplying the dist object we just created
                var prototype = (Reference)Activator.CreateInstance(objectType, flags, null, new object[] { nativeRef }, null);

                // register a factory
                RegisterObjecttHierarchy(objectType.Name, nativeBaseTypename, factoryObj);

                // use the overloaded AddFactory to register with correct typename
                AddFactory(prototype, objectType.Name);
            }

            public T GetEvent<T>() where T : DistEvent
            {
                return GetEvent(typeof(T).Name) as T;
            }

            public void RegisterEvent<T>() where T : DistEvent
            {
                RegisterEventHierarchy(typeof(T));
            }

            public void RegisterEvent(Type type)
            {
                RegisterEventHierarchy(type);
            }

            private void RegisterEventHierarchy(Type eventType)
            {
                // stop recurse if parent is already registered
                if (_typeRegistry.Contains(eventType))
                {
                    return;
                }

                _typeRegistry.Add(eventType);

                var baseType = eventType.BaseType;
                if (baseType == typeof(DistEvent))
                {
                    RegisterEventPrototype(eventType, "gzDistEvent");
                    return;
                }

                // recurse down, gizmo requires that we register in a bottom up order, otherwise type relations will not work
                RegisterEventHierarchy(baseType);

                RegisterEventPrototype(eventType, baseType.Name);
            }

            private void RegisterEventPrototype(Type eventType, string nativeBaseTypename)
            {
                const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance |
                                                                System.Reflection.BindingFlags.NonPublic |
                                                                System.Reflection.BindingFlags.CreateInstance |
                                                                System.Reflection.BindingFlags.OptionalParamBinding;

                // manually construct native dist event to use as factory object
                var factoryObj = new DistEvent();
                var nativeRef = factoryObj.GetNativeReference();

                // create managed implementation, supplying the dist event we just created
                var prototype = (Reference)Activator.CreateInstance(eventType, flags, null, new object[] { nativeRef }, null);

                // register a factory
                RegisterEventHierarchy(eventType.Name, nativeBaseTypename, factoryObj);

                // use the overloaded AddFactory to register with correct typename
                AddFactory(prototype, eventType.Name);
            }


            public bool Start(IDistRemoteChannelInterface sessionChannel = null, IDistRemoteChannelInterface serverChannel = null)
            {
                return DistManager_start(GetNativeReference(), sessionChannel == null ? IntPtr.Zero : sessionChannel.GetNativeReference(), serverChannel == null ? IntPtr.Zero : serverChannel.GetNativeReference());
            }

            public void Shutdown(bool wait = false)
            {
                DistSessionInstanceManager.Clear();
                DistObjectInstanceManager.Clear();
                DistManager_shutDown(GetNativeReference(), wait);
            }

            public bool HasPendingData()
            {
                return DistManager_hasPendingData(GetNativeReference());
            }

            public bool IsRunning()
            {
                return DistManager_isRunning(GetNativeReference());
            }


            public bool EnableDebug(bool enable = true, bool wait = false)
            {
                bool retval = DistManager_enableDebug(GetNativeReference(), enable);

                if (wait)
                    License.SplashLicenseText("Waiting for debugger", "Press (OK) when debugger is started and connected");

                return retval;
            }

            public DistSession GetSession(string sessionName, bool create = false, bool global = false, ServerPriority prio = ServerPriority.PRIO_NORMAL)
            {
                return DistSessionInstanceManager.GetSession(DistManager_getSession(GetNativeReference(), sessionName, create, global, prio));
            }

            public DistEvent GetEvent(string typeName = "gzDistEvent")
            {
                return Reference.CreateObject(DistManager_getEvent(GetNativeReference(), typeName)) as DistEvent;
            }

            public DistObject GetObject(string objectName, string typeName = "gzDistObject")
            {
                return DistObjectInstanceManager.GetObject(DistManager_getObject(GetNativeReference(), objectName, typeName));
            }

            public bool RegisterEventHierarchy(string typeName, string parentTypeName = "gzDistEvent", DistEvent factoryEvent = null)
            {
                return DistManager_registerEventHierarchy(GetNativeReference(), typeName, parentTypeName, factoryEvent?.GetNativeReference() ?? IntPtr.Zero);
            }

            public bool RegisterObjecttHierarchy(string typeName, string parentTypeName = "gzDistObject", DistObject factoryObject = null)
            {
                return DistManager_registerObjectHierarchy(GetNativeReference(), typeName, parentTypeName, factoryObject?.GetNativeReference() ?? IntPtr.Zero);
            }

            public bool ProcessCustomThreadClients(bool waitForTrigger = false)
            {
                var res = DistManager_processCustomThreadClients(GetNativeReference(), waitForTrigger);
                if (res)
                {
                    OnPostProcess?.Invoke(this, EventArgs.Empty);
                }

                return res;
            }

            public DistObjectInstanceManager DistObjectInstanceManager { get; private set; } = new DistObjectInstanceManager();
            public DistSessionInstanceManager DistSessionInstanceManager { get; private set; } = new DistSessionInstanceManager();

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistManager_getManager(bool create, string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistManager_start(IntPtr manager, IntPtr sessionChannel, IntPtr serverChannel);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistManager_getSession(IntPtr manager, string sessionName, bool create, bool global, ServerPriority prio);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistManager_getEvent(IntPtr manager, string className);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistManager_getObject(IntPtr manager, string objectName, string className);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistManager_enableDebug(IntPtr manager, bool enable);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistManager_shutDown(IntPtr manager, bool wait);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistManager_registerEventHierarchy(IntPtr manager, string className, string parentClassName, IntPtr factory_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistManager_registerObjectHierarchy(IntPtr manager, string className, string parentClassName, IntPtr factory_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistManager_hasPendingData(IntPtr manager);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistManager_isRunning(IntPtr manager);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistManager_processCustomThreadClients(IntPtr manager, bool waitForTrigger);
        }
    }
}
