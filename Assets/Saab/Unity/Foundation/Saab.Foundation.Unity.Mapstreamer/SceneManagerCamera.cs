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
// File			: SceneManagerCamera.cs
// Module		:
// Description	: Extensions to convert between GizmoSDK and Unity3D
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
// AMO	180607	Created file        (2.9.1)
//
//******************************************************************************

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GizmoSDK.Coordinate;
using GizmoSDK.GizmoBase;
using Saab.Foundation.Map;
using Saab.Unity.Extensions;
using Saab.Utility.Map;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer
{
    public delegate void Traverse();

    [RequireComponent(typeof(Camera))]
    public class SceneManagerCamera : MonoBehaviour, ISceneManagerCamera
    {
        public event Traverse OnPreTraverse;
        public event Traverse OnPostTraverse;

        private MapPos _position = new MapPos();

        public LatPos Latpos
        {
            get
            {
                LatPos res;
                MapControl.SystemMap.LocalToWorld(_position, out res);
                return res;
            }
            set
            {
                _position.SetLatPos(value.Latitude, value.Longitude, value.Altitude);
            }
        }

        public MapPos MapPosition
        {
            get { return _position; }
        }

        public Vector3 Up
        {
            get
            {
                return _position.local_orientation.GetCol(2).ToVector3();
            }
        }

        public Camera Camera
        {
            get
            {
                return gameObject.GetComponent<Camera>();
            }
        }

        public Vec3D GlobalPosition
        {
            get { return MapControl.SystemMap.LocalToGlobal(_position); }

            set
            {
                // only calling this function does not create the map pos correctly... local_orientation is not set
                _position = MapControl.SystemMap.GlobalToLocal(value);
            }
        }

        public virtual void PreTraverse()
        {
            _position.Step(0, default(LocationOptions));

            OnPreTraverse?.Invoke();
        }

        public virtual void PostTraverse()
        {
            OnPostTraverse?.Invoke();
        }

        public virtual void MapChanged()
        {

        }
    }
}
