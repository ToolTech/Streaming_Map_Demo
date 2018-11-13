//******************************************************************************
// File			: Structs.cs
// Module		: Coordinate C#
// Description	: C# Bridge to various structs
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
        public struct LatPos
        {
            public LatPos(double _latitude=0,double _longitude=0,double _altitude=0)
            {
                latitude = _latitude;
                longitude = _longitude;
                altitude = _altitude;
            }

            public double latitude;
            public double longitude;
            public double altitude;

            public override string ToString()
            {
                return Marshal.PtrToStringUni(LatPos_asString(this));
            }

            public static implicit operator DynamicType(LatPos pos)
            {
                return new DynamicType(LatPos_create_dynamic(pos));
            }

            public static implicit operator LatPos(DynamicType data)
            {
                LatPos pos = new LatPos();

                if(LatPos_create_pos(data.GetNativeReference(),ref pos))
                    return pos;
                else
                    throw (new Exception("DynamicType is not a LatPos"));
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr LatPos_asString(LatPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr LatPos_create_dynamic(LatPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool LatPos_create_pos(IntPtr native_reference,ref LatPos pos);
        }

        public struct CartPos
        {
            public CartPos(double _x = 0, double _y = 0, double _z = 0)
            {
                x = _x;
                y = _y;
                z = _z;
            }

            public double x;
            public double y;
            public double z;

            public override string ToString()
            {
                return Marshal.PtrToStringUni(CartPos_asString(this));
            }

            public static implicit operator DynamicType(CartPos pos)
            {
                return new DynamicType(CartPos_create_dynamic(pos));
            }

            public static implicit operator CartPos(DynamicType data)
            {
                CartPos pos = new CartPos();

                if (CartPos_create_pos(data.GetNativeReference(), ref pos))
                    return pos;
                else
                    throw (new Exception("DynamicType is not a CartPos"));
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr CartPos_asString(CartPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr CartPos_create_dynamic(CartPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool CartPos_create_pos(IntPtr native_reference, ref CartPos pos);
        }

        public struct ProjPos
        {
            public ProjPos(double _x = 0, double _y = 0, double _h = 0)
            {
                x = _x;
                y = _y;
                h = _h;
            }

            public double x;
            public double y;
            public double h;

            public override string ToString()
            {
                return Marshal.PtrToStringUni(ProjPos_asString(this));
            }

            public static implicit operator DynamicType(ProjPos pos)
            {
                return new DynamicType(ProjPos_create_dynamic(pos));
            }

            public static implicit operator ProjPos(DynamicType data)
            {
                ProjPos pos = new ProjPos();

                if (ProjPos_create_pos(data.GetNativeReference(), ref pos))
                    return pos;
                else
                    throw (new Exception("DynamicType is not a ProjPos"));
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr ProjPos_asString(ProjPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr ProjPos_create_dynamic(ProjPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool ProjPos_create_pos(IntPtr native_reference, ref ProjPos pos);

        }

        public struct UTMPos
        {
            public UTMPos(int _zone=0, bool _north=true,double _northing = 0, double _easting = 0, double _h = 0)
            {
                zone = _zone;
                north = _north;

                northing = _northing;
                easting = _easting;
                h = _h;
            }

            public int zone;
            public bool north;

            public double northing;
            public double easting;
            public double h;

            public override string ToString()
            {
                return Marshal.PtrToStringUni(UTMPos_asString(this));
            }

            public static implicit operator DynamicType(UTMPos pos)
            {
                return new DynamicType(UTMPos_create_dynamic(pos));
            }

            public static implicit operator UTMPos(DynamicType data)
            {
                UTMPos pos = new UTMPos();

                if (UTMPos_create_pos(data.GetNativeReference(), ref pos))
                    return pos;
                else
                    throw (new Exception("DynamicType is not a UTMPos"));
            }

            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr UTMPos_asString(UTMPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr UTMPos_create_dynamic(UTMPos pos);
            [DllImport(Platform.BRIDGE, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            private static extern bool UTMPos_create_pos(IntPtr native_reference, ref UTMPos pos);

        }

        public enum Datum
        {
            WGS84,
            GRS80,
            RR92,
            CLARKE_1866,
            USER_DEFINED,
        }
        public enum Ellipsoid
        {
            WGS84,
            GRS80,
            BESSEL_1841,
            CLARKE_1866,
            USER_DEFINED,
        }

        public enum FlatGaussProjection
        {
            RT90,
            SWEREF99,

            USER_DEFINED,
            NOT_DEFINED,
        };

    }
}

