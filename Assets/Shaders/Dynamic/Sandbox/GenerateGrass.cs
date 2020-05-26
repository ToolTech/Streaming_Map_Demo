using Saab.Unity.Core.ComputeExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saab.Unity.Sandbox
{
    public class Grass
    {
        public ComputeShader ComputeShader;
        public Guid ID;
        public bool Initlized = false;

        //Compute kernels
        public ComputeKernel MeshGrassGeneratorKernel;
        public ComputeKernel IndirectGrassKernel;
        public ComputeKernel CullKernal;

        public GameObject GameObject;
        public Texture2D SplatMap;
        public Texture2D NodeTexture;

        //Compute buffers
        public ComputeBuffer GrassBuffer;
        public ComputeBuffer SurfaceVertices;
        public ComputeBuffer SurfaceIndices;
        public ComputeBuffer SurfaceUVs;

        //public Mesh PointMeshGrass;
        public Mesh SurfaceMesh;

        public int MaxGrass;
    }

    public struct ComputeShaderID
    {
        // Buffers
        static public int surfaceVertices = Shader.PropertyToID("surfaceVertices");
        static public int surfaceIndices = Shader.PropertyToID("surfaceIndices");
        static public int surfaceUVs = Shader.PropertyToID("surfaceUVs");

        // Calculated points
        static public int terrainBuffer = Shader.PropertyToID("terrainPoints");
        //cull
        static public int cullInBuffer = Shader.PropertyToID("Input");
        static public int cullOutBuffer = Shader.PropertyToID("Output");

        static public int closeBuffer = Shader.PropertyToID("closeBuffer");

        // Indirect buffer
        static public int indirectBuffer = Shader.PropertyToID("indirectBuffer");

        // Textures           
        static public int splatMap = Shader.PropertyToID("splatMap");
        static public int nodeTexture = Shader.PropertyToID("NodeTexture");
        static public int placementMap = Shader.PropertyToID("PlacementMap");

        // Scalars & vectors
        static public int objToWorld = Shader.PropertyToID("ObjToWorld");

        static public int surfaceGridStep = Shader.PropertyToID("surfaceGridStep");
        static public int cullCount = Shader.PropertyToID("cullCount");
        static public int indexCount = Shader.PropertyToID("indexCount");

        static public int frustumPlanes = Shader.PropertyToID("frustumPlanes");
        static public int terrainResolution = Shader.PropertyToID("terrainResolution");
    }

    [Serializable]
    public struct TerrainTextures
    {
        public Texture2D FeatureTexture;
        public Texture2D NormalTexture;

        public Vector2 Height;
        public Vector2 Width;

        public Vector4 GetMinMaxWidthHeight
        {
            get
            {
                return new Vector4(Width.x, Width.y, Height.x, Height.y);
            }
        }
    }

    public class GenerateGrass : MonoBehaviour
    {
        private void Start()
        {
            OnlyCullOnce = false;

            if (PerlinNoise == null)
            {
                PerlinNoise = Resources.Load("Textures/PerlinNoiseRGB") as Texture2D;
            }

            _grassTextures = Create2DArray(GrassTextures);
            _minMaxWidthHeight = GetMinMaxWidthHeight(GrassTextures);

            _megaBuffer = new ComputeBuffer(1000000, sizeof(float) * 4, ComputeBufferType.Append);
            _inderectBuffer = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);
            _inderectBuffer.SetData(new uint[] { 0, 1, 0, 0 });

            GenerateFrustumPlane();
            // Initialize materials
            InitializeMaterial();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void AddGrass(GameObject go)
        {
            var _mesh = go.GetComponent<MeshFilter>().mesh;
            var _texture = go.GetComponent<MeshRenderer>().material.mainTexture;

            Grass grass = new Grass
            {
                ID = Guid.NewGuid(),
                GameObject = go,
                SurfaceMesh = _mesh,
                SplatMap = DefaultSplatMap,
                NodeTexture = (Texture2D)_texture
            };

            Initialize(grass);

            //add grass to list
            _drawGrass.Add(grass);
        }
        public void RemoveGrass(GameObject gameobj)
        {
            if (_drawGrass.Count == 0) { return; }
            var grass = _drawGrass.Where(go => go.GameObject == gameobj).First();
            if (grass == null) { return; }
            _drawGrass.Remove(grass);

            SafeRelease(grass);
        }
        private Vector4[] GenerateFrustumPlane(bool drawdistance = false)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            var frustumVector4 = new Vector4[6];

            for (int i = 0; i < 6; i++)
            {
                frustumVector4[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
            }

            if (drawdistance)
            {
                frustumVector4[5].w = DrawDistance;
            }
            else
            {
                frustumVector4[5].w = RenderDistance;
            }

            return frustumVector4;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private struct ShaderID
        {
            // Bufers
            static public int grassBuffer = Shader.PropertyToID("_GrassBuffer");

            // Textures
            static public int nodeTexture = Shader.PropertyToID("_NodeTexture");
            static public int grassTexture = Shader.PropertyToID("_MainTexGrass");
            static public int perlinNoise = Shader.PropertyToID("_PerlinNoise");

            // Matrix
            static public int worldToObj = Shader.PropertyToID("_worldToObj");

            // wind
            static public int grassWind = Shader.PropertyToID("_GrassTextureWaving");

            static public int frustumPlanes = Shader.PropertyToID("_FrustumPlanes");

            static public int minMaxWidthHeight = Shader.PropertyToID("_MinMaxWidthHeight");

            static public int viewDir = Shader.PropertyToID("_ViewDir");
        }

        //////////////////////////////////////////////////// Grass Settings ////////////////////////////////////////////////////

        private List<Grass> _drawGrass = new List<Grass>();

        public TerrainTextures[] GrassTextures;
        private Vector4[] _minMaxWidthHeight;

        public Texture2D PerlinNoise;
        public Texture2D DefaultSplatMap;

        public Texture2D GetSplatMap
        {
            get
            {
                return DefaultSplatMap;
            }
        }

        // ********** frustum/Cull **********
        public int RenderDistance = 400;
        public int DrawDistance = 250;
        private Vector4[] _frustumPlanes;

        public bool UpdateGrass = true;
        public bool DrawGrassShadows = true;
        public float GrassWind = 0.01f;
        public float GrassDensity = 0.0413f;

        [Header("****** SHADERS ******")]
        public ComputeShader ComputeShader;
        public Shader GrassShader;

        private Material _grassMaterial;

        private ComputeBuffer _megaBuffer;
        private ComputeBuffer _inderectBuffer;

        private Texture2DArray _grassTextures;
        public Transform RoiTransform;
        public bool DebugOn;
        public bool OnlyCullOnce;

        // Compute kernel names
        private const string _indirectGrassKernelName = "IndirectGrass";
        private const string _meshGrassGeneratorKernelName = "MeshGrassGenerator";
        private const string _cullKernelName = "Cull";

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Texture2DArray Create2DArray(TerrainTextures[] texture)
        {
            var textureCount = texture.Length;

            var textureResolution = Math.Max(texture.Max(item => item.FeatureTexture.width), texture.Max(item => item.FeatureTexture.height));

            int[] availableGrassResolutions = new int[] { 64, 128, 256, 512, 1024, 2048, 4096 };

            textureResolution = Mathf.Min(textureResolution, availableGrassResolutions[availableGrassResolutions.Length - 1]);
            for (int i = 0; i < availableGrassResolutions.Length; i++)
            {
                if (textureResolution <= availableGrassResolutions[i])
                {
                    textureResolution = availableGrassResolutions[i];
                    break;
                }
            }

            var textureArray = new Texture2DArray(textureResolution, textureResolution, textureCount, TextureFormat.DXT5, true)
            {
                wrapMode = TextureWrapMode.Clamp
            };


            RenderTexture temporaryGrassRenderTexture = new RenderTexture(textureResolution, textureResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
            {
                useMipMap = true,
                antiAliasing = 1
            };

            for (int i = 0; i < textureCount; i++)
            {
                Graphics.Blit(texture[i].FeatureTexture, temporaryGrassRenderTexture);
                Texture2D temporaryGrassTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.ARGB32, true);
                RenderTexture.active = temporaryGrassRenderTexture;
                temporaryGrassTexture.ReadPixels(new Rect(0, 0, temporaryGrassTexture.width, temporaryGrassTexture.height), 0, 0);
                RenderTexture.active = null;
                temporaryGrassTexture.Apply(true);
                temporaryGrassTexture.Compress(true);

                Graphics.CopyTexture(temporaryGrassTexture, 0, textureArray, i);
                Destroy(temporaryGrassTexture);
            }
            textureArray.Apply(false, true);

            Destroy(temporaryGrassRenderTexture);

            return textureArray;
        }
        private bool IsInFrustum(Vector3 positionAfterProjection, float treshold = -1)
        {
            float cullValue = treshold;

            return (Vector3.Dot(_frustumPlanes[0], positionAfterProjection) >= cullValue &&
                Vector3.Dot(_frustumPlanes[1], positionAfterProjection) >= cullValue &&
                Vector3.Dot(_frustumPlanes[2], positionAfterProjection) >= cullValue &&
                Vector3.Dot(_frustumPlanes[3], positionAfterProjection) >= cullValue) &&
            (_frustumPlanes[5].w >= Mathf.Abs(Vector3.Distance(Vector3.zero, positionAfterProjection)));
        }
        private Vector4[] GetMinMaxWidthHeight(TerrainTextures[] textures)
        {
            List<Vector4> MinMaxWidthHeight = new List<Vector4>();

            foreach (TerrainTextures item in textures)
            {
                MinMaxWidthHeight.Add(item.GetMinMaxWidthHeight);
            }

            return MinMaxWidthHeight.ToArray();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // --- GrassComponent ---
        private void Initialize(Grass grass)
        {
            //Init shaders
            grass.ComputeShader = Instantiate(ComputeShader);

            // Init kernels
            grass.MeshGrassGeneratorKernel = new ComputeKernel(_meshGrassGeneratorKernelName, grass.ComputeShader);
            grass.IndirectGrassKernel = new ComputeKernel(_indirectGrassKernelName, grass.ComputeShader);
            grass.CullKernal = new ComputeKernel(_cullKernelName, grass.ComputeShader);

            var maxGrassMesh = (int)Mathf.Ceil((grass.SurfaceMesh.bounds.size.x * grass.SurfaceMesh.bounds.size.z) / GrassDensity);
            grass.MaxGrass = maxGrassMesh;

            grass.GrassBuffer = new ComputeBuffer(grass.MaxGrass, sizeof(float) * 4, ComputeBufferType.Append);

            var surfaceVertices = grass.SurfaceMesh.vertices;
            var surfaceIndices = grass.SurfaceMesh.GetIndices(0);
            var surfaceUVs = grass.SurfaceMesh.uv;

            grass.SurfaceVertices = new ComputeBuffer(surfaceVertices.Length, sizeof(float) * 3, ComputeBufferType.Default);
            grass.SurfaceIndices = new ComputeBuffer(surfaceIndices.Length, sizeof(int), ComputeBufferType.Default);
            grass.SurfaceUVs = new ComputeBuffer(surfaceUVs.Length, sizeof(float) * 2, ComputeBufferType.Default);

            grass.ComputeShader.SetInt(ComputeShaderID.cullCount, grass.MaxGrass);
            grass.ComputeShader.SetInt(ComputeShaderID.indexCount, surfaceIndices.Length);

            // fill surface vertices
            grass.SurfaceVertices.SetData(surfaceVertices);
            grass.MeshGrassGeneratorKernel.SetBuffer(ComputeShaderID.surfaceVertices, grass.SurfaceVertices);
            grass.SurfaceIndices.SetData(surfaceIndices);
            grass.MeshGrassGeneratorKernel.SetBuffer(ComputeShaderID.surfaceIndices, grass.SurfaceIndices);
            grass.SurfaceUVs.SetData(surfaceUVs);
            grass.MeshGrassGeneratorKernel.SetBuffer(ComputeShaderID.surfaceUVs, grass.SurfaceUVs);

        }
        // --- GrassComponent ---
        private void InitializeMaterial()
        {
            _grassMaterial = new Material(GrassShader);

            // ********************** Grass material **********************
            _grassMaterial.SetBuffer(ShaderID.grassBuffer, _megaBuffer);

            _grassMaterial.SetTexture(ShaderID.grassTexture, _grassTextures);
            _grassMaterial.SetTexture(ShaderID.perlinNoise, PerlinNoise);

            _grassMaterial.SetVectorArray(ShaderID.minMaxWidthHeight, _minMaxWidthHeight);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void Render(List<Grass> grassList)
        {
            if (!UpdateGrass) { return; }

            // Reset grass buffer and run generation
            if (!OnlyCullOnce)
            {
                _megaBuffer.SetCounterValue(0);
            }
            _frustumPlanes = GenerateFrustumPlane();

            foreach (Grass grass in grassList)
            {
                if (!grass.GameObject.activeSelf) { return; }

                UpdatePlanePos(grass);

                var longestSide = Mathf.Max(grass.SurfaceMesh.bounds.size.x, grass.SurfaceMesh.bounds.size.z, grass.SurfaceMesh.bounds.size.y);
                _frustumPlanes[5].w = DrawDistance + longestSide * 0.75f;

                if (!IsInFrustum(grass.GameObject.transform.position, -longestSide * 1.75f))
                {
                    continue;
                }

                UpdateShaderValues(grass);

                if (!grass.Initlized)
                {
                    grass.MeshGrassGeneratorKernel.SetBuffer(ComputeShaderID.terrainBuffer, grass.GrassBuffer);                 // Grass generator output
                    grass.GrassBuffer.SetCounterValue(0);
                    var GenX = Mathf.CeilToInt(grass.SurfaceMesh.triangles.Length / 8.0f);

                    grass.MeshGrassGeneratorKernel.Dispatch(GenX, 1, 1);
                    grass.Initlized = true;

                    grass.CullKernal.SetBuffer(ComputeShaderID.cullInBuffer, grass.GrassBuffer);

                    grass.SurfaceVertices.SafeRelease();
                    grass.SurfaceIndices.SafeRelease();
                    grass.SurfaceUVs.SafeRelease();
                }

                if (!OnlyCullOnce && grass.Initlized)
                {
                    _frustumPlanes[5].w = DrawDistance;

                    grass.ComputeShader.SetVectorArray(ComputeShaderID.frustumPlanes, _frustumPlanes);

                    // TODO: move entirely to GPU with DispatchIndirect 
                    grass.CullKernal.SetBuffer(ComputeShaderID.indirectBuffer, _inderectBuffer);
                    ComputeBuffer.CopyCount(grass.GrassBuffer, _inderectBuffer, 0);

                    var x = Mathf.CeilToInt(grass.MaxGrass / 128.0f);
                    grass.CullKernal.Dispatch(x, 1, 1);
                }
            }

            // ********* Update grassmaterial *********
            _grassMaterial.SetFloat(ShaderID.grassWind, GrassWind);
            if (RoiTransform != null)
            {
                _grassMaterial.SetMatrix(ShaderID.worldToObj, RoiTransform.worldToLocalMatrix);
            }
            _grassMaterial.SetVector(ShaderID.viewDir, Camera.main.transform.forward);

            // Culling      
            ComputeBuffer.CopyCount(_megaBuffer, _inderectBuffer, 0);

            if (DebugOn)
            {
                DebugOn = false;
                var array = new uint[] { 0, 0, 0, 0 };
                _inderectBuffer.GetData(array);
                Debug.Log(array[0]);
            }

            var bounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));
            Graphics.DrawProceduralIndirect(_grassMaterial, bounds, MeshTopology.Points, _inderectBuffer, 0, null, null, DrawGrassShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);
            //Graphics.DrawMeshInstancedIndirect(_mesh, 0, grass.GrassMaterial, new Bounds(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue)), grass.ArgsBufferGrass, 0, null, DrawGrassShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);
        }
        private void UpdateShaderValues(Grass grass)
        {
            grass.ComputeShader.SetInt(ComputeShaderID.terrainResolution, grass.SplatMap.height);
            grass.ComputeShader.SetMatrix(ComputeShaderID.objToWorld, grass.GameObject.transform.localToWorldMatrix);
            grass.ComputeShader.SetFloat(ComputeShaderID.surfaceGridStep, GrassDensity);

            grass.MeshGrassGeneratorKernel.SetTexture(ComputeShaderID.splatMap, grass.SplatMap);
            grass.MeshGrassGeneratorKernel.SetTexture(ComputeShaderID.nodeTexture, grass.NodeTexture);

            grass.CullKernal.SetBuffer(ComputeShaderID.cullOutBuffer, _megaBuffer);
        }
        private void UpdatePlanePos(Grass grass)
        {
            grass.ComputeShader.SetVectorArray(ComputeShaderID.frustumPlanes, _frustumPlanes);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void Update()
        {
            Render(_drawGrass);
        }

        private void OnDestroy()
        {
            foreach (Grass grass in _drawGrass)
            {
                SafeRelease(grass);
            }

            _inderectBuffer.SafeRelease();
            _megaBuffer.SafeRelease();
        }

        private void SafeRelease(Grass grass)
        {
            grass.GrassBuffer.SafeRelease();

            Destroy(grass.ComputeShader);

            grass.SurfaceVertices.SafeRelease();
            grass.SurfaceIndices.SafeRelease();
            grass.SurfaceUVs.SafeRelease();
        }
    }
}