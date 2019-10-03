//******************************************************************************
// File			: DynamicLoaderManager.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzDynamicLoaderManager class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.4
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
// AMO	181210	Created file 	
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
        public class DynamicLoaderManager
        {
            static public void StartManager(bool startAll=true, Byte manager_index =0)
            {
                DynamicLoaderManager_startManager(startAll, manager_index);
            }

            static public void StopManager(bool stopAll = true, Byte manager_index = 0)
            {
                DynamicLoaderManager_stopManager(stopAll, manager_index);
            }

            #region Native dll interface ----------------------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicLoaderManager_startManager(bool startAll, Byte manager_index);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DynamicLoaderManager_stopManager(bool startAll, Byte manager_index);
            #endregion
        }
    }
}
