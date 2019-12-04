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

    public class MapControl : IMapLocationProvider<MapPos, Node>
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
            MapType = MapType.UNKNOWN;
            _topRoi = null;
            _currentMap = null;
            _nodeURL = null;
        }

        public bool GetScreenVectors(int x,int y,uint size_x,uint size_y,out Vec3D position,out Vec3 direction)
        {
            position = new Vec3D();
            direction = new Vec3();

            if (_camera== null || !_camera.IsValid())
                return false;

            Camera.GetScreenVectors(x, y, size_x, size_y, out position, out direction);

            return true;
        }

        public bool GetScreenGroundPosition(int x, int y, uint size_x, uint size_y,out MapPos result, ClampFlags flags= ClampFlags.DEFAULT)
        {
            result = new MapPos();

            Vec3D position;
            Vec3 direction;

            
            if (!GetScreenVectors(x, y, size_x, size_y, out position, out direction))
                return false;

            // Coordinate is now in world Cartesian coordinates (Roi Position)

            Vec3D origo = new Vec3D(0, 0, 0);       // Set used origo

            Intersector isect = new Intersector();

            // Check camera frustrum -----------------------------------------------------------

            if (_camera != null && _camera.IsValid())
            {
                origo = _camera.Position;

                if ((flags & ClampFlags.FRUSTRUM_CULL) != 0)
                    isect.SetCamera(_camera);
            }

            // Adjust intersector to use origo as center
            position = position - origo;

            isect.SetStartPosition((Vec3)(position));
            isect.SetDirection(direction);

            if (isect.Intersect(_currentMap, IntersectQuery.NEAREST_POINT | IntersectQuery.NORMAL | ((flags & ClampFlags.WAIT_FOR_DATA) != 0 ? IntersectQuery.WAIT_FOR_DYNAMIC_DATA : 0), 1, true, origo))
            {
                IntersectorResult res = isect.GetResult();

                IntersectorData data = res.GetData(0);

                result.position = data.position + origo;

                result.normal = data.normal;

                result.clamped = true;
            }

            isect.Dispose();   // Drop handle and ignore GC

            if (_topRoi != null)
            {
                RoiNode roi= _topRoi.GetClosestRoiNode(result.position);
                result.node = roi;

                // Remove roiNode position as offset - Go to local RoiNode based coordinate system

                if (roi != null && roi.IsValid())
                    result.position -= roi.Position;
            }

            return true;
        }

        public Vec3D LocalToWorld(MapPos mappos)
        {
            Vec3D result = mappos.position;

            RoiNode roi = mappos.node as RoiNode;

            if (roi != null && roi.IsValid())
            {
                result += roi.Position;
            }

            return result;
        }

        public MapPos WorldToLocal(Vec3D position)
        {
            MapPos result = new MapPos();

            result.position = position/* - _origin*/;

            if (_topRoi != null)
            {
                RoiNode roi= _topRoi.GetClosestRoiNode(result.position);
                result.node = roi;

                // Remove roiNode position as offset - Go to local RoiNode based coordinate system

                if (roi != null && roi.IsValid())
                    result.position -= roi.Position;
            }

            return result;
        }

        public double GetAltitude(LatPos pos , ClampFlags flags = ClampFlags.DEFAULT)
        {
            MapPos mapPos;

            if (!GetPosition(pos, out mapPos, GroundClampType.GROUND, flags))
                return 0;

            LatPos updatedPos;

            if (!GetLatPos(mapPos, out updatedPos))
                return 0;

            return updatedPos.Altitude;
        }

        public bool UpdatePosition(ref MapPos result, GroundClampType groundClamp = GroundClampType.NONE, ClampFlags flags = ClampFlags.DEFAULT)
        {
            if (_currentMap == null)    // No map
                return false;

            if (groundClamp != GroundClampType.NONE)
            {

                // Add ROINode position as offset   - Go to global 3D coordinate system as we need to clamp in global 3D

                RoiNode roi = result.node as RoiNode;

                if (roi != null && roi.IsValid())
                    result.position += roi.Position;

                Intersector isect = new Intersector();

                Vec3D origo = new Vec3D(0,0,0);

                // Check camera frustrum -----------------------------------------------------------

                if (_camera != null && _camera.IsValid())
                {
                    origo = _camera.Position;

                    if ((flags & ClampFlags.FRUSTRUM_CULL) != 0)
                        isect.SetCamera(_camera);
                }

                if ((flags & ClampFlags.ISECT_LOD_QUALITY) != 0)                // Lets stand in the ray to get highest quality
                    origo = result.position;

                Vec3 up = new Vec3(result.local_orientation.v13, result.local_orientation.v23, result.local_orientation.v33);

                // TODO: Fix this....
                if (up.x == 0 && up.y == 0 && up.z == 0)
                    up.y = 1;

                isect.SetStartPosition((Vec3)(result.position - origo) + 10000.0f * up);
                isect.SetDirection(-up);

                if (isect.Intersect(_currentMap,    IntersectQuery.NEAREST_POINT|
                                                    IntersectQuery.NORMAL | 
                                                    ( (flags & ClampFlags.WAIT_FOR_DATA)!=0 ? IntersectQuery.WAIT_FOR_DYNAMIC_DATA : 0)|
                                                    ((flags & ClampFlags.UPDATE_DATA) != 0 ? IntersectQuery.UPDATE_DYNAMIC_DATA : 0)
                                                    , 1, true, origo))
                {
                    IntersectorResult res = isect.GetResult();

                    IntersectorData data = res.GetData(0);

                    result.position = data.position + origo;
                    
                    result.normal = data.normal;
                    result.clamped = true;
                }
                else
                {
                    result.normal = up;
                    result.clamped = false;
                }

                if (groundClamp == GroundClampType.GROUND)
                {
                    result.normal = result.local_orientation.GetCol(2);
                }


                isect.Dispose();    // Drop handle and ignore GC

                // Remove ROINode position as offset - Go to local coordinate system under ROI Node

                if (roi != null && roi.IsValid())
                    result.position -= roi.Position;
            }
                       

            return true;
        }

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

        public bool GetLatPos(MapPos pos,out LatPos result)
        {
            Coordinate converter = new Coordinate();

            Vec3D position = pos.position;

            result = new LatPos();

            // Check possibly local 3D under a roiNode

            RoiNode roi = pos.node as RoiNode;

            if (roi != null && roi.IsValid())
                position += roi.Position;

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

            return converter.GetLatPos(out result);
        }

        public bool GetPosition(LatPos pos, out MapPos result,GroundClampType groundClamp= GroundClampType.NONE, ClampFlags flags = ClampFlags.DEFAULT)
        {
            Coordinate converter = new Coordinate();
            converter.SetLatPos(pos);

            result = new MapPos
            {
                local_orientation = new Matrix3(new Vec3(1, 0, 0), new Vec3(0, 0, -1), new Vec3(0, 1, 0))    // East North Up vectors
            };

            // Convert to global 3D coordinate in appropriate target system

            switch (_mapType)
            {
                case MapType.UNKNOWN:
                    {
                        return false;
                    }

                case MapType.UTM:

                    UTMPos utmpos;

                    if (!converter.GetUTMPos(out utmpos))
                    {
                        Message.Send("Controller", MessageLevel.WARNING, "Failed to convert to UTM");
                        return false;
                    }

                    result.position = new Vec3D(utmpos.Easting, utmpos.H, -utmpos.Northing) - _origin;

                    
                    // Possibly compensate for zone and north as well XXX

                    break;

                case MapType.GEOCENTRIC:

                    CartPos cartpos;

                    if (!converter.GetCartPos(out cartpos))
                    {
                        Message.Send("Controller", MessageLevel.WARNING, "Failed to convert to Geocentric");
                        return false;
                    }

                    result.local_orientation = converter.GetOrientationMatrix(cartpos);

                    result.position = new Vec3D(cartpos.X, cartpos.Y, cartpos.Z) - _origin;
                                       
                    break;
            }

            // We have now global 3D coordinates


            // Check possibly local 3D under a roiNode

            if (_topRoi != null)
            {
                RoiNode roi= _topRoi.GetClosestRoiNode(result.position);
                result.node = roi;

                // Remove roiNode position as offset - Go to local RoiNode based coordinate system

                if (roi != null && roi.IsValid())
                    result.position -= roi.Position;
            }

            return UpdatePosition(ref result, groundClamp,flags);
            
        }

        public Roi FindTopRoi(Node map)
        {
            if (map == null)
                return null;

            Roi roi = map as Roi;

            if (roi !=null)
                return roi;

            Group grp = map as Group;

            if (grp!=null)
            {
                foreach(Node child in grp)
                {
                    Roi sub = FindTopRoi(child);

                    if (sub != null)
                        return sub;
                }
            }

            return null;
        }

        public bool GetContext(double lat, double lon, double alt, LocationOptions options, out MapPos location)
        {
            var clampType = GroundClampType.NONE;
            if (options.PositionOptions == PositionOptions.Surface)
                clampType = GroundClampType.GROUND;

            var clampFlags = ClampFlags.DEFAULT;
            if (options.LoadOptions == LoadOptions.Load)
                clampFlags = ClampFlags.WAIT_FOR_DATA;

            if (options.QualityOptions == QualityOptions.Highest)
                clampFlags |= ClampFlags.ISECT_LOD_QUALITY;

            MapPos mp;
            if (!GetPosition(new LatPos(lat, lon, alt), out mp, clampType, clampFlags))
            {
                location = default(MapPos);
                return false;
            }

            location = mp;
            return true;
        }

        public Node CurrentMap
        {
            get
            {
                return _currentMap;
            }

            set
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
                    else if (projection == DBI_PROJECTION_FLAT_EARTH)
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

                    if(_topRoi==null)   // We have no roi. We must add one
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

                NodeLock.UnLock();
            }
        }

        public MapType MapType
        {
            get { return _mapType; }
            set { _mapType = value; }
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
