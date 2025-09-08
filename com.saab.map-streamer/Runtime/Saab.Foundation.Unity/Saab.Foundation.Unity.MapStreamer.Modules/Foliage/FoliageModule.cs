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

using GizmoSDK.Coordinate;
using GizmoSDK.GizmoBase;
using Saab.Foundation.Map;
using Saab.Utility.GfxCaps;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

using ProfilerMarker = global::Unity.Profiling.ProfilerMarker;
using ProfilerCategory = global::Unity.Profiling.ProfilerCategory;

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
        /// <summary>
        /// Node LOD's larger than this will not use features from this set.
        /// </summary>
        public uint NodeMaxWidth;
        public bool Shadows;
        public bool Crossboard;
        [Header("calculated drawDistance")]
        public float DrawDistance;

        public FoliageFeature FoliageFeature;
        public ComputeBuffer InderectBuffer;
        public ComputeBuffer FoliageData;

        public float MaxHeight { get; set; }

        public Material FoliageMaterial { get; set; }

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

        //_PerlinNoise
        public Texture2D PerlinNoise;

        [Header("Debug Settings")]
        public bool DebugPrintCount = false;
        public bool Disabled = false;
        public bool DebugNoDraw = false;
        public bool NativeLeakDetection = false;
        public bool Occlusion = true;
        public Material DownsampleMaterial;

        [Header("Foliage Draw")]
        public List<FeatureSet> Features = new List<FeatureSet>();

        private Vector4[] _frustum = new Vector4[6];
        private float _maxHeight;
        private int[] _mappingTable;

        // **************** Generate HeightMap ****************
        private RenderTexture _heightMap;
        private RenderTexture _surfaceheightMap;
        private ComputeBuffer _minXY;
        private RenderTexture _depthMap;
        private bool _hasDepthTexture = false;

        private Queue<FoliageJob> _futurePool = new Queue<FoliageJob>();
        private Dictionary<SettingsFeatureType, SettingsFeature> _settingsCache = new Dictionary<SettingsFeatureType, SettingsFeature>();

        private static class PlacementParameterID
        {
            public static readonly int FoliageData = Shader.PropertyToID("_foliageData");
            public static readonly int TerrainResolution = Shader.PropertyToID("terrainResolution");
            public static readonly int TerrainSize = Shader.PropertyToID("terrainSize");
            public static readonly int NodeOffset = Shader.PropertyToID("NodeOffset");
            public static readonly int Resolution = Shader.PropertyToID("Resolution");
            public static readonly int ObjToWorld = Shader.PropertyToID("ObjToWorld");
            public static readonly int IndexCount = Shader.PropertyToID("indexCount");
            public static readonly int UvCount = Shader.PropertyToID("uvCount");
            public static readonly int FrameCount = Shader.PropertyToID("FrameCount");
            public static readonly int Occlusion = Shader.PropertyToID("Occlusion");
            public static readonly int DownscaleFactor = Shader.PropertyToID("DownscaleFactor");
            public static readonly int HeightMap = Shader.PropertyToID("HeightMap");
            public static readonly int IndexBuffer = Shader.PropertyToID("IndexBuffer");
            public static readonly int VertexBuffer = Shader.PropertyToID("VertexBuffer");
            public static readonly int MeshBoundsMax = Shader.PropertyToID("MeshBoundsMax");
            public static readonly int VertexBufferStride = Shader.PropertyToID("VertexBufferStride");
            public static readonly int TexcoordOffset = Shader.PropertyToID("TexcoordOffset");
            public static readonly int PositionOffset = Shader.PropertyToID("PositionOffset");
            public static readonly int SurfaceHeightMap = Shader.PropertyToID("SurfaceHeightMap");
            public static readonly int Texture = Shader.PropertyToID("Texture");
            public static readonly int Density = Shader.PropertyToID("Density");

            // Material
            public static readonly int IsToggled = Shader.PropertyToID("_isToggled");
            public static readonly int PerlinNoise = Shader.PropertyToID("_PerlinNoise");
            public static readonly int FoliageCount = Shader.PropertyToID("_foliageCount");
            public static readonly int MainTexArray = Shader.PropertyToID("_MainTexArray");
            public static readonly int PointBuffer = Shader.PropertyToID("_PointBuffer");

            public static readonly string CROSSBOARD_ON = "CROSSBOARD_ON";

        }
        public int GetFoliageCount
        {
            get
            {
                var count = 0;
                foreach (var feature in Features)
                {
                    count += feature.FoliageFeature.FoliageCount;
                }
                return count;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            _mappingTable = TerrainMapping.MapFeatureData();

            StartCoroutine(WaitForDepth());
            SceneManager.OnNewTerrain += SceneManager_OnNewTerrain;
            SceneManager.OnPostTraverse += SceneManager_OnPostTraverse;
            SceneManager.OnRemoveTerrain += SceneManager_OnRemoveTerrain;

            _minXY = new ComputeBuffer(2, sizeof(uint), ComputeBufferType.Default);

            for (int i = 0; i < Features.Count; i++)
            {
                var featureSet = Features[i];
                var settings = GetSettings(featureSet.SettingsType);
                featureSet.Enabled = settings.Enabled;

                featureSet.FoliageFeature = new FoliageFeature(Mathf.CeilToInt(featureSet.BufferSize * settings.Density), featureSet.Density * settings.Density, TerrainMapping.FeatureTruthTable(_mappingTable, featureSet.mapFeature), ComputeShader);

                var inderectBuffer = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);
                inderectBuffer.SetData(new uint[] { 0, 1, 0, 0 });
                featureSet.InderectBuffer = inderectBuffer;
                featureSet.FoliageMaterial = new Material(FoliageShader);

                //TODO: use to create a random noise
                featureSet.FoliageMaterial.SetTexture(PlacementParameterID.PerlinNoise, PerlinNoise);

                if (featureSet.Crossboard)
                {
                    featureSet.FoliageMaterial.SetFloat(PlacementParameterID.IsToggled, 0);
                    featureSet.FoliageMaterial.EnableKeyword(PlacementParameterID.CROSSBOARD_ON);
                }
                else
                {
                    featureSet.FoliageMaterial.SetFloat(PlacementParameterID.IsToggled, 1);
                    featureSet.FoliageMaterial.DisableKeyword(PlacementParameterID.CROSSBOARD_ON);
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

            featureSet.FoliageMaterial.SetInt(PlacementParameterID.FoliageCount, foliageTypes.Count);
            featureSet.FoliageMaterial.SetTexture(PlacementParameterID.MainTexArray, mainTexs);

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
            featureSet.FoliageMaterial.SetBuffer(PlacementParameterID.FoliageData, featureSet.FoliageData);
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
                antiAliasing = 1,
                name = "foliagemodule - temporaryRenderTexture - 2dArray"
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

            temporaryRenderTexture.Release();
            DestroyImmediate(temporaryRenderTexture);

            return textureArray;
        }

        private readonly Plane[] _frustrumPlanes = new Plane[6];
        private void GenerateFrustumPlane(UnityEngine.Camera camera)
        {
            GeometryUtility.CalculateFrustumPlanes(camera, _frustrumPlanes);

            for (int i = 0; i < 6; i++)
            {
                Vector3 normal = _frustrumPlanes[i].normal;
                _frustum[i].x = normal.x;
                _frustum[i].y = normal.y;
                _frustum[i].z = normal.z;
                _frustum[i].w = _frustrumPlanes[i].distance;
            }
        }
        private void SceneManager_OnRemoveTerrain(GameObject go)
        {
            foreach (var set in Features)
            {
                set.FoliageFeature?.RemoveFoliage(go);
            }
        }

        struct FoliageJob
        {
            public GameObject gameObject { get; set; }
            public int FrameCount { get; set; }
        }

        private void SceneManager_OnNewTerrain(GameObject go, bool isAsset)
        {
            if (isAsset)
                return;

            AddJob(go);
        }

        private readonly Coordinate _coordConverter = new Coordinate();

        private void AddJob(GameObject go)
        {
            if (Disabled || !go.activeInHierarchy)
                return;

            if (!go.TryGetComponent<MeshFilter>(out var meshFilter))
                return;

            var mesh = meshFilter.sharedMesh;
            if (!mesh)
                return;

            if (!go.TryGetComponent<NodeHandle>(out var nodeHandle))
                return;

            if (!nodeHandle.texture || !nodeHandle.feature)
                return;

            var featureInfo = nodeHandle.featureInfo;

            var pixelSize = new Vector2((float)featureInfo.v11, (float)featureInfo.v22);
            float scale = 1000;

            var nodeOffset = new Vector2(
                (float)(featureInfo.v13 + featureInfo.v11) % scale,
                (float)(featureInfo.v23 + featureInfo.v22) % scale);

            var tex = nodeHandle.texture;
            var texSize = new Vector2(tex.width, tex.height);

            var fullNodeSize = pixelSize * texSize;
            var nodeSide = Mathf.Max(fullNodeSize.x, fullNodeSize.y);

            if (nodeSide > 2048)
                return;

            ComputeShader.SetVector(PlacementParameterID.TerrainResolution, texSize);
            ComputeShader.SetVector(PlacementParameterID.TerrainSize, mesh.bounds.size);
            ComputeShader.SetVector(PlacementParameterID.NodeOffset, nodeOffset);
            ComputeShader.SetVector(PlacementParameterID.Resolution, pixelSize);
            ComputeShader.SetMatrix(PlacementParameterID.ObjToWorld, go.transform.localToWorldMatrix);

            var meshCenter = nodeHandle.node.BoundaryCenter;

            MapControl.SystemMap.GlobalToWorld(meshCenter, out GizmoSDK.Coordinate.CartPos cartPos);

            _coordConverter.SetCartPos(cartPos);
            _coordConverter.GetUTMPos(out var utmPos);

            var topLeftCorner = nodeHandle.featureInfo * new Vec3D(0, 0, 1);

            var nodeSize = (texSize * pixelSize);
            var nodeOffsetDiff = nodeSize - new Vector2(mesh.bounds.size.x, mesh.bounds.size.z);
            var centerOffset = new Vec3D(topLeftCorner.x - utmPos.Easting, 0, topLeftCorner.y - utmPos.Northing);

            if (Math.Abs(centerOffset.x) < nodeSize.x / 2)
                nodeOffsetDiff.x *= -1;

            if (Math.Abs(centerOffset.y) > nodeSize.y / 2)
                nodeOffsetDiff.y *= -1;

            centerOffset.x += nodeOffsetDiff.x;
            centerOffset.y += nodeOffsetDiff.y;

            var heightmap = GenerateHeight(texSize, pixelSize, mesh, centerOffset);

            RenderTexture surface = null;
            if (nodeHandle.surfaceHeight == null)
                surface = GenerateSurfaceHeight(tex);

            bool requireCleanup = true;

            foreach (var set in Features)
            {
                if (!set.Enabled)
                    continue;

                var setting = GetSettings(set.SettingsType);
                ComputeShader.SetFloat(PlacementParameterID.Density, set.Density * setting.Density);

                if (nodeSide < set.NodeMaxWidth)
                {
                    set.FoliageFeature.AddFoliage(go, nodeHandle, heightmap, surface);
                }

                requireCleanup = false;
            }

            if (requireCleanup)
            {
                heightmap?.Release();
                surface?.Release();
            }
        }

        private void OnDestroy()
        {
            _minXY?.Release();

            foreach (var set in Features)
            {
                set?.Dispose();
            }

            _heightMap?.Release();
            _surfaceheightMap?.Release();
            _depthMap?.Release();
        }

        private RenderTexture GenerateSurfaceHeight(UnityEngine.Texture texture)
        {
            // ************* Generate surface Height Map  ************* //

            _surfaceheightMap = new RenderTexture(texture.width, texture.height, 24, RenderTextureFormat.RFloat);
            _surfaceheightMap.enableRandomWrite = true;
            _surfaceheightMap.name = "foliagemodule - surfaceheightMap";
            _surfaceheightMap.Create();

            var kernel = ComputeShader.FindKernel("CSSurfaceHeightMap");
            ComputeShader.SetTexture(kernel, PlacementParameterID.Texture, texture);
            ComputeShader.SetTexture(kernel, PlacementParameterID.SurfaceHeightMap, _surfaceheightMap);

            var threadx = Mathf.CeilToInt(texture.width / 8f) < 1 ? 1 : Mathf.CeilToInt(texture.width / 8f);
            var thready = Mathf.CeilToInt(texture.height / 8f) < 1 ? 1 : Mathf.CeilToInt(texture.height / 8f);

            ComputeShader.Dispatch(kernel, threadx, thready, 1);

            return _surfaceheightMap;
        }

        private RenderTexture GenerateHeight(Vector2 texSize, Vector2 pixelSize, Mesh mesh, Vec3D offset)
        {

            ComputeShader.SetVector(PlacementParameterID.Resolution, pixelSize);

            // according to documentation vertexBufferTarget should not be able to be a structured but it works
            // if any future problems occur test doing the same as indexBuffer -> indexbufferGpuCopy
            mesh.vertexBufferTarget |= GraphicsBuffer.Target.Structured;
            mesh.indexBufferTarget |= GraphicsBuffer.Target.CopySource;

            var vertexBuffer = mesh.GetVertexBuffer(0);
            var indexBuffer = mesh.GetIndexBuffer();

            var indexbufferGpuCopy = new GraphicsBuffer(GraphicsBuffer.Target.CopyDestination | GraphicsBuffer.Target.Raw, Mathf.CeilToInt(indexBuffer.count / 2f), sizeof(uint));
            Graphics.CopyBuffer(indexBuffer, indexbufferGpuCopy);

            var stride = mesh.GetVertexBufferStride(0);
            var texOffset = mesh.GetVertexAttributeOffset(UnityEngine.Rendering.VertexAttribute.TexCoord0);
            var posOffset = mesh.GetVertexAttributeOffset(UnityEngine.Rendering.VertexAttribute.Position);

            ComputeShader.SetInt(PlacementParameterID.PositionOffset, posOffset / 4);
            ComputeShader.SetInt(PlacementParameterID.TexcoordOffset, texOffset / 4);
            ComputeShader.SetInt(PlacementParameterID.VertexBufferStride, stride / 4);

            var indicesCount = mesh.GetIndexCount(0);
            ComputeShader.SetInt(PlacementParameterID.UvCount, vertexBuffer.count);

            // ************* find node corners ************* //

            //var nodeTexCenter = mesh.bounds.center + new Vector3((float)offset.x + mesh.bounds.extents.x, (float)offset.y, (float)offset.z + mesh.bounds.extents.z);
            //var nodeExtents = new Vector3((texSize.x - 2) * pixelSize.x / 2, mesh.bounds.size.y, texSize.y * pixelSize.y / 2);
            //var nodeTexTopLeft = nodeTexTopLeft + nodeExtents;
            var nodeTexTopLeft = mesh.bounds.center - new Vector3((float)offset.x + (texSize.x * pixelSize.x), (float)offset.y, (float)offset.z);
            ComputeShader.SetVector(PlacementParameterID.MeshBoundsMax, nodeTexTopLeft);

            // ************* Generate Height Map  ************* //

            var kernelHeight = ComputeShader.FindKernel("CSHeightMap");

            if (_heightMap != null)
                _heightMap.Release();

            _heightMap = new RenderTexture((int)texSize.x, (int)texSize.y, 24, RenderTextureFormat.RFloat);
            _heightMap.enableRandomWrite = true;
            _heightMap.name = "foliagemodule - HeightMap";
            _heightMap.Create();

            var triangleCount = Mathf.CeilToInt(indicesCount / 3f);

            ComputeShader.SetInt(PlacementParameterID.IndexCount, triangleCount);

            ComputeShader.SetBuffer(kernelHeight, PlacementParameterID.VertexBuffer, vertexBuffer);
            ComputeShader.SetBuffer(kernelHeight, PlacementParameterID.IndexBuffer, indexbufferGpuCopy);
            ComputeShader.SetTexture(kernelHeight, PlacementParameterID.HeightMap, _heightMap);

            var threads = Mathf.CeilToInt(triangleCount / 4f) < 1 ? 1 : Mathf.CeilToInt(triangleCount / 4f);
            ComputeShader.Dispatch(kernelHeight, threads, 1, 1);

            indexBuffer.Dispose();
            vertexBuffer.Dispose();
            indexbufferGpuCopy.Dispose();

            return _heightMap;
        }

        private IEnumerator WaitForDepth()
        {
            yield return new WaitUntil(() => Shader.GetGlobalTexture("_CameraDepthTexture") != null);
            _hasDepthTexture = true;
        }

        private void DownscaleDepth(int downscale)
        {
            if (_depthMap == null)
            {
                _depthMap = new RenderTexture(Screen.width / downscale, Screen.height / downscale, 0, RenderTextureFormat.RFloat);
                _depthMap.name = "foliagemodule - depthmap";
                _depthMap.filterMode = FilterMode.Point;
                _depthMap.useMipMap = false;
                _depthMap.Create();
            }

            Graphics.Blit(null, _depthMap, DownsampleMaterial);
            DownsampleMaterial.mainTexture = _depthMap;
            ComputeShader.SetInt(PlacementParameterID.DownscaleFactor, downscale);
        }

        float CalculateDesiredDistance(UnityEngine.Camera camera, float objectHeight, float coverage)
        {
            // Convert FOV from degrees to radians and halve it
            float halfFov = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;

            // Calculate the desired distance so that the object covers the desired percentage of the screen
            float desiredDistance = objectHeight / (2.0f * Mathf.Tan(halfFov) * coverage);

            return desiredDistance;
        }

        private static readonly ProfilerMarker _profilerMarker = new ProfilerMarker(ProfilerCategory.Render, "Foliage-Render");

        private void SceneManager_OnPostTraverse(bool locked)
        {
            _profilerMarker.Begin();

            Render();

            _profilerMarker.End();
        }

        private void Render()
        {
            if (Disabled || !_hasDepthTexture)
                return;

            if (NativeLeakDetection)
                UnsafeUtility.SetLeakDetectionMode(NativeLeakDetectionMode.EnabledWithStackTrace);
            else
                UnsafeUtility.SetLeakDetectionMode(NativeLeakDetectionMode.Disabled);

            var cam = SceneManager.SceneManagerCamera.Camera;
            GenerateFrustumPlane(cam);

            if (cam.depthTextureMode != DepthTextureMode.Depth)
                cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.Depth;

            ComputeShader.SetBool(PlacementParameterID.Occlusion, Occlusion);
            ComputeShader.SetInt(PlacementParameterID.FrameCount, UnityEngine.Time.frameCount);

            DownscaleDepth(4);

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

                var dist = CalculateDesiredDistance(cam, set.MaxHeight, set.ScreenCoverage);
                set.DrawDistance = dist * settings.DrawDistance;
                _frustum[5].w = set.DrawDistance;  // draw distance

                // Render all points
                var buffer = set.FoliageFeature.Cull(_frustum, cam, _maxHeight, _depthMap, set);
                set.FoliageMaterial.SetBuffer(PlacementParameterID.PointBuffer, buffer);

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