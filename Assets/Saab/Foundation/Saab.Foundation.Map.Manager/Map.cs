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
// File			: Manager.cs
// Module		: Saab.Foundation.Map.Manager
// Description	: Map Manager of maps in BTA
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
// AMO  191023  Fixed issue in double position in screenvectors
//
//******************************************************************************

using System;

using GizmoSDK.Gizmo3D;
using GizmoSDK.GizmoBase;
using GizmoSDK.Coordinate;
using Saab.Utility.Map;

namespace Saab.Foundation.Map
{
    public enum MapType
    {
        UNKNOWN,
        PLAIN,
        UTM,
        GEOCENTRIC
    }

    [Flags]
    public enum GroundClampType
    {
        NONE,
        GROUND,
        GROUND_NORMAL_TO_SURFACE,
        BUILDING,
    }

    [Flags]
    public enum ClampFlags
    {
        NONE                    = 0,
        WAIT_FOR_DATA           = 1<<0,
        ISECT_LOD_QUALITY       = 1<<1,
        FRUSTRUM_CULL           = 1<<2,
        UPDATE_DATA               = 1<<3,

        DEFAULT = FRUSTRUM_CULL,
    }

    public class MapControl
    {
        const string USER_DATA_DB_INFO              = "UserDataDbInfo";
        const string DBI_PROJECTION                 = "DbI-Projection";
        const string DBI_PROJECTION_UTM             = "UTM";
        const string DBI_PROJECTION_FLAT_EARTH      = "Flat Earth";
        const string DBI_PROJECTION_SPHERE          = "Sphere";
        const string DBI_ORIGIN                     = "DbI-Database Origin";
        const string DBI_MAX_LOD_RANGE			    = "DbI-LR";

        public MapControl()
        {
            Reset();
        }

        public void Reset()
        {
            try
            {
                NodeLock.WaitLockEdit();        // All change of map parameters shall be done in locked edit mode

                _mapType = MapType.UNKNOWN;
                _topRoi = null;
                _currentMap = null;
                _nodeURL = null;
            }
            finally
            {
                NodeLock.UnLock();
            }
        }

        #region ------------------------ Screen Functions --------------------------------------

        /// <summary>
        /// Get Global position and global direction for a screen coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size_x"></param>
        /// <param name="size_y"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool GetScreenVectors(int x,int y,uint size_x,uint size_y,out Vec3D position,out Vec3 direction)
        {
            position = new Vec3D();
            direction = new Vec3();

            if (_camera== null || !_camera.IsValid())
                return false;

            Camera.GetScreenVectors(x, y, size_x, size_y, out position, out direction);

            return true;
        }

        /// <summary>
        /// Get local ground position using intersector in screen coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size_x"></param>
        /// <param name="size_y"></param>
        /// <param name="result"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool GetScreenGroundPosition(int x, int y, uint size_x, uint size_y, out MapPos result, ClampFlags flags = ClampFlags.DEFAULT)
        {
            Vec3D position;
            Vec3 direction;


            if (!GetScreenVectors(x, y, size_x, size_y, out position, out direction))
            {
                result = null;
                return false;
            }

            return GetGroundPosition(position, direction, out result, flags);
        }

        #endregion

        #region ------------------------ Local/Global ------------------------------------------

        /// <summary>
        /// Convert a local mappos to a global 3D position with ENU offset
        /// </summary>
        /// <param name="mappos"></param>
        /// <param name="enu_offset"></param>
        /// <returns></returns>
        public Vec3D LocalToGlobal(MapPos mappos, Vec3 enu_offset = default)
        {
            return mappos.GlobalPosition(enu_offset);
        }

        public MapPos GlobalToLocal(Vec3D global_position)
        {
            MapPos result = new MapPos();

            result.position = global_position;

            result.local_orientation = GetLocalOrientation(global_position);

            ToLocal(result);

            return result;
        }

        #endregion

        #region ------------------------ Global/World ------------------------------------------

        public bool GlobalToWorld(Vec3D global_position, out LatPos result)
        {
            Coordinate converter = new Coordinate();

            if (!GlobalToWorld(converter, global_position))
            {
                result = null;
                return false;
            }

            return converter.GetLatPos(out result);
        }

        public bool GlobalToWorld(Vec3D global_position, out CartPos result)
        {
            Coordinate converter = new Coordinate();

            if (!GlobalToWorld(converter, global_position))
            {
                result = null;
                return false;
            }

            return converter.GetCartPos(out result);
        }

