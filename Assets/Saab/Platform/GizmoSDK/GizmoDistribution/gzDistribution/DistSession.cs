//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s)
// herein belong to SAAB AB. The program(s) may be used and/or
// copied only with the written permission of Saab AB, or in
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
// File			: DistSession.cs
// Module		: GizmoDistribution C#
// Description	: C# Bridge to gzDistSession class
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
// AMO  181210  Added Concurrent reading of dictionary
//
//******************************************************************************

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;


namespace GizmoSDK
{
    namespace GizmoDistribution
    {
        public class DistSession : Reference 
        {
            public DistSession(IntPtr nativeReference) : base(nativeReference)
            {
                ReferenceDictionary<DistSession>.AddObject(this);
            }
            
            override public void Release()
            {
                ReferenceDictionary<DistSession>.RemoveObject(this);
                base.Release();
            }

            public string GetName()
            {
                return Marshal.PtrToStringUni(DistSession_getName(GetNativeReference()));
            }

            public DistObject FindObject(string objectName)
            {
                var res = DistSession_findObject(GetNativeReference(), objectName);
                return ReferenceDictionary<DistObject>.GetObject(res);
            }

            #region --------------------------------- private --------------------------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistSession_getName(IntPtr session_reference);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr DistSession_findObject(IntPtr session_reference, string object_name);


            #endregion
        }
    }
}
