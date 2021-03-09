﻿//******************************************************************************
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
// File			: Node.cs
// Module		: Gizmo3D C#
// Description	: C# Bridge to gzNode class
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.7
//		
//
//			
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows, Android, iOS and HoloLens for  
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
