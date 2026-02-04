using GizmoSDK.Coordinate;
using GizmoSDK.GizmoBase;
using Saab.Foundation.Map;
using Saab.Foundation.Unity.MapStreamer;
using Saab.Unity.Extensions;
using System;
using UnityEngine;

public class SphereTest : MonoBehaviour
{

    public Vector3 JawPitchRoll = Vector3.zero;
    //[SerializeField]private Vector3 _vec;

    [SerializeField]
    private float _latitude = 59.338026f;

    [SerializeField]
    private float _longitude = 18.128561f;

    [SerializeField]
    private NodeHandle _nodeHandle;

    [SerializeField]
    private Vector2 _uvCoord;

    private readonly Coordinate _coordConverter = new Coordinate();

    private void OnDrawGizmos()
    {
        var latpos = new LatPos(_latitude * Coordinate.DEG2RAD, _longitude * Coordinate.DEG2RAD, 0);

        var pos = new MapPos();
        pos.SetLatPos(latpos.Latitude, latpos.Longitude, latpos.Altitude);

        //DrawMatrix(pos.LocalToEnu());
        //DrawMatrix(pos.EnuToLocal());
        //DrawOrientationMatrix(latpos);

        var enu = Coordinate.GetOrientationMatrix(latpos);
        DrawMatrix(enu);

        if (_nodeHandle)
        {
            DrawNodeOrientationMatrix(latpos, _nodeHandle);
            DrawPixelCoord(_nodeHandle);
        }

    }

    private Matrix4x4 FromBasis(Vector3 right, Vector3 up, Vector3 forward)
    {
        var m = Matrix4x4.identity;

        // Columns are the basis vectors.
        m.SetColumn(0, new Vector4(right.x, right.y, right.z, 0f));
        m.SetColumn(1, new Vector4(up.x, up.y, up.z, 0f));
        m.SetColumn(2, new Vector4(forward.x, forward.y, forward.z, 0f));

        return m;
    }

    private Vector3 Flip(Vector3 v)
    {
        return new Vector3(v.x, v.y, -v.z);
    }

    private void DrawNodeOrientationMatrix(LatPos latPos, NodeHandle handle)
    {
        // ---------- Draw Center of Node ----------
        Gizmos.color = Color.yellow;
        //var centerPos = handle.node.BoundaryCenter.ToVector3()
        var centerPos = (handle.transform.position) + Flip(handle.node.BoundaryCenter.ToVector3());

        Gizmos.DrawWireSphere(handle.transform.localToWorldMatrix * Flip(centerPos), 50f);
        Gizmos.DrawSphere(handle.transform.localToWorldMatrix * Flip(centerPos), 10f);

        // ---------- latpos---------- 
        var latpos = new LatPos(_latitude * Coordinate.DEG2RAD, _longitude * Coordinate.DEG2RAD, 0);
        var pos = new MapPos();
        pos.SetLatPos(latpos.Latitude, latpos.Longitude, latpos.Altitude);
        //var localToEnu = pos.LocalToEnu();

        // ---------- Matrix ----------

        var NodeMatrix = Matrix4x4.Translate(-centerPos);
        var enu = Coordinate.GetOrientationMatrix(latPos);

        var east = enu * new Vec3(1, 0, 0);
        var north = enu * new Vec3(0, 1, 0);
        var up = enu * new Vec3(0, 0, 1);

        var localToEnu = FromBasis((east.ToVector3()), (north.ToVector3()), (up.ToVector3()));

        var finalMatrix = handle.transform.localToWorldMatrix * NodeMatrix.inverse * localToEnu * NodeMatrix;

        DrawMatrix(finalMatrix, handle.transform, handle.transform.localToWorldMatrix * Flip(centerPos));
    }

