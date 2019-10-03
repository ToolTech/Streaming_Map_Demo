//******************************************************************************
// File			: Platforms.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzCoordinate.cpp
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
    namespace Coordinate
    {
        public class Platform
        {
            public static bool Initialize()
            {
                bool result = Platform_initialize();

                return result;
            }

            public static bool Uninitialize(bool forceShutdown = false)
            {
                return Platform_uninitialize(forceShutdown);
            }

#if INTERNAL_LIB
            public const string BRIDGE = "__Internal";
#else
            public const string BRIDGE = "gzCoordinateBridge" + GizmoSDK.Platform.GZ_LIB_EXT;
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
