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
// File			: Scene.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzScene class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.5
//		
//
//			
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows (Win32) and Android for  
//			usage in Game or VisSim development.
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GizmoSDK.GizmoBase;


namespace GizmoSDK
{
    namespace Gizmo3D
    {
        public class Scene : Group
        {
            public Scene(IntPtr nativeReference) : base(nativeReference) { }

            public Scene(string name="") : base(Scene_create(name)) { }

            public new static void InitializeFactory()
            {
                AddFactory(new Scene());
            }

            public new static void UninitializeFactory()
            {
                RemoveFactory("gzScene");
            }

            public override Reference Create(IntPtr nativeReference)
            {
                return new Scene(nativeReference) as Reference;
            }
            
            #region Native dll interface ---------------------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Scene_create(string name);
            
            #endregion
        }
    }
}