    Vector3 NodeTopLeft(NodeHandle handle, Vector2 texSize, Vector2 pixelSize)
    {
        if (!handle.TryGetComponent<MeshFilter>(out var meshFilter))
            return Vector3.zero;

        var mesh = meshFilter.sharedMesh;
        var meshCenter = handle.node.BoundaryCenter;

        MapControl.SystemMap.GlobalToWorld(meshCenter, out GizmoSDK.Coordinate.CartPos cartPos);
        _coordConverter.SetCartPos(cartPos);
        _coordConverter.GetUTMPos(out var utmPos);

        var topLeftCorner = handle.featureInfo * new Vec3D(0, 0, 1);

        var nodeSize = (texSize * pixelSize);
        var nodeOffsetDiff = nodeSize - new Vector2(mesh.bounds.size.x, mesh.bounds.size.z);    // HERE!!!!!! assume flat mesh !!!!!!!!!
        var centerOffset = new Vec3D(topLeftCorner.x - utmPos.Easting, 0, topLeftCorner.y - utmPos.Northing);

        if (Math.Abs(centerOffset.x) < nodeSize.x / 2)
            nodeOffsetDiff.x *= -1;

        if (Math.Abs(centerOffset.y) > nodeSize.y / 2)
            nodeOffsetDiff.y *= -1;

        centerOffset.x += nodeOffsetDiff.x;
        centerOffset.y += nodeOffsetDiff.y;

        var nodeTexTopLeft = mesh.bounds.center -
        new Vector3((float)centerOffset.x + (texSize.x * pixelSize.x),
                    (float)centerOffset.y,
                    (float)centerOffset.z);

        return nodeTexTopLeft;
    }

    Vector3 NodeTopLeftNew(NodeHandle handle, Vector2 texSize, Vector2 pixelSize)
    {
        if (!handle.TryGetComponent<MeshFilter>(out var meshFilter))
            return Vector3.zero;

        var mesh = meshFilter.sharedMesh;
        var nodeCenter = handle.node.BoundaryCenter;

        MapControl.SystemMap.GlobalToWorld(nodeCenter, out GizmoSDK.Coordinate.CartPos cartPos);
        _coordConverter.SetCartPos(cartPos);
        _coordConverter.GetUTMPos(out var utmPos);

        var topLeftCorner = handle.featureInfo * new Vec3D(0, 0, 1);
        var nodeSize = (texSize * pixelSize);
        
        var leftCornerNode = new Vec3D(topLeftCorner.x - utmPos.Easting, 0, topLeftCorner.y - utmPos.Northing);
        var nodeOffsetDiff = pixelSize * 2f;
        nodeOffsetDiff.x *= -1;

        if (Math.Abs(leftCornerNode.x) < nodeSize.x / 2)
            nodeOffsetDiff.x *= -1;

        if (Math.Abs(leftCornerNode.y) > nodeSize.y / 2)
            nodeOffsetDiff.y *= -1;

        leftCornerNode.x += nodeOffsetDiff.x;
        leftCornerNode.y += nodeOffsetDiff.y;

        var nodeTexTopLeft = mesh.bounds.center -
        new Vector3((float)-leftCornerNode.x,
                    (float)0,
                    (float)leftCornerNode.z);

        return nodeTexTopLeft;
    }

    Vector3 GetPixelCoord(Vector2 uv, Vector2 pixelResolution, Vector3 nodeTexTopLeft, float height = 0)
    {
        // z-coordinate is negative because of the infamous BTA Z-flip	
        var coord = new Vector3(uv.x * pixelResolution.x + nodeTexTopLeft.x, nodeTexTopLeft.y + height, -uv.y * pixelResolution.y + nodeTexTopLeft.z);
        return coord;
    }

