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
// File			: Matrix.cs
// Module		: GizmoBase C#
// Description	: C# Bridge to gzMatrix class
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
//
//******************************************************************************

using System.Runtime.InteropServices;
using System;

namespace GizmoSDK
{
    namespace GizmoBase
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Matrix3 
        {
            public float v11, v21, v31;
            public float v12, v22, v32;
            public float v13, v23, v33;

            public Matrix3(Vec3 column_0,Vec3 column_1,Vec3 column_2)
            {
                v11 = column_0.x; v12 = column_1.x; v13 = column_2.x;
                v21 = column_0.y; v22 = column_1.y; v23 = column_2.y;
                v31 = column_0.z; v32 = column_1.z; v33 = column_2.z;
            }

            public static Matrix3 Euler_YXZ(float y_rot,float x_rot,float z_rot)
            {
                Matrix3 mat = new Matrix3();

                Matrix3_euler_yxz(ref mat, y_rot, x_rot, z_rot);

                return mat;
            }

            public static Matrix3 Euler_ZXY(float z_rot, float x_rot, float y_rot)
            {
                Matrix3 mat = new Matrix3();

                Matrix3_euler_zxy(ref mat, z_rot, x_rot, y_rot);

                return mat;
            }

            public Quaternion Quaternion()
            {
                Quaternion quat = new Quaternion();

                Matrix3_quaternion(ref this,ref quat);

                return quat;
            }

            public Vec3 GetCol(int column)
            {
                switch(column)
                {
                    case 0: return new Vec3(v11, v21, v31);
                    case 1: return new Vec3(v12, v22, v32);
                    case 2: return new Vec3(v13, v23, v33);
                }

                throw (new Exception("Column index is outside range (0-2)"));
            }

            public static Matrix3 operator *(Matrix3 m1, Matrix3 m2)
            {
                Matrix3 retval = new Matrix3
                {
                    v11 = m1.v11 * m2.v11 + m1.v12 * m2.v21 + m1.v13 * m2.v31,
                    v12 = m1.v11 * m2.v12 + m1.v12 * m2.v22 + m1.v13 * m2.v32,
                    v13 = m1.v11 * m2.v13 + m1.v12 * m2.v23 + m1.v13 * m2.v33,
                   
                    v21 = m1.v21 * m2.v11 + m1.v22 * m2.v21 + m1.v23 * m2.v31,
                    v22 = m1.v21 * m2.v12 + m1.v22 * m2.v22 + m1.v23 * m2.v32,
                    v23 = m1.v21 * m2.v13 + m1.v22 * m2.v23 + m1.v23 * m2.v33,
                   
                    v31 = m1.v31 * m2.v11 + m1.v32 * m2.v21 + m1.v33 * m2.v31,
                    v32 = m1.v31 * m2.v12 + m1.v32 * m2.v22 + m1.v33 * m2.v32,
                    v33 = m1.v31 * m2.v13 + m1.v32 * m2.v23 + m1.v33 * m2.v33
                };

                return retval;
            }

            public static Vec3 operator * (Matrix3 m,Vec3 v)
            {
                Vec3 retval = new Vec3
                {
                    x = m.v11 * v.x + m.v12 * v.y + m.v13 * v.z,
                    y = m.v21 * v.x + m.v22 * v.y + m.v23 * v.z,
                    z = m.v31 * v.x + m.v32 * v.y + m.v33 * v.z
                };

                return retval;
            }

            public bool Inverse(out Matrix3 destination)
            {
                destination = new Matrix3();
                return Matrix3_inverse(ref this,ref destination);
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(Matrix3_asString(ref this));
            }


            #region // --------------------- Native calls -----------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Matrix3_inverse(ref Matrix3 source,ref Matrix3 dest);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Matrix3_euler_yxz(ref Matrix3 mat, float y_rot, float x_rot, float z_rot);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Matrix3_euler_zxy(ref Matrix3 mat, float z_rot, float x_rot, float y_rot);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Matrix3_asString(ref Matrix3 mat);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Matrix3_quaternion(ref Matrix3 mat,ref Quaternion quat);
            #endregion

        }

