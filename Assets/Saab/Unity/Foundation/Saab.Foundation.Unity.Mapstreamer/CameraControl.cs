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
// File			: CameraControl.cs
// Module		:
// Description	: manages camera updates
// Author		: Anders Modén
// Product		: GizmoBase 2.10.5
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
// AMO	180607	Created file                                        (2.9.1)
//
//******************************************************************************

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************

//#define TEST_ROTATION   // Just test some default rotation

using GizmoSDK.GizmoBase;
using Saab.Utility.Unity.NodeUtils;
using System.Collections.Generic;
using UnityEngine;

using Quaternion = UnityEngine.Quaternion;



namespace Saab.Foundation.Unity.MapStreamer
{
    public class CameraControl : MonoBehaviour , ISceneManagerCamera
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

        public Camera Camera
        {
            get
            {
                return GetComponent<Camera>();
            }
        }

        public Vec3D Position
        {
            get { return Coordinate; }
            set
            {
                X = value.x;
                Y = value.y;
                Z = value.z;
            }
        }

        public Vector3 Up
        {
            get { return Vector3.up; }
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
            System.Numerics.Quaternion.CreateFromYawPitchRoll(0, 0, 0);
            return Quaternion.Euler(rotationSpeed * UnityEngine.Time.unscaledDeltaTime, 0, 0);
        }

        private Quaternion Pan(float rotationSpeed)
        {
            return Quaternion.Euler(0, rotationSpeed * UnityEngine.Time.unscaledDeltaTime, 0);
        }

            // Update is called once per frame
        void Update()
        {
            try
            {
                Performance.Enter("CameraControl.Update");
                // Check mouse click

                if (Input.GetButtonDown("Fire1"))
                {
                    Map.MapPos mapPos;

                    if (Map.MapControl.SystemMap.GetScreenGroundPosition((int)Input.mousePosition.x, (int)(Screen.height - Input.mousePosition.y), (uint)Screen.width, (uint)Screen.height, out mapPos, Map.ClampFlags.DEFAULT))
                    {
                        List<GameObject> list;

                        if (NodeUtils.FindGameObjects(mapPos.node.GetNativeReference(), out list))
                        {
                            foreach (GameObject o in list)
                            {
                                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                                sphere.transform.parent = o.transform;
                                sphere.transform.transform.localPosition = new Vector3((float)mapPos.position.x, (float)mapPos.position.y, (float)mapPos.position.z);
                                sphere.transform.localScale = new Vector3(10, 10, 10);
                            }
                        }
                    }


                }

                if (Input.GetButtonDown("Fire2"))
                {
                    //GizmoSDK.Coordinate.LatPos latpos = new GizmoSDK.Coordinate.LatPos
                    //{
                    //    Altitude = 245.52585220821,
                    //    Latitude = 1.00778345058085,
                    //    Longitude = 0.251106492463706

                    //};

                    //Map.MapPos mappos;

                    //if (Map.MapControl.SystemMap.GetPosition(latpos, out mappos, Map.GroundClampType.GROUND, Map.ClampFlags.WAIT_FOR_DATA))
                    //{
                    //    Debug.Log("Hit Ground ok");
                    //}

                    Performance.DumpPerformanceInfo();
                }

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

#if TEST_ROTATION
                rot = Pan(-rotspeed) * rot;
#endif

                transform.rotation = rot;

            }
            finally
            {
                Performance.Leave();
            }
        }

        public void PreTraverse()
        {
            // Called before traverser runs
        }

        public void PostTraverse()
        {
            // Called after all nodes have updated their transforms
        }

        public void MapChanged()
        {
            // Called when global map has changed
        }
    }
}

