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

using ProfilerMarker = global::Unity.Profiling.ProfilerMarker;
using ProfilerCategory = global::Unity.Profiling.ProfilerCategory;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    public struct FoliagePoint
    {
        Vector3 Position;
        Vector3 Color;
        float Height;
        float Random;
    }

    public struct FeatureData : IDisposable
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

            HeightMap = null;
            surfaceHeight = null;
            Texture = null;
            TerrainPoints = null;
            FeatureMap = null;
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
        private readonly int _kernelCull;
        private readonly int _kernelClear;
        private readonly int _kernelPlacement;
        private readonly float _density;
        private readonly float _scale = 10000;

        // *********** buffers ***********
        private ComputeBuffer _mappingBuffer;
        private readonly ComputeBuffer _pointCloud;

        public int FoliageCount
        {
            get { return _items.Count; }
        }

        public FoliageFeature(int BufferSize, float density, int[] map, ComputeShader computeShader)
        {
            _placement = computeShader;
            _kernelCull = _placement.FindKernel("CSCull");
            _kernelClear = _placement.FindKernel("CSClear");
            _kernelPlacement = _placement.FindKernel("CSPlacement");
            _density = density;
            _pointCloud = new ComputeBuffer(BufferSize <= 0 ? 1 : BufferSize, sizeof(float) * 8, ComputeBufferType.Append);

            // we only need to set this once
            _placement.SetBuffer(_kernelCull, PlacementParameterID.OutputBuffer, _pointCloud);
            _mappingBuffer = new ComputeBuffer(map.Length, sizeof(int));
            _mappingBuffer.SetData(map);
            _placement.SetBuffer(_kernelPlacement, PlacementParameterID.FeatureMap, _mappingBuffer);
        }

        public bool AddFoliage(GameObject go, NodeHandle node, RenderTexture heightMap, Texture surfaceHeight = null)
        {
            if (heightMap == null)
                return false;

            Texture2D featureMap = node.feature;
            if (featureMap == null)
                return false;

            Texture height = node.surfaceHeight ?? surfaceHeight;
            if (height == null)
                return false;

            var size = FindBufferSize(featureMap);
            if (size >= ushort.MaxValue * 128)
                return false;

            _resolution = new Vector2((float)node.featureInfo.v11, (float)node.featureInfo.v22);
            var maxside = Mathf.Max(featureMap.width, featureMap.height);

            Texture2D texture = node.texture;
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

            return true;
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
        private void ClearFeature(in FeatureData data)
        {
            _placement.SetBuffer(_kernelClear, PlacementParameterID.TerrainPoints, data.TerrainPoints);
            _placement.SetInt(PlacementParameterID.BufferCount, data.TerrainPoints.count);
            if (data.TerrainPoints.count > 0)
                _placement.Dispatch(_kernelClear, Mathf.CeilToInt(data.TerrainPoints.count / 128f), 1, 1);

            data.Dispose();
        }

        private int FindBufferSize(Texture2D featureMap)
        {
            var maxSize =
                Mathf.CeilToInt(featureMap.width * _resolution.x * _density) *
                Mathf.CeilToInt(featureMap.height * _resolution.y * _density);

            return Mathf.CeilToInt(maxSize) < 1 ? 1 : Mathf.CeilToInt(maxSize);

            //// ################## find minimum memory needed for buffer ##################
            //
            //
            //var bufferKernel = _placement.FindKernel("CSFindBufferSize");
            //var result = new uint[1];
            //
            //var sizeBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Default);
            //sizeBuffer.SetData(result);
            //
            //_placement.SetBuffer(bufferKernel, "BufferSize", sizeBuffer);
            //_placement.SetTexture(bufferKernel, "SplatMap", node.feature);
            //_placement.SetBuffer(bufferKernel, "FeatureMap", _mappingBuffer);
            //
            //int threadsX = Mathf.CeilToInt((node.feature.width) / (float)8);
            //int threadsY = Mathf.CeilToInt((node.feature.height) / (float)8);
            //
            //_placement.Dispatch(bufferKernel, threadsX, threadsY, 1);
            //
            //sizeBuffer.GetData(result);
            //var percentage = result[0] / (float)(node.feature.width * node.feature.height);
            //
            ////var maxMem = maxSize * sizeof(float) * 8;
            ////var savedMem = maxSize * (1 - percentage) * sizeof(float) * 8;
            ////Debug.Log($"worst case {maxMem} Bytes saved memory {savedMem} Bytes");
            //
            //sizeBuffer.Release();
            //return Mathf.CeilToInt(maxSize * percentage) < 1 ? 1 : Mathf.CeilToInt(maxSize * percentage);
            
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
            _placement.SetTexture(_kernelPlacement, PlacementParameterID.SplatMap, node.FeatureMap);
            _placement.SetTexture(_kernelPlacement, PlacementParameterID.Texture, node.Texture);
            _placement.SetTexture(_kernelPlacement, PlacementParameterID.HeightMap, node.HeightMap);
            _placement.SetTexture(_kernelPlacement, PlacementParameterID.HeightSurface, node.surfaceHeight);

            _placement.SetVector(PlacementParameterID.heightResolution, new Vector2(node.surfaceHeight.width, node.surfaceHeight.height));
            _placement.SetBuffer(_kernelPlacement, PlacementParameterID.TerrainPoints, node.TerrainPoints);
            _placement.SetBuffer(_kernelPlacement, PlacementParameterID.PixelToWorld, node.PlacementMatrix);
            

            int threadsX = Mathf.CeilToInt(node.FeatureMap.width / 4f);
            int threadsY = Mathf.CeilToInt(node.FeatureMap.height / 4f);

            node.TerrainPoints.SetCounterValue(0);
            _placement.Dispatch(_kernelPlacement, threadsX < 1 ? 1 : threadsX, threadsY < 1 ? 1 : threadsY, 1);
        }

        public Matrix4x4 GetClipToWorld(Camera camera)
        {
            var p = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);// Unity flips its 'Y' vector depending on if its in VR, Editor view or game view etc... (facepalm)
            p[2, 3] = p[3, 2] = 0.0f;
            p[3, 3] = 1.0f;
            //_worldToClip = (p * camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), UnityEngine.Quaternion.identity, Vector3.one);
            return Matrix4x4.Inverse(p * camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), UnityEngine.Quaternion.identity, Vector3.one);
        }

        private static class PlacementParameterID
        {
            public static readonly int DepthTexture = Shader.PropertyToID("DepthTexture");
            public static readonly int WorldToScreen = Shader.PropertyToID("WorldToScreen");
            public static readonly int maxHeight = Shader.PropertyToID("maxHeight");
            public static readonly int OutputBuffer = Shader.PropertyToID("OutputBuffer");
            public static readonly int CameraPosition = Shader.PropertyToID("CameraPosition");
            public static readonly int CameraRightVector = Shader.PropertyToID("CameraRightVector");
            public static readonly int frustumPlanes = Shader.PropertyToID("frustumPlanes");
            public static readonly int InputBuffer = Shader.PropertyToID("InputBuffer");
            public static readonly int ObjToWorld = Shader.PropertyToID("ObjToWorld");
            public static readonly int TerrainPoints = Shader.PropertyToID("TerrainPoints");
            public static readonly int BufferCount = Shader.PropertyToID("BufferCount");
            public static readonly int SplatMap = Shader.PropertyToID("SplatMap");
            public static readonly int Texture = Shader.PropertyToID("Texture");
            public static readonly int HeightMap = Shader.PropertyToID("HeightMap");
            public static readonly int HeightSurface = Shader.PropertyToID("HeightSurface");
            public static readonly int heightResolution = Shader.PropertyToID("heightResolution");
            public static readonly int PixelToWorld = Shader.PropertyToID("PixelToWorld");
            public static readonly int FeatureMap = Shader.PropertyToID("FeatureMap");
        }

        private static readonly ProfilerMarker _profilerMarker = new ProfilerMarker(ProfilerCategory.Render, "Foliage-Cull");

        public ComputeBuffer Cull(Vector4[] frustum, Camera camera, float maxHeight, RenderTexture Depth)
        {
            _profilerMarker.Begin();

            _pointCloud.SetCounterValue(0);     // only once every frame
            
            Matrix4x4 world2Screen = camera.projectionMatrix * camera.worldToCameraMatrix;

            _placement.SetTexture(_kernelCull, PlacementParameterID.DepthTexture, Depth);
            _placement.SetMatrix(PlacementParameterID.WorldToScreen, world2Screen);
            _placement.SetFloat(PlacementParameterID.maxHeight, maxHeight);
            _placement.SetVector(PlacementParameterID.CameraPosition, camera.transform.position);
            _placement.SetVector(PlacementParameterID.CameraRightVector, camera.transform.right);
            _placement.SetVectorArray(PlacementParameterID.frustumPlanes, frustum);

            var count = 0;
            var points = 0;

            for (var i = 0; i < _items.Count; ++i)
            {
                var item = _items[i];
                var go = item.Object;

                // don't cull disabled objects
                if (!go.activeInHierarchy)
                    continue;

                count++;

                int itemPoints = item.TerrainPoints.count;
                points += itemPoints;
                int groups = Mathf.CeilToInt(itemPoints / 128f);

                _placement.SetBuffer(_kernelCull, PlacementParameterID.InputBuffer, item.TerrainPoints);
                _placement.SetMatrix(PlacementParameterID.ObjToWorld, go.transform.localToWorldMatrix);

                _placement.Dispatch(_kernelCull, groups < 1 ? 1 : groups, 1, 1);
            }

            _profilerMarker.End();
            return _pointCloud;
        }
    }
}