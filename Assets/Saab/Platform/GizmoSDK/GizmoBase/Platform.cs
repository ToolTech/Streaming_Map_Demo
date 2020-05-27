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
// File			: Platforms.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzReference.cpp
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
// AMO	180301	Created file 	
// AMO  191120  Added platform defines to detect UNITY configurations (2.10.5)
//
//******************************************************************************

// -------------------- Check platform specifics ----------------------

// ------------------- UNITY ------------------------------------------

#if !UNIX && !UNIX64 && !WIN32 && !WIN64
// ----------------- We have no usual defines ------------
#if UNITY_ANDROID || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
// ----------------- We are a UNIX based system ----------
#if NATIVE_64
// ----------------- We are a 64 bits Unix ---------------
#define UNIX64
#else
// ----------------- We are a 32 bits Unix ---------------
#define UNIX
#endif
#else           
#if NATIVE_64
// ----------------- We are a 64 bits Windows ---------------
#define WIN64
#else
// ----------------- We are a 32 bits Windows ---------------
#define WIN32
#endif
#endif
#endif

// ------------------ End UNITY ---------------------------------------

using System;
using System.Runtime.InteropServices;

namespace GizmoSDK
{
    public class Platform
    {

#if WIN32  // ---------------- WinNT , 2000, XP  ---------------

#if NATIVE_DEBUG
        public const string GZ_LIB_EXT = "_d";
#else
        public const string GZ_LIB_EXT = "";
#endif

#elif WIN64  // ---------------- WinNT , 2000, XP  ---------------

#if NATIVE_DEBUG
        public const string GZ_LIB_EXT = "64_d";
#else
        public const string GZ_LIB_EXT = "64";
#endif


#elif UNIX  // ---------------- Unix systems  ---------------

#if NATIVE_DEBUG
        public const string GZ_LIB_EXT = "-g";
#else
        public const string GZ_LIB_EXT = "";
#endif

#elif UNIX64  // ---------------- Unix 64 bits systems  ---------------

#if NATIVE_DEBUG
        public const string GZ_LIB_EXT = "64-g";
#else
        public const string GZ_LIB_EXT = "64";
#endif

#else
        #error "No platform Definition WIN64,WIN32,UNIX64,ANDROID etc.."
#endif

    }

    namespace GizmoBase
    {
        /// <summary>
        /// Used to enable callbacks from Unity IL2CPP
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class MonoPInvokeCallbackAttribute : Attribute
        {
            public MonoPInvokeCallbackAttribute(Type type) { }
        }

        public class Platform
        {
            static public void InitializeFactories()
            {
                Module.InitializeFactory();
                Image.InitializeFactory();
            }

            static public void UninitializeFactories()
            {
                Module.UninitializeFactory();
                Image.UninitializeFactory();
            }

            public static bool Initialize()
            {
                bool result = Platform_initialize();

                if (result)
                {
                    InitializeFactories();
                    Message.Initialize();
                    DynamicEventReceiver.Initialize();
                }

                return result;
            }

            public static bool Uninitialize(bool forceShutdown = false)
            {
                DynamicEventReceiver.Uninitialize();
                Message.Uninitialize();

                UninitializeFactories();
                return Platform_uninitialize(forceShutdown);
            }

            public static string GetPlatformExtension()
            {
                return GizmoSDK.Platform.GZ_LIB_EXT;
            }

#if INTERNAL_LIB
            public const string BRIDGE = "__Internal";
#else
            public const string BRIDGE = "gzBaseBridge" + GizmoSDK.Platform.GZ_LIB_EXT;
#endif

#region Native dll interface ----------------------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Platform_initialize();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Platform_uninitialize(bool forceShutdown);
#endregion

        }
    }
    
}