        private bool WorldToGlobal(Coordinate converter, ref Vec3D pos, ref Matrix3 orientationMatrix)
        {

            // Convert to global 3D coordinate in appropriate target system

            switch (_mapType)
            {
                case MapType.UTM:
                    {
                        UTMPos utmpos;

                        if (!converter.GetUTMPos(out utmpos))
                        {
                            Message.Send("Controller", MessageLevel.WARNING, "Failed to convert to UTM");
                            return false;
                        }

                        pos = new Vec3D(utmpos.Easting, utmpos.H, -utmpos.Northing) - _origin;

                    }
                    break;

                case MapType.GEOCENTRIC:
                    {
                        CartPos cartpos;

                        if (!converter.GetCartPos(out cartpos))
                        {
                            Message.Send("Controller", MessageLevel.WARNING, "Failed to convert to Geocentric");
                            return false;
                        }

                        pos = new Vec3D(cartpos.X, cartpos.Y, cartpos.Z) - _origin;
                    }
                    break;

                default:
                    return false;
            }

            // Set up orientation

            orientationMatrix = GetLocalOrientation(pos);

            return true;
        }

        public bool GlobalToWorld(Coordinate converter, Vec3D position)
        {
            switch (_mapType)
            {
                case MapType.UNKNOWN:
                    {
                        return false;
                    }

                case MapType.UTM:

                    UTMPos utmpos = new UTMPos(_utmZone, _north, -(position.z + _origin.z), position.x + _origin.x, position.y + _origin.y);

                    converter.SetUTMPos(utmpos);

                    break;

                case MapType.GEOCENTRIC:

                    CartPos cartpos = new CartPos(position.x + _origin.x, position.y + _origin.y, position.z + _origin.z);

                    converter.SetCartPos(cartpos);

                    break;
            }

            return true;
        }

        #endregion

        #region ------------------------ Local/World -------------------------------------------

        public bool LocalToWorld(MapPos pos, out LatPos result)
        {
            Vec3D position = pos.position;

            RoiNode roi = pos.node as RoiNode;

            if (roi != null && roi.IsValid())   // Convert to a global position
                position += roi.Position;

            return GlobalToWorld(position, out result);
        }

        public bool LocalToWorld(MapPos pos, out CartPos result)
        {
            Vec3D position = pos.position;

            RoiNode roi = pos.node as RoiNode;

            if (roi != null && roi.IsValid())   // Convert to a global position
                position += roi.Position;

            return GlobalToWorld(position, out result);
        }

        #endregion

        #region ------------------------ Intersector Queries -----------------------------------

        /// <summary>
        /// Get a Local mappos in Global ray direction
        /// </summary>
        /// <param name="global_position"></param>
        /// <param name="direction"></param>
        /// <param name="result"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool GetGroundPosition(Vec3D global_position, Vec3 direction, out MapPos result, ClampFlags flags = ClampFlags.DEFAULT)
        {
            result = new MapPos();

            // Coordinate is now in world Cartesian coordinates (Roi Position)

            Vec3D origo = new Vec3D(0, 0, 0);       // Set used origo for clamp operation

            Intersector isect = new Intersector();

            // Check camera frustrum -----------------------------------------------------------

            if (_camera != null && _camera.IsValid())
            {
                origo = _camera.Position;

                if ((flags & ClampFlags.FRUSTRUM_CULL) != 0)
                    isect.SetCamera(_camera);
            }

            // Adjust intersector to use origo as center
            global_position = global_position - origo;

            isect.SetStartPosition((Vec3)global_position);
            isect.SetDirection(direction);

            if (isect.Intersect(_currentMap, IntersectQuery.ABC_TRI | IntersectQuery.NEAREST_POINT | IntersectQuery.NORMAL | (flags.HasFlag(ClampFlags.WAIT_FOR_DATA) ? IntersectQuery.WAIT_FOR_DYNAMIC_DATA : 0), 1, true, origo))
            {
                IntersectorResult res = isect.GetResult();

                IntersectorData data = res.GetData(0);

                result.position = data.coordinate + origo;

                if ((data.resultMask & IntersectQuery.NORMAL) != 0)
                    result.normal = data.normal;

                if (data.resultMask.HasFlag(IntersectQuery.ABC_TRI))
                {
                    result.a = data.a + origo;
                    result.b = data.b + origo;
                    result.c = data.c + origo;
                }

                result.clamp_result = data.resultMask;
            }
            else
                result.clamp_result = IntersectQuery.NULL;

            isect.Dispose();   // Drop handle and ignore GC

            result.local_orientation = GetLocalOrientation(result.position);

            if (result.clamp_result == IntersectQuery.NULL)
                result.normal = result.local_orientation.GetCol(2);

            return ToLocal(result);
        }

