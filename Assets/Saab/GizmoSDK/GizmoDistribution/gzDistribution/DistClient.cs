//******************************************************************************
// File			: DistClient.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistClientInterface class
// Author		: Anders Modén		
// Product		: GizmoDistribution 2.10.1
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

                #region --------- event callback setup ------------------------
                m_dispatcher_OnTick = new DistEventHandler_OnTick_Callback(OnTick_callback);
                DistClient_SetCallback_OnTick(GetNativeReference(), m_dispatcher_OnTick);

                m_dispatcher_OnNewSession = new DistEventHandler_OnNewSession_Callback(OnNewSession_callback);
                DistClient_SetCallback_OnNewSession(GetNativeReference(), m_dispatcher_OnNewSession);

                m_dispatcher_OnRemoveSession = new DistEventHandler_OnRemoveSession_Callback(OnRemoveSession_callback);
                DistClient_SetCallback_OnRemoveSession(GetNativeReference(), m_dispatcher_OnRemoveSession);

                m_dispatcher_OnEvent = new DistEventHandler_OnEvent_Callback(OnEvent_callback);
                DistClient_SetCallback_OnEvent (GetNativeReference(), m_dispatcher_OnEvent);

                m_dispatcher_OnNewObject = new DistEventHandler_OnNewObject_Callback(OnNewObject_callback);
                DistClient_SetCallback_OnNewObject(GetNativeReference(), m_dispatcher_OnNewObject);

                m_dispatcher_OnRemoveObject = new DistEventHandler_OnRemoveObject_Callback(OnRemoveObject_callback);
                DistClient_SetCallback_OnRemoveObject(GetNativeReference(), m_dispatcher_OnRemoveObject);

                m_dispatcher_OnNewAttributes = new DistEventHandler_OnNewAttributes_Callback(OnNewAttributes_callback);
                DistClient_SetCallback_OnNewAttributes(GetNativeReference(), m_dispatcher_OnNewAttributes);

                m_dispatcher_OnUpdateAttributes = new DistEventHandler_OnUpdateAttributes_Callback(OnUpdateAttributes_callback);
                DistClient_SetCallback_OnUpdateAttributes(GetNativeReference(), m_dispatcher_OnUpdateAttributes);

                m_dispatcher_OnRemoveAttributes = new DistEventHandler_OnRemoveAttributes_Callback(OnRemoveAttributes_callback);
                DistClient_SetCallback_OnRemoveAttributes(GetNativeReference(), m_dispatcher_OnRemoveAttributes);

                #endregion

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
                return manager.DistSessionInstanceManager.GetSession(DistClient_getSession(GetNativeReference(), sessionName, create, global, prio));
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
                if (e.GetType().IsDefined(typeof(DistPropertyAutoStore),true))
                    e.StorePropertiesAndFields();

                return DistClient_sendEvent(GetNativeReference(), e.GetNativeReference(), session.GetNativeReference());
            }

            public T SendEventAndAwaitResponse<T>(DistEvent e,DistSession session,UInt32 timeout=100) where T : DistEvent
            {
                return SendEventAndAwaitResponse(e, session, manager.GetEvent<T>(), timeout) as T;
            }

            public DistEvent SendEventAndAwaitResponse(DistEvent e, DistSession session, DistEvent responseEventType, UInt32 timeout=100)
            {
                if (e.GetType().IsDefined(typeof(DistPropertyAutoStore), true))
                    e.StorePropertiesAndFields();

                DistEvent response = Reference.CreateObject(DistClient_sendEventAndAwaitResponse(GetNativeReference(), e.GetNativeReference(), session.GetNativeReference(), responseEventType.GetNativeReference(), timeout)) as DistEvent;

                if(response?.IsValid() ?? false)
                    if (response.GetType().IsDefined(typeof(DistPropertyAutoRestore), true))
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
                    if (response.GetType().IsDefined(typeof(DistPropertyAutoRestore), true))
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
                return DistClient_updateObject(GetNativeReference(), transaction.GetNativeReference(),o.GetNativeReference(), timeOut);
            }

            public bool UpdateObject(string name,DynamicType value, DistObject o, Int32 timeOut = 0)
            {
                return DistClient_updateObject_name(GetNativeReference(), name,value.GetNativeReference(), o.GetNativeReference(), timeOut);
            }

            public DistObject WaitForObject(string objectName, DistSession session, Int32 timeOut = 10)
            {
                return manager.DistObjectInstanceManager.GetObject(DistClient_waitForObject(GetNativeReference(), objectName, session.GetNativeReference(), timeOut));
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

            #region ------------------------ Private Callbacks ---------------------------------------------

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnTick_Callback();
           

            private DistEventHandler_OnTick_Callback m_dispatcher_OnTick;
            private void OnTick_callback()
            {
                OnTick?.Invoke(this);
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnNewSession_Callback(IntPtr session);

            private DistEventHandler_OnNewSession_Callback m_dispatcher_OnNewSession;
            private void OnNewSession_callback(IntPtr session)
            {
                OnNewSession?.Invoke(this, manager.DistSessionInstanceManager.GetSession(session));
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnRemoveSession_Callback(IntPtr session);

            private DistEventHandler_OnRemoveSession_Callback m_dispatcher_OnRemoveSession;
            private void OnRemoveSession_callback(IntPtr session)
            {
                OnRemoveSession?.Invoke(this, manager.DistSessionInstanceManager.GetSession(session));
                manager.DistSessionInstanceManager.DropSession(session);
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnEvent_Callback(IntPtr e);

            private DistEventHandler_OnEvent_Callback m_dispatcher_OnEvent;
            private void OnEvent_callback(IntPtr e)
            {
                DistEvent @event = Reference.CreateObject(e) as DistEvent;

                if (@event != null)
                {
                    if (@event.GetType().IsDefined(typeof(DistPropertyAutoRestore), true))
                        @event.RestorePropertiesAndFields();

                    OnEvent?.Invoke(this, @event);
                }
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnNewObject_Callback(IntPtr o,IntPtr session);

            private DistEventHandler_OnNewObject_Callback m_dispatcher_OnNewObject;
            private void OnNewObject_callback(IntPtr o,IntPtr session)
            {
                OnNewObject?.Invoke(this, manager.DistObjectInstanceManager.GetObject(o), manager.DistSessionInstanceManager.GetSession(session));
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnRemoveObject_Callback(IntPtr o, IntPtr session);

            private DistEventHandler_OnRemoveObject_Callback m_dispatcher_OnRemoveObject;
            private void OnRemoveObject_callback(IntPtr o, IntPtr session)
            {
                OnRemoveObject?.Invoke(this, manager.DistObjectInstanceManager.GetObject(o), manager.DistSessionInstanceManager.GetSession(session));
                manager.DistObjectInstanceManager.DropObject(o);
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnNewAttributes_Callback(IntPtr notif,IntPtr o, IntPtr session);

            private DistEventHandler_OnNewAttributes_Callback m_dispatcher_OnNewAttributes;
            private void OnNewAttributes_callback(IntPtr notif,IntPtr o, IntPtr session)
            {
                OnNewAttributes?.Invoke(this, new DistNotificationSet(notif), manager.DistObjectInstanceManager.GetObject(o), manager.DistSessionInstanceManager.GetSession(session));
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnUpdateAttributes_Callback(IntPtr notif, IntPtr o, IntPtr session);

            private DistEventHandler_OnUpdateAttributes_Callback m_dispatcher_OnUpdateAttributes;
            private void OnUpdateAttributes_callback(IntPtr notif, IntPtr o, IntPtr session)
            {
                OnUpdateAttributes?.Invoke(this, new DistNotificationSet(notif), manager.DistObjectInstanceManager.GetObject(o), manager.DistSessionInstanceManager.GetSession(session));
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public delegate void DistEventHandler_OnRemoveAttributes_Callback(IntPtr notif, IntPtr o, IntPtr session);

            private DistEventHandler_OnRemoveAttributes_Callback m_dispatcher_OnRemoveAttributes;
            private void OnRemoveAttributes_callback(IntPtr notif, IntPtr o, IntPtr session)
            {
                OnRemoveAttributes?.Invoke(this, new DistNotificationSet(notif), manager.DistObjectInstanceManager.GetObject(o), manager.DistSessionInstanceManager.GetSession(session));
            }

            #endregion

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

            #region ------------------ SetCallback ------------------------------------------------------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnTick(IntPtr client, DistEventHandler_OnTick_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnNewSession(IntPtr client, DistEventHandler_OnNewSession_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnRemoveSession(IntPtr client, DistEventHandler_OnRemoveSession_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnEvent(IntPtr client, DistEventHandler_OnEvent_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnNewObject(IntPtr client, DistEventHandler_OnNewObject_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnRemoveObject(IntPtr client, DistEventHandler_OnRemoveObject_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnNewAttributes(IntPtr client, DistEventHandler_OnNewAttributes_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnUpdateAttributes(IntPtr client, DistEventHandler_OnUpdateAttributes_Callback fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DistClient_SetCallback_OnRemoveAttributes(IntPtr client, DistEventHandler_OnRemoveAttributes_Callback fn);
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

        }
    }
}
