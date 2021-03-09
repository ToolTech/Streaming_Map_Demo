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

using Saab.Unity.Core.ComputeExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    public class TreeModule : TerrainFeature
    {
        [Header("****** SETTINGS ******")]
        public TerrainTextures[] TreeTextures;
        public Texture2D PerlinNoise;
        public Texture2D DefaultSplatMap;
        public ComputeShader ComputeShader; // <-- TODO: Replace with 1 shader per responsibility (generator, culling, rendering)
        public int BufferLimit = 1000000;
        public bool UsePlacementMap;
        public bool PointCloud;
        public bool MeshTree;

        // maximum concurrent GPU jobs, higher values increases memory footprint
        private const int MAX_JOBS_PER_FRAME = 6;

        // list of all current jobs being processed by the GPU
        private readonly List<JobOutput> _currentJobs = new List<JobOutput>(MAX_JOBS_PER_FRAME);

        // list of all jobs not yet processed
        private readonly List<PendingJob> _pendingJobs = new List<PendingJob>();

        // list of all instances currently being rendered
        private readonly List<Item> _items = new List<Item>(128);

        // used to draw all instances
        private RenderingShader _renderingShader;

        // used to generate instance data from a mesh
        private readonly Stack<InstanceGenerator> _pointGenerators = new Stack<InstanceGenerator>();

        // TEMP TEST JUNK
        public Mesh TestMesh;
        public Material TestMat;

        private void Start()
        {
            if (PerlinNoise == null)
            {
                PerlinNoise = Resources.Load("Textures/PerlinNoiseRGB") as Texture2D;
            }

            _renderingShader = new RenderingShader(ComputeShader, Shader, BufferLimit, MeshTree, TestMesh, TestMat)
            {
                Noise = PerlinNoise,
                ColorVariance = PerlinNoise,
            };

            _renderingShader.DebugMode = DebugMode;

#if UNITY_ANDROID
            var format = TextureFormat.ARGB32;
            Debug.Log("Tree Use ETC2");
#else
            var format = TextureFormat.DXT5;
#endif

            // ...
            _renderingShader.SetBillboardData(Create2DArray(TreeTextures, format),
                TreeTextures.Select(x => x.GetMinMaxWidthHeight).ToArray(),
                TreeTextures.Select(x => x.Yoffset).ToArray());

            _renderingShader.SetQuads(GetQuads(TreeTextures));
        }

        public void AddTree(GameObject go, Texture2D placementMap = null)
        {
            var meshFilter = go.GetComponent<MeshFilter>();
            if (!meshFilter)
                return;

            var meshRenderer = go.GetComponent<MeshRenderer>();
            if (!meshRenderer)
                return;

            var material = meshRenderer.material;
            if (!material)
                return;

            var diffuse = material.mainTexture as Texture2D;
            if (diffuse == null)
                return;

            System.Diagnostics.Debug.Assert(meshFilter.mesh.GetTopology(0) == MeshTopology.Triangles);

            _pendingJobs.Add(new PendingJob()
            {
                GameObject = go,
                Mesh = meshFilter.mesh,
                Diffuse = diffuse,
                PlacementMap = placementMap
            });
        }
        public void RemoveTree(GameObject gameobj)
        {
            for (var i = 0; i < _currentJobs.Count; ++i)
            {
                if (_currentJobs[i].GameObject != gameobj)
                    continue;

                _currentJobs[i].PointBuffer.SafeRelease();
                _pointGenerators.Push(_currentJobs[i].Generator);

                if ((i + 1) < _currentJobs.Count)
                    _currentJobs[i] = _currentJobs[_currentJobs.Count - 1];

                _currentJobs.RemoveAt(_currentJobs.Count - 1);

                return;
            }

            for (var i = 0; i < _items.Count; ++i)
            {
                if (_items[i].GameObject != gameobj)
                    continue;

                _items[i].Dispose();

                if ((i + 1) < _items.Count)
                    _items[i] = _items[_items.Count - 1];

                _items.RemoveAt(_items.Count - 1);

                return;
            }

            for (var i = 0; i < _pendingJobs.Count; ++i)
            {
                if (_pendingJobs[i].GameObject != gameobj)
                    continue;

                if ((i + 1) < _pendingJobs.Count)
                    _pendingJobs[i] = _pendingJobs[_pendingJobs.Count - 1];

                _pendingJobs.RemoveAt(_pendingJobs.Count - 1);

                return;
            }
        }

        private void AddJobs()
        {
            // TODO: Can we avoid sorting per-frame? Change swap-remove to something else and only sort on
            if (SortByDistance)
            {
                _pendingJobs.Sort((a, b) =>
                {
                    var d1 = a.GameObject.transform.position.sqrMagnitude;
                    var d2 = b.GameObject.transform.position.sqrMagnitude;
                    return d1.CompareTo(d2);
                });
            }

            for (var i = 0; i < _pendingJobs.Count; ++i)
            {
                var job = _pendingJobs[i];
                var go = job.GameObject;

                // get new job, according to certain rules (maybe priority later)
                if (!go.activeInHierarchy)
                    continue;

                var mesh = job.Mesh;
                var bounds = mesh.bounds;

                var extents = bounds.extents;
                var maxExtent = Mathf.Max(extents.x, extents.y, extents.z);
                var centerWorld = go.transform.TransformPoint(bounds.center);

                var frustum = _frustum;
                _frustum[5].w += maxExtent + DrawDistance;

                if (!IsInFrustum(centerWorld - CameraPosition, -maxExtent * 1.75f))
                    continue;

                var triangleCount = mesh.GetIndices(0).Length / 3f;
                var bufferSize = Math.Max(1, (maxExtent * maxExtent) / Density);

                bufferSize = bufferSize < mesh.vertexCount ? mesh.vertexCount : bufferSize;

                _frustum = frustum;

                if (_pointGenerators.Count == 0)
                {
                    var feature = InstanceGenerator.Feature.Tree;
                    if (PointCloud)
                    {
                        feature = InstanceGenerator.Feature.PointCloud;
                    }

                    _pointGenerators.Push(new InstanceGenerator(Instantiate(ComputeShader), feature)
                    {
                        Density = Density,
                        SplatMap = DefaultSplatMap
                    });
                }

                var pointGenerator = _pointGenerators.Pop();
                var outputBuffer = new ComputeBuffer(Mathf.CeilToInt(bufferSize), sizeof(float) * 4, ComputeBufferType.Append);

                outputBuffer.SetCounterValue(0);
                pointGenerator.SetMesh(mesh, PointCloud);
                pointGenerator.PlacementMapEnabled = false;

                if (job.PlacementMap != null && UsePlacementMap)
                {
                    pointGenerator.PlacementMapEnabled = true;
                    pointGenerator.PlacementMap = job.PlacementMap;
                }
                else
                {
                    pointGenerator.PlacementMap = new Texture2D(job.Diffuse.width, job.Diffuse.height);
                }

                pointGenerator.ColorMap = job.Diffuse;
                pointGenerator.OutputBuffer = outputBuffer;

                var threadGroups = Mathf.CeilToInt(triangleCount / 16f);
                pointGenerator.Dispatch(threadGroups > 0 ? threadGroups : 1);

                // swap remove
                if ((i + 1) < _pendingJobs.Count)
                    _pendingJobs[i] = _pendingJobs[_pendingJobs.Count - 1];
                _pendingJobs.RemoveAt(_pendingJobs.Count - 1);

                _currentJobs.Add(new JobOutput()
                {
                    GameObject = go,
                    Bounds = bounds,
                    PointBuffer = outputBuffer,
                    Generator = pointGenerator
                });

                if (_currentJobs.Count == MAX_JOBS_PER_FRAME)
                    return;
            }
        }

        public int GetMemoryFootprint
        {
            get
            {
                var size = GetModuleBufferMemory(_items);
                size += GetModuleBufferMemory(_currentJobs);
                size += _renderingShader.GetMemoryFootPrint;
                //Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, "Grass :: total buffer memory size {0} MB", size / 1000000f);
                return size;
            }
        }

        private void Render()
        {
            var camera = CurrentCamera;

            if (!camera)
                return;

            _renderingShader.Depth = DepthTexture;

            //var sw = System.Diagnostics.Stopwatch.StartNew();
            for (var i = 0; i < _currentJobs.Count; ++i)
            {
                var job = _currentJobs[i];

                var cullShader = new CullingShader(Instantiate(ComputeShader))
                {
                    InputBuffer = job.PointBuffer,
                    RenderBufferNear = _renderingShader.RenderBufferNear,
                    RenderBufferFar = _renderingShader.RenderBufferFar,
                };


                var item = new Item()
                {
                    GameObject = job.GameObject,
                    CullShader = cullShader,
                    Bounds = job.Bounds,
                };

                _items.Add(item);

                _pointGenerators.Push(job.Generator);
            }

            _currentJobs.Clear();

            AddJobs();

            var fadeFarAmmount = DrawDistance / 3;
            var fadeFarValue = DrawDistance - fadeFarAmmount;

            fadeFarAmmount = fadeFarAmmount > 0 ? fadeFarAmmount : 1;
            fadeFarValue = fadeFarValue > 0 ? fadeFarValue : 1;

            _renderingShader.SetNearFade(NearFadeStart, NearFadeEnd);
            _renderingShader.SetFarFade(fadeFarValue, fadeFarAmmount);

            _renderingShader.Wind = Wind;
            _renderingShader.Frustum = _frustum;

            _renderingShader.RenderBegin();


            GameObject any = null;
            float maxSide = 0;

            // Culling      
            for (var i = 0; i < _items.Count; ++i)
            {
                var item = _items[i];

                var go = item.GameObject;
                if (!go.activeInHierarchy)
                    continue;

                any = go;

                var bounds = item.Bounds;

                var maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z) * 2;
                maxSide = maxExtent + DrawDistance;

                var centerWorld = go.transform.TransformPoint(bounds.center);

                var frustum = _frustum;
                _frustum[5].w += maxSide;

                if (!IsInFrustum(centerWorld - CameraPosition, (-maxSide * 1.75f)))
                    continue;

                _frustum[5].w = DrawDistance;

                var cullShader = item.CullShader;
                cullShader.LocalToWorld = go.transform.localToWorldMatrix;
                cullShader.Frustum = frustum;
                cullShader.CameraPosition = CameraPosition;

                cullShader.Dispatch();
            }

            if (any == null)
                return;

            // TODO: 
            var roiTransform = FindFirstNodeParent(any.transform);

            var worldToLocal = roiTransform == null ? Matrix4x4.identity : roiTransform.worldToLocalMatrix;

            _renderingShader.WorldToLocal = worldToLocal;
            _renderingShader.ViewDirection = CurrentCamera.transform.forward;

            var renderBounds = new Bounds(Vector3.zero, new Vector3(maxSide, maxSide, maxSide));

            var shadows = DrawShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderingShader.ShadowCastingMode = shadows;

            if (MeshTree)
            {
                _renderingShader.RenderEnd3D(renderBounds, TestMesh);
            }
            else
            {
                _renderingShader.RenderEnd(renderBounds);
            }
        }

        public void Camera_OnPostTraverse()
        {
            _renderingShader.DebugMode = DebugMode;
            Render();
        }

        private void OnDestroy()
        {
            for (var i = 0; i < _items.Count; ++i)
                _items[i].Dispose();

            for (var i = 0; i < _currentJobs.Count; ++i)
            {
                _currentJobs[i].PointBuffer.SafeRelease();
                _currentJobs[i].Generator.Dispose();
            }

            while (_pointGenerators.Count > 0)
                _pointGenerators.Pop().Dispose();

            if (_renderingShader != null)
                _renderingShader.Dispose();
        }
    }
}