        public double GetAltitude(LatPos pos, ClampFlags flags = ClampFlags.DEFAULT)
        {
            MapPos mapPos;

            if (!GetPosition(pos, out mapPos, GroundClampType.GROUND, flags))
                return 0;

            LatPos updatedPos;

            if (!LocalToWorld(mapPos, out updatedPos))
                return 0;

            return updatedPos.Altitude;
        }

        public static bool Intersect(Vec3D origin, Vec3D direction, Vec3D V0, Vec3D V1, Vec3D V2, out Vec3D p)
        {
            p = new Vec3D();

            Vec3D E1 = V1 - V0;
            Vec3D E2 = V2 - V0;

            Vec3D P = direction.Cross(E2);

            double factor = P.Dot(E1);

            if (factor >= 0.0)
            {
                if (factor < 1e-8)
                    return false;

                Vec3D T = origin - V0;

                double u = P.Dot(T);

                if (u < 0 || u > factor)
                    return false;

                Vec3D Q = T.Cross(E1);

                double v = Q.Dot(direction);

                if (v < 0 || (u + v) > factor)
                    return false;

                double mag = Q.Dot(E2) / factor;

                if (mag < 0)
                    return false;

                p = origin + mag * direction;
            }
            else
            {
                if (factor > -1e-8)
                    return false;

                Vec3D T = origin - V0;

                double u = P.Dot(T);

                if (u > 0 || u < factor)
                    return false;

                Vec3D Q = T.Cross(E1);

                double v = Q.Dot(direction);

                if (v > 0 || (u + v) < factor)
                    return false;

                double mag = Q.Dot(E2) / factor;

                if (mag < 0)
                    return false;

                p = origin + mag * direction;
            }

            return true;
        }

        public bool UpdatePosition(MapPos result, GroundClampType groundClamp = GroundClampType.NONE, ClampFlags flags = ClampFlags.DEFAULT)
        {
            if (_currentMap == null)    // No map
                return false;

            if (groundClamp != GroundClampType.NONE)
            {

                // Add ROINode position as offset   - Go to global 3D coordinate system as we need to clamp in global 3D

                RoiNode roi = result.node as RoiNode;

                if (roi != null && roi.IsValid())
                    result.position += roi.Position;

                // The defined down vector

                Vec3 down = new Vec3(-result.local_orientation.v13, -result.local_orientation.v23, -result.local_orientation.v33);


                // Check triangel ground

                if (result.clamp_result.HasFlag(IntersectQuery.ABC_TRI))
                {
                    if (Intersect(result.position, down, result.a, result.b, result.c, out Vec3D p))
                    {
                        result.position = p;
                        ToLocal(result);

                        return true;
                    }
                }

                // Check new intersector


                Intersector isect = new Intersector();

                Vec3D origo = new Vec3D(0, 0, 0);

                // Check camera frustrum -----------------------------------------------------------

                if (_camera != null && _camera.IsValid())
                {
                    origo = _camera.Position;

                    if ((flags & ClampFlags.FRUSTRUM_CULL) != 0)
                        isect.SetCamera(_camera);
                }

                if ((flags & ClampFlags.ISECT_LOD_QUALITY) != 0)                // Lets stand in the ray to get highest quality
                    origo = result.position;


                isect.SetStartPosition((Vec3)(result.position - origo) - 10000.0f * down);  // Move backwards
                isect.SetDirection(down);

                if (isect.Intersect(_currentMap, IntersectQuery.NEAREST_POINT | IntersectQuery.ABC_TRI |
                                                    IntersectQuery.NORMAL |
                                                    (flags.HasFlag(ClampFlags.WAIT_FOR_DATA) ? IntersectQuery.WAIT_FOR_DYNAMIC_DATA : 0) |
                                                    (flags.HasFlag(ClampFlags.UPDATE_DATA) ? IntersectQuery.UPDATE_DYNAMIC_DATA : 0)
                                                    , 1, true, origo))
                {
                    IntersectorResult res = isect.GetResult();

                    IntersectorData data = res.GetData(0);

                    result.position = data.coordinate + origo;

                    if ((data.resultMask & IntersectQuery.NORMAL) != 0)
                        result.normal = data.normal;

                    if (data.resultMask.HasFlag(IntersectQuery.ABC_TRI))
                    {
                        result.a = data.a + origo;
                        result.b = data.b + origo;
                        result.c = data.c + origo;
                    }

                    result.clamp_result = data.resultMask;
                }
                else
                    result.clamp_result = IntersectQuery.NULL;

                if (groundClamp == GroundClampType.GROUND)
                {
                    result.normal = result.local_orientation.GetCol(2);
                }


                isect.Dispose();    // Drop handle and ignore GC

                // Remove ROINode position as offset - Go to local coordinate system under ROI Node

                ToLocal(result);
            }


            return true;
        }

