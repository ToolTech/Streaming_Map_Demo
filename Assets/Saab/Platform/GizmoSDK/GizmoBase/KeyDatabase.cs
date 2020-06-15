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
// Product		: GizmoBase 2.10.6
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
            static public bool SetLocalRegistry(string url, string password = "")
            {
                SerializeAdapter.AdapterError error = SerializeAdapter.AdapterError.NO_ERROR;
                IntPtr nativeErrorString = IntPtr.Zero;

                return KeyDatabase_setLocalRegistry(url,password,ref nativeErrorString,ref error);
            }

            static public bool SetLocalRegistry(string url, ref string errorString, ref SerializeAdapter.AdapterError error, string password="")
            {
                IntPtr nativeErrorString = IntPtr.Zero;

                bool result=KeyDatabase_setLocalRegistry(url, password, ref nativeErrorString, ref error);

                if (nativeErrorString != IntPtr.Zero)
                    errorString = Marshal.PtrToStringUni(nativeErrorString);

                return result;
            }

            static public bool SetDefaultRegistry(string url,string password = "")
            {
                SerializeAdapter.AdapterError error = SerializeAdapter.AdapterError.NO_ERROR;
                IntPtr nativeErrorString = IntPtr.Zero;

                return KeyDatabase_setDefaultRegistry(url,password, ref nativeErrorString, ref error);
            }

            static public bool SetDefaultRegistry(string url, ref string errorString, ref SerializeAdapter.AdapterError error, string password = "")
            {
                IntPtr nativeErrorString = IntPtr.Zero;

                bool result = KeyDatabase_setDefaultRegistry(url, password, ref nativeErrorString, ref error);

                if (nativeErrorString != IntPtr.Zero)
                    errorString = Marshal.PtrToStringUni(nativeErrorString);

                return result;
            }

            static public T GetUserKey<T>(string key, string password="", bool onlyUserKey=false)
            {
                T result;
                if (!TryGetUserKey(key, out result, password, onlyUserKey))
                    throw new ArgumentException($"user-key does not exist or could not be parsed. key={key} type={typeof(T).Name}");
                return result;
            }

            static public bool TryGetUserKey<T>(string key, out T value, string password="", bool onlyUserKey=false)
            {
                var keyval = Marshal.PtrToStringUni(KeyDatabase_getUserKey(key, password, onlyUserKey));

                if (keyval == null)
                {
                    value = default(T);
                    return false;
                }

                return TryConvert(keyval, out value);
            }

            static public T GetDefaultUserKey<T>(string key, T defaultValue=default(T),string password = "", bool onlyUserKey = false)
            {
                T result;
                if (TryGetUserKey(key, out result, password, onlyUserKey))
                    return result;

                return defaultValue;
            }

            static public T GetGlobalKey<T>(string key, string password = "")
            {
                T result;
                if (!TryGetGlobalKey(key, out result, password))
                    throw new ArgumentException($"global-key does not exist or could not be parsed. key={key} type={typeof(T).Name}");
                return result;
            }

            static public bool TryGetGlobalKey<T>(string key, out T value, string password = "")
            {
                var keyval = Marshal.PtrToStringUni(KeyDatabase_getGlobalKey(key, password));

                if (keyval == null)
                {
                    value = default(T);
                    return false;
                }

                return TryConvert(keyval, out value);
            }

            static public T GetDefaultGlobalKey<T>(string key, T defaultValue = default(T), string password = "")
            {
                T result;
                if (TryGetGlobalKey(key, out result, password))
                    return result;

                return defaultValue;
            }

            private static bool TryConvert<T>(string value, out T result)
            {
                TypeConverter converter = null;

                try
                {
                    converter = TypeDescriptor.GetConverter(typeof(T));

                    // try invariant conversion
                    result = (T)converter.ConvertFromInvariantString(value);
                    return true;
                }
                catch
                {
                    Message.Send(Message.GIZMOSDK, MessageLevel.WARNING, $"Failed to convert '{value}' in {nameof(TryConvert)}<{typeof(T).Name}>");
                }

                result = default(T);
                return false;
            }

            #region // --------------------- Native calls -----------------------

            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool KeyDatabase_setDefaultRegistry(string url, string password, ref IntPtr nativeErrorString, ref SerializeAdapter.AdapterError error);
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool KeyDatabase_setLocalRegistry(string url, string password, ref IntPtr nativeErrorString, ref SerializeAdapter.AdapterError error);
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

