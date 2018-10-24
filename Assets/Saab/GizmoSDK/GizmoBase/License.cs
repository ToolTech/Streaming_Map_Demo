//******************************************************************************
// File			: License.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzLicense class
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
using System;

namespace GizmoSDK
{
    namespace GizmoBase
    {
       
        public class License
        {
          
            public static UInt64 SplashLicenseText(string header,string text,UInt64 id=0)
            {
                return License_splashLicenseText(header, text, id);
            }

            public static UInt64 GetMachineID()
            {
                return License_getMachineID();
            }

            #region // --------------------- Native calls -----------------------
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt64 License_splashLicenseText(string header,string text,UInt64 id);
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt64 License_getMachineID();
            #endregion
        }



        

       
    }
}

