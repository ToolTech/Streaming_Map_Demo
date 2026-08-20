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
// File			: GeodeticCameraControl.cs
// Module		:
// Description	: Manages camera updates with large coordinates.
//              : Translates and rotates the camera geodetically. (e.g. the camera won't fly off into space as it moves towards the horizon)
// Author		: Mats Edvinsson
//
// Revision History...
//
// Who	    Date	Description
//
// u068671	260616	Created file
//
//******************************************************************************



using GizmoSDK.Coordinate;
using GizmoSDK.GizmoBase;
using Saab.Foundation.Map;
using Saab.Unity.Extensions;
using Saab.Utility.Unity.NodeUtils;
using System;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace Saab.Foundation.Unity.MapStreamer
{
    [RequireComponent(typeof(CameraShaderUpdater))]
    public class GeodeticCameraControl : CameraControlBase, ISceneManagerCamera
    {
        private bool _initialized = false;

        private static Coordinate _converter = new Coordinate();

        private MapPos _mapPos;

        /*
         * For synchronizing camera orientation with node transform updates
         */
        private bool needsOrientationSync;
        private Quaternion _orientationSync;

        /*
         * Ground drag
         */
        Plane _grabbedPlane;
        Vector3 _grabbedEnuInitial;
        Vector3 _grabbedEnuCurrent;
        Vec3D _cameraEcefInitial;

        /* 
         * LatestOrientationChange (loc)
         */
        Vector3 _loc_East;
        Vector3 _loc_North;
        Vector3 _loc_Up;
        Quaternion _loc_Orientation;

        /*
         * Orbit
         */
        Vector3 _orbitPoint;
        float _orbitPitch;
        float _orbitYaw;

        float? _minPitchDegrees = 1f;
        float? _maxPitchDegrees = null;

        /*
         * SETTINGS
         */

        //How much ground to cover per step when zooming. (1 = all the way to the target, 0 = no zoom at all)
        private const float zoomFactor = 0.2f;

        //Distances in meters as hard limits for the zoom-function
        private const float minZoomDistance = 10;
        private const float maxZoomDistance = 10000;

        private void Awake()
        {
            OnPreTraverse += smc_OnPreTraverse;
        }

        private void smc_OnPreTraverse(bool locked)
        {
            if (needsOrientationSync)
            {
                transform.rotation = _orientationSync;
                needsOrientationSync = false;
            }
        }

        private bool GetMouseTerrainPosition(float x, float y, out MapPos result)
        {
            result = default;

            int mouseX = (int)x;
            int mouseY = (int)(Screen.height - y);

            var layerMask = GroundClampType.GROUND;

            MapPos mp;
            if (MapControl.SystemMap.GetScreenGroundPosition(mouseX, mouseY, (uint)Screen.width, (uint)Screen.height, out mp, layerMask, ClampFlags.FRUSTRUM_CULL))
            {
                result = mp;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the orientation of the camera and updates internal state.
        /// This method should never be called passively (e.g. called every frame because it simplifies logic event when the camera does not need to be oriented differently)
        /// as doing so will prevent the camera controller from orienting the camera geodetically.
        /// </summary>
        /// <param name="orientation"></param>
        protected void SetOrientation(Quaternion orientation)
        {
            _loc_East = _unityEast;
            _loc_North = _unityNorth;
            _loc_Up = _unityUp;

            _loc_Orientation = orientation;
            needsOrientationSync = true;
            _orientationSync = orientation;
        }

        private void GetLocalPosition(MapPos mp, out Vector3 localPosition)
        {
            var transform = NodeUtils.FindFirstGameObjectTransform(mp.node.GetNativeReference());

            if (transform != null)
            {
                localPosition = transform.TransformPoint(new Vector3((float)mp.position.x, (float)mp.position.y, (float)mp.position.z));
                return;
            }

            localPosition = default;
        }

        private void SetLocalPosition(MapPos mp, in Vector3 localPosition)
        {
            var transform = NodeUtils.FindFirstGameObjectTransform(mp.node.GetNativeReference());

            if (transform != null)
            {
                mp.position = transform.InverseTransformPoint(localPosition).ToVec3D();
                return;
            }
        }

        /// <summary>
        /// Initiates or sustains a ground drag camera navigation state and updates the cameras position using the given input.
        /// </summary>
        /// <param name="mousePosition">The screen coordinate of the mouse in pixels (z-component is ignored)</param>
        /// <param name="wasJustPressed">A flag indicating whether to start a new ground drag operation (true) or continue one that was initiated during a different frame (false)</param>
        public void TranslateByGroundDrag(Vector3 mousePosition, bool regrab)
        {
            if (regrab)
            {
                if (!GetMouseTerrainPosition(mousePosition.x, mousePosition.y, out MapPos result))
                    return;

                GetLocalPosition(result, out Vector3 localPosition);

                var positionInEun = _localToEun * localPosition;

                SetLocalPosition(_mapPos, Vector3.zero);
                var (e, n, u) = _mapPos.GetEnuBasisVectors(true);

                var east = Vector3.Dot(e.ToVector3(), localPosition);
                var north = Vector3.Dot(n.ToVector3(), localPosition);
                var up = Vector3.Dot(u.ToVector3(), localPosition);

                _grabbedEnuInitial = new Vector3(east, north, up);

                if (MapControl.SystemMap.GlobalToWorld(new Vec3D(X, Y, Z), out CartPos cp))
                {
                    _cameraEcefInitial = new Vec3D(cp.X, cp.Y, cp.Z);
                }
                _grabbedPlane = new Plane(_unityUp, localPosition);
            }
            else
            {
                var ray = Camera.ScreenPointToRay(Input.mousePosition);
                _grabbedPlane.Raycast(ray, out float enter);
                var result = ray.origin + ray.direction * enter;

                SetLocalPosition(_mapPos, Vector3.zero);
                var (e, n, u) = _mapPos.GetEnuBasisVectors(true);

                var east = Vector3.Dot(e.ToVector3(), result);
                var north = Vector3.Dot(n.ToVector3(), result);
                var up = Vector3.Dot(u.ToVector3(), result);

                _grabbedEnuCurrent = new Vector3(east, north, up);

                if (MapControl.SystemMap.GlobalToWorld(new Vec3D(X, Y, Z), out CartPos cp))
                {
                    var offset = _grabbedEnuCurrent - _grabbedEnuInitial;

                    var newPos = MoveEcefAlongEllipsoid(_cameraEcefInitial, -offset.x, -offset.y);
                    _mapPos.SetCartPos(newPos.x, newPos.y, newPos.z);
                    GlobalPosition = _mapPos.GlobalPosition();

                    UpdateOrientation();
                }
            }
        }

        /// <summary>
        /// Moves the camera towards or away from the ground beneath the mouse cursor, or the center of the map if the mouse is not pointing at the ground.
        /// The camera is limited to a minimum and maximum distance from the ground, and will not zoom in closer than <see cref="minZoomDistance"/>  meters or further than <see cref="maxZoomDistance"/> meters.
        /// Zooming in by some amount and then zooming out by the same amount will return the camera to the same position, regardless of the distance to the ground.
        /// </summary>
        /// <param name="mousePosition">The mouse position on the screen in pixels (z-component is ignored)</param>
        /// <param name="amount">The amount to zoom in (negative values zoom out)</param>
        public void Zoom(Vector3 mousePosition, float amount)
        {
            if (AcquireZoomReferencePoint(mousePosition, out var result))
            {
                var zoomPointOffset = result.GlobalPosition() - GlobalPosition;
                var distance = zoomPointOffset.Length();
                var towardsZoomPoint = Vec3D.Normalize(zoomPointOffset);

                distance = EvaluateZoomFunction(distance, amount);

                if (amount > 0 && distance < minZoomDistance)
                    distance = minZoomDistance;

                if (amount < 0 && distance > maxZoomDistance)
                    distance = maxZoomDistance;

                GlobalPosition = result.GlobalPosition() - distance * towardsZoomPoint;
            }
        }

        private double EvaluateZoomFunction(double distance, float steps)
        {
            return distance * Math.Pow(1 - zoomFactor, steps);
        }

        private bool AcquireZoomReferencePoint(Vector3 mousePosition, out MapPos result)
        {
            return GetMouseTerrainPosition(mousePosition.x, mousePosition.y, out result);
        }

        /// <summary>
        /// Initiates or sustains an orbit camera navigation state and updates the cameras position using the given input.
        /// </summary>
        /// <param name="mousePosition">The screen coordinate of the mouse in pixels (z-component is ignored)</param>
        /// <param name="wasJustPressed">A flag indicating whether to start a new orbit operation (true) or continue one that was initiated during a different frame (false)</param>
        public void Orbit(Vector3 mousePosition, bool wasJustPressed)
        {
            (Vector3 e, Vector3 n, Vector3 u) frame;
            if (wasJustPressed)
            {
                AcquireOrbitReferencePoint();
                if (!AcquireCameraEnuFrame(out frame))
                    throw new Exception();
                AcquireOrbitOrientation(frame);

                _previousMousePosition = mousePosition;
            }
            else
            {
                if (!AcquireCameraEnuFrame(out frame))
                    throw new Exception();
            }

            Vector2 offset = CalculateOffset(mousePosition);
            CalculatePosition(offset, frame);
            SetOrientation(CalculateOrientation(frame));
        }

        /// <summary>
        /// Acquires either the point on the ground that the camera is pointing towards, or a point in the air in front of the camera if the camera is not aimed at the ground.
        /// </summary>
        private void AcquireOrbitReferencePoint()
        {
            if (GetMouseTerrainPosition(Screen.width / 2, Screen.height / 2, out MapPos result))
            {
                GetLocalPosition(result, out _orbitPoint);

                if (result.GlobalPosition().Length() == 0)
                    _orbitPoint = Vector3.zero;
            }
            else
            {
                _orbitPoint = Vector3.zero;
            }

            if (_orbitPoint.magnitude == 0)
            {
                //TODO: Conversions not necessary since we are working in local coordinates (Camera always at (0, 0, 0))
                if (!MapControl.SystemMap.GlobalToWorld(new Vec3D(X, Y, Z), out CartPos cp))
                    throw new Exception("CONVERSION TO WORLD FAILED!!");

                if (!_mapPos.SetCartPos(cp.X, cp.Y, cp.Z))
                    throw new Exception("CARTPOS ASSIGNMENT FAILED!!");

                GetLocalPosition(_mapPos, out Vector3 cameraLocalPosition);

                //TODO: Adjust orbit point distance from hardcoded 500m to something like the distance to the center of the map. (That way, if you look towards the map but not directly at it, the orbit point will still be somewhat close to the map)
                _orbitPoint = cameraLocalPosition + Camera.transform.forward * 500;
            }
        }

        private bool AcquireCameraEnuFrame(out (Vector3 e, Vector3 n, Vector3 u) frame)
        {
            frame = default;

            if (!TryAcquireEnuBasis(out var east, out var north, out var up))
                return false;

            frame = (east.ToVector3FlipZ(), north.ToVector3FlipZ(), up.ToVector3FlipZ());

            return true;
        }

        private bool TryAcquireEnuBasis(out Vec3 east, out Vec3 north, out Vec3 up)
        {
            east = default;
            north = default;
            up = default;

            if (MapControl.SystemMap?.CurrentMap == null)
                return false;

            if (!MapControl.SystemMap.GlobalToWorld(new Vec3D(X, Y, Z), out LatPos latPos))
                return false;

            if (_mapPos == null)
            {
                _mapPos = new MapPos();
            }

            _mapPos.SetLatPos(latPos.Latitude, latPos.Longitude, latPos.Altitude);
            var enu = _mapPos.EnuToLocal();

            //TODO: Simplify these operations to a single matrix operation. (Create LocalToEnu-matrix)
            east = enu * new Vec3(1, 0, 0);
            north = enu * new Vec3(0, 1, 0);
            up = enu * new Vec3(0, 0, 1);

            return true;
        }

        private void AcquireOrbitOrientation((Vector3 e, Vector3 n, Vector3 u) frame)
        {
            //Obtain vector pointing from orbit center towards camera.
            var orbitVector = -_orbitPoint;

            //Project vector onto local ENU-frame
            float orbitEast = Vector3.Dot(frame.e, orbitVector);
            float orbitNorth = Vector3.Dot(frame.n, orbitVector);
            float orbitUp = Vector3.Dot(frame.u, orbitVector);

            //Normalize direction in local ENU-frame
            Vector3 normalizedOrbit = new Vector3(orbitEast, orbitNorth, orbitUp).normalized;

            //Pitch is arc-sine of up in ENU
            _orbitPitch = Mathf.Asin(normalizedOrbit.z);

            //Atan2 gives 0 when input vector is (1, 0), therefore we send north as x and east as y, meaning yaw will increase as our direction changes from north to east.
            _orbitYaw = Mathf.Atan2(normalizedOrbit.x, normalizedOrbit.y);
        }

        Vector3 _previousMousePosition;
        private Vector2 CalculateOffset(Vector3 mousePosition)
        {
            Vector2 offset = mousePosition - _previousMousePosition;
            _previousMousePosition = mousePosition;

            return offset;
        }

        float degreesPerPixel = 0.125f;

        private void CalculatePosition(Vector2 offset, (Vector3 e, Vector3 n, Vector3 u) frame)
        {
            _orbitYaw += offset.x * Mathf.PI / 180 * degreesPerPixel;
            wrapOrbitYaw();

            _orbitPitch -= offset.y * Mathf.PI / 180 * degreesPerPixel;
            ClampOrbitPitch();

            var orbitDistance = _orbitPoint.magnitude;
            var pitchSine = Mathf.Sin(_orbitPitch);
            var pitchCosine = Mathf.Cos(_orbitPitch);
            var yawSine = Mathf.Sin(_orbitYaw);
            var yawCosine = Mathf.Cos(_orbitYaw);
            var orbitDirectionVector = frame.u * pitchSine + (frame.n * yawCosine + frame.e * yawSine) * pitchCosine;
            var orbitVector = orbitDirectionVector * orbitDistance;
            var orbitTarget = _orbitPoint + orbitVector;

            SetLocalPosition(_mapPos, orbitTarget);

            GlobalPosition = MapControl.SystemMap.LocalToGlobal(_mapPos);

            //Since _orbitPoint is relative to the camera, it needs to be updated, otherwise it will move with the camera.
            _orbitPoint = -orbitVector;
        }

        private void wrapOrbitYaw()
        {
            if (_orbitYaw > Mathf.PI * 2)
                _orbitYaw -= Mathf.PI * 2;

            if (_orbitYaw < 0)
                _orbitYaw += Mathf.PI * 2;
        }

        private void ClampOrbitPitch()
        {
            var minPitch = _minPitchDegrees ?? -90;
            var maxPitch = _maxPitchDegrees ?? 90;

            if (minPitch <= -90)
                minPitch = -89;

            if (maxPitch >= 90)
                maxPitch = 89;

            minPitch *= Mathf.PI / 180;
            maxPitch *= Mathf.PI / 180;

            _orbitPitch = Mathf.Clamp(_orbitPitch, minPitch, maxPitch);
        }

        private Quaternion CalculateOrientation((Vector3 e, Vector3 n, Vector3 u) frame)
        {
            var towardsTarget = Vector3.Normalize(_orbitPoint);

            Debug.Log($"Towards orbit point: {towardsTarget}");
            return Quaternion.LookRotation(towardsTarget, frame.u);
        }

        private void UpdateOrientation()
        {
            var rotation = FromBasisToBasis(
                _loc_East, _loc_North, _loc_Up,
                _unityEast, _unityNorth, _unityUp
            );

            transform.rotation = rotation * _loc_Orientation;
        }

        public static Quaternion FromBasisToBasis(
            Vector3 e0, Vector3 n0, Vector3 u0,
            Vector3 e1, Vector3 n1, Vector3 u1)
        {
            e0.Normalize(); n0.Normalize(); u0.Normalize();
            e1.Normalize(); n1.Normalize(); u1.Normalize();

            Matrix4x4 b0 = Matrix4x4.identity;
            b0.SetColumn(0, new Vector4(e0.x, e0.y, e0.z, 0f));
            b0.SetColumn(1, new Vector4(n0.x, n0.y, n0.z, 0f));
            b0.SetColumn(2, new Vector4(u0.x, u0.y, u0.z, 0f));

            Matrix4x4 b1 = Matrix4x4.identity;
            b1.SetColumn(0, new Vector4(e1.x, e1.y, e1.z, 0f));
            b1.SetColumn(1, new Vector4(n1.x, n1.y, n1.z, 0f));
            b1.SetColumn(2, new Vector4(u1.x, u1.y, u1.z, 0f));

            Matrix4x4 r = b1 * b0.transpose;

            return r.rotation;
        }

        public Vec3D MoveEcefAlongEllipsoid(Vec3D ecef, double moveEastMeters, double moveNorthMeters)
        {
            _converter.SetCartPos(new CartPos(ecef.x, ecef.y, ecef.z));
            _converter.GetLatPos(out LatPos lp);

            var moved = _mapPos.MoveAlongEllipsoid(lp, moveEastMeters, moveNorthMeters);

            _converter.SetLatPos(moved);
            _converter.GetCartPos(out CartPos cp);
            return new Vec3D(cp.X, cp.Y, cp.Z);
        }

        public void LateUpdate()
        {
            if (!TryAcquireEnuBasis(out var east, out var north, out var up))
                return;

            _unityEast = east.ToVector3FlipZ();
            _unityNorth = north.ToVector3FlipZ();
            _unityUp = up.ToVector3FlipZ();

            _localToEun = MapUtil.FromBasis(east.ToVector3(), up.ToVector3(), north.ToVector3());

            if (!_initialized)
            {
                // look north
                var offset = up.ToVector3() * 100f;
                X += offset.x;
                Y += offset.y;
                Z += offset.z;

                SetOrientation(Quaternion.LookRotation(_unityNorth, _unityUp));
                _initialized = true;
            }
        }

        public override double UpdateCamera(double renderTime)
        {
            return renderTime;
        }

        public override void MapChanged()
        {
            // Called when global map has changed
        }
    }
}