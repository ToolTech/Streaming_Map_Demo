//******************************************************************************
// File			: UserData.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzUserData class
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
        public abstract class UserData : Reference
        {
            public UserData(IntPtr nativeReference) : base(nativeReference) { }

            #region -------------- Native calls ------------------

             [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr UserData_create();
            

            #endregion
        }
    }
}

