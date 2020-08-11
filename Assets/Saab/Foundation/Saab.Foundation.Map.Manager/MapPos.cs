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
// File			: MapPos.cs
// Module		: Saab.Foundation.Map.Manager
// Description	: Definition of the MapPos map position structure
// Author		: Anders Modén		
// Product		: Gizmo3D 2.10.6
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
// AMO	180301	Created file 	
//
//******************************************************************************

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************


using GizmoSDK.Coordinate;
using GizmoSDK.Gizmo3D;
using GizmoSDK.GizmoBase;
using Saab.Utility.Map;
using Transform = Saab.Utility.Map.Transform;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;
using Matrix4x4 = System.Numerics.Matrix4x4;
using System;

namespace Saab.Foundation.Map
{
    //------------------ Coordinates -------------------------------------------------------------------------------------------------------------
    //
    // The map exposes three coordinate systems
    //
    // 1.   The World Coordinate System. All coodinates are in LatPos or CartPos double precision   (LatPos, CartPos)
    //      Always a geocentric system
    //
    // 2.   The Global coordinate system. All coordnates are in X,Y,Z RightON Handed                (MapPos Global)
    //      The interior database is stored in this level or transformed here                      
    //      Can be flat UTM, Flat Projected or Spherical Cartesian (or other) (uses _origin)
    //
    // 3.   The Local coordnate system.                                                             (MapPos Local)
    //      Has a local origin and local normals ENU (East, North,Up)
    //      In UTM its aligned with X to the east, Y up and Z to the south
    //      In Sperical its aligned with XYZ to Cartesian Coordnates
    //
    // 4.   The local ENU (East-North-Up) coordinate system.
    //      A MapPos local coordnate is given in a fixed 3D frame aligned with Unitys coordnate system
    //      but with flipped Z axis. In this the east,north,up vectors might be aligned in another direction
    //      A ENU transform can be performed on a local MapPos to get it to ENU MapPos              (MapPos ENU)
    //
    //  In Unity you have a Left ON system and with a Z into the screen so be careful using Unitys quarternios etc.
    //  Positioning is made under the Top RoiNode and it has a fliped Z transform so in under this node you dont need to take the flip into account
    //
    //-----------------------------------------------------------------------------------------------------------------------------------------------



    public class MapPos : IMapLocation<Node>
    {
        public Node             node;                   // Local Context, Can be null for Global context
        public Vec3D            position;               // Relative position to context in double precision
        public IntersectQuery   clamp_result;           // non zero if this position is clamped
        public Vec3             normal;                 // Normal in local coordinate system
        public Matrix3          local_orientation;      // East North Up base matrix
        protected Vec3            _euler_enu;              // Yaw, Pitch, Roll - euler angles around East,North,Up right ON
        public Vec3D            a, b, c;                // Place on triangle

        public bool IsLocal()
        {
            return node != null;
        }

        public bool IsGlobal()
        {
            return node == null;
        }

        public Node Context
        {
            get
            {
                return node;
            }
        }

        /// <summary>
        /// This position is a relative local position to the parent context (RoiNode etc) in float
        /// </summary>
        public Vector3 LocalPosition
        {
            get
            {
                if (IsLocal())
                    return new Vector3((float)position.x, (float)position.y, (float)position.z);
                else
                    throw new SystemException("Not a local position");
            }
        }
        /// <summary>
        /// This is the database global position
        /// </summary>
        /// <param name="enu_offset"></param>
        /// <returns></returns>
        public Vec3D GlobalPosition(Vec3 enu_offset = default)
        {
            Vec3D result = position;

            RoiNode roi = node as RoiNode;

            // If we are a roi node based position we add roi position as local origin

            if (roi != null && roi.IsValid())
            {
                result += roi.Position;
            }

            result += local_orientation * enu_offset;

            return result;
        }

        public Quaternion Rotation
        {
            get { return Quaternion.CreateFromYawPitchRoll(_euler_enu.x, _euler_enu.y, _euler_enu.z); }
        }

        public Matrix3 Orientation
        {
            get { return Matrix3.Euler_YXZ(_euler_enu.x, _euler_enu.y, _euler_enu.z); }
        }

        public Matrix3 EnuToLocal(LocationOptions options)
        {
            Vec3 up;                                    // up in local coordinate system

            if (normal.x != 0)                          // Use normal as up
                up = normal;
            else 
                up = local_orientation.GetCol(2);       // If no normal use 
           
            Vec3 east = local_orientation.GetCol(0);

            Vec3 north = local_orientation.GetCol(1);

            if (options.RotationOptions.HasFlag(RotationOptions.AlignToSurface))
            {
                east = Vec3.Orthogonal(east,up);
                north = up.Cross(east);
            }

            return new Matrix3(east, north, up);
        }


