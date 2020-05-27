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
// Module		: GizmoDistribution C#
// Description	: C# Bridge PLatform
// Author		: Anders Modén		
// Product		: GizmoDistribution 2.10.6
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
                bool result = GizmoBase.Platform.Initialize();

                if(result)
                    result = Platform_initialize();

                if (result)
                {
                    InitializeFactories();

                    DistClient.Initialize_();
                }

                return result;
            }

            public static bool Uninitialize(bool forceShutdown = false, bool shutdownBase = false)
            {
                DistClient.Uninitialize_();

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
