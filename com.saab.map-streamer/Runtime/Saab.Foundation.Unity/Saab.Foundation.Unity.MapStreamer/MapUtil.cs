//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s) 
// herein belong to Saab AB. The program(s) may be used and/or
// copied only with the written permission of Saab AB, or in
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
// File			: MapUtil.cs
// Module		:
// Description	: Utilities for Map  
// Author		: Johan Gustavsson
// Product		: Gizmo3D 2.12.155
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
// ZJP	220428	Created file                                        (2.11.79)
//
//******************************************************************************

using System;
using GizmoSDK.Coordinate;
using Saab.Foundation.Map;
using Saab.Utility.Unity.NodeUtils;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer
{
    public static class MapUtil
    {
        [Flags]
        public enum ClampOptions
        {
            None = 0,
            GroundLayer = 1 << 0,
            BuildingLayer = 1 << 1,
            AnyLayer = GroundLayer | BuildingLayer,

            LoadTerrain = 1 << 16,
        }

        private static ClampFlags GetClampFlags(ClampOptions options)
        {
            return (options & ClampOptions.LoadTerrain) == ClampOptions.LoadTerrain
                ? ClampFlags.UPDATE_DATA | ClampFlags.WAIT_FOR_DATA
                : ClampFlags.FRUSTRUM_CULL;
        }

        private static GroundClampType GetClampType(ClampOptions options)
        {
            return (GroundClampType)(int)(options);
        }

        public static bool WorldToUnity(Transform transform, LatPos position, 
            ClampOptions clampOptions = ClampOptions.None)
        {
            var clampFlags = GetClampFlags(clampOptions);
            var clampType = GetClampType(clampOptions);

            var mp = new MapPos();
            mp.clampFlags = clampFlags;

            if (!MapControl.SystemMap.SetPosition(mp, position, clampType, clampFlags))
                return false;

            return MapToUnity(transform, mp);
        }

        public static bool WorldToUnity(Transform transform, CartPos position, 
            ClampOptions clampOptions = ClampOptions.None)
        {
            var clampFlags = GetClampFlags(clampOptions);
            var clampType = GetClampType(clampOptions);

            var mp = new MapPos();
            mp.clampFlags = clampFlags;

            if (!MapControl.SystemMap.SetPosition(mp, position, clampType, clampFlags))
                return false;

            return MapToUnity(transform, mp);
        }

        public static bool MapToUnity(Transform transform, MapPos position)
        {
            if (!MapControl.SystemMap.ToLocal(position))
                return false;
                
            var roi = NodeUtils.FindFirstGameObjectTransform(position.node.GetNativeReference());
            if (!roi)
                return false;

            transform.parent = roi;

            var local = position.LocalPosition;
            transform.localPosition = new Vector3(local.x, local.y, local.z);

            return true;
        }

        public static bool MapToUnity(MapPos position, out Transform roi, out Vector3 offset)
        {
            roi = default;
            offset = default;
            
            if (!MapControl.SystemMap.ToLocal(position))
                return false;
            
            roi = NodeUtils.FindFirstGameObjectTransform(position.node.GetNativeReference());
            if (!roi)
                return false;

            var local = position.LocalPosition;
            offset = new Vector3(local.x, local.y, local.z);
            
            return true;
        }

        public static bool UnityToWorld(Transform transform, out LatPos position)
        {
            if (!UnityToMap(transform, out MapPos mp))
            {
                position = default;
                return false;
            }

            return MapControl.SystemMap.GlobalToWorld(mp.GlobalPosition(), out position);
        }

        public static bool UnityToWorld(Transform transform, out CartPos position)
        {
            if (!UnityToMap(transform, out MapPos mp))
            {
                position = default;
                return false;
            }
            
            return MapControl.SystemMap.GlobalToWorld(mp.GlobalPosition(), out position);
        }

        public static bool UnityToMap(Transform transform, out MapPos position)
        {
            var node = transform.GetComponentInParent<NodeHandle>();
            if (!node)
            {
                position = default;
                return false;
            }

            position = new MapPos();
            position.clampFlags = ClampFlags.NONE;
            position.node = node.node;

            var nodeRelative = node.transform.InverseTransformPoint(transform.position);
            position.position = new GizmoSDK.GizmoBase.Vec3D(nodeRelative.x, nodeRelative.y, nodeRelative.z);

            return true;
        }

        public static bool MapToWorld(MapPos mappos, out LatPos latpos)
        {
            return MapControl.SystemMap.GlobalToWorld(mappos.GlobalPosition(), out latpos);
        }

        public static bool MapToWorld(MapPos mappos, out CartPos cartpos)
        {
            return MapControl.SystemMap.GlobalToWorld(mappos.GlobalPosition(), out cartpos);
        }

        public static bool WorldToMap(LatPos latpos, out MapPos mappos, 
            ClampOptions clampOptions = ClampOptions.None)
        {
            var clampFlags = GetClampFlags(clampOptions);
            var clampType = GetClampType(clampOptions);

            mappos = new MapPos();

            return MapControl.SystemMap.SetPosition(mappos, latpos, clampType, clampFlags);
        }

        public static bool WorldToMap(CartPos cartpos, out MapPos mappos,
            ClampOptions clampOptions = ClampOptions.None)
        {
            var clampFlags = GetClampFlags(clampOptions);
            var clampType = GetClampType(clampOptions);

            mappos = new MapPos();
            mappos.clampFlags = clampFlags;

            return MapControl.SystemMap.SetPosition(mappos, cartpos, clampType, clampFlags);
        }

        public static class Debug
        {
            public static GameObject CreatePrimitive(PrimitiveType primType, LatPos latpos, float scale = 1f, Color? color = null)
            {
                if (!WorldToMap(latpos, out MapPos mappos, ClampOptions.None))
                    return null;

                return CreatePrimitive(primType, mappos, scale, color);
            }

            public static GameObject CreatePrimitive(PrimitiveType primType, CartPos cartpos, float scale = 1f, Color? color = null)
            {
                if (!WorldToMap(cartpos, out MapPos mappos, ClampOptions.None))
                    return null;

                return CreatePrimitive(primType, mappos, scale, color);
            }

            public static GameObject CreatePrimitive(PrimitiveType primType, MapPos mappos, float scale = 1f, Color? color = null)
            {
                var go = GameObject.CreatePrimitive(primType);

                if (!MapToUnity(go.transform, mappos))
                {
                    GameObject.Destroy(go);
                    return null;
                }

                go.transform.localScale = Vector3.one * scale;

                // remove any collider
                GameObject.Destroy(go.GetComponent<Collider>());

                if (color.HasValue)
                    go.GetComponent<Renderer>().material.color = color.Value;

                return go;
            }

            public static GameObject DrawLine(CartPos from, CartPos to, float size = 1f, Color? color = null)
            {
                if (!WorldToMap(from, out var mapFrom, ClampOptions.None))
                    return null;
                
                if (!WorldToMap(to, out var mapTo, ClampOptions.None))
                    return null;

                return DrawLine(mapFrom, mapTo, size, color);


                
            }

            public static GameObject DrawLine(MapPos from, MapPos to, float size = 1f, Color? color = null)
            {
                var go = new GameObject("Line");

                if (!MapToUnity(go.transform, from))
                {
                    GameObject.Destroy(go);
                    return null;
                }

                var d = to.position - from.position;



                var lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.numCapVertices = 2;

                lr.SetPositions(new Vector3[]
                {
                    Vector3.zero,
                    new Vector3((float)d.x, (float)d.y, -(float)d.z),
                });

                lr.widthMultiplier = size;

                if (color.HasValue)
                {
                    lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
                    lr.startColor = color.Value;
                    lr.endColor = color.Value;
                }
                
                return go;
            }
        }
    }
}
