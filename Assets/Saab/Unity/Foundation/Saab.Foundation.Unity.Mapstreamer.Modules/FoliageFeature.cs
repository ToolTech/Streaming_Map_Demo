using UnityEngine;
using System.Collections.Generic;
using System;
using GizmoSDK.GizmoBase;
using System.Linq;
//using System.IO;

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
        public ComputeBuffer PlacementMatrix { get; private set; }
        public Vector2 NodeOffset { get; private set; }
        public ComputeBuffer MinXY { get; private set; }
        public ComputeBuffer TerrainPoints;
        public Texture2D FeatureMap;
        public Texture2D Texture;
        public Texture2D surfaceHeight;
        public Texture2D surfaceHeightNew;
        public RenderTexture HeightMap;

        public FeatureData(GameObject gameObject, Matrix3D matrix, float density, uint maxSide, float scale = 1000)
        {
            Object = gameObject;
            var stepsize = (1 / density) * 10;

            MinXY = new ComputeBuffer(2, sizeof(uint), ComputeBufferType.Default);
            uint[] max = { maxSide * 2, maxSide * 2 };
            MinXY.SetData(max);

            PlacementMatrix = new ComputeBuffer(9, sizeof(float), ComputeBufferType.Default);
            float[] data = {
                (float)matrix.v11, (float)matrix.v12, (float)((matrix.v13) % stepsize),
                (float)matrix.v21, (float)matrix.v22, (float)((matrix.v23) % stepsize),
                (float)matrix.v31, (float)matrix.v32, (float)matrix.v33
            };

            NodeOffset = new Vector2((float)(matrix.v13 + matrix.v11) % scale, (float)(matrix.v23 + matrix.v22) % scale);
            PlacementMatrix.SetData(data);
        } 

        public void Dispose()
        {
            var data = new FoliagePoint[TerrainPoints.count];
            TerrainPoints.SetData(data);
            TerrainPoints.Release();

            HeightMap.Release();
            PlacementMatrix.Release();
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

        public FeatureData AddFoliage(GameObject go, NodeHandle node)
        {
            Texture2D featureMap = node.feature;
            Texture2D texture = node.texture;
            Texture2D height = node.surfaceHeight;

            if (featureMap == null)
            {
                //Debug.LogWarning($"node:{go.name}, no feature Map exist");
                return null;
            }
            if (texture == null)
            {
                //Debug.LogWarning($"node:{go.name}, no texture exist");
                return null;
            }
            if (height == null)
            {
                //Debug.LogWarning($"node:{go.name}, no surface height exist");
                return null;
            }
            if (!go.activeInHierarchy)
            {
                Debug.LogWarning($"node:{go.name}, gameobject disabled");
                return null;
            }

            //if (go.name != "15_48_1")
            //    return;

            var mesh = go.GetComponent<MeshFilter>().sharedMesh;
            _resolution = new Vector2((float)node.featureInfo.v11, (float)node.featureInfo.v22);

            var maxside = Mathf.Max(featureMap.width, featureMap.height);

            var data = new FeatureData(go, node.featureInfo, _density, (uint)maxside, _scale)
            {
                FeatureMap = featureMap,
                Texture = texture,
                surfaceHeight = height
            };

            GenerateSharedData(data);
            GenerateHeight(data, mesh);
            FeauturePlacement(data);

            _items.Add(data);
            return data;
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
        public int GetVideoMemoryUsage()
        {
            var size = 0;
            foreach(var item in _items)
            {
                size += item.TerrainPoints.stride * item.TerrainPoints.count;
                size += item.HeightMap.width * item.HeightMap.width * sizeof(float);
            }
            size += _pointCloud.stride * _pointCloud.count;
            size += _indices.stride * _indices.count;
            size += _vertices.stride * _vertices.count;
            size += _texcoords.stride * _texcoords.count;
            return size;
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

        Texture2D toTexture2D(RenderTexture rTex)
        {
            var tmp = RenderTexture.active;
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RFloat, false);
            tex.filterMode = rTex.filterMode;
            // ReadPixels looks at the active RenderTexture.
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            RenderTexture.active = tmp;
            return tex;
        }
        //private void SaveImage(Texture2D tex, string name)
        //{
        //    if (!tex.isReadable)
        //    {
        //        Debug.LogWarning("could not read file");
        //        return;
        //    }

        //    var data = tex.EncodeToPNG();
        //    var dirPath = Application.dataPath + "/../SaveImages/";
        //    if (!Directory.Exists(dirPath))
        //    {
        //        Directory.CreateDirectory(dirPath);
        //    }
        //    File.WriteAllBytes(dirPath + name + ".png", data);
        //}

        private RenderTexture GenerateSurfaceHeight(FeatureData node)
        {
            var kernel = _placement.FindKernel("CSUpSampleVegetation");

            var surfaceHeightMap = new RenderTexture(node.surfaceHeight.width , node.surfaceHeight.height, 24, RenderTextureFormat.RFloat);
            surfaceHeightMap.filterMode = FilterMode.Point;
            surfaceHeightMap.enableRandomWrite = true;
            surfaceHeightMap.Create();

            _placement.SetVector("heightResolution", new Vector2(node.surfaceHeight.width, node.surfaceHeight.height));
            _placement.SetTexture(kernel, "VegetationHeightMap", surfaceHeightMap);
            _placement.SetTexture(kernel, "HeightSurface", node.surfaceHeight);
            _placement.SetTexture(kernel, "SplatMap", node.FeatureMap);

            var threadx = Mathf.CeilToInt(surfaceHeightMap.width / 4f) < 1 ? 1 : Mathf.CeilToInt(surfaceHeightMap.width / 4f);
            var thready = Mathf.CeilToInt(surfaceHeightMap.height / 4f) < 1 ? 1 : Mathf.CeilToInt(surfaceHeightMap.height / 4f);

            _placement.Dispatch(kernel, threadx, thready, 1);

            return surfaceHeightMap;
        }
        private void GenerateSharedData(FeatureData node)
        {
            var size = FindBufferSize(node);
            node.TerrainPoints = new ComputeBuffer(size < 1 ? 1 : size, sizeof(float) * 8, ComputeBufferType.Append);

            var mesh = node.Object.GetComponent<MeshFilter>().sharedMesh;
            _placement.SetVector("terrainResolution", new Vector2(node.FeatureMap.width, node.FeatureMap.height));
            _placement.SetVector("terrainSize", mesh.bounds.size);

            _placement.SetVector("NodeOffset", node.NodeOffset);
            _placement.SetVector("Resolution", _resolution);
            _placement.SetFloat("Density", _density);
            _placement.SetMatrix("ObjToWorld", node.Object.transform.localToWorldMatrix);
        }
        private void GenerateHeight(FeatureData node, Mesh mesh)
        {
            // ************* hack to find min uv, a bit of a performance thief ************* //

            SetupBuffers(mesh);

            _vertices.SetData(mesh.vertices);
            _indices.SetData(mesh.GetIndices(0));
            _texcoords.SetData(mesh.uv);

            var kernelFindUV = _placement.FindKernel("CSFindMinUv");
            _placement.SetInt("uvCount", mesh.vertexCount);
            _placement.SetBuffer(kernelFindUV, "MinXY", node.MinXY);
            _placement.SetBuffer(kernelFindUV, "surfaceUVs", _texcoords);
            var threads = Mathf.CeilToInt(mesh.vertexCount / 32f) < 1 ? 1 : Mathf.CeilToInt(mesh.vertexCount / 32f);
            _placement.Dispatch(kernelFindUV, threads, 1, 1);

            var data = new uint[2];
            node.MinXY.GetData(data);

            // ************* Find center of Node ************* //

            var offsetX = ((node.FeatureMap.width - 2) * _resolution.x / 2 - mesh.bounds.extents.x);
            offsetX = data[0] > 0 ? -offsetX : offsetX;
            var offsetY = ((node.FeatureMap.height - 2) * _resolution.y / 2 - mesh.bounds.extents.z);
            offsetY = data[1] > 0 ? offsetY : -offsetY;

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

            var triangleCount = Mathf.CeilToInt(mesh.GetIndices(0).Length / 3f);

            _placement.SetInt("indexCount", triangleCount);
            _placement.SetBuffer(kernelHeight, "surfaceVertices", _vertices);
            _placement.SetBuffer(kernelHeight, "surfaceIndices", _indices);
            _placement.SetBuffer(kernelHeight, "surfaceUVs", _texcoords);
            _placement.SetTexture(kernelHeight, "HeightMap", node.HeightMap);

            threads = Mathf.CeilToInt(triangleCount / 4f) < 1 ? 1 : Mathf.CeilToInt(triangleCount / 4f);
            _placement.Dispatch(kernelHeight, threads, 1, 1);
        }
        private void FeauturePlacement(FeatureData node)
        {
            //var surfaceHeight = GenerateSurfaceHeight(node);

            var kernelPlacement = _placement.FindKernel("CSPlacement");
            _placement.SetTexture(kernelPlacement, "SplatMap", node.FeatureMap);
            _placement.SetTexture(kernelPlacement, "Texture", node.Texture);
            _placement.SetTexture(kernelPlacement, "HeightMap", node.HeightMap);
            _placement.SetTexture(kernelPlacement, "HeightSurface", node.surfaceHeight);

            _placement.SetVector("heightResolution", new Vector2(node.surfaceHeight.width, node.surfaceHeight.height));
            //node.surfaceHeightNew = toTexture2D(surfaceHeight);
            //SaveImage(toTexture2D(surfaceHeight), Guid.NewGuid().ToString());

            _placement.SetBuffer(kernelPlacement, "TerrainPoints", node.TerrainPoints);
            _placement.SetBuffer(kernelPlacement, "PixelToWorld", node.PlacementMatrix);

            int threadsX = Mathf.CeilToInt((node.FeatureMap.width) / (float)4);
            int threadsY = Mathf.CeilToInt((node.FeatureMap.height) / (float)4);

            node.TerrainPoints.SetCounterValue(0);
            _placement.Dispatch(kernelPlacement, threadsX < 1 ? 1 : threadsX, threadsY < 1 ? 1 : threadsY, 1);

            //surfaceHeight.Release();
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

            return _pointCloud;
        }
    }
}