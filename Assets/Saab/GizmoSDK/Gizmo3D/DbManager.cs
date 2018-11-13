//******************************************************************************
// File			: DbManager.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzDbManager class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.1
//		
// Copyright © 2003- Saab Training Systems AB, Sweden
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
        public class DbManager
        {
            [Flags]
            public enum AdapterFlags
            {
                DEFAULT = SerializeAdapter.AdapterFlags.DEFAULT,

            }

           
            static public Node LoadDB(string url, string extension="", AdapterFlags flags=0, UInt32 version=0, string password="", Reference associatedData=null)
            {
                return Reference.CreateObject((DbManager_loadDB(url,extension,flags,version, password,associatedData?.GetNativeReference() ?? IntPtr.Zero))) as Node;
            }
            
            static public void Initialize()
            {
                DbManager_initialize();
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DbManager_loadDB(string url, string extension , AdapterFlags flags, UInt32 version ,  string password , IntPtr associatedData);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DbManager_initialize();
        }
    }
}
