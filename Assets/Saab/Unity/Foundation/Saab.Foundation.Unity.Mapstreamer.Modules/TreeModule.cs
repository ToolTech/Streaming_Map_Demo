using Saab.Unity.Core.ComputeExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    public class TreeModule : MonoBehaviour
    {
        [Header("****** SETTINGS ******")]
        public TerrainTextures[] TreeTextures;
        public Texture2D PerlinNoise;
        public Texture2D DefaultSplatMap;
        public ComputeShader ComputeShader; // <-- TODO: Replace with 1 shader per responsibility (generator, culling, rendering)

        // Rendering settings
        [Header("****** RENDER SETTINGS ******")]
        public Shader TreeShader;
        public int DrawDistance = 4500;
        public float NearFadeStart = 5;
        public float NearFadeEnd = 5;
        public bool DrawTreeShadows = true;
        public float Wind = 0.0f;
        public float Density = 22.127f;


        // Active rendered item
        private struct Item : IDisposable
        {
            // source object
            public GameObject GameObject;

            // culls instances and output to rendering buffer
            public CullingShader CullShader;

            // local bounds of the object
            public Bounds Bounds;

            public void Dispose()
            {
                CullShader.Dispose();
            }
        }

        // GPU work waiting to be processed by the CPU and placed into the active items list
        private struct JobOutput
        {
            public GameObject GameObject;
            public ComputeBuffer PointBuffer;
            public Bounds Bounds;
            public InstanceGenerator Generator;
        }

        // Objects not yet processed, will be processed by the GPU in batches during update
        private struct PendingJob
        {
            public GameObject GameObject;
            public Mesh Mesh;
            public Texture2D Diffuse;
        }



        // maximum concurrent GPU jobs, higher values increases memory footprint
        private const int MAX_JOBS_PER_FRAME = 8;

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

        // camera frustum planes, updated in render
        private readonly Vector4[] _frustum = new Vector4[6];

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


            _renderingShader = new RenderingShader(ComputeShader, TreeShader)
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
            _renderingShader.SetBillboardData(Create2DArray(TreeTextures, format),
                TreeTextures.Select(x => x.GetMinMaxWidthHeight).ToArray(),
                TreeTextures.Select(x => x.Yoffset).ToArray());

            _renderingShader.SetQuads(GetQuads());
        }

        public void AddTree(GameObject go)
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
        public void RemoveTree(GameObject gameobj)
        {
            for (var i = 0; i < _currentJobs.Count; ++i)
            {
                if (_currentJobs[i].GameObject != gameobj)
                    continue;

                _currentJobs[i].PointBuffer.SafeRelease();

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

        private void GenerateFrustumPlane(Camera camera)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);


            for (int i = 0; i < 6; i++)
            {
                _frustum[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
            }

            _frustum[5].w = DrawDistance;
        }

        private static Texture2DArray Create2DArray(TerrainTextures[] texture, TextureFormat targetFormat)
        {
            var textureCount = texture.Length;

            var textureResolution = Math.Max(texture.Max(item => item.FeatureTexture.width), texture.Max(item => item.FeatureTexture.height));

            textureResolution = (int)NextPowerOfTwo((uint)textureResolution);

            Texture2DArray textureArray;

            textureArray = new Texture2DArray(textureResolution, textureResolution, textureCount, targetFormat, true)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            RenderTexture temporaryTreeRenderTexture = new RenderTexture(textureResolution, textureResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
            {
                useMipMap = true,
                antiAliasing = 1
            };

            for (int i = 0; i < textureCount; i++)
            {
                Graphics.Blit(texture[i].FeatureTexture, temporaryTreeRenderTexture);

                Texture2D temporaryTreeTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.ARGB32, true);

                RenderTexture.active = temporaryTreeRenderTexture;

                temporaryTreeTexture.ReadPixels(new Rect(0, 0, temporaryTreeTexture.width, temporaryTreeTexture.height), 0, 0);
                RenderTexture.active = null;
                temporaryTreeTexture.Apply(true);
                temporaryTreeTexture.Compress(true);

                //TexToFile(temporaryGrassTexture, Application.dataPath + "/../grassTextureArraySaved_" + i + ".png");

                Graphics.CopyTexture(temporaryTreeTexture, 0, textureArray, i);
                Destroy(temporaryTreeTexture);
            }
            textureArray.Apply(false, true);

            Destroy(temporaryTreeRenderTexture);

            return textureArray;
        }
        private bool IsInFrustum(Vector3 positionAfterProjection, float treshold = -1)
        {
            float cullValue = treshold;

            return (Vector3.Dot(_frustum[0], positionAfterProjection) >= cullValue &&
                Vector3.Dot(_frustum[1], positionAfterProjection) >= cullValue &&
                Vector3.Dot(_frustum[2], positionAfterProjection) >= cullValue &&
                Vector3.Dot(_frustum[3], positionAfterProjection) >= cullValue) &&
            (_frustum[5].w >= Mathf.Abs(Vector3.Distance(Vector3.zero, positionAfterProjection)));
        }

        // TODO: Shaders should use the TRANSFORM of the node to 
        private Transform FindFirstNodeParent(Transform child)
        {
            var parent = child.parent;
            if (parent == null)
            {
                return child;
            }

            var node = parent.GetComponent<NodeHandle>();

            if (node == null)
            {
                return child;
            }

            return FindFirstNodeParent(parent);
        }

        private Vector4[] GetQuads()
        {
            // NOTE: GetPixels() is slowing this down, and we cant use multithreading to help with this,
            // so instead to improve the performance of this code, try using the GPU

            var quads = new Vector4[TreeTextures.Length * 3];

            var i = 0;

            foreach (TerrainTextures terrain in TreeTextures)
            {
                quads[i++] = GetQuad(terrain.FeatureTexture, Sides.Front);
                quads[i++] = GetQuad(terrain.FeatureTexture, Sides.Side);
                quads[i++] = GetQuad(terrain.FeatureTexture, Sides.Top);
            }

            return quads;
        }

        private static Vector4 GetQuad(Texture2D texture, Sides side)
        {
            // get pixel data for a 1/4 'side' sub image
            var pixels = GrabRect(texture, side);

            // get minimum fit
            var rc = FitMinimumUV(pixels, texture.width / 2);

            var w = texture.width / 2f;
            var h = texture.height / 2f;

            return new Vector4(rc.xMin / w, rc.xMax / w, rc.yMin / h, rc.yMax / h);
        }

        private void AddJobs()
        {
            for (var i = 0; i < _pendingJobs.Count; ++i)
            {
                var job = _pendingJobs[i];
                var go = job.GameObject;

                // get new job, according to certain rules (maybe priority later)
                if (!go.activeSelf)
                    continue;

                var mesh = job.Mesh;
                var bounds = mesh.bounds;

                var extents = bounds.extents;
                var maxExtent = Mathf.Max(extents.x, extents.y, extents.z);
                var centerWorld = go.transform.TransformPoint(bounds.center);

                if (!IsInFrustum(centerWorld, -maxExtent * 1.25f))
                    continue;

                if (_pointGenerators.Count == 0)
                {
                    _pointGenerators.Push(new InstanceGenerator(Instantiate(ComputeShader))
                    {
                        Density = Density,
                        SplatMap = DefaultSplatMap,
                    });
                }

                var pointGenerator = _pointGenerators.Pop();

                var bufferSize = Math.Max(1, (int)Mathf.Ceil((extents.x * extents.z) / Density) / 2);

                var outputBuffer = new ComputeBuffer(bufferSize, sizeof(float) * 4, ComputeBufferType.Append);
                outputBuffer.SetCounterValue(0);

                pointGenerator.SetMesh(mesh);

                pointGenerator.ColorMap = job.Diffuse;
                pointGenerator.OutputBuffer = outputBuffer;



                var triangleCount = mesh.GetIndexCount(0) / 3;
                var threadGroups = Mathf.CeilToInt(triangleCount / 8.0f);

                pointGenerator.Dispatch(threadGroups);



                // swap remove
                if ((i + 1) < _pendingJobs.Count)
                    _pendingJobs[i] = _pendingJobs[_pendingJobs.Count - 1];
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

            var sw = System.Diagnostics.Stopwatch.StartNew();
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
            sw.Stop();

            //// TODO: stop processing if per-frame budget is broken
            //if (_currentJobs.Count > 0 && sw.Elapsed.TotalMilliseconds > 0.1)
            //{
            //    var e = sw.Elapsed.TotalMilliseconds;
            //    Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, "process completed jobs = {0:0.0000} ms", e);
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

            // Culling      
            for (var i = 0; i < _items.Count; ++i)
            {
                var item = _items[i];

                var go = item.GameObject;
                if (!go.activeSelf)
                    continue;

                any = go;

                var bounds = item.Bounds;

                var maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                var centerWorld = go.transform.TransformPoint(bounds.center);

                if (!IsInFrustum(centerWorld, -maxExtent * 1.25f))
                    continue;

                var cullShader = item.CullShader;

                cullShader.LocalToWorld = go.transform.localToWorldMatrix;
                cullShader.Frustum = _frustum;

                var bufferSize = Math.Max(1, (int)Mathf.Ceil((bounds.extents.x * bounds.extents.z) / Density) / 2);
                var threadGroups = Mathf.CeilToInt(bufferSize / 128.0f);

                cullShader.Dispatch(threadGroups);
            }

            if (any == null)
                return;

            // TODO: 
            var roiTransform = FindFirstNodeParent(any.transform);

            var worldToLocal = roiTransform == null ? Matrix4x4.identity : roiTransform.worldToLocalMatrix;

            _renderingShader.WorldToLocal = worldToLocal;
            _renderingShader.ViewDirection = Camera.main.transform.forward;

            var renderBounds = new Bounds(Vector3.zero, new Vector3(DrawDistance + DrawDistance / 3, DrawDistance + DrawDistance / 3, DrawDistance + DrawDistance / 3) * 1.5f);

            var shadows = DrawTreeShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderingShader.ShadowCastingMode = shadows;

            _renderingShader.RenderEnd(renderBounds);

            //Graphics.DrawMeshInstancedIndirect(TestMesh, 0, TestMat, bounds, _closeInderectBuffer, 0, null, DrawTreeShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);
        }

        private enum Sides
        {
            Front = 1 << 0,
            Side = 1 << 1,
            Top = 1 << 2,
        };

        private static Color[] GrabRect(Texture2D source, Sides side)
        {
            int sx = 0;
            int sy = 0;

            switch (side)
            {
                case Sides.Front:
                    sx = 0;
                    sy = source.height / 2;
                    break;
                case Sides.Side:
                    sx = 0;
                    sy = 0;
                    break;
                case Sides.Top:
                    sx = source.width / 2;
                    sy = 0;
                    break;
            }

            return source.GetPixels(sx, sy, source.width / 2, source.height / 2, 0);
        }

        private static RectInt FitMinimumUV(Color[] data, int width)
        {
            var height = data.Length / width;

            const float cutoff = 0.9f;

            int maxX = 0;
            int maxY = 0;
            int minX = width - 1;
            int minY = height - 1;

            for (int y = 0; y < height; y++)
            {
                int lx = 0;

                var p = y * width;

                for (; lx < width; lx++)
                {
                    var color = data[p++];
                    if (color.a > cutoff)
                    {
                        minX = lx < minX ? lx : minX;
                        minY = y < minY ? y : minY;
                        maxY = y > maxY ? y : maxY;
                        break;
                    }
                }

                p = (y + 1) * width - 1;

                for (var rx = width - 1; rx >= lx; rx--)
                {
                    var color = data[p--];
                    if (color.a > cutoff)
                    {
                        maxX = rx > maxX ? rx : maxX;
                        break;
                    }
                }
            }

            return new RectInt(minX, minY, maxX - minX, maxY - minY);
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

            if(_renderingShader!=null)
                _renderingShader.Dispose();
        }

        private static uint NextPowerOfTwo(uint v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;

            return v + 1;
        }
    }
}
