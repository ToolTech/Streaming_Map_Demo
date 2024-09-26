/* 
 * Copyright (C) SAAB AB
 *
 * All rights, including the copyright, to the computer program(s) 
 * herein belong to Saab AB. The program(s) may be used and/or
 * copied only with the written permission of Saab AB, or in
 * accordance with the terms and conditions stipulated in the
 * agreement/contract under which the program(s) have been
 * supplied. 
 * 
 * Information Class:          COMPANY RESTRICTED
 * Defence Secrecy:            UNCLASSIFIED
 * Export Control:             NOT EXPORT CONTROLLED
 */

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
        public ComputeBuffer PlacementMatrix { get; private set; }
        public Vector2 NodeOffset { get; private set; }
        //public ComputeBuffer MinXY { get; private set; }
        public ComputeBuffer TerrainPoints;
        public Texture2D FeatureMap;
        public Texture2D Texture;
        public Texture surfaceHeight;
        public RenderTexture HeightMap;

        public FeatureData(GameObject gameObject, Matrix3D matrix, float density, uint maxSide, float scale = 1000)
        {
            Object = gameObject;
            var stepsize = (1 / density) * 10;

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
            TerrainPoints?.Release();
            PlacementMatrix?.Release();
            HeightMap?.Release();
        }
    }

    public class FoliageFeature : IDisposable
    {
        // list of all instances currently being rendered
        private readonly List<FeatureData> _items = new List<FeatureData>(128);
        // if a go exists in the render list, it exists in this lookup, used to avoid searching the list
        private readonly HashSet<GameObject> _itemLookup = new HashSet<GameObject>();
        
        private Vector2 _resolution;
        private readonly ComputeShader _placement;
        private readonly float _density;
        private readonly float _scale = 10000;

        // *********** buffers ***********
        private ComputeBuffer _mappingBuffer;
        private readonly ComputeBuffer _pointCloud;

        public int FoliageCount
        {
            get { return _items.Count(); }
        }

        public FoliageFeature(int BufferSize, float density, int[] map, ComputeShader computeShader)
        {
            _placement = computeShader;
            _density = density;
            _pointCloud = new ComputeBuffer(BufferSize, sizeof(float) * 8, ComputeBufferType.Append);
            _mappingBuffer = new ComputeBuffer(map.Length, sizeof(int));
            _mappingBuffer.SetData(map);
        }

        public FeatureData AddFoliage(GameObject go, NodeHandle node, RenderTexture heightMap = null, Texture surfaceHeight = null)
        {
            Texture2D featureMap = node.feature;
            Texture2D texture = node.texture;
            Texture height = node.surfaceHeight;

            if (featureMap == null)
            {
                //Debug.LogWarning($"node:{go.name}, no feature Map exist");
                return null;
            }
            if (height == null)
            {
                if (surfaceHeight)
                    height = surfaceHeight;
                else
                    return null;
            }
            if(heightMap == null)
            {
                //Debug.LogWarning($"node:{go.name}, no terrain height exist");
                return null;
            }

            _resolution = new Vector2((float)node.featureInfo.v11, (float)node.featureInfo.v22);
            var maxside = Mathf.Max(featureMap.width, featureMap.height);

            var size = FindBufferSize(featureMap);
            if (size >= ushort.MaxValue * 128)
                return null;

            var data = new FeatureData(go, node.featureInfo, _density, (uint)maxside, _scale)
            {
                FeatureMap = featureMap,
                Texture = texture,
                surfaceHeight = height,
                HeightMap = heightMap
            };

            data.TerrainPoints = new ComputeBuffer(size < 1 ? 1 : size, sizeof(float) * 8, ComputeBufferType.Append);

            FeaturePlacement(data);

            _items.Add(data);
            _itemLookup.Add(go);
            //Debug.LogWarning($"added FeatureData");

            //_mappingBuffer.Release();

            return data;
        }
        public void RemoveFoliage(GameObject gameObj)
        {
            if (!_itemLookup.Contains(gameObj))
                return;

            _itemLookup.Remove(gameObj);

            for (var i = 0; i < _items.Count; ++i)
            {
                if (_items[i].Object != gameObj)
                    continue;

                ClearFeature(_items[i]);

                if ((i + 1) < _items.Count)
                    _items[i] = _items[_items.Count - 1];

                _items.RemoveAt(_items.Count - 1);

                return;
            }
        }
        public void Dispose()
        {
            //Debug.LogWarning($"Begin Dispose FoliageFeature");
            _pointCloud?.Release();
            _mappingBuffer?.Release();

            for (var i = 0; i < _items.Count; ++i)
            {
                ClearFeature(_items[i]);
            }
            _items.Clear();
        }

        // needed to clear old valid tree data from gpu memory, if skipped when frustum culling old trees might get valid/visable
        private void ClearFeature(FeatureData data)
        {
            var kernal = _placement.FindKernel("CSClear");
            _placement.SetBuffer(kernal, "TerrainPoints", data.TerrainPoints);
            _placement.SetInt("BufferCount", data.TerrainPoints.count);
            if (data.TerrainPoints.count > 0)
                _placement.Dispatch(kernal, Mathf.CeilToInt(data.TerrainPoints.count / 128f), 1, 1);
            data.Dispose();
        }

        private int FindBufferSize(Texture2D featureMap)
        {
            var maxSize =
                Mathf.CeilToInt(featureMap.width * _resolution.x * _density) *
                Mathf.CeilToInt(featureMap.height * _resolution.y * _density);

            return Mathf.CeilToInt(maxSize) < 1 ? 1 : Mathf.CeilToInt(maxSize);

            // ################## find minimum memory needed for buffer ##################

            /*
            var bufferKernel = _placement.FindKernel("CSFindBufferSize");
            var result = new uint[1];

            var sizeBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Default);
            sizeBuffer.SetData(result);

            _placement.SetBuffer(bufferKernel, "BufferSize", sizeBuffer);
            _placement.SetTexture(bufferKernel, "SplatMap", node.FeatureMap);
            _placement.SetBuffer(bufferKernel, "FeatureMap", _mappingBuffer);

            int threadsX = Mathf.CeilToInt((node.FeatureMap.width) / (float)8);
            int threadsY = Mathf.CeilToInt((node.FeatureMap.height) / (float)8);

            _placement.Dispatch(bufferKernel, threadsX, threadsY, 1);

            sizeBuffer.GetData(result);
            var percentage = result[0] / (float)(node.FeatureMap.width * node.FeatureMap.height);

            //var maxMem = maxSize * sizeof(float) * 8;
            //var savedMem = maxSize * (1 - percentage) * sizeof(float) * 8;
            //Debug.Log($"worst case {maxMem} Bytes saved memory {savedMem} Bytes");

            sizeBuffer.Release();
            return Mathf.CeilToInt(maxSize * percentage) < 1 ? 1 : Mathf.CeilToInt(maxSize * percentage);
            */
        }
        // used for debuging the min and max height of the node
        private void GetMinMax(string node, Texture2D heightmap)
        {
            var buff = new ComputeBuffer(2, sizeof(uint), ComputeBufferType.Default);
            uint[] max = { 255, 0 };
            buff.SetData(max);

            var maxmin = _placement.FindKernel("CSFindMinMax");
            _placement.SetBuffer(maxmin, "MinXY", buff);
            _placement.SetTexture(maxmin, "HeightSurface", heightmap);

            int threadsX = Mathf.CeilToInt((heightmap.width) / (float)8);
            int threadsY = Mathf.CeilToInt((heightmap.height) / (float)8);

            _placement.Dispatch(maxmin, threadsX, threadsY, 1);

            var tmp = new uint[2];
            buff.GetData(tmp);

            Debug.LogError($"MinMax: {tmp[0]}, {tmp[1]}");
            buff.Release();
        }

        private void FeaturePlacement(FeatureData node)
        {
            var kernelPlacement = _placement.FindKernel("CSPlacement");
            _placement.SetTexture(kernelPlacement, "SplatMap", node.FeatureMap);
            _placement.SetTexture(kernelPlacement, "Texture", node.Texture);
            _placement.SetTexture(kernelPlacement, "HeightMap", node.HeightMap);
            _placement.SetTexture(kernelPlacement, "HeightSurface", node.surfaceHeight);

            _placement.SetVector("heightResolution", new Vector2(node.surfaceHeight.width, node.surfaceHeight.height));
            _placement.SetBuffer(kernelPlacement, "TerrainPoints", node.TerrainPoints);
            _placement.SetBuffer(kernelPlacement, "PixelToWorld", node.PlacementMatrix);
            _placement.SetBuffer(kernelPlacement, "FeatureMap", _mappingBuffer);

            int threadsX = Mathf.CeilToInt((node.FeatureMap.width) / (float)4);
            int threadsY = Mathf.CeilToInt((node.FeatureMap.height) / (float)4);

            node.TerrainPoints.SetCounterValue(0);
            _placement.Dispatch(kernelPlacement, threadsX < 1 ? 1 : threadsX, threadsY < 1 ? 1 : threadsY, 1);
        }

        public Matrix4x4 GetClipToWorld(Camera camera)
        {
            var p = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);// Unity flips its 'Y' vector depending on if its in VR, Editor view or game view etc... (facepalm)
            p[2, 3] = p[3, 2] = 0.0f;
            p[3, 3] = 1.0f;
            //_worldToClip = (p * camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), UnityEngine.Quaternion.identity, Vector3.one);
            return Matrix4x4.Inverse(p * camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), UnityEngine.Quaternion.identity, Vector3.one);
        }

        public ComputeBuffer Cull(Vector4[] frustum, Camera camera, float maxHeight, RenderTexture Depth)
        {
            _pointCloud.SetCounterValue(0);     // only once every frame
            var kernelCull = _placement.FindKernel("CSCull");

            //var clipToWorld = GetClipToWorld(camera);
            Matrix4x4 world2Screen = camera.projectionMatrix * camera.worldToCameraMatrix;

            _placement.SetTexture(kernelCull, "DepthTexture", Depth);
            _placement.SetMatrix("WorldToScreen", world2Screen);
            _placement.SetFloat("maxHeight", maxHeight);
            _placement.SetBuffer(kernelCull, "OutputBuffer", _pointCloud);
            _placement.SetVector("CameraPosition", camera.transform.position);
            _placement.SetVector("CameraRightVector", camera.transform.right);
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