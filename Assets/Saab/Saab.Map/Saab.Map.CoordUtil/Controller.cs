//******************************************************************************
// File			: Controller.cs
// Module		: Saab.Map.CoordUtil
// Description	: Controller of the CoordUtil toolkit
// Author		: Anders Modén		
//		
// Copyright © 2003- Saab Training Systems AB, Sweden
//
// Revision History...							
//									
// Who	Date	Description						
//									
// AMO	180301	Created file 	
//
//******************************************************************************

using System;

using GizmoSDK.Gizmo3D;
using GizmoSDK.GizmoBase;
using GizmoSDK.Coordinate;

namespace Saab.Map.CoordUtil
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
        WAIT_FOR_DATA           = 1<<0,
        ISECT_LOD_QUALITY       = 1<<1,
        FRUSTRUM_CULL           = 1<<2,

        DEFAULT = FRUSTRUM_CULL,
    }

    public class Controller
    {
        const string USER_DATA_DB_INFO = "UserDataDbInfo";

        const string DBI_PROJECTION = "DbI-Projection";

        const string DBI_PROJECTION_UTM = "UTM";
        const string DBI_PROJECTION_FLAT_EARTH = "Flat Earth";
        const string DBI_PROJECTION_SPHERE = "Sphere";

        const string DBI_ORIGIN = "DbI-Database Origin";

        public Controller()
        {
            Reset();
        }

        public void Reset()
        {
            MapType = MapType.UNKNOWN;
            _topRoi = null;
            _currentMap = null;
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

            Intersector isect = new Intersector();

            if ( (flags&ClampFlags.FRUSTRUM_CULL)!=0 && _camera != null && _camera.IsValid())
            {
                 isect.SetCamera(_camera);

                if (_camera.RoiPosition)
                    position = position - _camera.Position;
            }

            isect.SetStartPosition((Vec3)(position));
            isect.SetDirection(direction);

            if (isect.Intersect(_currentMap, IntersectQuery.NEAREST_POINT | IntersectQuery.NORMAL | ((flags & ClampFlags.WAIT_FOR_DATA) != 0 ? IntersectQuery.WAIT_FOR_DYNAMIC_DATA : 0), 1, true, _camera.Position))
            {
                IntersectorResult res = isect.GetResult();

                IntersectorData data = res.GetData(0);

                result.position = data.position;

                if (_camera.RoiPosition)
                    result.position = result.position + _camera.Position;

                result.normal = data.normal;

                result.clamped = true;

            }

            isect.Dispose();   // Drop handle and ignore GC

            if (_topRoi != null)
            {

                result.roiNode = _topRoi.GetClosestRoiNode(result.position);

                // Remove roiNode position as offset - Go to local RoiNode based coordinate system

                if (result.roiNode != null && result.roiNode.IsValid())
                    result.position -= result.roiNode.Position;
            }

            return true;
        }

        public Vec3D LocalToWorld(MapPos mappos)
        {
            Vec3D result = mappos.position;

            if (mappos.roiNode != null && mappos.roiNode.IsValid())
                result += mappos.roiNode.Position;

            //result += _origin;

            return result;
        }

        public MapPos WorldToLocal(Vec3D position)
        {
            MapPos result = new MapPos();

            result.position = position/* - _origin*/;

            if (_topRoi != null)
            {

                result.roiNode = _topRoi.GetClosestRoiNode(result.position);

                // Remove roiNode position as offset - Go to local RoiNode based coordinate system

                if (result.roiNode != null && result.roiNode.IsValid())
                    result.position -= result.roiNode.Position;
            }

            return result;
        }

        public double GetAltitude(LatPos pos , ClampFlags flags = ClampFlags.DEFAULT)
        {
            MapPos mapPos;

            if (!GetPosition(pos, out mapPos, GroundClampType.GROUND, flags))
                return 0;

            LatPos updatedPos;

            if (!GetPosition(mapPos, out updatedPos))
                return 0;

            return updatedPos.Altitude;
        }

        public bool UpdatePosition(ref MapPos result, GroundClampType groundClamp = GroundClampType.NONE, ClampFlags flags = ClampFlags.DEFAULT)
        {
            if (groundClamp != GroundClampType.NONE)
            {
                // Add ROINode position as offset   - Go to global 3D coordinate system as we need to clamp in global 3D

                if (result.roiNode != null && result.roiNode.IsValid())
                    result.position += result.roiNode.Position;

                Intersector isect = new Intersector();

                Vec3D eyePos = new Vec3D(0,0,0);

                if (_camera != null && _camera.IsValid())
                {
                    eyePos = _camera.Position;

                    if ((flags & ClampFlags.FRUSTRUM_CULL) != 0)
                        isect.SetCamera(_camera);
                }

                if ((flags & ClampFlags.ISECT_LOD_QUALITY) != 0)                // Lets stand in the ray to get highest quality
                    eyePos = result.position;

                Vec3 up = new Vec3(result.local_orientation.v13, result.local_orientation.v23, result.local_orientation.v33);

                // TODO: Fix this....
                if (up.x == 0 && up.y == 0 && up.z == 0)
                    up.y = 1;

                isect.SetStartPosition((Vec3)(result.position - eyePos) + 10000.0f * up);
                isect.SetDirection(-up);

                if (isect.Intersect(_currentMap, IntersectQuery.NEAREST_POINT|IntersectQuery.NORMAL | ( (flags & ClampFlags.WAIT_FOR_DATA)!=0 ? IntersectQuery.WAIT_FOR_DYNAMIC_DATA : 0), 1, true, eyePos))
                {
                    IntersectorResult res = isect.GetResult();

                    IntersectorData data = res.GetData(0);

                    result.position.x = data.position.x + eyePos.x;
                    result.position.y = data.position.y + eyePos.y;
                    result.position.z = data.position.z + eyePos.z;

                    
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

                if (result.roiNode != null && result.roiNode.IsValid())
                    result.position -= result.roiNode.Position;
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

        public bool GetPosition(MapPos pos,out LatPos result)
        {
            Coordinate converter = new Coordinate();

            Vec3D position = pos.position;

            result = new LatPos();

            // Check possibly local 3D under a roiNode

            if (pos.roiNode != null && pos.roiNode.IsValid())
                position += pos.roiNode.Position;

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
           
                result.roiNode = _topRoi.GetClosestRoiNode(result.position);

                // Remove roiNode position as offset - Go to local RoiNode based coordinate system

                if (result.roiNode != null && result.roiNode.IsValid())
                    result.position -= result.roiNode.Position;
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

                    _topRoi = FindTopRoi(value);
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

        #region ----- Private variables ----------------

        private MapType _mapType;
        private Node    _currentMap;

        private Vec3D   _origin;
        private Roi     _topRoi;
        private int     _utmZone;
        private bool    _north;
        private Camera  _camera;
        

        #endregion
    }
}

//// Check attributes for map

//var projection = node.GetAttribute("UserDataDbInfo", "DbI-Projectionx");
//var origin = node.GetAttribute("UserDataDbInfo", "DbI-Database Origin");
//UTMPos utm_origin = origin;