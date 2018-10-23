using GizmoSDK.Coordinate;
using GizmoSDK.GizmoBase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Saab.Core
{
    public interface IWorldCoord
    {
        Vec3D Position { get; }
    }
}