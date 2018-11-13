//******************************************************************************
// File			: Platforms.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzReference.cpp
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
        public const string GZ_LIB_EXT="_d.dll";
#else
        public const string GZ_LIB_EXT=".dll";
#endif

#elif WIN64  // ---------------- WinNT , 2000, XP  ---------------
#if NATIVE_DEBUG
        public const string GZ_LIB_EXT = "64_d.dll";
#else
        public const string GZ_LIB_EXT="64.dll";
#endif


#elif UNIX  // ---------------- Unix systems  ---------------
#if NATIVE_DEBUG
        public const string GZ_LIB_EXT = "-g.so";
#else
        public const string GZ_LIB_EXT=".so";
#endif

#elif UNIX64  // ---------------- Unix 64 bits systems  ---------------
#if NATIVE_DEBUG
        public const string GZ_LIB_EXT = "64-g.so";
#else
        public const string GZ_LIB_EXT="64.so";
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

            static public void UnInitializeFactories()
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

            public static bool UnInitialize(bool forceShutdown = false)
            {
                Message.UnInitialize();

                UnInitializeFactories();
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
