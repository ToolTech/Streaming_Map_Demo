using Saab.Unity.Core.ComputeExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    public class GrassModule : TerrainFeature
    {
        [Header("****** SETTINGS ******")]
        public TerrainTextures[] GrassTextures;
        public Texture2D PerlinNoise;
        public Texture2D DefaultSplatMap;
        public ComputeShader ComputeShader; // <-- TODO: Replace with 1 shader per responsibility (generator, culling, rendering)

        // maximum concurrent GPU jobs, higher values increases memory footprint
        private const int MAX_JOBS_PER_FRAME = 2;

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

            //var subMeshIndex = 0;
            //subMeshIndex = Mathf.Clamp(subMeshIndex, 0, TestMesh.subMeshCount - 1);
            //_closeInderectBuffer.SetData(new uint[5] { TestMesh.GetIndexCount(subMeshIndex), 0, TestMesh.GetIndexStart(subMeshIndex), //TestMesh.GetBaseVertex(subMeshIndex), 0 });
            //
            // Initialize materials


            _renderingShader = new RenderingShader(ComputeShader, Shader)
            {
                Noise = PerlinNoise,
                ColorVariance = PerlinNoise,
            };

#if UNITY_ANDROID
            var format = TextureFormat.ARGB32;
#else
            var format = TextureFormat.DXT5;
#endif

            // ...
            _renderingShader.SetBillboardData(Create2DArray(GrassTextures, format),
                GrassTextures.Select(x => x.GetMinMaxWidthHeight).ToArray(),
                GrassTextures.Select(x => x.Yoffset).ToArray());

            _renderingShader.SetQuads(GetQuads(GrassTextures, true));
        }

        public void AddGrass(GameObject go)
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
            });
        }
        public void RemoveGrass(GameObject gameobj)
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
            // Add/Remove

            //var sw = System.Diagnostics.Stopwatch.StartNew();
            // sort front to back
            _pendingJobs.Sort((a, b) =>
            {
                var d1 = a.GameObject.transform.position.sqrMagnitude;
                var d2 = b.GameObject.transform.position.sqrMagnitude;
                return d1.CompareTo(d2);
            });
            //sw.Stop();

            //Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, "{0:0.0000} ms", sw.Elapsed.TotalMilliseconds);

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
                var maxExtent = Mathf.Max(extents.x, extents.y, extents.z) * 2;
                var centerWorld = go.transform.TransformPoint(bounds.center);

                var frustum = _frustum;
                _frustum[5].w += maxExtent;

                if (!IsInFrustum(centerWorld, -maxExtent * 1.75f))
                    continue;

                _frustum = frustum;

                //GenerateFrustumPlane(Camera.main);

                if (_pointGenerators.Count == 0)
                {
                    _pointGenerators.Push(new InstanceGenerator(Instantiate(ComputeShader), InstanceGenerator.Feature.Grass)
                    {
                        Density = Density,
                        SplatMap = DefaultSplatMap,
                    });
                }

                var pointGenerator = _pointGenerators.Pop();

                var triangleCount = mesh.GetIndexCount(0) / 3;
                var bufferSize = Math.Max(1, (maxExtent * maxExtent) / Density);
                bufferSize = bufferSize < triangleCount ? triangleCount : bufferSize;

                var outputBuffer = new ComputeBuffer((int)bufferSize, sizeof(float) * 4, ComputeBufferType.Append);
                outputBuffer.SetCounterValue(0);
                pointGenerator.SetMesh(mesh);

                pointGenerator.ColorMap = job.Diffuse;
                pointGenerator.OutputBuffer = outputBuffer;

                //var threadGroups = Mathf.CeilToInt(triangleCount / 8.0f);
                pointGenerator.Dispatch((int)triangleCount / 8);

                // swap remove
                if ((i + 1) < _pendingJobs.Count)
                    _pendingJobs[i--] = _pendingJobs[_pendingJobs.Count - 1];
                _pendingJobs.RemoveAt(_pendingJobs.Count - 1);

                _currentJobs.Add(new JobOutput()
                {
                    GameObject = go,
                    Bounds = bounds,
                    PointBuffer = outputBuffer,
                    Generator = pointGenerator,
                });

                if (_currentJobs.Count == MAX_JOBS_PER_FRAME)
                    return;
            }
        }

        private void Render()
        {
            var camera = Camera.main;

            if (!camera)
                return;

            GenerateFrustumPlane(camera);

            //var size = GetModuleBufferMemory(_items);
            //Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, "Grass :: total buffer memory size {0} mb", size / 1000000f);
            _frustum[5].w = DrawDistance;


            //var sw = System.Diagnostics.Stopwatch.StartNew();
            for (var i = 0; i < _currentJobs.Count; ++i)
            {
                var job = _currentJobs[i];

                var cullShader = new CullingShader(Instantiate(ComputeShader), CullingShader.CullingType.Fade)
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
            //sw.Stop();

            //// TODO: stop processing if per-frame budget is broken
            //if (_currentJobs.Count > 0 && sw.Elapsed.TotalMilliseconds > 0.1)
            //{
            //    var e = sw.Elapsed.TotalMilliseconds;
            //    Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, "grass process completed jobs = {0:0.0000} ms", e);
            //}

            _currentJobs.Clear();

            AddJobs();

            var fadeFarAmmount = DrawDistance / 3;
            var fadeFarValue = DrawDistance - fadeFarAmmount;

            _renderingShader.SetNearFade(NearFadeStart, NearFadeEnd);
            _renderingShader.SetFarFade(fadeFarValue, fadeFarAmmount);
            _renderingShader.Wind = Wind;

            _renderingShader.Frustum = _frustum;

            _renderingShader.RenderBegin();

            GameObject any = null;
            var maxSide = 0f;

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

                if (!IsInFrustum(centerWorld, (-maxSide * 1.75f)))
                    continue;

                _frustum[5].w = DrawDistance;

                var cullShader = item.CullShader;

                cullShader.LocalToWorld = go.transform.localToWorldMatrix;
                cullShader.Frustum = frustum;

                //var triangleCount = go.GetComponent<MeshFilter>().mesh.GetIndices(0).Length / 3;
                //var bufferSize = Math.Max(1, (maxExtent * maxExtent) / Density);
                //bufferSize = bufferSize < triangleCount ? triangleCount : bufferSize;

                cullShader.Dispatch();
            }

            if (any == null)
                return;

            // TODO: 
            var roiTransform = FindFirstNodeParent(any.transform);

            var worldToLocal = roiTransform == null ? Matrix4x4.identity : roiTransform.worldToLocalMatrix;

            _renderingShader.WorldToLocal = worldToLocal;
            _renderingShader.ViewDirection = Camera.main.transform.forward;

            var renderBounds = new Bounds(Vector3.zero, new Vector3(maxSide, maxSide, maxSide));
            //var renderBounds = new Bounds(Vector3.zero, new Vector3(DrawDistance + DrawDistance / 3, DrawDistance + DrawDistance / 3, DrawDistance + DrawDistance / 3) * 1.5f);

            var shadows = DrawShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderingShader.ShadowCastingMode = shadows;

            _renderingShader.RenderEnd(renderBounds);
        }

        public void Camera_OnPostTraverse()
        {
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
