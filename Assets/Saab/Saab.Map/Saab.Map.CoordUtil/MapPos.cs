//******************************************************************************
// File			: MapPos.cs
// Module		: Saab.Map.CoordUtil
// Description	: Map Position in the CoordUtil toolkit
// Author		: Anders Modén		
//		
// Copyright © 2003- Saab Training Systems AB, Sweden
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

namespace Saab.Map.CoordUtil
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

