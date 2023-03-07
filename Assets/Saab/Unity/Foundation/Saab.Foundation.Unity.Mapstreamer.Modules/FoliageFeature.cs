using UnityEngine;
using System.Collections.Generic;
using System;
using GizmoSDK.GizmoBase;
using System.Linq;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    public struct FoliagePoint
    {
        Vector3 Position;
        Vector3 Color;
        float Height;
        float Random;
    }

    public class FeatureData : IDisposable
    {
        public GameObject Object { get; private set; }
        public ComputeBuffer Matrix { get; private set; }
        public ComputeBuffer MinXY { get; private set; }
        public ComputeBuffer TerrainPoints;
        public Texture2D FeatureMap;
        public Texture2D Texture;
        public Texture2D surfaceHeight;
        public RenderTexture HeightMap;

        public FeatureData(GameObject gameObject, Matrix3D matrix, float density, float scale = 1)
        {
            Object = gameObject;
            var stepsize = (1 / density) * scale;

            MinXY = new ComputeBuffer(1, sizeof(float) * 2, ComputeBufferType.Default);

            Matrix = new ComputeBuffer(9, sizeof(float), ComputeBufferType.Default);
            float[] data = {
                (float)matrix.v11, (float)matrix.v12, (float)(matrix.v13 % stepsize),
                (float)matrix.v21, (float)matrix.v22, (float)(matrix.v23 % stepsize),
                (float)matrix.v31, (float)matrix.v32, (float)matrix.v33
            };

            Matrix.SetData(data);
        }

        public void Dispose()
        {
            var data = new FoliagePoint[TerrainPoints.count];
            TerrainPoints.SetData(data);
            TerrainPoints.Release();

            HeightMap.Release();
            Matrix.Release();
            MinXY.Release();
        }
    }

    public class FoliageFeature : IDisposable
    {
        // list of all instances currently being rendered
        private readonly List<FeatureData> _items = new List<FeatureData>(128);
        private Vector2 _resolution;
        private readonly ComputeShader _placement;
        private readonly float _density;
        private readonly float _scale = 1000;

        // *********** buffers ***********
        private ComputeBuffer _vertices;
        private ComputeBuffer _indices;
        private ComputeBuffer _texcoords;
        private int _maxVertexCount;
        private int _maxIndexCount;
        private readonly ComputeBuffer _pointCloud;

        public FoliageFeature(int BufferSize, float density, ComputeShader computeShader)
        {
            _placement = computeShader;
            _density = density;
            _pointCloud = new ComputeBuffer(BufferSize, sizeof(float) * 8, ComputeBufferType.Append);
        }

        public void AddFoliage(GameObject go, NodeHandle node)
        {
            Texture2D featureMap = node.feature;
            Texture2D texture = node.texture;
            Texture2D height = node.surfaceHeight;

            if (featureMap == null)
            {
                //Debug.LogWarning($"node:{go.name}, no feature Map exist");
                return;
            }
            if (texture == null)
            {
                //Debug.LogWarning($"node:{go.name}, no texture exist");
                return;
            }
            if(height == null)
            {
                //Debug.LogWarning($"node:{go.name}, no surface height exist");
                return;
            }
            if (!go.activeInHierarchy)
            {
                Debug.LogWarning($"node:{go.name}, gameobject disabled");
                return;
            }


            var mesh = go.GetComponent<MeshFilter>().sharedMesh;
            _resolution = new Vector2((float)node.featureInfo.v11, (float)node.featureInfo.v22);

            var data = new FeatureData(go, node.featureInfo, _density, _scale)
            {
                FeatureMap = featureMap,
                Texture = texture,
                surfaceHeight = height
            };

            GenerateSharedData(data);
            GenerateHeight(data, mesh);
            FeauturePlacement(data);

            _items.Add(data);
            //return toTexture2D(data.HeightMap);
        }
        public void RemoveFoliage(GameObject gameobj)
        {
            for (var i = 0; i < _items.Count; ++i)
            {
                if (_items[i].Object != gameobj)
                    continue;

                _items[i].Dispose();

                if ((i + 1) < _items.Count)
                    _items[i] = _items[_items.Count - 1];

                _items.RemoveAt(_items.Count - 1);

                return;
            }
        }
        public void Dispose()
        {
            for (var i = 0; i < _items.Count; ++i)
            {
                _items[i].Dispose();
            }
            _items.Clear();

            _pointCloud?.Release();
            _vertices?.Release();
            _indices?.Release();
            _texcoords?.Release();
        }

        private void SetupBuffers(Mesh mesh)
        {
            if (mesh.vertexCount > _maxVertexCount)
            {
                if (_vertices != null)
                {
                    _vertices.Release();
                    _texcoords.Release();
                }

                _vertices = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 3, ComputeBufferType.Default);
                _texcoords = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 2, ComputeBufferType.Default);
                _maxVertexCount = _vertices.count;
            }

            if (mesh.GetIndexCount(0) > _maxIndexCount)
            {
                if (_indices != null)
                {
                    _indices.Release();
                }
                _indices = new ComputeBuffer((int)mesh.GetIndexCount(0), sizeof(int), ComputeBufferType.Default);
                _maxIndexCount = (int)mesh.GetIndexCount(0);
            }

        }

        private int FindBufferSize(FeatureData node)
        {
            var maxSize =
                Mathf.CeilToInt((node.FeatureMap.width) * _resolution.x * _density) *
                Mathf.CeilToInt((node.FeatureMap.height) * _resolution.y * _density);

            return maxSize;

            var bufferKernel = _placement.FindKernel("CSFindBufferSize");
            var result = new uint[1];

            var sizeBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Default);
            sizeBuffer.SetData(result);

            _placement.SetBuffer(bufferKernel, "BufferSize", sizeBuffer);
            _placement.SetTexture(bufferKernel, "SplatMap", node.FeatureMap);
            _placement.Dispatch(bufferKernel, Mathf.CeilToInt((node.FeatureMap.width - 2) / 8f) < 1 ? 1 : Mathf.CeilToInt((node.FeatureMap.height - 2) / 8f), 1, 1);
            
            sizeBuffer.GetData(result);
            var percentage = (result[0] * 8 * 8) / (float)((node.FeatureMap.width - 2) * (node.FeatureMap.height - 2));
            Debug.Log($"{(result[0] * 8 * 8)}/{node.FeatureMap.width * node.FeatureMap.height}");

            //var size = Mathf.CeilToInt(maxSize * percentage);
            var area1 = Mathf.Sqrt(result[0] * 8 * 8) * _resolution;
            var fArea = Mathf.Sqrt(node.FeatureMap.width * node.FeatureMap.height) * _resolution;
            //var percentage = area

            //var calcSize = Mathf.CeilToInt(area1.x * _density) * Mathf.CeilToInt(area1.y * _density);

            //Debug.Log($"{size}/{maxSize} or {calcSize}/{maxSize} : {percentage:F3}");

            //sizeBuffer.Release();
            //Debug.Log($"saved: {size * (sizeof(float) * 8) / 1000000f:F2} mb / {maxSize * (sizeof(float) * 8) / 1000000f:F2} mb = {(1 - (size / (float)maxSize)) * 100:F2}%");
            return maxSize;
        }

        private void GenerateSharedData(FeatureData node)
        {
            var size = FindBufferSize(node);
            node.TerrainPoints = new ComputeBuffer(size < 1 ? 1 : size, sizeof(float) * 8, ComputeBufferType.Append);

            var mesh = node.Object.GetComponent<MeshFilter>().sharedMesh;
            _placement.SetVector("terrainResolution", new Vector2(node.FeatureMap.width, node.FeatureMap.height));
            _placement.SetVector("heightResolution", new Vector2(node.surfaceHeight.width, node.surfaceHeight.height));
            _placement.SetVector("terrainSize", mesh.bounds.size);

            _placement.SetVector("Resolution", _resolution);
            _placement.SetFloat("Density", _density);
            _placement.SetMatrix("ObjToWorld", node.Object.transform.localToWorldMatrix);
        }
        private void GenerateHeight(FeatureData node, Mesh mesh)
        {
            // ************* hack to find min uv ************* //

            SetupBuffers(mesh);

            var kernelFindUV = _placement.FindKernel("CSFindMinUv");
            _placement.SetInt("uvCount", mesh.vertexCount);
            _placement.SetBuffer(kernelFindUV, "MinXY", node.MinXY);
            _placement.SetBuffer(kernelFindUV, "surfaceUVs", _texcoords);
            _placement.Dispatch(kernelFindUV, 1, 1, 1);

            var data = new Vector2[1];
            node.MinXY.GetData(data);

            var offsetX = ((node.FeatureMap.width - 2) * _resolution.x / 2 - mesh.bounds.extents.x);
            offsetX = data[0].x > 0 ? -offsetX : offsetX;
            var offsetY = ((node.FeatureMap.height - 2) * _resolution.y / 2 - mesh.bounds.extents.z);
            offsetY = data[0].y > 0 ? offsetY : -offsetY;

            var center = mesh.bounds.center;
            center.x += offsetX;
            center.z += offsetY;

            var extents = new Vector3(((node.FeatureMap.width - 2) * _resolution.x / 2), mesh.bounds.size.y, (node.FeatureMap.height * _resolution.y / 2));
            _placement.SetVector("MeshBoundsMax", center + extents);

            // ************* Generate Height Map  ************* //

            var kernelHeight = _placement.FindKernel("CSHeightMap");

            node.HeightMap = new RenderTexture(node.FeatureMap.width, node.FeatureMap.height, 24, RenderTextureFormat.RFloat);
            node.HeightMap.enableRandomWrite = true;
            node.HeightMap.Create();

            _vertices.SetData(mesh.vertices);
            _indices.SetData(mesh.GetIndices(0));
            _texcoords.SetData(mesh.uv);

            var triangleCount = Mathf.CeilToInt(mesh.GetIndices(0).Length / 3f);

            _placement.SetInt("indexCount", triangleCount);
            _placement.SetBuffer(kernelHeight, "surfaceVertices", _vertices);
            _placement.SetBuffer(kernelHeight, "surfaceIndices", _indices);
            _placement.SetBuffer(kernelHeight, "surfaceUVs", _texcoords);
            _placement.SetTexture(kernelHeight, "HeightMap", node.HeightMap);

            var threads = Mathf.CeilToInt(triangleCount / 4f) < 1 ? 1 : Mathf.CeilToInt(triangleCount / 4f);
            _placement.Dispatch(kernelHeight, threads, 1, 1);
        }

        private void FeauturePlacement(FeatureData node)
        {
            var kernelPlacement = _placement.FindKernel("CSPlacement");
            _placement.SetTexture(kernelPlacement, "SplatMap", node.FeatureMap);
            _placement.SetTexture(kernelPlacement, "Texture", node.Texture);
            _placement.SetTexture(kernelPlacement, "HeightMap", node.HeightMap);
            _placement.SetTexture(kernelPlacement, "HeightSurface", node.surfaceHeight);

            _placement.SetBuffer(kernelPlacement, "TerrainPoints", node.TerrainPoints);
            _placement.SetBuffer(kernelPlacement, "PixelToWorld", node.Matrix);

            int threadsX = Mathf.CeilToInt((node.FeatureMap.width) / (float)4);
            int threadsY = Mathf.CeilToInt((node.FeatureMap.height) / (float)4);

            node.TerrainPoints.SetCounterValue(0);
            _placement.Dispatch(kernelPlacement, threadsX < 1 ? 1 : threadsX, threadsY < 1 ? 1 : threadsY, 1);
        }
        public ComputeBuffer Cull(Vector4[] frustum, Camera camera, float maxHeight)
        {
            _pointCloud.SetCounterValue(0);     // only once every frame
            var kernelCull = _placement.FindKernel("CSCull");

            _placement.SetFloat("maxHeight", maxHeight);
            _placement.SetBuffer(kernelCull, "OutputBuffer", _pointCloud);
            _placement.SetVector("CameraPosition", camera.transform.position);
            _placement.SetVectorArray("frustumPlanes", frustum);

            var count = 0;
            var points = 0;

            foreach (var item in _items)
            {
                // don't cull disabled objects
                if (!item.Object.activeInHierarchy)
                    continue;

                count++;
                points += item.TerrainPoints.count;
                _placement.SetBuffer(kernelCull, "InputBuffer", item.TerrainPoints);
                _placement.SetMatrix("ObjToWorld", item.Object.transform.localToWorldMatrix);
                _placement.Dispatch(kernelCull, Mathf.CeilToInt(item.TerrainPoints.count / 128f) < 1 ? 1 : Mathf.CeilToInt(item.TerrainPoints.count / 128f), 1, 1);
            }

            //Debug.LogWarning($"culled {count} nodes with average {Math.Round(points / (float)count)} points");

            return _pointCloud;
        }
    }
}