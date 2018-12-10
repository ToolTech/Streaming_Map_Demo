//******************************************************************************
// File			: DistProperty.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistAttribute class
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

using System;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;

 

namespace GizmoSDK
{
    namespace GizmoDistribution
    {
        [System.AttributeUsage(System.AttributeTargets.Property| System.AttributeTargets.Field, AllowMultiple = false)]
        public class DistProperty : System.Attribute
        {
        }

        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
        public class DistPropertyAutoStore : System.Attribute
        {
        }

        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
        public class DistPropertyAutoRestore : System.Attribute
        {
        }
    }
}
