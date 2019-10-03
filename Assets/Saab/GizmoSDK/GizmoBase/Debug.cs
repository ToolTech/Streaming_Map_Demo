//******************************************************************************
// File			: Debug.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzDebug.cpp
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
using System;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        [Flags]
        public enum DebugFlags
        {
            OFF = 0,
            SHOW_ALL = -1,
        }
        public interface IDebugInterface
        {
            void Debug(DebugFlags features = DebugFlags.SHOW_ALL);
        }
       
    }
}
