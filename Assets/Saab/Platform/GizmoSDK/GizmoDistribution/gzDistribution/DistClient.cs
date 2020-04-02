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
// File			: DistClient.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistClientInterface class
// Author		: Anders Modén		
// Product		: GizmoDistribution 2.10.5
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
// AMO  281211  Added UseAutoProperty to enable automatic store/restore of properties
//
//******************************************************************************

using System;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;

 

namespace GizmoSDK
{
    namespace GizmoDistribution
    {
        public class DistClient : Reference
        {
            public const UInt32 DIST_POOL_ID_CUSTOM = 0xFFFFFFFF;

            #region --------------- DistEvents --------------------------------------------------------------

            public delegate void DistEventHandler_OnTick(DistClient sender);
            public event DistEventHandler_OnTick OnTick;

            public delegate void DistEventHandler_OnNewSession(DistClient sender,DistSession session);
            public event DistEventHandler_OnNewSession OnNewSession;

            public delegate void DistEventHandler_OnRemoveSession(DistClient sender, DistSession session);
            public event DistEventHandler_OnRemoveSession OnRemoveSession;

            public delegate void DistEventHandler_OnEvent(DistClient sender, DistEvent e);
            public event DistEventHandler_OnEvent OnEvent;

            public delegate void DistEventHandler_OnNewObject(DistClient sender, DistObject o,DistSession session);
            public event DistEventHandler_OnNewObject OnNewObject;

            public delegate void DistEventHandler_OnRemoveObject(DistClient sender, DistObject o, DistSession session);
            public event DistEventHandler_OnRemoveObject OnRemoveObject;

            public delegate void DistEventHandler_OnNewAttributes(DistClient sender, DistNotificationSet notif, DistObject o, DistSession session);
            public event DistEventHandler_OnNewAttributes OnNewAttributes;

            public delegate void DistEventHandler_OnUpdateAttributes(DistClient sender, DistNotificationSet notif, DistObject o, DistSession session);
            public event DistEventHandler_OnUpdateAttributes OnUpdateAttributes;

            public delegate void DistEventHandler_OnRemoveAttributes(DistClient sender, DistNotificationSet notif, DistObject o, DistSession session);
            public event DistEventHandler_OnRemoveAttributes OnRemoveAttributes;

            #endregion

            // ---------------------------- Interface --------------------------------------------------

            public DistClient(string name,DistManager current_manager) : base(DistClient_getClient(name))
            {
                manager = current_manager;      // We always need a current manager

                ReferenceDictionary<DistClient>.AddObject(this);

            }

            /// <summary>
            /// Override Release behaviour to remove from Reference Dictionary
            /// </summary>
            override public void Release()
            {
                ReferenceDictionary<DistClient>.RemoveObject(this);
                base.Release();
            }

            public DistManager manager { get; set; }
            public bool Initialize(double tickInterval = 0, UInt32 poolId = 0, bool responseBoost = false, DistManager manager = null)
            {
                if (manager != null)
                    this.manager = manager;

                return DistClient_initialize(GetNativeReference(),tickInterval, poolId, responseBoost, this.manager?.GetNativeReference() ?? IntPtr.Zero);
            }

            public bool Uninitialize(bool wait = false)
            {
                return DistClient_uninitialize(GetNativeReference(),wait);
            }

            public bool IsInitialized()
            {
                return DistClient_isInitialized(GetNativeReference());
            }

            public DistSession GetSession(string sessionName, bool create = false, bool global = false, ServerPriority prio = ServerPriority.PRIO_NORMAL)
            {
                return GetSession(DistClient_getSession(GetNativeReference(), sessionName, create, global, prio));
            }

            public bool JoinSession(DistSession session, Int32 timeOut = 0)
            {
                return DistClient_joinSession(GetNativeReference(), session.GetNativeReference(), timeOut);
            }

            public bool ResignSession(DistSession session, Int32 timeOut = 0)
            {
                return DistClient_resignSession(GetNativeReference(), session.GetNativeReference(), timeOut);
            }

