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
// File			: License.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzLicense class
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.4
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

            public static UInt16 GetFeatureKey(string feature="")
            {
                return License_getFeatureKey(feature);
            }

            #region // --------------------- Native calls -----------------------
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt64 License_splashLicenseText(string header,string text,UInt64 id);
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt64 License_getMachineID();
            [DllImport(GizmoSDK.GizmoBase.Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern UInt16 License_getFeatureKey(string feature);
            #endregion
        }



        

       
    }
}

