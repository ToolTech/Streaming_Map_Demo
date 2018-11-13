//******************************************************************************
// File			: Scene.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzScene class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.1
//		
// Copyright © 2003- Saab Training Systems AB, Sweden
//			
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows (Win32) and IRIX® for  
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

            public new static void UnInitializeFactory()
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
