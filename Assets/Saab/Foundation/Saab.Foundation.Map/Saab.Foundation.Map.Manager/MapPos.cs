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
// File			: MapPos.cs
// Module		: Saab.Foundation.Map.Manager
// Description	: Definition of the MapPos map position structure
// Author		: Anders Modén		
//		
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	180301	Created file 	
//
//******************************************************************************


using GizmoSDK.Gizmo3D;
using GizmoSDK.GizmoBase;

namespace Saab.Foundation.Map
{
    public struct MapPos
    {
        public RoiNode  roiNode;
        public Vec3D    position;
        public bool     clamped;
        public Vec3     normal;
        public Matrix3  local_orientation;      // East North Up
    }
}