        #endregion



        /// <summary>
        /// Get local ENU orientation matrix for Global Position Vec3D
        /// </summary>
        /// <param name="global_position"></param>
        /// <returns></returns>
        public Matrix3 GetLocalOrientation(Vec3D global_position)
        {
            Coordinate converter = new Coordinate();

            switch (_mapType)
            {
                default:
                    {
                        return new Matrix3();
                    }

                case MapType.UTM:

                    return new Matrix3(new Vec3(1, 0, 0), new Vec3(0, 0, -1), new Vec3(0, 1, 0));    // East North Up vectors


                case MapType.GEOCENTRIC:

                    return converter.GetOrientationMatrix(new CartPos(global_position.x+_origin.x,global_position.y+_origin.y, global_position.z+_origin.z));
            }
        }

        /// <summary>
        /// Get local ENU orientation matrix for LatPos
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Matrix3 GetLocalOrientation(LatPos pos)
        {
            Coordinate converter = new Coordinate();
            converter.SetLatPos(pos);

            switch (_mapType)
            {
                default:
                    {
                        return new Matrix3();
                    }

                case MapType.UTM:

                    return new Matrix3(new Vec3(1, 0, 0), new Vec3(0, 0, -1), new Vec3(0, 1, 0));    // East North Up vectors


                case MapType.GEOCENTRIC:

                    CartPos cartpos;

                    if (!converter.GetCartPos(out cartpos))
                    {
                        Message.Send("Controller", MessageLevel.WARNING, "Failed to convert to Geocentric");
                        return new Matrix3();
                    }

                    return  converter.GetOrientationMatrix(cartpos);
            }
        }

        

       

        /// <summary>
        /// Converts a global mappos to a local under a roi
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool ToLocal(MapPos result)
        {
            if (result.node != null)        // Already local
                return true;

            if (_topRoi == null)
                return false;
            
            RoiNode roi = _topRoi.GetClosestRoiNode(result.position);
            result.node = roi;

            // Remove roiNode position as offset - Go to local RoiNode based coordinate system

            if (roi != null && roi.IsValid())
            {
                result.position -= roi.Position;
                
                return true;
            }
            return false;   // We failed to convert
        }

        /// <summary>
        /// Converts a local mappos under a roi to a global position
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool ToGlobal(MapPos result)
        {
            if (result.node == null)    // Already global
                return true;

            // Remove roiNode position as offset - Go to local RoiNode based coordinate system

            RoiNode roi = result.node as RoiNode;

            if (roi != null && roi.IsValid())
            {
                result.position += roi.Position;

                result.node = null;

                return true;
            }

            return false;   // We failed to convert
        }


        public bool SetPosition(MapPos result, LatPos pos, GroundClampType groundClamp = GroundClampType.NONE, ClampFlags flags = ClampFlags.DEFAULT)
        {
            Coordinate converter = new Coordinate();
            converter.SetLatPos(pos);

            if (!WorldToGlobal(converter, ref result.position, ref result.local_orientation))
                return false;

            ToLocal(result);

            return UpdatePosition(result, groundClamp, flags);
        }

        public bool SetPosition(MapPos result, CartPos pos, GroundClampType groundClamp = GroundClampType.NONE, ClampFlags flags = ClampFlags.DEFAULT)
        {
            Coordinate converter = new Coordinate();
            converter.SetCartPos(pos);

            if (!WorldToGlobal(converter, ref result.position, ref result.local_orientation))
                return false;

            // Check possibly local 3D under a roiNode

            ToLocal(result);


            return UpdatePosition(result, groundClamp, flags);
        }
             

        public bool GetPosition(LatPos pos, out MapPos result,GroundClampType groundClamp= GroundClampType.NONE, ClampFlags flags = ClampFlags.DEFAULT)
        {
            result = new MapPos
            {
                local_orientation = new Matrix3(new Vec3(1, 0, 0), new Vec3(0, 0, -1), new Vec3(0, 1, 0))    // East North Up vectors
            };

            return SetPosition(result, pos, groundClamp, flags);                 
        }

        
        public Vec3D Origin
        {
            get
            {
                try
                {
                    NodeLock.WaitLockEdit();

                    return _origin;
                }
                finally
                {
                    NodeLock.UnLock();
                }
            }
        }

