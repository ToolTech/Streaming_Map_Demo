using Saab.Utility.GfxCaps;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
{
    public class FoliageModule : MonoBehaviour
    {
        public SceneManager SceneManager;
        public ComputeShader ComputeShader;
        public Material FoliageMaterial;

        [Header("Main Settings")]
        public int BufferSize = 1000000;
        public float DrawDistance = 3000;
        public float Density = 0.5f;
        public bool Shadows = false;

        [Header("Debug Settings")]
        public bool DebugPrintCount = false;
        public bool Disabled = false;
        public bool DebugNoDraw = false;

        [Header("Auto filled Settings")]
        public List<Foliage> FoliageTypes = new List<Foliage>();

        private ComputeBuffer _inderectBuffer;
        private FoliageFeature _foliage;
        private Vector4[] _frustum = new Vector4[6];
        private ComputeBuffer _foliageData;
        private float _maxHeight;

        // Start is called before the first frame update
        void Start()
        {
            SceneManager.OnNewGeometry += SceneManager_OnNewGeometry;
            SceneManager.OnPostTraverse += SceneManager_OnPostTraverse;
            SceneManager.OnEnterPool += SceneManager_OnEnterPool;

            Disabled = !GfxCaps.CurrentCaps.HasFlag(Capability.UseFoliageCrossboards);

            var FoliageSetting = GfxCaps.GetFoliageSettings;
            DrawDistance = FoliageSetting.DrawDistance;
            Shadows = FoliageSetting.Shadows;
            Density = FoliageSetting.Density;

            _foliage = new FoliageFeature(BufferSize, Density, ComputeShader);

            // ------- Setup -------
            _inderectBuffer = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);
            _inderectBuffer.SetData(new uint[] { 0, 1, 0, 0 });

            SetupFoliage();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            FoliageTypes = AssetDatabase.FindAssets($"t: {typeof(Foliage).Name}").ToList()
                     .Select(AssetDatabase.GUIDToAssetPath)
                     .Select(AssetDatabase.LoadAssetAtPath<Foliage>)
                     .ToList();
#endif
        }

        struct FoliageShaderData
        {
            public Vector2 MaxMin;
            public Vector2 Offset;
            public float Weight;
        };

        private void SetupFoliage()
        {


#if UNITY_ANDROID
            var format = TextureFormat.ETC2_RGBA8;
            Debug.Log("foliage Use ETC2");
#else
            var format = TextureFormat.DXT5;
#endif           
            var mainTexs = Create2DArray(FoliageTypes, format);

            FoliageMaterial.SetInt("_foliageCount", FoliageTypes.Count);
            FoliageMaterial.SetTexture("_MainTexArray", mainTexs);

            var data = new FoliageShaderData[FoliageTypes.Count];
            for (int i = 0; i < FoliageTypes.Count; i++)
            {
                var foliage = FoliageTypes[i];
                _maxHeight = Mathf.Max(_maxHeight, foliage.MaxMin.y);

                var item = new FoliageShaderData()
                {
                    MaxMin = foliage.MaxMin,
                    Offset = foliage.Offset,
                    Weight = foliage.Weight,
                };
                data[i] = item;
            }

            if (_foliageData != null)
                _foliageData.Release();

            _foliageData = new ComputeBuffer(FoliageTypes.Count, sizeof(float) * 5, ComputeBufferType.Default);
            _foliageData.SetData(data);
            FoliageMaterial.SetBuffer("_foliageData", _foliageData);
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

        public int GetMemoryfootprint()
        {
            return _foliage.GetVideoMemoryUsage();
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
        private void GenerateFrustumPlane(Camera camera)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);

            for (int i = 0; i < 6; i++)
            {
                _frustum[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
            }
            _frustum[5].w = DrawDistance;  // draw distance
        }
        private void SceneManager_OnEnterPool(GameObject go)
        {
            _foliage.RemoveFoliage(go);
        }
        private void SceneManager_OnNewGeometry(GameObject go)
        {
            var nodehandle = go.GetComponent<NodeHandle>();

            if (nodehandle != null && !Disabled)
            {
                if (nodehandle.node.BoundaryRadius < 890 && nodehandle.node.BoundaryRadius > 0)
                {
                    var res = _foliage.AddFoliage(go, nodehandle);

                    //if (Mathf.Abs(nodehandle.node.BoundaryRadius - 361.1371f) < 0.001f)
                    //    _foliage.AddFoliage(go, nodehandle);
                    //if (nodehandle.name == "15_38_85" || nodehandle.name == "15_38_84" || nodehandle.name == "15_39_85" || nodehandle.name == "15_39_84")
                    //    _foliage.AddFoliage(go, nodehandle);
                    //if (res != null)
                    //  go.GetComponent<MeshRenderer>().material.mainTexture = res.surfaceHeight;
                }
            }
        }

        private void OnDestroy()
        {
            if (_foliage != null)
                _foliage.Dispose();
            _inderectBuffer.Release();
            _foliageData.Dispose();
        }

        private void SceneManager_OnPostTraverse(bool locked)
        {
            if (Disabled)
                return;

            var cam = SceneManager.SceneManagerCamera.Camera;
            GenerateFrustumPlane(cam);

            // Render all points
            var buffer = _foliage.Cull(_frustum, cam, _maxHeight);
            FoliageMaterial.SetBuffer("_PointBuffer", buffer);

            // ------- Render -------
            ComputeBuffer.CopyCount(buffer, _inderectBuffer, 0);

            if (DebugPrintCount)
            {
                DebugPrintCount = false;
                int[] array = new int[4];
                _inderectBuffer.GetData(array);
                UnityEngine.Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, "Current buffer size :: {0}/{1}", array[0].ToString(), buffer.count);
            }

            if (DebugNoDraw)
                return;

            var renderBounds = new Bounds(Vector3.zero, new Vector3(DrawDistance, DrawDistance, DrawDistance));
            Graphics.DrawProceduralIndirect(FoliageMaterial, renderBounds, MeshTopology.Points, _inderectBuffer, 0, null, null, Shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);
        }
    }
}