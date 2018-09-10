using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Saab.Unity.MapStreamer
{
    public class CameraControl : MonoBehaviour
    {

        public float speed = 20f;
        // Use this for initialization

        public float rotspeed = 20f;

        public double X = 0;
        public double Y = 0;
        public double Z = 0;

        private void MoveForward(float moveSpeed)
        {
            X = X + moveSpeed * Time.deltaTime * transform.forward.x;
            Y = Y + moveSpeed * Time.deltaTime * transform.forward.y;
            Z = Z + moveSpeed * Time.deltaTime * transform.forward.z;
        }

        private void MoveRight(float moveSpeed)
        {
            X = X + moveSpeed * Time.deltaTime * transform.right.x;
            Y = Y + moveSpeed * Time.deltaTime * transform.right.y;
            Z = Z + moveSpeed * Time.deltaTime * transform.right.z;
        }

        private Quaternion Tilt(float rotationSpeed)
        {
            return Quaternion.Euler(rotationSpeed * Time.deltaTime, 0, 0);
        }

        private Quaternion Pan(float rotationSpeed)
        {
            return Quaternion.Euler(0, rotationSpeed * Time.deltaTime, 0);
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