            public bool SubscribeSessions(bool notifyExisting = false, Int32 timeOut = 0)
            {
                return DistClient_subscribeSessions(GetNativeReference(), notifyExisting, timeOut);
            }

            public bool UnSubscribeSessions(Int32 timeOut = 0)
            {
                return DistClient_unsubscribeSessions(GetNativeReference(), timeOut);
            }

            public bool SubscribeEvents<T>(DistSession session, Int32 timeOut = 0) where T : DistEvent
            {
                return SubscribeEvents(session, typeof(T).Name, timeOut);
            }

            public bool SubscribeEvents(DistSession session,string typeName=null,Int32 timeOut=0)
            {
                return DistClient_subscribeEvents(GetNativeReference(),session.GetNativeReference(), typeName, timeOut);
            }

            public bool UnSubscribeEvents(DistSession session, string typeName = null, Int32 timeOut = 0)
            {
                return DistClient_unsubscribeEvents(GetNativeReference(), session.GetNativeReference(), typeName, timeOut);
            }

            public bool SendEvent(DistEvent e,DistSession session)
            {
                if (UseAutoProperty && e.GetType().IsDefined(typeof(DistPropertyAutoStore),true))
                    e.StorePropertiesAndFields();

                return DistClient_sendEvent(GetNativeReference(), e.GetNativeReference(), session.GetNativeReference());
            }

            public T SendEventAndAwaitResponse<T>(DistEvent e,DistSession session,UInt32 timeout=100) where T : DistEvent
            {
                return SendEventAndAwaitResponse(e, session, manager.GetEvent<T>(), timeout) as T;
            }

            public DistEvent SendEventAndAwaitResponse(DistEvent e, DistSession session, DistEvent responseEventType, UInt32 timeout=100)
            {
                if (UseAutoProperty && e.GetType().IsDefined(typeof(DistPropertyAutoStore), true))
                    e.StorePropertiesAndFields();

                DistEvent response = Reference.CreateObject(DistClient_sendEventAndAwaitResponse(GetNativeReference(), e.GetNativeReference(), session.GetNativeReference(), responseEventType.GetNativeReference(), timeout)) as DistEvent;

                if(response?.IsValid() ?? false)
                    if (UseAutoProperty && response.GetType().IsDefined(typeof(DistPropertyAutoRestore), true))
                        response.RestorePropertiesAndFields();

                return response;
            }

            public T AwaitResponse<T>(UInt32 timeout = 100) where T : DistEvent
            {
                return AwaitResponse(manager.GetEvent<T>(), timeout) as T;
            }

            public DistEvent AwaitResponse(DistEvent responseEventType, UInt32 timeout = 100)
            {
                DistEvent response = Reference.CreateObject(DistClient_awaitResponse(GetNativeReference(), responseEventType.GetNativeReference(), timeout)) as DistEvent;

                if (response?.IsValid() ?? false)
                    if (UseAutoProperty && response.GetType().IsDefined(typeof(DistPropertyAutoRestore), true))
                        response.RestorePropertiesAndFields();

                return response;
            }

            public bool AddObject(DistObject o, DistSession session, Int32 timeOut = 0)
            {
                return DistClient_addObject(GetNativeReference(), o.GetNativeReference(), session.GetNativeReference(), timeOut);
            }

            public bool RemoveObject(DistObject o, Int32 timeOut = 0)
            {
                return DistClient_removeObject(GetNativeReference(), o.GetNativeReference(), timeOut);
            }

            public bool SubscribeObjects(DistSession session, string typeName = null, bool notifyExisting=false,Int32 timeOut = 0)
            {
                return DistClient_subscribeObjects(GetNativeReference(), session.GetNativeReference(), typeName, notifyExisting,timeOut);
            }

            public bool UnSubscribeObjects(DistSession session, string typeName = null, Int32 timeOut = 0)
            {
                return DistClient_unsubscribeObjects(GetNativeReference(), session.GetNativeReference(), typeName, timeOut);
            }

