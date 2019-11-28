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
// File			: DynamicLoader.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzDynamicLoader class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.5
//		
//
//			
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows (Win32) and Android for  
//			usage in Game or VisSim development.
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;


namespace GizmoSDK
{
    namespace Gizmo3D
    {
        public enum DynamicLoadingState
        {
            UNLOADED,               // SYNC in NodeLock
            LOADED,                 // SYNC in NodeLock
            REQUEST_LOAD,           // SYNC from thread
            REQUEST_LOAD_CANCEL,    // SYNC from thread
            IN_LOADING,             // ASYNC
            IN_DESTROY,             // SYNC in NodeLock
            IN_TRAVERSAL,           // No callback
            REQUEST_UNLOAD,         // ASYNC
            NOT_FOUND,              // ASYNC
        }
        public class DynamicLoader : Group
        {
            public delegate void EventHandler_OnDynamicLoad(DynamicLoadingState state,DynamicLoader loader,Node node);

            static public event EventHandler_OnDynamicLoad OnDynamicLoad;

            public DynamicLoader(IntPtr nativeReference) : base(nativeReference) { }

            public DynamicLoader(string name="") : base(DynamicLoader_create(name)) { }

            public new static void InitializeFactory()
            {
                AddFactory(new DynamicLoader());
            }

            public static new void UninitializeFactory()
            {
                RemoveFactory("gzDynamicLoader");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new DynamicLoader(nativeReference) as Reference;
            }

            public string NodeURL
            {
                get
                {
                    return Marshal.PtrToStringUni(DynamicLoader_getNodeURL(GetNativeReference()));
                }

                set
                {
                    DynamicLoader_setNodeURL(GetNativeReference(), value);
                }
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

            #region -------- Private ------------------------------------------------------------

            private sealed class Initializer
            {
                public Initializer()
                {
                    if (s_dispatcher == null)
                    {
                        s_dispatcher = new Native_OnDynamicLoad(OnDynamicLoad_callback);
                        DynamicLoader_SetCallback(s_dispatcher);
                    }
                }
                
                ~Initializer()
                {
                    if (s_dispatcher != null)
                    {
                        DynamicLoader_SetCallback(null);
                        s_dispatcher = null;
                    }
                }
            }

            static private Initializer s_class_init = new Initializer();

            [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            delegate void Native_OnDynamicLoad(DynamicLoadingState state, IntPtr loader_ref, IntPtr node_ref);

            [MonoPInvokeCallback(typeof(Native_OnDynamicLoad))]
            private static void OnDynamicLoad_callback(DynamicLoadingState state, IntPtr loader_reference, IntPtr node_reference)
            {
                OnDynamicLoad?.Invoke(state,CreateObject(loader_reference) as DynamicLoader,CreateObject(node_reference) as Node);
            }

            static private Native_OnDynamicLoad s_dispatcher;

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicLoader_SetCallback(Native_OnDynamicLoad fn);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicLoader_create(string name);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DynamicLoader_getNodeURL(IntPtr loader_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicLoader_setNodeURL(IntPtr loader_reference,string url);
            #endregion

            #endregion
        }
    }
}