        public enum FaceMatrixMode
        {
            CW  = 0x0900,
            CCW = 0x0901,
            
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Matrix4
        {
            public float v11, v21, v31, v41;
            public float v12, v22, v32, v42;
            public float v13, v23, v33, v43;
            public float v14, v24, v34, v44;

            public Matrix4(Vec4 column_0, Vec4 column_1, Vec4 column_2,Vec4 column_3)
            {
                v11 = column_0.x; v12 = column_1.x; v13 = column_2.x; v14 = column_3.x;
                v21 = column_0.y; v22 = column_1.y; v23 = column_2.y; v24 = column_3.y;
                v31 = column_0.z; v32 = column_1.z; v33 = column_2.z; v34 = column_3.z;
                v41 = column_0.w; v42 = column_1.w; v43 = column_2.w; v44 = column_3.w;
            }


            public Vec4 GetCol(int column)
            {
                switch (column)
                {
                    case 0: return new Vec4(v11, v21, v31,v41);
                    case 1: return new Vec4(v12, v22, v32,v42);
                    case 2: return new Vec4(v13, v23, v33,v43);
                    case 3: return new Vec4(v14, v24, v34,v44);
                }

                throw (new Exception("Column index is outside range (0-3)"));
            }

            public static Matrix4 operator *(Matrix4 m1, Matrix4 m2)
            {
                Matrix4 retval = new Matrix4
                {
                    v11 = m1.v11 * m2.v11 + m1.v12 * m2.v21 + m1.v13 * m2.v31 + m1.v14 * m2.v41,
                    v12 = m1.v11 * m2.v12 + m1.v12 * m2.v22 + m1.v13 * m2.v32 + m1.v14 * m2.v42,
                    v13 = m1.v11 * m2.v13 + m1.v12 * m2.v23 + m1.v13 * m2.v33 + m1.v14 * m2.v43,
                    v14 = m1.v11 * m2.v14 + m1.v12 * m2.v24 + m1.v13 * m2.v34 + m1.v14 * m2.v44,

                    v21 = m1.v21 * m2.v11 + m1.v22 * m2.v21 + m1.v23 * m2.v31 + m1.v24 * m2.v41,
                    v22 = m1.v21 * m2.v12 + m1.v22 * m2.v22 + m1.v23 * m2.v32 + m1.v24 * m2.v42,
                    v23 = m1.v21 * m2.v13 + m1.v22 * m2.v23 + m1.v23 * m2.v33 + m1.v24 * m2.v43,
                    v24 = m1.v21 * m2.v14 + m1.v22 * m2.v24 + m1.v23 * m2.v34 + m1.v24 * m2.v44,

                    v31 = m1.v31 * m2.v11 + m1.v32 * m2.v21 + m1.v33 * m2.v31 + m1.v34 * m2.v41,
                    v32 = m1.v31 * m2.v12 + m1.v32 * m2.v22 + m1.v33 * m2.v32 + m1.v34 * m2.v42,
                    v33 = m1.v31 * m2.v13 + m1.v32 * m2.v23 + m1.v33 * m2.v33 + m1.v34 * m2.v43,
                    v34 = m1.v31 * m2.v14 + m1.v32 * m2.v24 + m1.v33 * m2.v34 + m1.v34 * m2.v44,

                    v41 = m1.v41 * m2.v11 + m1.v42 * m2.v21 + m1.v43 * m2.v31 + m1.v44 * m2.v41,
                    v42 = m1.v41 * m2.v12 + m1.v42 * m2.v22 + m1.v43 * m2.v32 + m1.v44 * m2.v42,
                    v43 = m1.v41 * m2.v13 + m1.v42 * m2.v23 + m1.v43 * m2.v33 + m1.v44 * m2.v43,
                    v44 = m1.v41 * m2.v14 + m1.v42 * m2.v24 + m1.v43 * m2.v34 + m1.v44 * m2.v44
                };

                return retval;
            }

            public static Vec4 operator *(Matrix4 m, Vec4 v)
            {
                Vec4 retval = new Vec4
                {
                    x = m.v11 * v.x + m.v12 * v.y + m.v13 * v.z + m.v14 * v.w,
                    y = m.v21 * v.x + m.v22 * v.y + m.v23 * v.z + m.v24 * v.w,
                    z = m.v31 * v.x + m.v32 * v.y + m.v33 * v.z + m.v34 * v.w,
                    w = m.v41 * v.x + m.v42 * v.y + m.v43 * v.z + m.v44 * v.w
                };

                return retval;
            }

            public static Vec3 operator *(Matrix4 m, Vec3 v)
            {
                float w = m.v41 * v.x + m.v42 * v.y + m.v43 * v.z + m.v44;

                Vec3 retval = new Vec3
                {
                    x = (m.v11 * v.x + m.v12 * v.y + m.v13 * v.z + m.v14)/w,
                    y = (m.v21 * v.x + m.v22 * v.y + m.v23 * v.z + m.v24)/w,
                    z = (m.v31 * v.x + m.v32 * v.y + m.v33 * v.z + m.v34)/w,
                };

                return retval;
            }

            public bool Inverse(out Matrix4 destination)
            {
                destination = new Matrix4();
                return Matrix4_inverse(ref this, ref destination);
            }

            public Vec3 Scale()
            {
                Vec3 result = new Vec3();

                Matrix4_scale(ref this, ref result);

                return result;
            }

            public Vec3 Translation()
            {
                return new Vec3(v14, v24, v34);
            }

            public Quaternion Quaternion()
            {
                Quaternion quat = new Quaternion();

                Matrix4_quaternion(ref this, ref quat);

                return quat;
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(Matrix4_asString(ref this));
            }

            public static Matrix4 Euler_YXZ(float y_rot, float x_rot, float z_rot, Vec3 translation = default)
            {
                Matrix4 mat = new Matrix4();

                Matrix4_euler_yxz(ref mat, y_rot, x_rot, z_rot,ref translation);

                return mat;
            }

            public static Matrix4 Euler_ZXY(float z_rot, float x_rot, float y_rot,Vec3 translation=default)
            {
                Matrix4 mat = new Matrix4();

                Matrix4_euler_zxy(ref mat, z_rot, x_rot, y_rot,ref translation);

                return mat;
            }

            

            #region // --------------------- Native calls -----------------------
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Matrix4_inverse(ref Matrix4 source, ref Matrix4 dest);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Matrix4_asString(ref Matrix4 mat);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Matrix4_euler_yxz(ref Matrix4 mat, float y_rot, float x_rot, float z_rot,ref Vec3 translation);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Matrix4_euler_zxy(ref Matrix4 mat, float z_rot, float x_rot, float y_rot,ref Vec3 translation);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Matrix4_scale(ref Matrix4 mat, ref Vec3 translation);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Matrix4_quaternion(ref Matrix4 mat, ref Quaternion quat);
            #endregion

        }

    }
}

