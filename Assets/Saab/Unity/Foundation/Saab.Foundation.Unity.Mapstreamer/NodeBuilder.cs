//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s) 
// herein belong to Saab AB. The program(s) may be used and/or
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
// File			: NodeBuilder.cs
// Module		:
// Description	: Generic Builder Interface
// Author		: Anders Modén
// Product		: Gizmo3D 2.10.6
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
// ZJP	200625	Created file                                        (2.10.6)
//
//******************************************************************************

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************
using UnityEngine;
using GizmoSDK.Gizmo3D;

namespace Saab.Foundation.Unity.MapStreamer
{
    public enum BuildPriority
    {
        Immediate,
        Low,
    }

    public interface INodeBuilder
    {
        PoolObjectFeature Feature { get; }
        BuildPriority Priority { get; }
        bool CanBuild(Node node);
        bool Build(NodeHandle nodeHandle, GameObject gameObject, NodeHandle activeStateNode);
        void BuiltObjectReturnedToPool(GameObject gameObject);
    }
}
