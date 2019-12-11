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
// File			: KeyDatabase.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzKeyDatabase class
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
// AMO	180816	Created file 	
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;
using System.ComponentModel;

namespace GizmoSDK
{
    namespace GizmoBase
    {
       
        public class KeyDatabase
        {
            static public bool SetLocalRegistry(string url)
            {
                return KeyDatabase_setLocalRegistry(url);
            }

            static public void SetDefaultRegistry(string url)
            {
                KeyDatabase_setDefaultRegistry(url);
            }

            static public T GetUserKey<T>(string key, string password="", bool onlyUserKey=false)
            {
                string keyval = Marshal.PtrToStringUni(KeyDatabase_getUserKey(key, password, onlyUserKey));

                try
                {
                    return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(keyval);
                }
                catch
                {
                    Message.Send(Message.GIZMOSDK, MessageLevel.WARNING, $"Failed to convert '{keyval}' in GetUserKey<T>");
                    return default(T);
                }
            }

            static public T GetDefaultUserKey<T>(string key, T defaultValue=default(T),string password = "", bool onlyUserKey = false)
            {
                string keyval = Marshal.PtrToStringUni(KeyDatabase_getDefaultUserKey(key, defaultValue.ToString(), password, onlyUserKey));
                try
                {
                    return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(keyval);
                }
                catch
                {
                    Message.Send(Message.GIZMOSDK, MessageLevel.WARNING, $"Failed to convert '{keyval}' in GetDefaultUserKey<T>");
                    return default(T);
                }
            }

            static public T GetGlobalKey<T>(string key, string password = "")
            {
                string keyval = Marshal.PtrToStringUni(KeyDatabase_getGlobalKey(key, password));

                try
                {
                    return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(keyval);
                }
                catch
                {
                    Message.Send(Message.GIZMOSDK, MessageLevel.WARNING, $"Failed to convert '{keyval}' in GetGlobalKey<T>");
                    return default(T);
                }
            }

            static public T GetDefaultGlobalKey<T>(string key, T defaultValue = default(T), string password = "")
            {
                string keyval = Marshal.PtrToStringUni(KeyDatabase_getDefaultGlobalKey(key, defaultValue.ToString(), password));
                try
                {
                    return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(keyval);
                }
                catch
                {
                    Message.Send(Message.GIZMOSDK, MessageLevel.WARNING, $"Failed to convert '{keyval}' in GetDefaultGlobalKey<T>");
                    return default(T);
                }
            }

            #region // --------------------- Native calls -----------------------

            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void KeyDatabase_setDefaultRegistry(string url);
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool KeyDatabase_setLocalRegistry(string url);
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr KeyDatabase_getUserKey(string key, string password , bool onlyUserKey);
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr KeyDatabase_getDefaultUserKey(string key, string defaultValue,string password, bool onlyUserKey);
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr KeyDatabase_getGlobalKey(string key, string password);
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr KeyDatabase_getDefaultGlobalKey(string key, string defaultValue, string password);

            #endregion
        }



        

       
    }
}

