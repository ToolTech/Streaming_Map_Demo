//******************************************************************************
// File			: DynamicTypeProperty.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzDynamicType property classes
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

using System;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;

 

namespace GizmoSDK
{
    namespace GizmoBase
    {
        [System.AttributeUsage(System.AttributeTargets.Property| System.AttributeTargets.Field, AllowMultiple = false)]
        public class DynamicTypeProperty : System.Attribute
        {
        }
               
        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
        public class DynamicTypePropertyAutoStore : System.Attribute
        {
        }

        [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
        public class DynamicTypePropertyAutoRestore : System.Attribute
        {
        }
    }
}