            public bool UpdateObject(DistTransaction transaction, DistObject o, Int32 timeOut = 0)
            {
                bool result=DistClient_updateObject(GetNativeReference(), transaction.GetNativeReference(),o.GetNativeReference(), timeOut);

                // Invalidate the tranaction
                transaction.Release();  

                return result;
            }

            public bool UpdateObject(string name,DynamicType value, DistObject o, Int32 timeOut = 0)
            {
                return DistClient_updateObject_name(GetNativeReference(), name,value.GetNativeReference(), o.GetNativeReference(), timeOut);
            }

            public DistObject WaitForObject(string objectName, DistSession session, Int32 timeOut = 10)
            {
                return GetObject(DistClient_waitForObject(GetNativeReference(), objectName, session.GetNativeReference(), timeOut));
            }

            public bool SubscribeAttributes(DistObject o, bool notifyExisting = false, Int32 timeOut = 0)
            {
                return DistClient_subscribeAttributes(GetNativeReference(), o.GetNativeReference(), notifyExisting, timeOut);
            }

            public bool UnSubscribeAttributes(DistObject o, Int32 timeOut = 0)
            {
                return DistClient_unsubscribeAttributes(GetNativeReference(), o.GetNativeReference(), timeOut);
            }

            public bool SubscribeAttributeValue(string attributeName,DistObject o, bool notifyExisting = false, Int32 timeOut = 0)
            {
                return DistClient_subscribeAttributeValue(GetNativeReference(), attributeName,o.GetNativeReference(), notifyExisting, timeOut);
            }

            public bool UnSubscribeAttributeValue(string attributeName,DistObject o, Int32 timeOut = 0)
            {
                return DistClient_unsubscribeAttributeValue(GetNativeReference(), attributeName,o.GetNativeReference(), timeOut);
            }

            public bool SubscribeAllAttributeValues( DistObject o, bool notifyExisting = false, Int32 timeOut = 0)
            {
                return DistClient_subscribeAllAttributeValues(GetNativeReference(),  o.GetNativeReference(), notifyExisting, timeOut);
            }

            public bool UnSubscribeAllAttributeValues( DistObject o, Int32 timeOut = 0)
            {
                return DistClient_unsubscribeAllAttributeValues(GetNativeReference(), o.GetNativeReference(), timeOut);
            }

            public bool SubscribeAttributeValue(DistTransaction transaction, DistObject o, bool notifyExisting = false, Int32 timeOut = 0)
            {
                return DistClient_subscribeAttributeValue_transaction(GetNativeReference(), transaction.GetNativeReference(), o.GetNativeReference(), notifyExisting, timeOut);
            }

            public bool UnSubscribeAttributeValue(DistTransaction transaction, DistObject o, Int32 timeOut = 0)
            {
                return DistClient_unsubscribeAttributeValue_transaction(GetNativeReference(), transaction.GetNativeReference(), o.GetNativeReference(), timeOut);
            }

            public bool SubscribeAttributeValue(DistNotificationSet notification_set, DistObject o, bool notifyExisting = false, Int32 timeOut = 0)
            {
                return DistClient_subscribeAttributeValue_notificationSet(GetNativeReference(), notification_set.GetNativeReference(), o.GetNativeReference(), notifyExisting, timeOut);
            }

            public bool UnSubscribeAttributeValue(DistNotificationSet notification_set, DistObject o, Int32 timeOut = 0)
            {
                return DistClient_unsubscribeAttributeValue_notificationSet(GetNativeReference(), notification_set.GetNativeReference(), o.GetNativeReference(), timeOut);
            }

            public UInt32 GetPendingData(bool outQueue=true)
            {
                return DistClient_getPendingData(GetNativeReference(), outQueue);
            }

            public DistClientID GetClientID()
            {
                return new DistClientID(DistClient_getClientID(GetNativeReference()));
            }

            static public bool HasDistThreadError()
            {
                return DistClient_hasDistThreadError();
            }

