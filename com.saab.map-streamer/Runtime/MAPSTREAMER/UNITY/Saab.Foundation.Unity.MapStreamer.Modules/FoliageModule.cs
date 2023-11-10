using Saab.Utility.GfxCaps;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Drawing.Drawing2D;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{

    [Serializable]
    public class FeatureSet
    {
        public SettingsFeatureType SettingsType;
        public MapFeature mapFeature;
        public FoliageSet FoliageSet;
        public bool Enabled;
        [Header("Main Settings")]
        public int BufferSize;
        [Range(0.001f, 1)]
        public float ScreenCoverage = 0.001f;
        public float Density;
        public uint BoundaryRadius;
        public bool Shadows;
        public bool Crossboard;
        [Header("calculated drawDistance")]
        public float DrawDistance;

        public FoliageFeature FoliageFeature;
        public ComputeBuffer InderectBuffer;
        public ComputeBuffer FoliageData;
        private float _maxHeight;


        public float MaxHeight
        {
            set { _maxHeight = value; }
            get
            {
                return _maxHeight;
            }
        }

        public Material FoliageMaterial
        {
            get; set;
        }

        public void Dispose()
        {
            FoliageFeature?.Dispose();
            InderectBuffer?.Dispose();
            FoliageData?.Dispose();
        }
    }

    public class FoliageModule : MonoBehaviour
    {
        public SceneManager SceneManager;
        public ComputeShader ComputeShader;
        public Shader FoliageShader;
        public Vector3 Wind;

        //_PerlinNoise
        public Texture2D PerlinNoise;

        [Header("Debug Settings")]
        public bool DebugPrintCount = false;
        public bool Disabled = false;
        public bool DebugNoDraw = false;
        public bool NativeLeakDetection = false;
        public bool Occlusion = true;

        [Header("Foliage Draw")]
        public List<FeatureSet> Features = new List<FeatureSet>();

        private Vector4[] _frustum = new Vector4[6];
        private float _maxHeight;
        private int[] _mappingTable;

        // **************** Generate HeightMap ****************
        private ComputeBuffer _vertices;
        private ComputeBuffer _texcoords;
        private ComputeBuffer _indices;
        private int _maxVertexCount;
        private int _maxIndexCount;
        private RenderTexture _heightMap;
        private RenderTexture _surfaceheightMap;
        private ComputeBuffer _minXY;
        private RenderTexture _depthMap;
        private bool _hasDepthTexture = false;

        private Dictionary<SettingsFeatureType, SettingsFeature> _settingsCache = new Dictionary<SettingsFeatureType, SettingsFeature>();

        // Start is called before the first frame update
        void Start()
        {
            _mappingTable = TerrainMapping.MapFeatureData();

            StartCoroutine(WaitForDepth());
            SceneManager.OnNewGeometry += SceneManager_OnNewGeometry;
            SceneManager.OnPostTraverse += SceneManager_OnPostTraverse;
            SceneManager.OnEnterPool += SceneManager_OnEnterPool;

            _minXY = new ComputeBuffer(2, sizeof(uint), ComputeBufferType.Default);

            for (int i = 0; i < Features.Count; i++)
            {
                var featureSet = Features[i];
                var settings = GetSettings(featureSet.SettingsType);
                featureSet.Enabled = settings.Enabled;

                if (!settings.Enabled)
                    continue;

                featureSet.FoliageFeature = new FoliageFeature(Mathf.CeilToInt(featureSet.BufferSize * settings.Density), featureSet.Density * settings.Density, TerrainMapping.FeatureTruthTable(_mappingTable, featureSet.mapFeature), ComputeShader);

                var inderectBuffer = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);
                inderectBuffer.SetData(new uint[] { 0, 1, 0, 0 });
                featureSet.InderectBuffer = inderectBuffer;
                featureSet.FoliageMaterial = new Material(FoliageShader);

                //TODO: use to create a random noise
                featureSet.FoliageMaterial.SetTexture("_PerlinNoise", PerlinNoise);

                if (featureSet.Crossboard)
                {
                    featureSet.FoliageMaterial.SetFloat("_isToggled", 0);
                    featureSet.FoliageMaterial.EnableKeyword("CROSSBOARD_ON");
                }
                else
                {
                    featureSet.FoliageMaterial.SetFloat("_isToggled", 1);
                    featureSet.FoliageMaterial.DisableKeyword("CROSSBOARD_ON");
                }
                featureSet.MaxHeight = featureSet.FoliageSet.GetMaxHeight;

                SetupFoliage(featureSet);
            }
        }

        struct FoliageShaderData
        {
            public Vector2 MaxMin;
            public Vector2 Offset;
            public float Weight;
        };

        private SettingsFeature GetSettings(SettingsFeatureType settingsType)
        {
            if (!_settingsCache.TryGetValue(settingsType, out var settings))
            {
                settings = GfxCaps.GetFoliageSettings(settingsType);
                _settingsCache[settingsType] = settings;
            }
            return settings;
        }

        private void SetupFoliage(FeatureSet featureSet)
        {
#if UNITY_ANDROID
            var format = TextureFormat.ETC2_RGBA8;
            Debug.Log("foliage Use ETC2");
#else
            var format = TextureFormat.DXT5;
#endif           
            var foliageTypes = featureSet.FoliageSet.GetFoliageList;
            var mainTexs = Create2DArray(foliageTypes, format);

            featureSet.FoliageMaterial.SetInt("_foliageCount", foliageTypes.Count);
            featureSet.FoliageMaterial.SetTexture("_MainTexArray", mainTexs);

            var data = new FoliageShaderData[foliageTypes.Count];
            for (int i = 0; i < foliageTypes.Count; i++)
            {
                var foliage = foliageTypes[i];
                _maxHeight = Mathf.Max(_maxHeight, foliage.MaxMin.y);

                var item = new FoliageShaderData()
                {
                    MaxMin = foliage.MaxMin,
                    Offset = foliage.Offset,
                    Weight = foliage.Weight,
                };
                data[i] = item;
            }

            if (featureSet.FoliageData != null)
                featureSet.FoliageData.Release();

            featureSet.FoliageData = new ComputeBuffer(foliageTypes.Count, sizeof(float) * 5, ComputeBufferType.Default);
            featureSet.FoliageData.SetData(data);
            featureSet.FoliageMaterial.SetBuffer("_foliageData", featureSet.FoliageData);
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
        public Texture2DArray Create2DArray(List<Foliage> foliages, TextureFormat targetFormat)
        {
            var textureCount = foliages.Count;
            var textureResolution = Mathf.Max(foliages.Max(item => item.MainTexture.width), foliages.Max(item => item.MainTexture.height));
            textureResolution = (int)NextPowerOfTwo((uint)textureResolution);

            Texture2DArray textureArray;

            textureArray = new Texture2DArray(textureResolution, textureResolution, textureCount, targetFormat, true)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            RenderTexture temporaryRenderTexture = new RenderTexture(textureResolution, textureResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
            {
                useMipMap = true,
                antiAliasing = 1
            };

            for (int i = 0; i < textureCount; i++)
            {
                //Debug.LogWarning($"graphic format: {foliages[i].MainTexture.graphicsFormat} format: {foliages[i].MainTexture.format}");
                Graphics.Blit(foliages[i].MainTexture, temporaryRenderTexture);
                RenderTexture.active = temporaryRenderTexture;

                Texture2D temporaryTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.ARGB32, true);
                temporaryTexture.ReadPixels(new Rect(0, 0, temporaryTexture.width, temporaryTexture.height), 0, 0);
                RenderTexture.active = null;
                temporaryTexture.Apply(true);
                temporaryTexture.Compress(true);
                Graphics.CopyTexture(temporaryTexture, 0, textureArray, i);

                DestroyImmediate(temporaryTexture);
            }
            textureArray.Apply(false, true);

            DestroyImmediate(temporaryRenderTexture);

            return textureArray;
        }
        private void GenerateFrustumPlane(UnityEngine.Camera camera)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);

            for (int i = 0; i < 6; i++)
            {
                _frustum[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
            }
        }
        private void SceneManager_OnEnterPool(GameObject go)
        {
            foreach (var set in Features)
            {
                if (set.Enabled)
                    set.FoliageFeature.RemoveFoliage(go);
            }
        }

        private void SceneManager_OnNewGeometry(GameObject go)
        {
            var nodehandle = go.GetComponent<NodeHandle>();

            if (nodehandle != null && !Disabled && go.activeInHierarchy && nodehandle.node.BoundaryRadius > 0)
            {
                if (nodehandle.texture == null || nodehandle.feature == null)
                {
                    return;
                }

                if (nodehandle.node.BoundaryRadius >= 900)
                    return;

                var pixelSize = new Vector2((float)nodehandle.featureInfo.v11, (float)nodehandle.featureInfo.v22);
                float scale = 1000;
                var nodeOffset = new Vector2((float)(nodehandle.featureInfo.v13 + nodehandle.featureInfo.v11) % scale, (float)(nodehandle.featureInfo.v23 + nodehandle.featureInfo.v22) % scale);
                var texSize = new Vector2(nodehandle.texture.width, nodehandle.texture.height);
                var mesh = go.GetComponent<MeshFilter>().sharedMesh;

                ComputeShader.SetVector("terrainResolution", texSize);
                ComputeShader.SetVector("terrainSize", mesh.bounds.size);
                ComputeShader.SetVector("NodeOffset", nodeOffset);
                ComputeShader.SetVector("Resolution", pixelSize);
                ComputeShader.SetMatrix("ObjToWorld", go.transform.localToWorldMatrix);

                var heightmap = GenerateHeight(texSize, pixelSize, mesh);

                RenderTexture surface = null;
                if (nodehandle.surfaceHeight == null)
                    surface = GenerateSurfaceHeight(nodehandle.texture);

                 foreach (var set in Features)
                {
                    if (!set.Enabled)
                        continue;

                    var setting = GetSettings(set.SettingsType);
                    ComputeShader.SetFloat("Density", set.Density * setting.Density);

                    if (nodehandle.node.BoundaryRadius < set.BoundaryRadius)
                    {
                        set.FoliageFeature.AddFoliage(go, nodehandle, heightmap);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            _vertices?.Release();
            _indices?.Release();
            _texcoords?.Release();
            _minXY?.Release();

            foreach (var set in Features)
            {
                set?.Dispose();
            }

            _heightMap?.Release();
            _surfaceheightMap?.Release();
        }

        private void SetupBuffers(Mesh mesh, int[] indices)
        {

            var vertices = mesh.vertices;
            var vertexCount = vertices.Length;

            if (vertexCount > _maxVertexCount)
            {
                if (_vertices != null)
                {
                    _vertices.Release();
                    _texcoords.Release();
                }

                _vertices = new ComputeBuffer(vertexCount, sizeof(float) * 3, ComputeBufferType.Default);
                _texcoords = new ComputeBuffer(vertexCount, sizeof(float) * 2, ComputeBufferType.Default);
                _maxVertexCount = _vertices.count;
            }

            var indexCount = indices.Length;

            if (indexCount > _maxIndexCount)
            {
                if (_indices != null)
                {
                    _indices.Release();
                }
                _indices = new ComputeBuffer((int)indexCount, sizeof(int), ComputeBufferType.Default);
                _maxIndexCount = (int)indexCount;
            }


            _vertices.SetData(vertices);
            _indices.SetData(indices);
            _texcoords.SetData(mesh.uv);
        }

        private RenderTexture GenerateSurfaceHeight(UnityEngine.Texture texture)
        {
            // ************* Generate surface Height Map  ************* //

            _surfaceheightMap = new RenderTexture(texture.width, texture.height, 24, RenderTextureFormat.RFloat);
            _surfaceheightMap.enableRandomWrite = true;
            _surfaceheightMap.Create();

            var kernel = ComputeShader.FindKernel("CSSurfaceHeightMap");
            ComputeShader.SetTexture(kernel, "Texture", texture);
            ComputeShader.SetTexture(kernel, "SurfaceHeightMap", _surfaceheightMap);

            var threadx = Mathf.CeilToInt(texture.width / 8f) < 1 ? 1 : Mathf.CeilToInt(texture.width / 8f);
            var thready = Mathf.CeilToInt(texture.height / 8f) < 1 ? 1 : Mathf.CeilToInt(texture.height / 8f);

            ComputeShader.Dispatch(kernel, threadx, thready, 1);

            return _surfaceheightMap;
        }

        private RenderTexture GenerateHeight(Vector2 texSize, Vector2 pixelSize, Mesh mesh)
        {

            ComputeShader.SetVector("Resolution", pixelSize);

            // *************find node corners ************* //
            uint maxside = (uint)Mathf.Max(texSize.x, texSize.y);

            uint[] max = { maxside * 2, maxside * 2 };
            _minXY.SetData(max);

            //var vertices = mesh.vertices;
            var indices = mesh.GetIndices(0);
            SetupBuffers(mesh, indices);

            var kernelFindUV = ComputeShader.FindKernel("CSFindMinUv");
            //ComputeShader.SetTextureFromGlobal(kernelFindUV, "DepthTexture", "_LastCameraDepthTexture");
            ComputeShader.SetInt("uvCount", mesh.vertexCount);
            ComputeShader.SetBuffer(kernelFindUV, "MinXY", _minXY);
            ComputeShader.SetBuffer(kernelFindUV, "surfaceUVs", _texcoords);
            var threads = Mathf.CeilToInt(mesh.vertexCount / 32f) < 1 ? 1 : Mathf.CeilToInt(mesh.vertexCount / 32f);
            ComputeShader.Dispatch(kernelFindUV, threads, 1, 1);

            var data = new uint[2];
            _minXY.GetData(data);

            // ************* Find center of Node ************* //

            var offsetX = ((texSize.x - 2) * pixelSize.x / 2 - mesh.bounds.extents.x);
            offsetX = data[0] > 0 ? -offsetX : offsetX;
            var offsetY = ((texSize.y - 2) * pixelSize.y / 2 - mesh.bounds.extents.z);
            offsetY = data[1] > 0 ? offsetY : -offsetY;

            var center = mesh.bounds.center;
            center.x += offsetX;
            center.z += offsetY;

            var extents = new Vector3(((texSize.x - 2) * pixelSize.x / 2), mesh.bounds.size.y, (texSize.y * pixelSize.y / 2));
            ComputeShader.SetVector("MeshBoundsMax", center + extents);

            // ************* Generate Height Map  ************* //

            var kernelHeight = ComputeShader.FindKernel("CSHeightMap");

            _heightMap = new RenderTexture((int)texSize.x, (int)texSize.y, 24, RenderTextureFormat.RFloat);
            _heightMap.enableRandomWrite = true;
            _heightMap.Create();

            var triangleCount = Mathf.CeilToInt(indices.Length / 3f);

            ComputeShader.SetInt("indexCount", triangleCount);
            ComputeShader.SetBuffer(kernelHeight, "surfaceVertices", _vertices);
            ComputeShader.SetBuffer(kernelHeight, "surfaceIndices", _indices);
            ComputeShader.SetBuffer(kernelHeight, "surfaceUVs", _texcoords);
            ComputeShader.SetTexture(kernelHeight, "HeightMap", _heightMap);

            threads = Mathf.CeilToInt(triangleCount / 4f) < 1 ? 1 : Mathf.CeilToInt(triangleCount / 4f);
            ComputeShader.Dispatch(kernelHeight, threads, 1, 1);

            return _heightMap;
        }

        private IEnumerator WaitForDepth()
        {
            yield return new WaitUntil(() => Shader.GetGlobalTexture("_CameraDepthTexture") != null);
            _hasDepthTexture = true;
        }

        private void DownscaleDepth(int downscale)
        {
            var kernel = ComputeShader.FindKernel("CSDownscaleDepth");
            var screenSize = new Vector2Int(Mathf.CeilToInt(Screen.width / (float)downscale), Mathf.CeilToInt(Screen.height / (float)downscale));

            if (_depthMap == null || (_depthMap.width != screenSize.x && _depthMap.height != screenSize.y))
            {
                _depthMap?.Release();
                _depthMap = new RenderTexture(screenSize.x, screenSize.y, 24, RenderTextureFormat.RFloat);
                _depthMap.enableRandomWrite = true;
                _depthMap.Create();
            }

            var threadx = Mathf.CeilToInt(screenSize.x / 8f) < 1 ? 1 : Mathf.CeilToInt(screenSize.x / 8f);
            var thready = Mathf.CeilToInt(screenSize.y / 8f) < 1 ? 1 : Mathf.CeilToInt(screenSize.y / 8f);

            ComputeShader.SetInt("Scale", downscale);
            ComputeShader.SetBool("Occlusion", Occlusion);
            ComputeShader.SetVector("DownscaleSize", new Vector2(screenSize.x, screenSize.y));
            ComputeShader.SetTextureFromGlobal(kernel, "DepthTexture", "_CameraDepthTexture");
            ComputeShader.SetTexture(kernel, "DownscaledDepthTexture", _depthMap);

            ComputeShader.Dispatch(kernel, threadx, thready, 1);
        }

        float CalculateDesiredDistance(UnityEngine.Camera camera, float objectHeight, float coverage)
        {
            // Convert FOV from degrees to radians and halve it
            float halfFov = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;

            // Calculate the desired distance so that the object covers the desired percentage of the screen
            float desiredDistance = objectHeight / (2.0f * Mathf.Tan(halfFov) * coverage);

            return desiredDistance;
        }

        private void SceneManager_OnPostTraverse(bool locked)
        {
            if (NativeLeakDetection)
                UnsafeUtility.SetLeakDetectionMode(NativeLeakDetectionMode.EnabledWithStackTrace);
            else
                UnsafeUtility.SetLeakDetectionMode(NativeLeakDetectionMode.Disabled);

            if (Disabled || !_hasDepthTexture)
                return;

            var cam = SceneManager.SceneManagerCamera.Camera;
            GenerateFrustumPlane(cam);


            if (cam.depthTextureMode != DepthTextureMode.Depth)
                cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.Depth;


            DownscaleDepth(Mathf.CeilToInt(Screen.width / 20f));

            foreach (var set in Features)
            {

                var settings = GetSettings(set.SettingsType);
                if (!settings.Enabled || !set.Enabled)
                    continue;

                if (set.FoliageMaterial == null)
                {
                    Debug.LogError($"FoliageMaterial for {set.mapFeature} can't be NULL!");
                    continue;
                }
                   

                set.FoliageMaterial.SetVector("_Wind", Wind);

                var dist = CalculateDesiredDistance(cam, set.MaxHeight, set.ScreenCoverage);
                set.DrawDistance = dist * settings.DrawDistance;
                _frustum[5].w = set.DrawDistance;  // draw distance

                // Render all points
                var buffer = set.FoliageFeature.Cull(_frustum, cam, _maxHeight, _depthMap);
                set.FoliageMaterial.SetBuffer("_PointBuffer", buffer);

                // ------- Render -------
                ComputeBuffer.CopyCount(buffer, set.InderectBuffer, 0);
                if (DebugPrintCount)
                {
                    int[] array = new int[4];
                    set.InderectBuffer.GetData(array);
                    UnityEngine.Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, "{2} :: {0}/{1}", array[0].ToString(), buffer.count, set.FoliageSet.name);
                }

                if (DebugNoDraw)
                    return;

                var renderBounds = new Bounds(Vector3.zero, new Vector3(set.DrawDistance, set.DrawDistance, set.DrawDistance));
                Graphics.DrawProceduralIndirect(set.FoliageMaterial, renderBounds, MeshTopology.Points, set.InderectBuffer, 0, null, null, settings.Shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);
            }

            DebugPrintCount = false;
        }
    }
}