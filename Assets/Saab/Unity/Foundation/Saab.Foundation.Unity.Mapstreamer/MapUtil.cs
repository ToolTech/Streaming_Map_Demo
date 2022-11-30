﻿//******************************************************************************
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
// Product		: Gizmo3D 2.12.33
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

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************
using GizmoSDK.Coordinate;
using Saab.Foundation.Map;
using Saab.Utility.Unity.NodeUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer
{
    public static class MapUtil
    {
        public static bool WorldToUnity(Transform transform, LatPos position, bool clampToGround = false)
        {
            var mp = new MapPos();

            if (!MapControl.SystemMap.SetPosition(mp, position, clampToGround ? GroundClampType.GROUND : GroundClampType.NONE))
                return false;

            return MapToUnity(transform, mp);
        }

        public static bool WorldToUnity(Transform transform, CartPos position, bool clampToGround = false)
        {
            var mp = new MapPos();

            if (!MapControl.SystemMap.SetPosition(mp, position, clampToGround ? GroundClampType.GROUND : GroundClampType.NONE))
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
            position.node = node.node;

            var local = node.transform.InverseTransformPoint(transform.localPosition);
            position.position = new GizmoSDK.GizmoBase.Vec3D(local.x, local.y, local.z);

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

        public static bool WorldToMap(LatPos latpos, out MapPos mappos, bool clampToGround = false)
        {
            mappos = new MapPos();
            return MapControl.SystemMap.SetPosition(mappos, latpos, clampToGround ? GroundClampType.GROUND : GroundClampType.NONE);
        }

        public static bool WorldToMap(CartPos cartpos, out MapPos mappos, bool clampToGround = false)
        {
            mappos = new MapPos();
            return MapControl.SystemMap.SetPosition(mappos, cartpos, clampToGround ? GroundClampType.GROUND : GroundClampType.NONE);
        }

        public static class Debug
        {
            public static GameObject CreatePrimitive(PrimitiveType primType, LatPos latpos, float scale = 1f, Color? color = null)
            {
                if (!WorldToMap(latpos, out MapPos mappos, false))
                    return null;

                return CreatePrimitive(primType, mappos, scale, color);
            }

            public static GameObject CreatePrimitive(PrimitiveType primType, CartPos cartpos, float scale = 1f, Color? color = null)
            {
                if (!WorldToMap(cartpos, out MapPos mappos, false))
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
        }
    }
}
