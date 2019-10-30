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
using Saab.Utility.Map;

namespace Saab.Foundation.Map
{
    public struct MapPos : ILocation<Node>
    {
        public Node     node;                   // Local Context
        public Vec3D    position;               // Relative position to context
        public bool     clamped;                // true if this position is clamped
        public Vec3     normal;                 // Normal i local coordinate system
        public Matrix3  local_orientation;      // East North Up

        public Node Context { get { return node; } }
        public Float3 Offset
        {
            get
            {
                return new Float3()
                {
                    X = (float)position.x,
                    Y = (float)position.y,
                    Z = (float)position.z,
                };
            }
        }
    }
}