            static public string GetDistThreadError(bool clearError=true)
            {
                return Marshal.PtrToStringUni(DistClient_getDistThreadError(clearError));
            }

            public bool UseAutoProperty = true;

            static public void Initialize_()
            {
                if (s_class_init == null)
                    s_class_init = new Initializer();
            }

            static public void Uninitialize_()
            {
                if (s_class_init != null)
                    s_class_init = null;
            }


            #region ---------------- Private functions ------------------------

            static private DistSession GetSession(IntPtr s)
            {
                DistSession session = ReferenceDictionary<DistSession>.GetObject(s);

                if (session == null)
                    session = new DistSession(s);

                return session;
            }

            static private DistObject GetObject(IntPtr o)
            {
                DistObject obj = ReferenceDictionary<DistObject>.GetObject(o);

                if (obj == null)
                    obj = Reference.CreateObject(o) as DistObject;

                return obj;
            }

            private sealed class Initializer
            {
                public Initializer()
                {
                    s_dispatcher_OnTick = new DistEventHandler_OnTick_Callback(OnTick_callback);
                    DistClient_SetCallback_OnTick(s_dispatcher_OnTick);

                    s_dispatcher_OnNewSession = new DistEventHandler_OnNewSession_Callback(OnNewSession_callback);
                    DistClient_SetCallback_OnNewSession(s_dispatcher_OnNewSession);

                    s_dispatcher_OnRemoveSession = new DistEventHandler_OnRemoveSession_Callback(OnRemoveSession_callback);
                    DistClient_SetCallback_OnRemoveSession(s_dispatcher_OnRemoveSession);

                    s_dispatcher_OnEvent = new DistEventHandler_OnEvent_Callback(OnEvent_callback);
                    DistClient_SetCallback_OnEvent(s_dispatcher_OnEvent);

                    s_dispatcher_OnNewObject = new DistEventHandler_OnNewObject_Callback(OnNewObject_callback);
                    DistClient_SetCallback_OnNewObject(s_dispatcher_OnNewObject);

                    s_dispatcher_OnRemoveObject = new DistEventHandler_OnRemoveObject_Callback(OnRemoveObject_callback);
                    DistClient_SetCallback_OnRemoveObject(s_dispatcher_OnRemoveObject);

                    s_dispatcher_OnNewAttributes = new DistEventHandler_OnNewAttributes_Callback(OnNewAttributes_callback);
                    DistClient_SetCallback_OnNewAttributes(s_dispatcher_OnNewAttributes);

                    s_dispatcher_OnUpdateAttributes = new DistEventHandler_OnUpdateAttributes_Callback(OnUpdateAttributes_callback);
                    DistClient_SetCallback_OnUpdateAttributes(s_dispatcher_OnUpdateAttributes);

                    s_dispatcher_OnRemoveAttributes = new DistEventHandler_OnRemoveAttributes_Callback(OnRemoveAttributes_callback);
                    DistClient_SetCallback_OnRemoveAttributes(s_dispatcher_OnRemoveAttributes);

                }

                ~Initializer()
                {
                    s_dispatcher_OnTick = null;
                    DistClient_SetCallback_OnTick(null);

                    s_dispatcher_OnNewSession = null;
                    DistClient_SetCallback_OnNewSession(null);

                    s_dispatcher_OnRemoveSession = null;
                    DistClient_SetCallback_OnRemoveSession(null);

                    s_dispatcher_OnEvent = null;
                    DistClient_SetCallback_OnEvent(null);

                    s_dispatcher_OnNewObject = null;
                    DistClient_SetCallback_OnNewObject(null);

                    s_dispatcher_OnRemoveObject = null;
                    DistClient_SetCallback_OnRemoveObject(null);

                    s_dispatcher_OnNewAttributes = null;
                    DistClient_SetCallback_OnNewAttributes(null);

                    s_dispatcher_OnUpdateAttributes = null;
                    DistClient_SetCallback_OnUpdateAttributes(null);

                    s_dispatcher_OnRemoveAttributes = null;
                    DistClient_SetCallback_OnRemoveAttributes(null);
                }
            }

