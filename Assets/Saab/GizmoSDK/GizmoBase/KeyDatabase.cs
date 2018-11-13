//******************************************************************************
// File			: KeyDatabase.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzKeyDatabase class
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
// AMO	180816	Created file 	
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;

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

            static public string GetUserKey(string key, string password="", bool onlyUserKey=false)
            {
                return Marshal.PtrToStringUni(KeyDatabase_getUserKey(key, password, onlyUserKey));
            }

            static public string GetDefaultUserKey(string key, string defaultValue="",string password = "", bool onlyUserKey = false)
            {
                return Marshal.PtrToStringUni(KeyDatabase_getDefaultUserKey(key, defaultValue,password, onlyUserKey));
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

            #endregion
        }



        

       
    }
}

