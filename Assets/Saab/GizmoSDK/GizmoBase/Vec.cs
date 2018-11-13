//******************************************************************************
// File			: Vec.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzVec class
// Author		: Anders Modén		
// Product		: GizmoBase 2.10.1
//		
// Copyright © 2003- Saab Training Systems AB, Sweden
//			
// NOTE:	GizmoBase is a platform abstraction utility layer for C++. It contains 
//			design patterns and C++ solutions for the advanced programmer.
//
//
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	180301	Created file 	
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        [Serializable]
        public struct Vec2 
        {
            public Vec2(float _x = 0, float _y = 0)
            {
                x = _x;
                y = _y;
            }

            public static Vec2 operator +(Vec2 a, Vec2 b)
            {
                return new Vec2(a.x + b.x, a.y + b.y);
            }

            public static Vec2 operator -(Vec2 a, Vec2 b)
            {
                return new Vec2(a.x - b.x, a.y - b.y);
            }

            public static Vec2 operator *(float a, Vec2 b)
            {
                return new Vec2(a * b.x, a * b.y);
            }

            public static Vec2 operator -(Vec2 b)
            {
                return new Vec2(-b.x, -b.y);
            }

            public float x, y;

            public static implicit operator Vec2D(Vec2 a)
            {
                return new Vec2D(a.x, a.y);
            }

            public void normalize()
            {
                var l = (float)Math.Sqrt(x * x + y * y);
                x /= l;
                y /= l;
            }

            public float length()
            {
                return (float)Math.Sqrt(x * x + y * y);
            }
        }
        [Serializable]
        public struct Vec3
        {
            public Vec3(float _x = 0, float _y = 0, float _z = 0)
            {
                x = _x;
                y = _y;
                z = _z;
            }

            public static Vec3 operator +(Vec3 a, Vec3 b)
            {
                return new Vec3(a.x + b.x, a.y + b.y,a.z+b.z);
            }

            public static Vec3 operator -(Vec3 a, Vec3 b)
            {
                return new Vec3(a.x - b.x, a.y - b.y,a.z-b.z);
            }

            public static Vec3 operator *(float a, Vec3 b)
            {
                return new Vec3(a * b.x, a * b.y, a * b.z);
            }

            public static Vec3 operator -(Vec3 b)
            {
                return new Vec3(-b.x, -b.y, -b.z);
            }

            public float x, y, z;

            public void normalize()
            {
                var l = (float)Math.Sqrt(x * x + y * y + z * z);
                x /= l;
                y /= l;
                z /= l;
            }

            public float length()
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(Vec3_asString(ref this));
            }

            public static implicit operator Vec3D(Vec3 a)
            {
                return new Vec3D(a.x, a.y, a.z);
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Vec3_asString(ref Vec3 vec);
        }

        [Serializable]
        public struct Vec4
        {
            public Vec4(float _x = 0, float _y = 0, float _z = 0,float _w=0)
            {
                x = _x;
                y = _y;
                z = _z;
                w = _w;
            }

            public static Vec4 operator +(Vec4 a, Vec4 b)
            {
                return new Vec4(a.x + b.x, a.y + b.y, a.z + b.z,a.w+b.w);
            }

            public static Vec4 operator -(Vec4 a, Vec4 b)
            {
                return new Vec4(a.x - b.x, a.y - b.y, a.z - b.z,a.w-b.w);
            }

            public static Vec4 operator *(float a, Vec4 b)
            {
                return new Vec4(a * b.x, a * b.y, a * b.z, a * b.w);
            }

            public static Vec4 operator -(Vec4 b)
            {
                return new Vec4(-b.x, -b.y,-b.z,-b.w);
            }

            public float x, y, z, w;

            public void normalize()
            {
                var l = (float)Math.Sqrt(x * x + y * y + z * z + w * w);
                x /= l;
                y /= l;
                z /= l;
                w /= l;
            }

            public float length()
            {
                return (float)Math.Sqrt(x * x + y * y + z * z + w * w);
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(Vec4_asString(ref this));
            }

            public static implicit operator Vec4D(Vec4 a)
            {
                return new Vec4D(a.x, a.y, a.z, a.w);
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Vec4_asString(ref Vec4 vec);
        }

        [Serializable]
        public struct Vec2D
        {
            public Vec2D(double _x = 0, double _y = 0)
            {
                x = _x;
                y = _y;
            }

            public static Vec2D operator +(Vec2D a, Vec2D b)
            {
                return new Vec2D(a.x + b.x, a.y + b.y);
            }

            public static Vec2D operator -(Vec2D a, Vec2D b)
            {
                return new Vec2D(a.x - b.x, a.y - b.y);
            }

            public static Vec2D operator *(double a, Vec2D b)
            {
                return new Vec2D(a * b.x, a * b.y);
            }

            public static explicit operator Vec2(Vec2D a)
            {
                return new Vec2((float)a.x, (float)a.y);
            }

            public static Vec2D operator -(Vec2D b)
            {
                return new Vec2D(-b.x, -b.y);
            }

            public double x, y;

            public void normalize()
            {
                var l = Math.Sqrt(x * x + y * y);
                x /= l;
                y /= l;
            }

            public double length()
            {
                return Math.Sqrt(x * x + y * y);
            }
        }

        [Serializable]
        public struct Vec3D
        {
            public Vec3D(double _x = 0, double _y = 0, double _z = 0)
            {
                x = _x;
                y = _y;
                z = _z;
            }

            public static Vec3D operator +(Vec3D a, Vec3D b)
            {
                return new Vec3D(a.x + b.x, a.y + b.y, a.z + b.z);
            }

            public static Vec3D operator -(Vec3D a, Vec3D b)
            {
                return new Vec3D(a.x - b.x, a.y - b.y, a.z - b.z);
            }

            public static Vec3D operator *(double a, Vec3D b)
            {
                return new Vec3D(a * b.x, a * b.y, a * b.z);
            }

            public static Vec3D operator -(Vec3D b)
            {
                return new Vec3D(-b.x, -b.y, -b.z);
            }

            public static explicit operator Vec3(Vec3D a)
            {
                return new Vec3((float)a.x, (float)a.y, (float)a.z);
            }

            public double x, y, z;

            public void normalize()
            {
                var l = Math.Sqrt(x * x + y * y + z * z);
                x /= l;
                y /= l;
                z /= l;

            }

            public double length()
            {
                return Math.Sqrt(x * x + y * y + z * z);
            }
        }

        [Serializable]
        public struct Vec4D
        {
            public Vec4D(double _x = 0, double _y = 0, double _z = 0, double _w = 0)
            {
                x = _x;
                y = _y;
                z = _z;
                w = _w;
            }

            public static Vec4D operator +(Vec4D a, Vec4D b)
            {
                return new Vec4D(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
            }

            public static Vec4D operator -(Vec4D a, Vec4D b)
            {
                return new Vec4D(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
            }

            public static Vec4D operator *(double a,Vec4D b)
            {
                return new Vec4D(a * b.x, a * b.y, a * b.z, a * b.w);
            }

            public static Vec4D operator -(Vec4D b)
            {
                return new Vec4D(-b.x, -b.y, -b.z, -b.w);
            }

            public static explicit operator Vec4(Vec4D a)
            {
                return new Vec4((float)a.x, (float)a.y, (float)a.z, (float)a.w);
            }

            public double x, y, z, w;

            public void normalize()
            {
                var l = Math.Sqrt(x * x + y * y + z * z + w * w);
                x /= l;
                y /= l;
                z /= l;
                w /= l;
            }

            public double length()
            {
                return Math.Sqrt(x * x + y * y + z * z + w * w);
            }
        }
    }
}