    private void DrawPixelCoord(NodeHandle handle)
    {
        var featureInfo = handle.featureInfo;
        var pixelSize = new Vector2((float)featureInfo.v11, (float)featureInfo.v22);

        var tex = handle.texture;
        var texSize = new Vector2(tex.width, tex.height);
        Vector2 pixelOverlap = new Vector2(2, 0);
        //_uvCoord += pixelOverlap

        var topLeft = NodeTopLeft(handle, texSize, pixelSize);
        var topLeftCorner = NodeTopLeftNew(handle, texSize, pixelSize);

        var pos = GetPixelCoord(_uvCoord + pixelOverlap, pixelSize, topLeft, 0);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere((handle.transform.position + Flip(pos)), pixelSize.x * 20f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere((handle.transform.position + Flip(handle.node.BoundaryCenter.ToVector3())), pixelSize.x * 30);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(Flip(topLeftCorner) + handle.transform.position, pixelSize.x * 40f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere((handle.transform.position + Flip(topLeft)), pixelSize.x * 30);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Flip(GetPixelCoord(new Vector2(texSize.x, 0) + pixelOverlap, pixelSize, topLeft, 0)) + handle.transform.position, pixelSize.x * 10f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(Flip(GetPixelCoord(new Vector2(0, texSize.y) + pixelOverlap, pixelSize, topLeft, 0)) + handle.transform.position, pixelSize.x * 10f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(Flip(GetPixelCoord(texSize + pixelOverlap, pixelSize, topLeft, 0)) + handle.transform.position, pixelSize.x * 10f);

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(Flip(GetPixelCoord(new Vector2(0, 0) + pixelOverlap, pixelSize, topLeft, 0)) + handle.transform.position, pixelSize.x * 10f);

    }

    private void DrawMatrix(Matrix4x4 matrix, Transform newTransform, Vector3 origin)
    {
        var east = matrix * new Vector3(1, 0, 0);
        var north = matrix * new Vector3(0, 1, 0);
        var up = matrix * new Vector3(0, 0, 1);

        //var centerPos = (handle.transform.position) + Flip(handle.node.BoundaryCenter.ToVector3());

        //Gizmos.DrawWireSphere(handle.transform.localToWorldMatrix * Flip(centerPos), 50f);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, newTransform.localToWorldMatrix * Flip(east) * 4000);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, newTransform.localToWorldMatrix * Flip(north) * 4000);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(origin, newTransform.localToWorldMatrix * Flip(up) * 4000);

        //Gizmos.color = Color.red;
        //Gizmos.DrawRay(origin, (east) * 4000);

        //Gizmos.color = Color.green;
        //Gizmos.DrawRay(origin, (north) * 4000);

        //Gizmos.color = Color.blue;
        //Gizmos.DrawRay(origin, (up) * 4000);
    }

    private void DrawMatrix(Matrix3 matrix)
    {
        var east = matrix * new Vec3(1, 0, 0);
        var north = matrix * new Vec3(0, 1, 0);
        var up = matrix * new Vec3(0, 0, 1);

        //Gizmos.color = Color.red;
        //Gizmos.DrawRay(transform.position, transform.localToWorldMatrix * east.ToVector3() * 400);

        //Gizmos.color = Color.green;
        //Gizmos.DrawRay(transform.position, transform.localToWorldMatrix * north.ToVector3() * 400);

        //Gizmos.color = Color.blue;
        //Gizmos.DrawRay(transform.position, transform.localToWorldMatrix * up.ToVector3() * 400);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.localToWorldMatrix * Flip(east.ToVector3()) * 4000);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.localToWorldMatrix * Flip(north.ToVector3()) * 4000);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.localToWorldMatrix * Flip(up.ToVector3()) * 4000);
    }

    private void DrawRay()
    {
        var vec = UnityEngine.Quaternion.Euler(JawPitchRoll.x, JawPitchRoll.y, JawPitchRoll.z) * Vector3.forward;


        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, vec * 1000);
    }

    [ContextMenu("Latpos/BergKulla")]
    public void BergKulla()
    {
        _latitude = 57.753616f;
        _longitude = 14.072026f;
    }


    [ContextMenu("Latpos/Gärdet")]
    public void Gardet()
    {
        _latitude = 59.338026f;
        _longitude = 18.128561f;
    }
}