            static private Initializer s_class_init = new Initializer();

            // ----------------------------- OnTick ------------------------------------------------

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnTick_Callback(IntPtr instance);

            private static DistEventHandler_OnTick_Callback s_dispatcher_OnTick;

            [MonoPInvokeCallback(typeof(DistEventHandler_OnTick_Callback))]
            static private void OnTick_callback(IntPtr instance)
            {
                DistClient client = ReferenceDictionary<DistClient>.GetObject(instance);

                try
                {
                    client?.OnTick?.Invoke(client);
                }
                catch (Exception ex)
                {
                    Message.SendException($"DistClient", ex);
                }
            }

            // ----------------------------- OnNewSesson ------------------------------------------------

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnNewSession_Callback(IntPtr instance,IntPtr session);

            private static DistEventHandler_OnNewSession_Callback s_dispatcher_OnNewSession;

            [MonoPInvokeCallback(typeof(DistEventHandler_OnNewSession_Callback))]
            static private void OnNewSession_callback(IntPtr instance,IntPtr session)
            {
                DistClient client = ReferenceDictionary<DistClient>.GetObject(instance);

                try
                { 
                    client?.OnNewSession?.Invoke(client, GetSession(session));
                }
                catch (Exception ex)
                {
                    Message.SendException("DistClient", ex);
                }
            }

            // ----------------------------- OnRemoveSesson ------------------------------------------------

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnRemoveSession_Callback(IntPtr instance,IntPtr session);

            static private DistEventHandler_OnRemoveSession_Callback s_dispatcher_OnRemoveSession;

            [MonoPInvokeCallback(typeof(DistEventHandler_OnRemoveSession_Callback))]
            static private void OnRemoveSession_callback(IntPtr instance,IntPtr session)
            {
                DistClient client = ReferenceDictionary<DistClient>.GetObject(instance);

                if (client != null)
                {
                    try
                    { 
                        client.OnRemoveSession?.Invoke(client, GetSession(session));
                    }
                    catch (Exception ex)
                    {
                        Message.SendException("DistClient", ex);
                    }

                    ReferenceDictionary<DistSession>.RemoveObject(session);
                }
            }

            // ----------------------------- OnEvent ------------------------------------------------

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnEvent_Callback(IntPtr instance,IntPtr e);

            static private DistEventHandler_OnEvent_Callback s_dispatcher_OnEvent;

            [MonoPInvokeCallback(typeof(DistEventHandler_OnEvent_Callback))]
            static private void OnEvent_callback(IntPtr instance,IntPtr e)
            {
                DistClient client = ReferenceDictionary<DistClient>.GetObject(instance);

                if (client != null)
                {
                    DistEvent @event = Reference.CreateObject(e) as DistEvent;

                    if (@event != null)
                    {
                        if (client.UseAutoProperty && @event.GetType().IsDefined(typeof(DistPropertyAutoRestore), true))
                            @event.RestorePropertiesAndFields();

                        try
                        { 
                            client.OnEvent?.Invoke(client, @event);
                        }
                        catch (Exception ex)
                        {
                            Message.SendException("DistClient", ex);
                        }
                    }
                }
            }

            // ----------------------------- OnNewObject ------------------------------------------------

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnNewObject_Callback(IntPtr instance,IntPtr o,IntPtr session);

            static private DistEventHandler_OnNewObject_Callback s_dispatcher_OnNewObject;

            [MonoPInvokeCallback(typeof(DistEventHandler_OnNewObject_Callback))]
            static private void OnNewObject_callback(IntPtr instance,IntPtr o,IntPtr session)
            {
                DistClient client = ReferenceDictionary<DistClient>.GetObject(instance);

                try
                { 
                    client?.OnNewObject?.Invoke(client, GetObject(o), GetSession(session));
                }
                catch (Exception ex)
                {
                    Message.SendException("DistClient", ex);
                }
            }

