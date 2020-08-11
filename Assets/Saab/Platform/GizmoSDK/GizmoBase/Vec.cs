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
// File			: Vec.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzVec class
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
// AMO	180301	Created file 	
// AMO	200311	Added string output of Vec2,Vec3,Vec4 and their double repr (2.10.5)
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

            public void Normalize()
            {
                var l = (float)Math.Sqrt (x * x + y * y );
                x /= l;
                y /= l;
            }

            public void Normalize(out float l)
            {
                l = (float)Math.Sqrt(x * x + y * y );
                x /= l;
                y /= l;
            }

            public float Length()
            {
                return (float)Math.Sqrt(x * x + y * y );
            }

            public static Vec2 Normalize(Vec2 v)
            {
                var result = v;
                result.Normalize();
                return result;
            }

            public static Vec2 Normalize(Vec2 v, out float l)
            {
                l = (float)Math.Sqrt(v.x * v.x + v.y * v.y);
                return new Vec2(v.x / l, v.y / l);
            }

            public static float Distance(ref Vec2 a, ref Vec2 b)
            {
                return (b - a).Length();
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(Vec2_asString(ref this));
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Vec2_asString(ref Vec2 vec);
        }
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct Vec3
        {
            public Vec3(float _x = 0, float _y = 0, float _z = 0)
            {
                x = _x;
                y = _y;
                z = _z;
            }

            static public float Dot(Vec3 a, Vec3 b)
            {
                return a.x* b.x + a.y * b.y + a.z * b.z;
            }

            static public Vec3 Cross(Vec3 a, Vec3 b)
            {
                return new Vec3(
                    a.y * b.z - a.z * b.y,
                    a.z * b.x - a.x * b.z,
                    a.x * b.y - a.y * b.x
                    );

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
            public static Vec3 operator *(Vec3 v, float f)
            {
                return new Vec3(f * v.x, f * v.y, f * v.z);
            }
            public static Vec3 operator /(Vec3 v, float f)
            {
                return new Vec3(v.x / f, v.y / f, v.z / f);
            }

            public static Vec3 operator -(Vec3 b)
            {
                return new Vec3(-b.x, -b.y, -b.z);
            }

            public float x, y, z;

            public void Normalize()
            {
                float l = (float)Math.Sqrt(x * x + y * y + z * z );
                x /= l;
                y /= l;
                z /= l;
            }

            public void Normalize(out float l)
            {
                l = (float)Math.Sqrt(x * x + y * y + z * z);
                x /= l;
                y /= l;
                z /= l;
            }

            public float Length()
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
            }

            public static Vec3 Normalize(Vec3 v)
            {
                var result = v;
                result.Normalize();
                return result;
            }

            public static Vec3 Normalize(Vec3 v, out float l)
            {
                l = (float)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z );
                return new Vec3(v.x / l, v.y / l, v.z / l);
            }

            public static float Distance(ref Vec3 a, ref Vec3 b)
            {
                return (b - a).Length();
            }

            public Vec3 Cross(Vec3 vector)
	        {
		        return new Vec3(y* vector.z - z* vector.y, z* vector.x - x* vector.z, x* vector.y - y* vector.x);
	        }

            public float Dot(Vec3 vector)
            {
                return x * vector.x + y * vector.y + z * vector.z;
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(Vec3_asString(ref this));
            }

            public static implicit operator Vec3D(Vec3 a)
            {
                return new Vec3D(a.x, a.y, a.z);
            }

            public static Vec3 Min(Vec3 a, Vec3 b)
            {
                return new Vec3(
                    a.x <= b.x ? a.x : b.x,
                    a.y <= b.y ? a.y : b.y,
                    a.z <= b.z ? a.z : b.z);
            }

            public static Vec3 Max(Vec3 a, Vec3 b)
            {
                return new Vec3(
                    a.x >= b.x ? a.x : b.x,
                    a.y >= b.y ? a.y : b.y,
                    a.z >= b.z ? a.z : b.z);
            }

            public static Vec3 Scale(Vec3 a, Vec3 b)
            {
                return new Vec3(a.x * b.x, a.y * b.y, a.z * b.z);
            }

            public static Vec3 Orthogonal(Vec3 self_,Vec3 base_)
            {
                Vec3_orthogonal(ref self_, ref base_);

                return self_;
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Vec3_asString(ref Vec3 vec);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Vec3_orthogonal(ref Vec3 vec,ref Vec3 base_);
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
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

            public void Normalize()
            {
                var l = (float)Math.Sqrt(x * x + y * y + z * z + w * w);
                x /= l;
                y /= l;
                z /= l;
                w /= l;
            }

            public void Normalize(out float l)
            {
                l = (float)Math.Sqrt(x * x + y * y + z * z + w * w);
                x /= l;
                y /= l;
                z /= l;
                w /= l;
            }

            public float Length()
            {
                return (float)Math.Sqrt(x * x + y * y + z * z + w * w);
            }

            public static Vec4 Normalize(Vec4 v)
            {
                var result = v;
                result.Normalize();
                return result;
            }

            public static Vec4 Normalize(Vec4 v, out float l)
            {
                l =(float) Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z + v.w * v.w);
                return new Vec4(v.x / l, v.y / l, v.z / l, v.w / l);
            }

            public static double Distance(ref Vec4 a, ref Vec4 b)
            {
                return (b - a).Length();
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
        [StructLayout(LayoutKind.Sequential)]
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

            public void Normalize()
            {
                var l = Math.Sqrt(x * x + y * y );
                x /= l;
                y /= l;
            }

            public void Normalize(out double l)
            {
                l = Math.Sqrt(x * x + y * y );
                x /= l;
                y /= l;
            }


            public double Length()
            {
                return Math.Sqrt(x * x + y * y );
            }

            public static Vec2D Normalize(Vec2D v)
            {
                var result = v;
                result.Normalize();
                return result;
            }

            public static Vec2D Normalize(Vec2D v, out double l)
            {
                l = Math.Sqrt(v.x * v.x + v.y * v.y);
                return new Vec2D(v.x / l, v.y / l);
            }

            public static double Distance(ref Vec2D a, ref Vec2D b)
            {
                return (b - a).Length();
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(Vec2D_asString(ref this));
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Vec2D_asString(ref Vec2D vec);
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct Vec3D
        {
            public static Vec3D Zero = default(Vec3D);
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

            public void Normalize()
            {
                var l = Math.Sqrt(x * x + y * y + z * z);
                x /= l;
                y /= l;
                z /= l;
            }

            public void Normalize(out double l)
            {
                l = Math.Sqrt(x * x + y * y + z * z);
                x /= l;
                y /= l;
                z /= l;
            }

            static public double Dot(Vec3D a, Vec3D b)
            {
                return a.x * b.x + a.y * b.y + a.z * b.z;
            }

            static public Vec3D Cross(Vec3D a, Vec3D b)
            {
                return new Vec3D(
                    a.y * b.z - a.z * b.y,
                    a.z * b.x - a.x * b.z,
                    a.x * b.y - a.y * b.x
                    );

            }

            public Vec3D Cross(Vec3D vector)
            {
                return new Vec3D(y * vector.z - z * vector.y, z * vector.x - x * vector.z, x * vector.y - y * vector.x);
            }

            public double Dot(Vec3D vector)
            {
                return x * vector.x + y * vector.y + z * vector.z;
            }

            public double Length()
            {
                return Math.Sqrt(x * x + y * y + z * z);
            }

            public static Vec3D Normalize(Vec3D v)
            {
                var result = v;
                result.Normalize();
                return result;
            }

            public static Vec3D Normalize(Vec3D v, out double l)
            {
                l = Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
                return new Vec3D(v.x / l, v.y / l, v.z / l);
            }

            public static Vec3D Lerp(ref Vec3D a, ref Vec3D b, double t)
            {
                t = Math.Min(1, Math.Max(0, t));

                return new Vec3D(
                    a.x + (b.x - a.x) * t,
                    a.y + (b.y - a.y) * t,
                    a.z + (b.z - a.z) * t);
            }

            public static double Distance(ref Vec3D a, ref Vec3D b)
            {
                return (b - a).Length();
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(Vec3D_asString(ref this));
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Vec3D_asString(ref Vec3D vec);
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
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

            public void Normalize()
            {
                var l = Math.Sqrt(x * x + y * y + z * z+w*w);
                x /= l;
                y /= l;
                z /= l;
                w /= l;
            }

            public void Normalize(out double l)
            {
                l = Math.Sqrt(x * x + y * y + z * z+w*w);
                x /= l;
                y /= l;
                z /= l;
                w /= l;
            }

            public double Length()
            {
                return Math.Sqrt(x * x + y * y + z * z+w*w);
            }

            public static Vec4D Normalize(Vec4D v)
            {
                var result = v;
                result.Normalize();
                return result;
            }

            public static Vec4D Normalize(Vec4D v, out double l)
            {
                l = Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z + v.w * v.w);
                return new Vec4D(v.x / l, v.y / l, v.z / l,v.w/l);
            }

            public static double Distance(ref Vec4D a, ref Vec4D b)
            {
                return (b - a).Length();
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(Vec4D_asString(ref this));
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Vec4D_asString(ref Vec4D vec);
        }
    }
}

