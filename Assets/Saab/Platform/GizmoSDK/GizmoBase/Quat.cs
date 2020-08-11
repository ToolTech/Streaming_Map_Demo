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
// File			: Quat.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzQuaternion class
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.6
//		
//
//			
// NOTE:	GizmoBase is a platform abstraction utility layer for C++. It contains 
//			design patterns and C++ solutions for the advanced programmer.
//
//
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	191106	Created file 	(2.10.4)
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct Quaternion 
        {
            public Quaternion(float _w = 0, float _x=0,float _y=0,float _z = 0)
            {
                w = _w;

                x = _x; 
                y = _y;
                z = _z;
            }

            public Quaternion(float angle,Vec3 n,float norm=1)
            {
                float len = n.Length();

                if (len > 0)
                {
                    float s = norm * (float)Math.Sin(angle);

                    w = norm * (float)Math.Cos(angle);

                    x = n.x * s / len;
                    y = n.y * s / len;
                    z = n.z * s / len;
                }
                else
                {
                    w = x = y = z = 0;
                }
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(Quaternion_asString(ref this));
            }

            public static Quaternion CreateFromEulerYXZ(float heading,float pitch,float roll)
            {
                Quaternion quat = new Quaternion();

                Quaternion_from_euler_yxz(ref quat, heading, pitch, roll);

                return quat;
            }

            public static Quaternion operator*(Quaternion lhs, Quaternion rhs)
            {
                return new Quaternion(
                        lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z,
                        lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
                        lhs.w * rhs.y - lhs.x * rhs.z + lhs.y * rhs.w + lhs.z * rhs.x,
                        lhs.w * rhs.z + lhs.x * rhs.y - lhs.y * rhs.x + lhs.z * rhs.w);
            }


            public float w, x, y, z;

            #region // --------------------- Native calls -----------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Quaternion_asString(ref Quaternion quat);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Quaternion_from_euler_yxz(ref Quaternion quat,float heading, float pitch,float roll);

            #endregion

        }

        [Serializable]
        public struct QuaternionD
        {
            public QuaternionD(double _w = 0, double _x = 0, double _y = 0, double _z = 0)
            {
                w = _w;

                x = _x;
                y = _y;
                z = _z;
            }

            public QuaternionD(double angle, Vec3D n, double norm = 1)
            {
                double len = n.Length();

                if (len > 0)
                {
                    double s = norm * Math.Sin(angle);

                    w = norm * Math.Cos(angle);

                    x = n.x * s / len;
                    y = n.y * s / len;
                    z = n.z * s / len;
                }
                else
                {
                    w = x = y = z = 0;
                }
            }

            public static QuaternionD operator *(QuaternionD lhs, QuaternionD rhs)
            {
                return new QuaternionD(
                        lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z,
                        lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y,
                        lhs.w * rhs.y - lhs.x * rhs.z + lhs.y * rhs.w + lhs.z * rhs.x,
                        lhs.w * rhs.z + lhs.x * rhs.y - lhs.y * rhs.x + lhs.z * rhs.w);
            }


            public double w, x, y, z;


        }

    }
}

