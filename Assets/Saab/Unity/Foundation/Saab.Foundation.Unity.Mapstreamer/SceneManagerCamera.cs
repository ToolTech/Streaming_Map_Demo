using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GizmoSDK.Coordinate;
using GizmoSDK.GizmoBase;
using Saab.Foundation.Map;
using Saab.Unity.Extensions;
using Saab.Utility.Map;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer
{
    public delegate void Traverse();

    [RequireComponent(typeof(Camera))]
    public class SceneManagerCamera : MonoBehaviour, ISceneManagerCamera
    {
        public event Traverse OnPreTraverse;
        public event Traverse OnPostTraverse;

        private MapPos _position = new MapPos();

        public LatPos Latpos
        {
            get
            {
                LatPos res;
                MapControl.SystemMap.GetLatPos(_position, out res);
                return res;
            }
            set
            {
                _position.SetLatPos(value.Latitude, value.Longitude, value.Altitude);
            }
        }

        public MapPos MapPosition
        {
            get { return _position; }
        }

        public Vector3 Up
        {
            get
            {
                return _position.local_orientation.GetCol(2).ToVector3();
            }
        }

        public Camera Camera
        {
            get
            {
                return gameObject.GetComponent<Camera>();
            }
        }

        public Vec3D Position
        {
            get { return MapControl.SystemMap.LocalToWorld(_position); }
            set
            {
                // only calling this function does not create the map pos correctly... local_orientation is not set
                _position = MapControl.SystemMap.WorldToLocal(value);


                // we perform this to get the map system to set the local_orientation...
                LatPos latpos;

                if(MapControl.SystemMap.GetLatPos(_position, out latpos))
                    MapControl.SystemMap.SetPosition(_position, latpos);
            }
        }

        public virtual void PreTraverse()
        {
            _position.Step(0, default(LocationOptions));

            OnPreTraverse?.Invoke();
        }

        public virtual void PostTraverse()
        {
            OnPostTraverse?.Invoke();
        }

        public virtual void MapChanged()
        {

        }
    }
}
