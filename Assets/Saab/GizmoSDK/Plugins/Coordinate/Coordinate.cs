//******************************************************************************
// File			: Coordinate.cs
// Module		: Coordinate C#
// Description	: C# Bridge to gzCoordinate class
// Author		: Anders Modén		
// Product		: Coordinate 2.9.1
//		
// Copyright © 2003- Saab Training Systems AB, Sweden
//			
// NOTE:	Coordinate is a platform abstraction utility layer for C++. It contains 
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
using GizmoSDK.GizmoBase;

namespace GizmoSDK
{
    namespace Coordinate
    {
        /// <summary>
        /// Coordinate conversion instance. Converts between geodetic, projected and geocentric coordinate systems
        /// </summary>
        public class Coordinate : Reference
        {
            public static double DEG2RAD = Math.PI / 180.0;
            public static double RAD2DEG = 180.0 / Math.PI;

            public static class Type
            {
                public static string LATPOS = "LatPos";
                public static string CARTPOS = "CartPos";
                public static string PROJPOS = "ProjPos";
                public static string UTMPOS = "UTMPos";
            }

            public Coordinate() : base(Coordinate_create()) { }
            public Coordinate(IntPtr nativeReference) : base(nativeReference) { }

            public void SetLatPos(LatPos pos, Datum datum = Datum.WGS84)
            {
                Coordinate_setLatPos(GetNativeReference(), pos, datum);
            }

            public bool GetLatPos(out LatPos pos, Datum datum = Datum.WGS84)
            {
                pos = new LatPos();

                return Coordinate_getLatPos(GetNativeReference(), ref pos, datum);
            }

            public void SetCartPos(CartPos pos, Datum datum = Datum.WGS84)
            {
                Coordinate_setCartPos(GetNativeReference(), pos, datum);
            }

            public bool GetCartPos(out CartPos pos, Datum datum = Datum.WGS84)
            {
                pos = new CartPos();

                return Coordinate_getCartPos(GetNativeReference(), ref pos, datum);
            }

            public void SetProjPos(ProjPos pos, FlatGaussProjection projection = FlatGaussProjection.RT90)
            {
                Coordinate_setProjPos(GetNativeReference(), pos, projection);
            }

            public bool GetProjPos(out ProjPos pos, FlatGaussProjection projection = FlatGaussProjection.RT90)
            {
                pos = new ProjPos();

                return Coordinate_getProjPos(GetNativeReference(), ref pos, projection);
            }

            public void SetUTMPos(UTMPos pos, Datum datum = Datum.WGS84)
            {
                Coordinate_setUTMPos(GetNativeReference(), pos, datum);
            }

            public bool GetUTMPos(out UTMPos pos, Datum datum = Datum.WGS84)
            {
                pos = new UTMPos();

                return Coordinate_getUTMPos(GetNativeReference(), ref pos, datum);
            }

            public void SetMGRS(string pos, Datum datum = Datum.WGS84)
            {
                Coordinate_setMGRSPos(GetNativeReference(), pos, datum);
            }

            public bool GetMGRS(out string pos, Datum datum = Datum.WGS84)
            {
                IntPtr result=Coordinate_getMGRSPos(GetNativeReference(), datum);

                if(result!=IntPtr.Zero)
                {
                    pos = Marshal.PtrToStringUni(result);
                    return true;
                }
                else
                {
                    pos = "";
                    return false;
                }
            }

            /// <summary>
            /// Returns a local orientation matrix for a specific LatPos position with east,north and up base vectors
            /// </summary>
            /// <param name="latpos">Geodetic position</param>
            /// <param name="ellipsoid"></param>
            /// <returns>Matrix of [east][north][up] vectors</returns>
            public Matrix3 GetOrientationMatrix(LatPos latpos, Ellipsoid ellipsoid = Ellipsoid.WGS84)
            {
                Matrix3 mat=new Matrix3();

                Coordinate_getOrientationMatrix_LatPos(GetNativeReference(), latpos, ellipsoid,ref mat);

                return mat;
            }

            /// <summary>
            /// Returns a local orientation matrix for a specific CartPos position with east,north and up base vectors
            /// </summary>
            /// <param name="cartpos"></param>
            /// <param name="ellipsoid"></param>
            /// <returns>Matrix of [east][north][up] vectors</returns>
            public Matrix3 GetOrientationMatrix(CartPos cartpos, Ellipsoid ellipsoid = Ellipsoid.WGS84)
            {
                Matrix3 mat = new Matrix3();

                Coordinate_getOrientationMatrix_CartPos(GetNativeReference(), cartpos, ellipsoid,ref mat);

                return mat;
            }

            #region -------------- Native calls ------------------

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Coordinate_create();
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Coordinate_setLatPos(IntPtr nativeReference,LatPos pos, Datum datum);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Coordinate_getLatPos(IntPtr nativeReference,ref LatPos pos, Datum datum);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Coordinate_setCartPos(IntPtr nativeReference, CartPos pos, Datum datum);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Coordinate_getCartPos(IntPtr nativeReference, ref CartPos pos, Datum datum);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Coordinate_setProjPos(IntPtr nativeReference, ProjPos pos, FlatGaussProjection projection);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Coordinate_getProjPos(IntPtr nativeReference, ref ProjPos pos, FlatGaussProjection projection);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Coordinate_setUTMPos(IntPtr nativeReference, UTMPos pos, Datum datum);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool Coordinate_getUTMPos(IntPtr nativeReference, ref UTMPos pos, Datum datum);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Coordinate_setMGRSPos(IntPtr nativeReference, string pos, Datum datum);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr Coordinate_getMGRSPos(IntPtr nativeReference, Datum datum);

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Coordinate_getOrientationMatrix_LatPos(IntPtr nativeReference, LatPos pos, Ellipsoid ellipsoid,ref Matrix3 matrix);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern void Coordinate_getOrientationMatrix_CartPos(IntPtr nativeReference, CartPos pos, Ellipsoid ellipsoid,ref Matrix3 matrix);



            #endregion
        }
    }
}

