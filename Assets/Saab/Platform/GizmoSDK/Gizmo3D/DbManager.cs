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
// File			: DbManager.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzDbManager class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.6
//		
//
//			
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows, Android, iOS and HoloLens for  
//			usage in Game or VisSim development.
//
//
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	180301	Created file 	
// AMO  200505  Added some db flags for retry loading db        (2.10.5)
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
            public enum AdapterFlags : UInt64
            {
                FLIP_FLIPPED_IMAGES     = Image.AdapterFlags.FLIP_FLIPPED_IMAGES,

                USE_ANIMATION           = 1 << (0 + (int)SerializeAdapter.AdapterFlags.FLAG_MAX_SIZE + (int)Image.AdapterFlags.FLAG_MAX_SIZE),

                RETRY_WAIT_LOAD         = 1 << (34 + (int)SerializeAdapter.AdapterFlags.FLAG_MAX_SIZE + (int)Image.AdapterFlags.FLAG_MAX_SIZE),
                DYN_LOAD_RETRY_WAIT     = 1 << (35 + (int)SerializeAdapter.AdapterFlags.FLAG_MAX_SIZE + (int)Image.AdapterFlags.FLAG_MAX_SIZE),
               

                FLAG_MAX_SIZE = 36,

                DEFAULT = DYN_LOAD_RETRY_WAIT,
            }


            static public Node LoadDB(string url, string extension="", AdapterFlags flags=AdapterFlags.DEFAULT, UInt32 version=0, string password="", Reference associatedData=null)
            {
                SerializeAdapter.AdapterError error = SerializeAdapter.AdapterError.NO_ERROR;
                IntPtr nativeErrorString = IntPtr.Zero;

                return Reference.CreateObject(DbManager_loadDB(url,extension,ref flags,version, password, associatedData?.GetNativeReference() ?? IntPtr.Zero,ref nativeErrorString,ref error)) as Node;
            }

            static public Node LoadDB(string url, ref string errorString, ref SerializeAdapter.AdapterError error,string extension = "", AdapterFlags flags = AdapterFlags.DEFAULT, UInt32 version = 0, string password = "", Reference associatedData = null)
            {
                IntPtr nativeErrorString = IntPtr.Zero;

                IntPtr node=DbManager_loadDB(url, extension, ref flags, version, password, associatedData?.GetNativeReference() ?? IntPtr.Zero, ref nativeErrorString, ref error);

                if (nativeErrorString != IntPtr.Zero)
                    errorString = Marshal.PtrToStringUni(nativeErrorString);

                return Reference.CreateObject(node) as Node;
            }

            static public void Initialize()
            {
                DbManager_initialize();
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DbManager_loadDB(string url, string extension , ref AdapterFlags flags, UInt32 version ,  string password , IntPtr associatedData, ref IntPtr nativeErrorString, ref SerializeAdapter.AdapterError error);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DbManager_initialize();
        }
    }
}
