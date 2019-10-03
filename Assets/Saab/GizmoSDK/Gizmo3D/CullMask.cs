//******************************************************************************
// File			: Node.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzNode class
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
        [Flags]
        public enum CullMaskValue
        {
            /// Your own definition
            CUSTOM = (1 << 0),

            /// Generic ground
            GROUND = (1 << 1),

            /// Generic water. Excluded from ground
            WATER = (1 << 2),

            /// Sum of all ground surface objects
            MAP_SURFACE = GROUND | WATER,

            /// Generic man made constructions
            BUILDING = (1 << 3),

            /// Generic Forest
            FOREST = (1 << 4),

            ALL = -1,
            NOTHING = 0
        };

        public interface ICullMask
        {
            void SetCullMask(CullMaskValue mask);
            CullMaskValue GetCullMask();
            bool IsCulled(CullMaskValue mask);
            bool IsCulled(ICullMask mask_iface);
        }
    }
}
