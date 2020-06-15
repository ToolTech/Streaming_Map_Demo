using Saab.Unity.Core.ComputeExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
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
        public Texture2D PlacementMap;

        //Compute buffers
        public ComputeBuffer GrassBuffer;
        public ComputeBuffer SurfaceVertices;
        public ComputeBuffer SurfaceIndices;
        public ComputeBuffer SurfaceUVs;

        //public Mesh PointMeshGrass;
        public Mesh SurfaceMesh;

        public Vector3 SurfaceSize;
        public int SurfaceTriangles;

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

        public Vector2 Height;
        public Vector2 Width;
        public float Yoffset;

        public Vector4 GetMinMaxWidthHeight
        {
            get
            {
                return new Vector4(Width.x, Width.y, Height.x, Height.y);
            }
        }
    }

    public class GrassModule : MonoBehaviour
    {
        private void Start()
        {
            if (PerlinNoise == null)
            {
                PerlinNoise = Resources.Load("Textures/PerlinNoiseRGB") as Texture2D;
            }

            _grassTextures = Create2DArray(GrassTextures, UseETC2);
            _minMaxWidthHeight = GetMinMaxWidthHeight(GrassTextures);

            _megaBuffer = new ComputeBuffer(2000000, sizeof(float) * 4, ComputeBufferType.Append);
            _inderectBuffer = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);
            _inderectBuffer.SetData(new uint[] { 0, 1, 0, 0 });

            // Initialize materials
            InitializeMaterial();
        }

        public void UpdateSceneCamera(ISceneManagerCamera newCam)
        {
            _sceneCamera = newCam;
            _frustumPlanes = GenerateFrustumPlane();
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
                NodeTexture = (Texture2D)_texture,
                PlacementMap = PlacementMap,
                ComputeShader = Instantiate(ComputeShader),
                SurfaceSize = new Vector3(_mesh.bounds.size.x, _mesh.bounds.size.y, _mesh.bounds.size.z)
            };

            //grass.SurfaceSize = new Vector3(grass.SurfaceMesh.bounds.size.x, grass.SurfaceMesh.bounds.size.y, grass.SurfaceMesh.bounds.size.z);

            //Initialize(grass);

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

        public void RemoveGrass(Grass grass)
        {
            _drawGrass.Remove(grass);
            SafeRelease(grass);
        }

        private Vector4[] GenerateFrustumPlane()
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            var frustumVector4 = new Vector4[6];

            for (int i = 0; i < 6; i++)
            {
                frustumVector4[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
            }

            frustumVector4[5].w = DrawDistance;

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
        public Texture2D PlacementMap;

        public Texture2D GetSplatMap
        {
            get
            {
                return DefaultSplatMap;
            }
        }


        // ********** frustum/Cull **********
        public int DrawDistance = 250;
        private Vector4[] _frustumPlanes;

        private ISceneManagerCamera _sceneCamera;

        public bool UpdateGrass = true;
        public bool DrawGrassShadows = false;
        public float GrassWind = 0.01f;
        public float GrassDensity = 0.0413f;

        [Header("****** SHADERS ******")]
        public ComputeShader ComputeShader;
        public Shader GrassShader;

        private Material _grassMaterial;

        private ComputeBuffer _megaBuffer;
        private ComputeBuffer _inderectBuffer;

        private Texture2DArray _grassTextures;
        private Transform _roiTransform;
        public bool UseETC2 = false;

        // Compute kernel names
        private const string _indirectGrassKernelName = "IndirectGrass";
        private const string _meshGrassGeneratorKernelName = "MeshGrassGenerator";
        private const string _cullKernelName = "Cull";

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Texture2DArray Create2DArray(TerrainTextures[] texture, bool etc2)
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

            Texture2DArray textureArray;

            if (etc2)
            {
                textureArray = new Texture2DArray(textureResolution, textureResolution, textureCount, TextureFormat.ARGB32, true)
                {
                    wrapMode = TextureWrapMode.Clamp
                };
            }
            else
            {
                textureArray = new Texture2DArray(textureResolution, textureResolution, textureCount, TextureFormat.DXT5, true)
                {
                    wrapMode = TextureWrapMode.Clamp
                };
            }


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

                //TexToFile(temporaryGrassTexture, Application.dataPath + "/../grassTextureArraySaved_" + i + ".png");

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

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // --- GrassComponent ---
        private void Initialize(Grass grass)
        {
            //Init shaders
            //grass.ComputeShader = Instantiate(ComputeShader);

            // Init kernels
            grass.MeshGrassGeneratorKernel = new ComputeKernel(_meshGrassGeneratorKernelName, grass.ComputeShader);
            grass.IndirectGrassKernel = new ComputeKernel(_indirectGrassKernelName, grass.ComputeShader);
            grass.CullKernal = new ComputeKernel(_cullKernelName, grass.ComputeShader);

            grass.SurfaceSize = new Vector3(grass.SurfaceMesh.bounds.size.x, grass.SurfaceMesh.bounds.size.y, grass.SurfaceMesh.bounds.size.z);
            grass.SurfaceTriangles = grass.SurfaceMesh.triangles.Length;

            var maxGrassMesh = (int)Mathf.Ceil((grass.SurfaceSize.x * grass.SurfaceSize.z) / GrassDensity) / 2;
            grass.MaxGrass = maxGrassMesh > 0 ? maxGrassMesh : 1;

            //Debug.LogWarning("About to add to memory: " + (grass.MaxGrass / 1000000.0).ToString("F4") + " mb");
            //PrintTotalBufferSize();

            grass.GrassBuffer = new ComputeBuffer(grass.MaxGrass, sizeof(float) * 4, ComputeBufferType.Append);

            var surfaceVertices = grass.SurfaceMesh.vertices;
            var surfaceIndices = grass.SurfaceMesh.GetIndices(0);
            var surfaceUVs = grass.SurfaceMesh.uv;

            //Destroy(grass.SurfaceMesh);

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

        private void PrintTotalBufferSize()
        {
            int size = 0;
            int extra = 0;
            foreach (Grass grass in _drawGrass)
            {
                if (grass.Initlized)
                {
                    size += grass.MaxGrass * sizeof(float) * 4;
                    extra++;
                }
                if (grass.SurfaceVertices != null)
                {
                    size += grass.SurfaceMesh.vertices.Length * sizeof(float) * 3;
                    size += grass.SurfaceMesh.GetIndices(0).Length * sizeof(int);
                    size += grass.SurfaceMesh.uv.Length * sizeof(float) * 2;
                }
            }
            Debug.LogFormat("buffer grass memory: " + (size / 1000000.0).ToString("F4") + " mb buffers: " + extra + " average: " + ((size / 1000000.0) / extra).ToString("F4"));
        }

        private void Render(List<Grass> grassList)
        {
            //PrintTotalBufferSize();
            if (Camera.main != null)
            {
                // Reset grass buffer and run generation
                _megaBuffer.SetCounterValue(0);
                _frustumPlanes = GenerateFrustumPlane();

                //int current = 0;
                //int active = grassList.Where(p => p.Initlized == true).Count();

                for (int i = 0; i < grassList.Count; i++)
                {
                    var grass = grassList[i];

                    if (grass.GameObject == null)
                    {
                        RemoveGrass(grass);
                        continue;
                    }
                    if (!grass.GameObject.activeSelf) { continue; }

                    UpdatePlanePos(grass);

                    var longestSide = Mathf.Max(grass.SurfaceSize.x, grass.SurfaceSize.z, grass.SurfaceSize.y);
                    _frustumPlanes[5].w = DrawDistance + longestSide * 1.25f;

                    if (!IsInFrustum(grass.GameObject.transform.position, -longestSide * 1.25f))
                    {
                        continue;
                    }

                    UpdateShaderValues(grass);

                    if (!grass.Initlized)
                    {
                        Initialize(grass);

                        grass.MeshGrassGeneratorKernel.SetTexture(ComputeShaderID.splatMap, grass.SplatMap);
                        grass.MeshGrassGeneratorKernel.SetTexture(ComputeShaderID.nodeTexture, grass.NodeTexture);
                        grass.MeshGrassGeneratorKernel.SetTexture(ComputeShaderID.placementMap, grass.PlacementMap);
                        grass.CullKernal.SetBuffer(ComputeShaderID.cullOutBuffer, _megaBuffer);

                        grass.MeshGrassGeneratorKernel.SetBuffer(ComputeShaderID.terrainBuffer, grass.GrassBuffer);                 // Grass generator output
                        grass.GrassBuffer.SetCounterValue(0);
                        var GenX = Mathf.CeilToInt(grass.SurfaceTriangles / 8.0f);

                        grass.MeshGrassGeneratorKernel.Dispatch(GenX, 1, 1);
                        grass.Initlized = true;

                        grass.CullKernal.SetBuffer(ComputeShaderID.cullInBuffer, grass.GrassBuffer);

                        grass.SurfaceVertices.SafeRelease();
                        grass.SurfaceIndices.SafeRelease();
                        grass.SurfaceUVs.SafeRelease();

                        _roiTransform = FindFirstNodeParent(grass.GameObject.transform);
                    }
                    //current++;

                    _frustumPlanes[5].w = DrawDistance;

                    grass.ComputeShader.SetVectorArray(ComputeShaderID.frustumPlanes, _frustumPlanes);

                    // TODO: move entirely to GPU with DispatchIndirect 
                    grass.CullKernal.SetBuffer(ComputeShaderID.indirectBuffer, _inderectBuffer);
                    ComputeBuffer.CopyCount(grass.GrassBuffer, _inderectBuffer, 0);

                    var x = Mathf.CeilToInt(grass.MaxGrass / 128.0f);
                    grass.CullKernal.Dispatch(x, 1, 1);
                }

                //Debug.LogFormat("grass buffers total: " + grassList.Count() + "  intlized: " + active + " Drawn: " + current);
                //Debug.LogFormat("active grass buffers: " + active + " drawn: " + current + " total: " + grassList.Count());

                // ********* Update grassmaterial *********
                _grassMaterial.SetFloat(ShaderID.grassWind, GrassWind);
                if (_roiTransform != null)
                {
                    _grassMaterial.SetMatrix(ShaderID.worldToObj, _roiTransform.worldToLocalMatrix);
                }
                _grassMaterial.SetVector(ShaderID.viewDir, Camera.main.transform.forward);

                // Culling      
                ComputeBuffer.CopyCount(_megaBuffer, _inderectBuffer, 0);
                var bounds = new Bounds(Vector3.zero, new Vector3(DrawDistance + DrawDistance / 3, DrawDistance + DrawDistance / 3, DrawDistance + DrawDistance / 3) * 1.5f);
                Graphics.DrawProceduralIndirect(_grassMaterial, bounds, MeshTopology.Points, _inderectBuffer, 0, null, null, DrawGrassShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);
                //Graphics.DrawMeshInstancedIndirect(_mesh, 0, grass.GrassMaterial, new Bounds(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue)), grass.ArgsBufferGrass, 0, null, DrawGrassShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);
            }
        }
        private void UpdateShaderValues(Grass grass)
        {
            grass.ComputeShader.SetInt(ComputeShaderID.terrainResolution, grass.SplatMap.height);
            grass.ComputeShader.SetMatrix(ComputeShaderID.objToWorld, grass.GameObject.transform.localToWorldMatrix);
            grass.ComputeShader.SetFloat(ComputeShaderID.surfaceGridStep, GrassDensity);

            //grass.MeshGrassGeneratorKernel.SetTexture(ComputeShaderID.splatMap, grass.SplatMap);
            //grass.MeshGrassGeneratorKernel.SetTexture(ComputeShaderID.nodeTexture, grass.NodeTexture);
            //grass.MeshGrassGeneratorKernel.SetTexture(ComputeShaderID.placementMap, grass.PlacementMap);

            //grass.CullKernal.SetBuffer(ComputeShaderID.cullOutBuffer, _megaBuffer);
        }
        private void UpdatePlanePos(Grass grass)
        {
            grass.ComputeShader.SetVectorArray(ComputeShaderID.frustumPlanes, _frustumPlanes);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Camera_OnPostTraverse()
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
