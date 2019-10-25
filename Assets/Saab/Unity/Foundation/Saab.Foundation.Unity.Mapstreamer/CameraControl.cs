﻿//******************************************************************************
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
// File			: CameraControl.cs
// Module		:
// Description	: manages camera updates
// Author		: Anders Modén
// Product		: Gizmo3D 2.10.1
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
// AMO	180607	Created file                                        (2.9.1)
//
//******************************************************************************


using Saab.Core;
using GizmoSDK.GizmoBase;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer
{
    public class CameraControl : MonoBehaviour , IWorldCoord
    {

        public float speed = 20f;
        // Use this for initialization

        public float rotspeed = 20f;

        public double X = 0;
        public double Y = 0;
        public double Z = 0;

        public Vec3D Coordinate
        {
            get
            {
                return new Vec3D(X, Y, Z);
            }
        }

        private void MoveForward(float moveSpeed)
        {
            X = X + moveSpeed * UnityEngine.Time.unscaledDeltaTime * transform.forward.x;
            Y = Y + moveSpeed * UnityEngine.Time.unscaledDeltaTime * transform.forward.y;
            Z = Z + moveSpeed * UnityEngine.Time.unscaledDeltaTime * transform.forward.z;
        }

        private void MoveRight(float moveSpeed)
        {
            X = X + moveSpeed * UnityEngine.Time.unscaledDeltaTime * transform.right.x;
            Y = Y + moveSpeed * UnityEngine.Time.unscaledDeltaTime * transform.right.y;
            Z = Z + moveSpeed * UnityEngine.Time.unscaledDeltaTime * transform.right.z;
        }

        private Quaternion Tilt(float rotationSpeed)
        {
            return Quaternion.Euler(rotationSpeed * UnityEngine.Time.unscaledDeltaTime, 0, 0);
        }

        private Quaternion Pan(float rotationSpeed)
        {
            return Quaternion.Euler(0, rotationSpeed * UnityEngine.Time.unscaledDeltaTime, 0);
        }

        // Update is called once per frame
        void Update()
        {

            //transform.position;

            if (Input.GetKey("w"))
            {
                MoveForward(speed);
            }
            if (Input.GetKey("s"))
            {
                MoveForward(-speed);
            }

            

            if (Input.GetKey("d"))
            {
                MoveRight(speed);
            }

            if (Input.GetKey("a"))
            {
                MoveRight(-speed);
            }

           


            //transform.position = pos;

            Quaternion rot = transform.rotation;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                rot = rot * Tilt(rotspeed);
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                rot = rot * Tilt(-rotspeed);
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                rot = Pan(-rotspeed) * rot;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                rot = Pan(rotspeed) * rot;
            }


            transform.rotation = rot;


        }
    }
}

