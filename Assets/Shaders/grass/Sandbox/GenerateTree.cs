using Saab.Unity.Core.ComputeExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saab.Unity.Sandbox
{
    public class Trees
    {
        public ComputeShader ComputeShader;
        public Guid ID;
        public bool Initlized = false;

        //Compute kernels
        public ComputeKernel MeshTreeGeneratorKernel;
        public ComputeKernel IndirectTreeKernel;
        public ComputeKernel CullKernal;

        public GameObject GameObject;
        public Texture2D SplatMap;
        public Texture2D NodeTexture;
        public Texture2D PlacementMap;

        //Compute buffers
        public ComputeBuffer TreeBuffer;
        public ComputeBuffer SurfaceVertices;
        public ComputeBuffer SurfaceIndices;
        public ComputeBuffer SurfaceUVs;

        //public Mesh PointMeshGrass;
        public Mesh SurfaceMesh;

        public int MaxTree;
    }
    public class GenerateTree : MonoBehaviour
    {
        private void Start()
        {
            OnlyCullOnce = false;

            if (PerlinNoise == null)
            {
                PerlinNoise = Resources.Load("Textures/PerlinNoiseRGB") as Texture2D;
            }

            _treeTextures = Create2DArray(TreeTextures);
            _minMaxWidthHeight = GetMinMaxWidthHeight(TreeTextures);
            //SceneManager.

            _megaBuffer = new ComputeBuffer(1000000, sizeof(float) * 4, ComputeBufferType.Append);
            _closeMegaBuffer = new ComputeBuffer(2000000, sizeof(float) * 4, ComputeBufferType.Append);

            _inderectBuffer = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);
            _inderectBuffer.SetData(new uint[] { 0, 1, 0, 0 });

            _closeInderectBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            var subMeshIndex = 0;
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, TestMesh.subMeshCount - 1);
            _closeInderectBuffer.SetData(new uint[5] { TestMesh.GetIndexCount(subMeshIndex),0, TestMesh.GetIndexStart(subMeshIndex), TestMesh.GetBaseVertex(subMeshIndex), 0 });

            GenerateFrustumPlane();
            // -------- Initialize materials -------- \\
            InitializeMaterial();

            StartCoroutine(Initlize());
        }

        //public void StartCoroutine()
        //{
        //    StartCoroutine(Initlize());
        //}
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void AddTree(GameObject go)
        {
            var _mesh = go.GetComponent<MeshFilter>().mesh;
            var _texture = go.GetComponent<MeshRenderer>().material.mainTexture;

            Trees tree = new Trees
            {
                ID = Guid.NewGuid(),
                GameObject = go,
                SurfaceMesh = _mesh,
                SplatMap = DefaultSplatMap,
                NodeTexture = (Texture2D)_texture,
                PlacementMap = PlacementMap,
            };
            Initialize(tree);

            //add grass to list
            _drawTree.Add(tree);
        }
        public void RemoveTree(GameObject gameobj)
        {
            if (_drawTree.Count == 0) { return; }
            var tree = _drawTree.Where(go => go.GameObject == gameobj).First();
            if (tree == null) { return; }
            _drawTree.Remove(tree);

            SafeRelease(tree);
        }
        public void RemoveTree(Trees tree)
        {
            _drawTree.Remove(tree);
            SafeRelease(tree);
        }
        private Vector4[] GenerateFrustumPlane()
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            var frustumVector4 = new Vector4[6];

            for (int i = 0; i < 6; i++)
            {
                frustumVector4[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
            }

            frustumVector4[5].w = RenderDistance;
            return frustumVector4;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private struct ShaderID
        {
            // Bufers
            static public int treeBuffer = Shader.PropertyToID("_GrassBuffer");

            // Textures
            static public int nodeTexture = Shader.PropertyToID("_NodeTexture");
            static public int treeTexture = Shader.PropertyToID("_MainTexGrass");
            static public int perlinNoise = Shader.PropertyToID("_PerlinNoise");
            static public int colorVariance = Shader.PropertyToID("_ColorVariance");

            // Matrix
            static public int worldToObj = Shader.PropertyToID("_worldToObj");
            //static public int worldToScreen = Shader.PropertyToID("_worldToScreen");

            // wind
            static public int Wind = Shader.PropertyToID("_GrassTextureWaving");

            static public int frustumPlanes = Shader.PropertyToID("_FrustumPlanes");

            static public int minMaxWidthHeight = Shader.PropertyToID("_MinMaxWidthHeight");

            static public int viewDir = Shader.PropertyToID("_ViewDir");
            static public int FadeFar = Shader.PropertyToID("_FadeFar");
            static public int FadeNear = Shader.PropertyToID("_FadeNear");
            static public int FadeNearAmount = Shader.PropertyToID("_FadeNearAmount");
            static public int FadeFarAmount = Shader.PropertyToID("_FadeFarAmount");
        }

        //////////////////////////////////////////////////// Grass Settings ////////////////////////////////////////////////////

        private List<Trees> _drawTree = new List<Trees>();

        public TerrainTextures[] TreeTextures;
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
        public int RenderDistance = 5000;
        public int DrawDistance = 4500;
        private Vector4[] _frustumPlanes;

        // TODO: replace with better solution
        public bool UpdateTree = true;
        public bool DrawTreeShadows = true;
        public float Wind = 0.0f;
        public float Density = 22.127f;

        [Header("****** SHADERS ******")]
        public ComputeShader ComputeShader;
        public Shader TreeShader;
        public float FadeFarValue = 3000;
        public float FadeNearValue = 50;
        public float FadeNearAmount = 50;
        public float FadeFarAmount = 1000;

        private Material _treeMaterial;

        private ComputeBuffer _megaBuffer;
        private ComputeBuffer _closeMegaBuffer;
        private ComputeBuffer _inderectBuffer;
        private ComputeBuffer _closeInderectBuffer;

        private Texture2DArray _treeTextures;
        public Transform RoiTransform;

        public Mesh TestMesh;
        public Material TestMat;
        public bool DebugOn;
        public bool OnlyCullOnce;

        // Compute kernel names
        private const string _indirectTreeKernelName = "IndirectGrass";
        private const string _meshTreeGeneratorKernelName = "MeshTreeGenerator";
        private const string _cullKernelName = "TreeCull";

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Texture2DArray Create2DArray(TerrainTextures[] texture)
        {
            var textureCount = texture.Length;

            var textureResolution = Math.Max(texture.Max(item => item.FeatureTexture.width), texture.Max(item => item.FeatureTexture.height));

            int[] availableResolutions = new int[] { 64, 128, 256, 512, 1024, 2048, 4096 };

            textureResolution = Mathf.Min(textureResolution, availableResolutions[availableResolutions.Length - 1]);
            for (int i = 0; i < availableResolutions.Length; i++)
            {
                if (textureResolution <= availableResolutions[i])
                {
                    textureResolution = availableResolutions[i];
                    break;
                }
            }

            var textureArray = new Texture2DArray(textureResolution, textureResolution, textureCount, TextureFormat.ARGB32, true)
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
        private void Initialize(Trees tree)
        {
            //Init shaders
            tree.ComputeShader = Instantiate(ComputeShader);

            // Init kernels
            tree.MeshTreeGeneratorKernel = new ComputeKernel(_meshTreeGeneratorKernelName, tree.ComputeShader);
            tree.IndirectTreeKernel = new ComputeKernel(_indirectTreeKernelName, tree.ComputeShader);
            tree.CullKernal = new ComputeKernel(_cullKernelName, tree.ComputeShader);

            var maxTreeMesh = (int)Mathf.Ceil((tree.SurfaceMesh.bounds.size.x * tree.SurfaceMesh.bounds.size.z) / Density);
            tree.MaxTree = maxTreeMesh > 0 ? maxTreeMesh : 1;

            tree.TreeBuffer = new ComputeBuffer(tree.MaxTree, sizeof(float) * 4, ComputeBufferType.Append);

            var surfaceVertices = tree.SurfaceMesh.vertices;
            var surfaceIndices = tree.SurfaceMesh.GetIndices(0);
            var surfaceUVs = tree.SurfaceMesh.uv;

            tree.SurfaceVertices = new ComputeBuffer(surfaceVertices.Length, sizeof(float) * 3, ComputeBufferType.Default);
            tree.SurfaceIndices = new ComputeBuffer(surfaceIndices.Length, sizeof(int), ComputeBufferType.Default);
            tree.SurfaceUVs = new ComputeBuffer(surfaceUVs.Length, sizeof(float) * 2, ComputeBufferType.Default);

            tree.ComputeShader.SetInt(ComputeShaderID.cullCount, tree.MaxTree);
            tree.ComputeShader.SetInt(ComputeShaderID.indexCount, surfaceIndices.Length);

            // fill surface vertices
            tree.SurfaceVertices.SetData(surfaceVertices);
            tree.MeshTreeGeneratorKernel.SetBuffer(ComputeShaderID.surfaceVertices, tree.SurfaceVertices);
            tree.SurfaceIndices.SetData(surfaceIndices);
            tree.MeshTreeGeneratorKernel.SetBuffer(ComputeShaderID.surfaceIndices, tree.SurfaceIndices);
            tree.SurfaceUVs.SetData(surfaceUVs);
            tree.MeshTreeGeneratorKernel.SetBuffer(ComputeShaderID.surfaceUVs, tree.SurfaceUVs);

            tree.ComputeShader.SetInt(ComputeShaderID.terrainResolution, tree.SplatMap.height);
            tree.ComputeShader.SetMatrix(ComputeShaderID.objToWorld, tree.GameObject.transform.localToWorldMatrix);
            tree.ComputeShader.SetFloat(ComputeShaderID.surfaceGridStep, Density);

            tree.MeshTreeGeneratorKernel.SetTexture(ComputeShaderID.splatMap, tree.SplatMap);
            tree.MeshTreeGeneratorKernel.SetTexture(ComputeShaderID.nodeTexture, tree.NodeTexture);
            tree.MeshTreeGeneratorKernel.SetTexture(ComputeShaderID.placementMap, tree.PlacementMap);

        }
        // --- GrassComponent ---
        private void InitializeMaterial()
        {
            _treeMaterial = new Material(TreeShader);

            // ********************** Grass material **********************
            _treeMaterial.SetBuffer(ShaderID.treeBuffer, _megaBuffer);
            TestMat.SetBuffer("_Buffer", _closeMegaBuffer);

            _treeMaterial.SetTexture(ShaderID.treeTexture, _treeTextures);
            _treeMaterial.SetTexture(ShaderID.nodeTexture, PerlinNoise);
            _treeMaterial.SetTexture(ShaderID.perlinNoise, PerlinNoise);
            _treeMaterial.SetTexture(ShaderID.colorVariance, PerlinNoise);

            _treeMaterial.SetVectorArray(ShaderID.minMaxWidthHeight, _minMaxWidthHeight);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        IEnumerator Initlize()
        {
            while (true)
            {
                if (Camera.main != null)
                {
                    _frustumPlanes = GenerateFrustumPlane();

                    if (!UpdateTree)
                    {
                        break;
                    }

                    for (int i = 0; i < _drawTree.Count; i++)
                    {
                        var tree = _drawTree[i];

                        if (tree.GameObject == null)
                        {
                            //RemoveTree(tree);
                            continue;
                        }

                        if (!tree.GameObject.activeSelf) { continue; }
                        var longestSide = Mathf.Max(tree.SurfaceMesh.bounds.size.x, tree.SurfaceMesh.bounds.size.z, tree.SurfaceMesh.bounds.size.y);
                        _frustumPlanes[5].w = DrawDistance + longestSide * 0.75f;

                        if (!IsInFrustum(tree.GameObject.transform.position, -longestSide * 0.75f))
                        {
                            continue;
                        }

                        //UpdatePlanePos(tree);
                        //UpdateShaderValues(tree);

                        if (!tree.Initlized)
                        {
                            tree.MeshTreeGeneratorKernel.SetBuffer(ComputeShaderID.terrainBuffer, tree.TreeBuffer);                 // tree generator output
                            tree.TreeBuffer.SetCounterValue(0);
                            var GenX = Mathf.CeilToInt(tree.SurfaceMesh.triangles.Length / 8.0f);

                            tree.MeshTreeGeneratorKernel.Dispatch(GenX, 1, 1);
                            tree.Initlized = true;

                            tree.CullKernal.SetBuffer(ComputeShaderID.cullInBuffer, tree.TreeBuffer);

                            tree.SurfaceVertices.SafeRelease();
                            tree.SurfaceIndices.SafeRelease();
                            tree.SurfaceUVs.SafeRelease();
                            yield return null;
                        }
                    }
                }
                yield return null;
            }
        }

        private void Render(List<Trees> treeList)
        {
            _treeMaterial.SetFloat(ShaderID.FadeFar, FadeFarValue);
            _treeMaterial.SetFloat(ShaderID.FadeNear, FadeNearValue);
            _treeMaterial.SetFloat(ShaderID.FadeNearAmount, FadeNearAmount);
            _treeMaterial.SetFloat(ShaderID.FadeFarAmount, FadeFarAmount);

            if (Camera.main != null)
            {
                if (!OnlyCullOnce)
                {
                    _megaBuffer.SetCounterValue(0);
                    _closeMegaBuffer.SetCounterValue(0);
                }
                _frustumPlanes = GenerateFrustumPlane();

                if (!UpdateTree)
                {
                    return;
                }

                for (int i = 0; i < _drawTree.Count; i++)
                {
                    var tree = _drawTree[i];

                    if (tree.GameObject == null)
                    {
                        RemoveTree(tree);
                        continue;
                    }

                    if (!tree.GameObject.activeSelf) { continue; }
                    var longestSide = Mathf.Max(tree.SurfaceMesh.bounds.size.x, tree.SurfaceMesh.bounds.size.z, tree.SurfaceMesh.bounds.size.y);
                    _frustumPlanes[5].w = DrawDistance + longestSide * 0.75f;

                    if (!IsInFrustum(tree.GameObject.transform.position, -longestSide * 0.75f))
                    {
                        continue;
                    }

                    UpdatePlanePos(tree);
                    UpdateShaderValues(tree);

                    //Debug.Log(Time.unscaledDeltaTime);

                    if (!OnlyCullOnce && tree.Initlized)
                    {
                        _frustumPlanes[5].w = DrawDistance;
                        tree.ComputeShader.SetVectorArray(ComputeShaderID.frustumPlanes, _frustumPlanes);

                        // TODO: move entirely to GPU with DispatchIndirect 
                        tree.CullKernal.SetBuffer(ComputeShaderID.indirectBuffer, _inderectBuffer);
                        ComputeBuffer.CopyCount(tree.TreeBuffer, _inderectBuffer, 0);

                        var x = Mathf.CeilToInt(tree.MaxTree / 128.0f);
                        tree.CullKernal.Dispatch(x, 1, 1);
                    }
                }

                // Culling      
                ComputeBuffer.CopyCount(_megaBuffer, _inderectBuffer, 0);
                ComputeBuffer.CopyCount(_closeMegaBuffer, _closeInderectBuffer, 4 * 1);

                if (DebugOn)
                {
                    DebugOn = false;
                    var array = new uint[] { 0, 0, 0, 0 };
                    _inderectBuffer.GetData(array);
                    Debug.Log(array[0]);
                }
            }

            // ********* Update grassmaterial *********
            _treeMaterial.SetFloat(ShaderID.Wind, Wind);
            if (RoiTransform != null)
            {
                _treeMaterial.SetMatrix(ShaderID.worldToObj, RoiTransform.worldToLocalMatrix);
                //_treeMaterial.SetMatrix(ShaderID.worldToScreen, _sceneCamera.Camera.worldToCameraMatrix);
            }
            _treeMaterial.SetVector(ShaderID.viewDir, Camera.main.transform.forward);

            var bounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000) * 1.5f);

            Graphics.DrawProceduralIndirect(_treeMaterial, bounds, MeshTopology.Points, _inderectBuffer, 0, null, null, DrawTreeShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);         
            //Graphics.DrawMeshInstancedIndirect(TestMesh, 0, TestMat, bounds, _closeInderectBuffer, 0, null, DrawTreeShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);
        }
        private void UpdateShaderValues(Trees tree)
        {
            tree.ComputeShader.SetInt(ComputeShaderID.terrainResolution, tree.SplatMap.height);
            tree.ComputeShader.SetMatrix(ComputeShaderID.objToWorld, tree.GameObject.transform.localToWorldMatrix);
            tree.ComputeShader.SetFloat(ComputeShaderID.surfaceGridStep, Density);

            tree.MeshTreeGeneratorKernel.SetTexture(ComputeShaderID.splatMap, tree.SplatMap);
            tree.MeshTreeGeneratorKernel.SetTexture(ComputeShaderID.nodeTexture, tree.NodeTexture);
            tree.MeshTreeGeneratorKernel.SetTexture(ComputeShaderID.placementMap, tree.PlacementMap);

            tree.CullKernal.SetBuffer(ComputeShaderID.cullOutBuffer, _megaBuffer);
            tree.CullKernal.SetBuffer(ComputeShaderID.closeBuffer, _closeMegaBuffer);
        }
        private void UpdatePlanePos(Trees tree)
        {
            tree.ComputeShader.SetVectorArray(ComputeShaderID.frustumPlanes, _frustumPlanes);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void Update()
        {
            Render(_drawTree);
        }

        private void OnDestroy()
        {
            foreach (Trees tree in _drawTree)
            {
                SafeRelease(tree);
            }

            _inderectBuffer.SafeRelease();
            _megaBuffer.SafeRelease();

            _closeMegaBuffer.SafeRelease();
            _closeInderectBuffer.SafeRelease();
        }
        private void SafeRelease(Trees tree)
        {
            tree.TreeBuffer.SafeRelease();

            Destroy(tree.ComputeShader);

            tree.SurfaceVertices.SafeRelease();
            tree.SurfaceIndices.SafeRelease();
            tree.SurfaceUVs.SafeRelease();
        }
    }
}
