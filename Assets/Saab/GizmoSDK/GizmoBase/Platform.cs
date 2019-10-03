//******************************************************************************
// File			: Platforms.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzReference.cpp
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.4
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
    #error "No platform Definition Win64,WIN32,UNIX64 etc.."
#endif

    }

    namespace GizmoBase
    {
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
                    InitializeFactories();

                Message.Initialize();

                return result;
            }

            public static bool Uninitialize(bool forceShutdown = false)
            {
                Message.Uninitialize();

                UninitializeFactories();
                return Platform_uninitialize(forceShutdown);
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
