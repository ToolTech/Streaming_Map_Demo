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
// File			: Structs.cs
// Module		: Coordinate C#
// Description	: C# Bridge to various structs
// Author		: Anders Modén		
// Product		: Coordinate 2.9.1
//		
//
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
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct LatPos
        {
            // NOTE: Important, order of fields must match the order of fields in gzLatPos
            [DynamicTypeProperty]
            public double Latitude;
            [DynamicTypeProperty]
            public double Longitude;
            [DynamicTypeProperty]
            public double Altitude;

            public LatPos(double latitude,double longitude,double altitude)
            {
                Latitude = latitude;
                Longitude = longitude;
                Altitude = altitude;
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(LatPos_asString(ref this));
            }

            public static implicit operator DynamicType(LatPos pos)
            {
                return new DynamicType(LatPos_create_dynamic(ref pos));
            }

            public static implicit operator LatPos(DynamicType data)
            {
                LatPos pos;
                if(!LatPos_create_pos(data.GetNativeReference(), out pos))
                    throw (new ArgumentException("DynamicType is not a LatPos", nameof(data)));

                return pos;
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr LatPos_asString(ref LatPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr LatPos_create_dynamic(ref LatPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool LatPos_create_pos(IntPtr native_reference,[Out] out LatPos pos);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CartPos
        {
            // NOTE: Important, order of fields must match the order of fields in gzCartPos
            [DynamicTypeProperty]
            public double X;
            [DynamicTypeProperty]
            public double Y;
            [DynamicTypeProperty]
            public double Z;

            public CartPos(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            

            public override string ToString()
            {
                return Marshal.PtrToStringUni(CartPos_asString(ref this));
            }

            public static implicit operator DynamicType(CartPos pos)
            {
                return new DynamicType(CartPos_create_dynamic(ref pos));
            }

            public static implicit operator CartPos(DynamicType data)
            {
                CartPos pos;
                if (!CartPos_create_pos(data.GetNativeReference(), out pos))
                    throw new ArgumentException("DynamicType is not a CartPos", nameof(data));

                return pos;
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr CartPos_asString(ref CartPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr CartPos_create_dynamic(ref CartPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool CartPos_create_pos(IntPtr native_reference, [Out] out CartPos pos);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ProjPos
        {
            // NOTE: Important, order of fields must match the order of fields in gzProjPos
            [DynamicTypeProperty]
            public double X;
            [DynamicTypeProperty]
            public double Y;
            [DynamicTypeProperty]
            public double H;

            public ProjPos(double x, double y, double h)
            {
                X = x;
                Y = y;
                H = h;
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(ProjPos_asString(ref this));
            }

            public static implicit operator DynamicType(ProjPos pos)
            {
                return new DynamicType(ProjPos_create_dynamic(ref pos));
            }

            public static implicit operator ProjPos(DynamicType data)
            {
                ProjPos pos;
                if (!ProjPos_create_pos(data.GetNativeReference(), out pos))
                    throw new ArgumentException("DynamicType is not a ProjPos", nameof(data));

                return pos;
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr ProjPos_asString(ref ProjPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr ProjPos_create_dynamic(ref ProjPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool ProjPos_create_pos(IntPtr native_reference, [Out] out ProjPos pos);

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UTMPos
        {
            // NOTE: Important, order of fields must match the order of fields in gzUTMPos
            [DynamicTypeProperty]
            public int Zone;


            [MarshalAs(UnmanagedType.U1)] // gzUTMPos uses gzBool, gzBool is defined as unsigned char
            [DynamicTypeProperty]
            public bool North;

            [DynamicTypeProperty]
            public double Northing;
            [DynamicTypeProperty]
            public double Easting;
            [DynamicTypeProperty]
            public double H;

            public UTMPos(int zone, bool north, double northing, double easting, double h)
            {
                Zone = zone;
                North = north;

                Northing = northing;
                Easting = easting;
                H = h;
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(UTMPos_asString(ref this));
            }

            public static implicit operator DynamicType(UTMPos pos)
            {
                return new DynamicType(UTMPos_create_dynamic(ref pos));
            }

            public static implicit operator UTMPos(DynamicType data)
            {
                UTMPos pos;
                if (!UTMPos_create_pos(data.GetNativeReference(), out pos))
                    throw new ArgumentException("DynamicType is not a UTMPos", nameof(data));

                return pos;
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr UTMPos_asString(ref UTMPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr UTMPos_create_dynamic(ref UTMPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool UTMPos_create_pos(IntPtr native_reference, [Out] out UTMPos pos);

        }

        public enum Type
        {
            GEOCENTRIC,
            GEODETIC,
            PROJECTED,
            UTM,
            MGRS,
            STATE_COUNT,
            NOT_DEFINED,
        };

        public enum Datum
        {
            WGS84_ELLIPSOID,            // Datums for ellipsoid height
            GRS80_ELLIPSOID,
            BESSEL_1841_ELLIPSOID,
            CLARKE_1866_ELLIPSOID,
            AIRY_1830_ELLIPSOID,

            WGS84_EGM2008,              // Additional datums for each alt model
            BESSEL_RR92,

            USER_DEFINED,
            NOT_DEFINED,
        }
        public enum Ellipsoid
        {
            WGS84,
            GRS80,
            BESSEL_1841,
            CLARKE_1866,
            AIRY_1830,

            USER_DEFINED,
        }

        public enum FlatGaussProjection
        {
            RT90,
            SWEREF99,
            UTM,

            USER_DEFINED,
            NOT_DEFINED,
        }

        public enum HeightModel
        {
            ELLIPSOID,
            EGM2008,
            RR92,
            EGM96,
            RH2000,
            RH70,
            NOT_DEFINED
        };


    }
}