        public Node CurrentMap
        {
            get
            {
                try
                {
                    NodeLock.WaitLockEdit();
                    return _currentMap;
                }
                finally
                {
                    NodeLock.UnLock();
                }
            }

            set
            {
                try
                {
                    NodeLock.WaitLockEdit();

                    _currentMap = value;

                    if (value != null && value.IsValid())
                    {
                        var projection = _currentMap.GetAttribute(USER_DATA_DB_INFO, DBI_PROJECTION);

                        if (projection == DBI_PROJECTION_UTM)
                        {
                            _mapType = MapType.UTM;
                            UTMPos utmOrigin = _currentMap.GetAttribute(USER_DATA_DB_INFO, DBI_ORIGIN);
                            _origin = new Vec3D(utmOrigin.Easting, utmOrigin.H, -utmOrigin.Northing);
                            _north = utmOrigin.North;
                            _utmZone = utmOrigin.Zone;
                        }
                        else if (projection == DBI_PROJECTION_FLAT_EARTH)   // To run a 3D world without world coordinates
                        {
                            _mapType = MapType.PLAIN;
                            _origin = (Vec3D)_currentMap.GetAttribute(USER_DATA_DB_INFO, DBI_ORIGIN).GetVec3();
                        }
                        else if (projection == DBI_PROJECTION_SPHERE)
                        {
                            _mapType = MapType.GEOCENTRIC;
                            CartPos cartOrigin = _currentMap.GetAttribute(USER_DATA_DB_INFO, DBI_ORIGIN);
                            _origin = new Vec3D(cartOrigin.X, cartOrigin.Y, cartOrigin.Z);
                        }
                        else
                            _mapType = MapType.UNKNOWN;

                        var maxLodDistance = _currentMap.GetAttribute(USER_DATA_DB_INFO, DBI_MAX_LOD_RANGE).GetNumber();

                        _topRoi = FindTopRoi(value);

                        if (_topRoi == null)   // We have no roi. We must add one
                        {
                            _topRoi = new Roi();

                            RoiNode roiNode = new RoiNode
                            {
                                LoadDistance = 2 * maxLodDistance,
                                PurgeDistance = 2 * maxLodDistance
                            };

                            if (_nodeURL != null)  // We have an URL
                            {
                                DynamicLoader loader = new DynamicLoader
                                {
                                    NodeURL = _nodeURL
                                };

                                roiNode.AddNode(loader);
                            }
                            else
                            {
                                roiNode.AddNode(value);
                            }


                            _topRoi.AddNode(roiNode);

                            _currentMap = _topRoi;
                        }
                    }
                    else
                    {
                        _mapType = MapType.UNKNOWN;
                        _topRoi = null;
                    }
                }
                finally
                {
                    NodeLock.UnLock();
                }
            }
        }

        public MapType MapType
        {
            get
            {
                try
                {
                    NodeLock.WaitLockEdit();
                    return _mapType;
                }
                finally
                {
                    NodeLock.UnLock();
                }
            }
        }

        public Camera Camera
        {
            get { return _camera; }
            set { _camera = value; }
        }

        public string NodeURL
        {
            get
            {
                return _nodeURL;
            }

            set
            {
                _nodeURL = value;
            }
        }

        public static MapControl SystemMap=new MapControl();

        #region ----- Private methods ------------------

        private Roi FindTopRoi(Node map)
        {
            // Must be called node locked

            if (map == null)
                return null;

            Roi roi = map as Roi;

            if (roi != null)
                return roi;

            Group grp = map as Group;

            if (grp != null)
            {
                foreach (Node child in grp)
                {
                    Roi sub = FindTopRoi(child);

                    if (sub != null)
                        return sub;
                }
            }

            return null;
        }

        #endregion

        #region ----- Private variables ----------------

        private MapType _mapType;
        private Node    _currentMap;

        private Vec3D   _origin;
        private Roi     _topRoi;
        private int     _utmZone;
        private bool    _north;
        private Camera  _camera;
        private string  _nodeURL;
        

        #endregion
    }
}

//// Check attributes for map

//var projection = node.GetAttribute("UserDataDbInfo", "DbI-Projectionx");
//var origin = node.GetAttribute("UserDataDbInfo", "DbI-Database Origin");
//UTMPos utm_origin = origin;