        public bool SetLatPos(double lat, double lon, double alt)
        {
            var mapControl = MapControl.SystemMap;

            if (mapControl == null)
            {
                return false;
            }

            return mapControl.SetPosition(this, new LatPos(lat,lon, alt));
        }

        public bool SetCartPos(double x, double y, double z)
        {
            var mapControl = MapControl.SystemMap;

            if (mapControl == null)
            {
                return false;
            }

            return mapControl.SetPosition(this, new CartPos(x, y, z));

        }

        public void SetRotation(float yaw, float pitch, float roll)
        {
            _euler_enu = new Vec3(yaw, pitch, roll);
        }

        virtual public Transform Step(double time, LocationOptions options)
        {
            Clamp(options);

            var up = normal;


            var east = local_orientation.GetCol(0);
            Vec3 north;

            if (options.RotationOptions.HasFlag(RotationOptions.AlignToSurface))
            {
                east = Vec3.Normalize(east - (Vec3.Dot(east, up) * up));
                north = Vec3.Cross(up, east);
            }
            else
            {
                north = local_orientation.GetCol(1);
            }

            // TODO: this seems wrong...
            var m = new Matrix3(east, up, -north);
            
            var heading = _euler_enu.x;
            var pitch = _euler_enu.y;
            var roll = _euler_enu.z;

            var local_rotation = GizmoSDK.GizmoBase.Quaternion.CreateFromEulerYXZ(heading, pitch, roll);

            var r = m.Quaternion() * local_rotation;

            return new Transform
            {
                Pos = { X = (float)position.x, Y = (float)position.y, Z = (float)position.z },
                Rot = new Quaternion(r.x, r.y, r.z, r.w)
            };
        }

        protected bool Clamp(LocationOptions options)
        {
            var mapControl = MapControl.SystemMap;
            if (mapControl == null)
            {
                return false;
            }

            var clampType = GroundClampType.NONE;
            if (options.PositionOptions == PositionOptions.Surface)
            {
                if (options.RotationOptions.HasFlag(RotationOptions.AlignToSurface))
                {
                    clampType = GroundClampType.GROUND_NORMAL_TO_SURFACE;
                }
                else
                {
                    clampType = GroundClampType.GROUND;
                }
            }

            var clampFlags = ClampFlags.DEFAULT;

            if (options.LoadOptions == LoadOptions.Load)
                clampFlags = ClampFlags.WAIT_FOR_DATA;

            if (options.QualityOptions == QualityOptions.Highest)
                clampFlags |= ClampFlags.ISECT_LOD_QUALITY;

            mapControl.UpdatePosition(this, clampType, clampFlags);

            return true;
        }
    }

    public class DynamicMapPos : MapPos, IDynamicLocation<Node>
    {
        private Vec3D _pos;
        private Vec3 _vel;
        private Vec3 _acc;
        private double _time;
        public double Timestamp => _time;
        public Vec3 Velocity => _vel;
        public Vec3 Acceleration => _acc;

        public bool SetKinematicParams(double posX, double posY, double posZ, Vector3 vel, Vector3 acc, double t)
        {
            if (!SetCartPos(posX, posY, posZ))
                return false;

            _pos = new Vec3D(posX, posY, posZ);

            _vel = new Vec3(vel.X, vel.Y, vel.Z);
            _acc = new Vec3(acc.X, acc.Y, acc.Z);
            _time = t;

            return true;
        }

        public override Transform Step(double time, LocationOptions options)
        {   
            var dt = (float)(time - _time);
            var s = _vel * dt + 0.5f * _acc * dt * dt;

            if(Vec3.Dot(s, s) == 0.0 )
            {
                return base.Step(time, options);
            }

            var newPos = _pos + s;
            SetCartPos(newPos.x, newPos.y, newPos.z);

            if (!options.RotationOptions.HasFlag(RotationOptions.AlignToVelocity))
                return base.Step(time, options);

            var dirWorld = _vel + _acc * dt;
            dirWorld.Normalize();

            var conv = new Coordinate();
            Matrix3 worldToENU;
            var ENUtoWorld = conv.GetOrientationMatrix(new CartPos(newPos.x, newPos.y, newPos.z));
            ENUtoWorld.Inverse(out worldToENU);

            var dirENU = worldToENU * dirWorld;

            //var up = ENUtoWorld.GetCol(2);
            //var north = ENUtoWorld.GetCol(1);

            //up = new Vec3(0, 0, 1);
            //north = new Vec3(0, 1, 0);

            //var projNorthEast = dirENU - Vec3.Dot(dirENU, up) * up;
            //var dot = Vec3.Dot(projNorthEast, north);
            //var det = Vec3.Dot(up, Vec3.Cross(north, projNorthEast));

            //_euler_enu.x = (float)Math.Atan2(det, dot);
            _euler_enu.x = -(float)Math.Atan2(dirENU.x, dirENU.y);


            return base.Step(time, options);
        }
    }

}