            // ----------------------------- OnRemoveObject ------------------------------------------------

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnRemoveObject_Callback(IntPtr instance,IntPtr o, IntPtr session);

            static private DistEventHandler_OnRemoveObject_Callback s_dispatcher_OnRemoveObject;

            [MonoPInvokeCallback(typeof(DistEventHandler_OnRemoveObject_Callback))]
            static private void OnRemoveObject_callback(IntPtr instance,IntPtr o, IntPtr session)
            {
                DistClient client = ReferenceDictionary<DistClient>.GetObject(instance);

                if (client != null)
                {
                    try
                    {
                        client.OnRemoveObject?.Invoke(client, GetObject(o), GetSession(session));
                    }
                    catch (Exception ex)
                    {
                        Message.SendException("DistClient", ex);
                    }
                    
                    ReferenceDictionary<DistSession>.RemoveObject(o);
                }
            }

            // ----------------------------- OnNewAttributes ------------------------------------------------

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnNewAttributes_Callback(IntPtr instance,IntPtr notif,IntPtr o, IntPtr session);

            static private DistEventHandler_OnNewAttributes_Callback s_dispatcher_OnNewAttributes;

            [MonoPInvokeCallback(typeof(DistEventHandler_OnNewAttributes_Callback))]
            static private void OnNewAttributes_callback(IntPtr instance,IntPtr notif,IntPtr o, IntPtr session)
            {
                DistClient client = ReferenceDictionary<DistClient>.GetObject(instance);

                try
                { 
                    client?.OnNewAttributes?.Invoke(client, new DistNotificationSet(notif), GetObject(o), GetSession(session));
                }
                catch (Exception ex)
                {
                    Message.SendException("DistClient", ex);
                }
            }

            // ----------------------------- OnUpdateAttributes ------------------------------------------------

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnUpdateAttributes_Callback(IntPtr instance,IntPtr notif, IntPtr o, IntPtr session);

            static private DistEventHandler_OnUpdateAttributes_Callback s_dispatcher_OnUpdateAttributes;

            [MonoPInvokeCallback(typeof(DistEventHandler_OnUpdateAttributes_Callback))]
            static private void OnUpdateAttributes_callback(IntPtr instance,IntPtr notif, IntPtr o, IntPtr session)
            {
                DistClient client = ReferenceDictionary<DistClient>.GetObject(instance);

                try
                { 
                    client?.OnUpdateAttributes?.Invoke(client, new DistNotificationSet(notif), GetObject(o), GetSession(session));
                }
                catch (Exception ex)
                {
                    Message.SendException("DistClient", ex);
                }
            }

            // ----------------------------- OnRemoveAttributes ------------------------------------------------

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnRemoveAttributes_Callback(IntPtr instance,IntPtr notif, IntPtr o, IntPtr session);

            static private DistEventHandler_OnRemoveAttributes_Callback s_dispatcher_OnRemoveAttributes;

            [MonoPInvokeCallback(typeof(DistEventHandler_OnRemoveAttributes_Callback))]
            static private void OnRemoveAttributes_callback(IntPtr instance,IntPtr notif, IntPtr o, IntPtr session)
            {
                DistClient client = ReferenceDictionary<DistClient>.GetObject(instance);

                try
                { 
                    client?.OnRemoveAttributes?.Invoke(client, new DistNotificationSet(notif), GetObject(o), GetSession(session));
                }
                catch (Exception ex)
                {
                    Message.SendException("DistClient", ex);
                }
            }


            #region ------------- Native functions -----------------------------------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistClient_getClient(string name);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_joinSession(IntPtr client,IntPtr session,Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_resignSession(IntPtr client, IntPtr session, Int32 timeOut);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt32 DistClient_getPendingData(IntPtr client, bool outQueue);


            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_hasDistThreadError();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistClient_getDistThreadError(bool clearError);

            #endregion

