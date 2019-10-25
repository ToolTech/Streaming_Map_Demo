//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s) 
// herein belong to Saab AB. The program(s) may be used and/or
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
// File			: PluginLoader.cs
// Module		:
// Description	: Replacement stub for Plugins
// Author		: Anders Modén
// Product		: Gizmo3D 2.9.1
//
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows (Win32) and IRIX® for  
//			usage in Game or VisSim development.
//
//
// Revision History...
//
// Who	Date	Description
//
// AMO	180607	Created file        (2.9.1)
//
//******************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GizmoSDK.GizmoBase;

using UnityEngine;

namespace Saab.Unity.PluginLoader
{
    public class Platform
    {
        public const string BRIDGE = "UnityPluginInterface";
    }

    class UnityPluginInitializer
    {
        public UnityPluginInitializer()
        {
            UnityPlugin_Initialize();

            Message.OnMessage += On_Gizmo_Message;

            // Activate local registry
            KeyDatabase.SetLocalRegistry("config.xml");

        }

        private static void On_Gizmo_Message(string sender, MessageLevel level, string message)
        {
            if ((level & (MessageLevel.DEBUG | MessageLevel.MEM_DEBUG)) > 0)
            {
                Debug.Log(message);
            }
            else if ((level & (MessageLevel.NOTICE | MessageLevel.ALWAYS)) > 0)
            {
                Debug.Log(message);
            }
            else if ((level & MessageLevel.WARNING) > 0)
            {
                Debug.LogWarning(message);
            }
            else if ((level & MessageLevel.ASSERT) > 0)
            {
                Debug.LogAssertion(message);
            }
            else if ((level & MessageLevel.FATAL) > 0)
            {
                Debug.LogError(message);
            }
        }

        ~UnityPluginInitializer()
        {
            Message.OnMessage -= On_Gizmo_Message;

            UnityPlugin_UnInitialize();
        }

        [DllImport("UnityPluginInterface", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void UnityPlugin_Initialize();
        [DllImport("UnityPluginInterface", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void UnityPlugin_UnInitialize();
    }

    public class UnityPlugin : Reference
    {
        public UnityPlugin(string name) : base(UnityPlugin_GetPlugin(name))
        {

        }

        public DynamicType InvokeMethod(string method,DynamicType arg0=null)
        {
            return new DynamicType(UnityPlugin_InvokeMethod(GetNativeReference(), method,arg0?.GetNativeReference() ?? IntPtr.Zero));
        }

        static public string GetVersionInfo()
        {
            return Marshal.PtrToStringUni(UnityPlugin_GetVersionInfo());
        }

        

        [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr UnityPlugin_GetPlugin(string module);
        [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr UnityPlugin_GetVersionInfo();
        [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr UnityPlugin_InvokeMethod(IntPtr plugin, string method,IntPtr arg0_reference);
 
    }
}
