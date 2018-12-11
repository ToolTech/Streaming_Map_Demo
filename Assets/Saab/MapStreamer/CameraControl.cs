//*****************************************************************************
// File			: CameraControl.cs
// Module		:
// Description	: Simple Camera Movement from GizmoSDK
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
// AMO	180607	Created file        (2.9.1)
//
//******************************************************************************using UnityEngine;

// Unity Managed classes
using UnityEngine;

// Saab Stuff
using Saab.Core;
using GizmoSDK.GizmoBase;
 
namespace Saab.Unity.MapStreamer
{
    public class CameraControl : MonoBehaviour, IWorldCoord
    {

        public float speed = 20f;
        // Use this for initialization

        public float rotspeed = 20f;

        public double X = 0;
        public double Y = 0;
        public double Z = 0;

        public Vec3D Position
        {
            get
            {
                return new Vec3D(X, Y, -Z);     // Note that we translate Unitys Coordinate system into Gizmo3D
            }
        }

        private void MoveForward(float moveSpeed)
        {
            X = X + moveSpeed * UnityEngine.Time.deltaTime * transform.forward.x;
            Y = Y + moveSpeed * UnityEngine.Time.deltaTime * transform.forward.y;
            Z = Z + moveSpeed * UnityEngine.Time.deltaTime * transform.forward.z;
        }

        private void MoveRight(float moveSpeed)
        {
            X = X + moveSpeed * UnityEngine.Time.deltaTime * transform.right.x;
            Y = Y + moveSpeed * UnityEngine.Time.deltaTime * transform.right.y;
            Z = Z + moveSpeed * UnityEngine.Time.deltaTime * transform.right.z;
        }

        private Quaternion Tilt(float rotationSpeed)
        {
            return Quaternion.Euler(rotationSpeed * UnityEngine.Time.deltaTime, 0, 0);
        }

        private Quaternion Pan(float rotationSpeed)
        {
            return Quaternion.Euler(0, rotationSpeed * UnityEngine.Time.deltaTime, 0);
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

