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


namespace GizmoSDK
{
    namespace Coordinate
    {
        public class Platform
        {
#if INTERNAL_LIB
            public const string BRIDGE = "__Internal";
#else
            public const string BRIDGE = "gzCoordinateBridge" + GizmoSDK.Platform.GZ_LIB_EXT;
#endif
        }
    }
}
