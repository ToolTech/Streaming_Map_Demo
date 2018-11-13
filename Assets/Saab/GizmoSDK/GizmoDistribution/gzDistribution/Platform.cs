//******************************************************************************
// File			: Platforms.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge PLatform
// Author		: Anders Modén		
// Product		: GizmoDistribution 2.10.1
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
    namespace GizmoDistribution
    {
        public class Platform
        {
            static public void InitializeFactories()
            {
                DistEvent.InitializeFactory();
                DistObject.InitializeFactory();
            }

            static public void UninitializeFactories()
            {
                DistEvent.UninitializeFactory();
                DistObject.UninitializeFactory();
            }

            public static bool Initialize()
            {
                bool result = Platform_initialize();

                if (result)
                    InitializeFactories();

                return result;
            }

            public static bool Uninitialize(bool forceShutdown = false, bool shutdownBase = false)
            {
                UninitializeFactories();
                return Platform_uninitialize(forceShutdown, shutdownBase);
            }

#if INTERNAL_LIB
            public const string BRIDGE = "__Internal";
            public const string GZ_DYLIB_REMOTE="__Internal";
#else
            public const string BRIDGE = "gzDistributionBridge" + GizmoSDK.Platform.GZ_LIB_EXT;
            public const string GZ_DYLIB_REMOTE = "gzRemoteDistributionBridge" + GizmoSDK.Platform.GZ_LIB_EXT;
#endif

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Platform_initialize();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Platform_uninitialize(bool forceShutdown, bool shutdownBase);
            #endregion
        }
    }
}