            #region ------------------ SetCallback ------------------------------------------------------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnTick(DistEventHandler_OnTick_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnNewSession(DistEventHandler_OnNewSession_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnRemoveSession(DistEventHandler_OnRemoveSession_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnEvent(DistEventHandler_OnEvent_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnNewObject(DistEventHandler_OnNewObject_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnRemoveObject(DistEventHandler_OnRemoveObject_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnNewAttributes(DistEventHandler_OnNewAttributes_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnUpdateAttributes(DistEventHandler_OnUpdateAttributes_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnRemoveAttributes(DistEventHandler_OnRemoveAttributes_Callback fn);
            #endregion

            #region ------------------- Init ------------------------------------------------------------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_initialize(IntPtr client,double tickInterval,UInt32 poolId,bool responseBoost,IntPtr manager);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_uninitialize(IntPtr client, bool wait);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_isInitialized(IntPtr client);
            #endregion

            #region ------------------- Session ---------------------------------------------------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistClient_getSession(IntPtr client, string sessionName, bool create , bool global, ServerPriority prio);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_subscribeSessions(IntPtr client, bool notifyExisting, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_unsubscribeSessions(IntPtr client, Int32 timeOut);

            #endregion

            #region ------------------- DistEvents ----------------------------------------------------------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_sendEvent(IntPtr client_ref, IntPtr event_ref, IntPtr session_ref);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistClient_sendEventAndAwaitResponse(IntPtr client_ref, IntPtr event_ref, IntPtr session_ref,IntPtr response_ref,UInt32 timeout);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistClient_awaitResponse(IntPtr client_ref, IntPtr response_ref, UInt32 timeout);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_subscribeEvents(IntPtr client_ref, IntPtr session_ref,string typeName,Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_unsubscribeEvents(IntPtr client_ref, IntPtr session_ref, string typeName, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistClient_getClientID(IntPtr client_ref);

            #endregion

            #region ------------------- DistObjects ---------------------------------------------------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_subscribeObjects(IntPtr client_ref, IntPtr session_ref, string typeName, bool notifyExisting,Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_unsubscribeObjects(IntPtr client_ref, IntPtr session_ref, string typeName, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_addObject(IntPtr client_ref, IntPtr object_ref, IntPtr session_ref, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_removeObject(IntPtr client_ref, IntPtr object_ref, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_updateObject(IntPtr client_ref, IntPtr trans_ref,IntPtr object_ref, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_updateObject_name(IntPtr client_ref, string name,IntPtr dynamic_ref, IntPtr object_ref, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistClient_waitForObject(IntPtr client_ref, string object_name, IntPtr session_ref, Int32 timeOut);
            #endregion

            #region ------------------- Attributes ------------------------------------------------------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_subscribeAttributes(IntPtr client_ref, IntPtr object_ref, bool notifyExisting, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_unsubscribeAttributes(IntPtr client_ref, IntPtr object_ref, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_subscribeAttributeValue(IntPtr client_ref, string name,IntPtr object_ref, bool notifyExisting, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_unsubscribeAttributeValue(IntPtr client_ref, string name,IntPtr object_ref, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_subscribeAllAttributeValues(IntPtr client_ref, IntPtr object_ref, bool notifyExisting, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_unsubscribeAllAttributeValues(IntPtr client_ref, IntPtr object_ref, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_subscribeAttributeValue_transaction(IntPtr client_ref, IntPtr trans_ref, IntPtr object_ref, bool notifyExisting, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_subscribeAttributeValue_notificationSet(IntPtr client_ref, IntPtr notif_ref, IntPtr object_ref, bool notifyExisting, Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_unsubscribeAttributeValue_transaction(IntPtr client_ref, IntPtr trans_ref, IntPtr object_ref,Int32 timeOut);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool DistClient_unsubscribeAttributeValue_notificationSet(IntPtr client_ref, IntPtr notif_ref, IntPtr object_ref, Int32 timeOut);
            #endregion

            #region ------------------------- Reference -------------------------------------
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Reference_getTypeName(IntPtr ptr);
            #endregion

            #endregion

        }
    }
}
