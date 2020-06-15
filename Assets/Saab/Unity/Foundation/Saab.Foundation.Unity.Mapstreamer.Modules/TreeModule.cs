using Saab.Unity.Core.ComputeExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saab.Foundation.Unity.MapStreamer.Modules
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

        public Vector3 SurfaceSize;
        public int SurfaceTriangles;

        public int MaxTree;
    }

    public class TreeModule : MonoBehaviour
    {
        private void Start()
        {
            if (PerlinNoise == null)
            {
                PerlinNoise = Resources.Load("Textures/PerlinNoiseRGB") as Texture2D;
            }

            _treeTextures = Create2DArray(TreeTextures, UseETC2);
            _minMaxWidthHeight = GetMinMaxWidthHeight(TreeTextures);
            _offsetArray = GetOffset(TreeTextures);

            _megaBuffer = new ComputeBuffer(1500000, sizeof(float) * 4, ComputeBufferType.Append);
            _closeMegaBuffer = new ComputeBuffer(1000000, sizeof(float) * 4, ComputeBufferType.Append);

            _inderectBuffer = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);
            _inderectBuffer.SetData(new uint[] { 0, 1, 0, 0 });

            _closeInderectBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            var subMeshIndex = 0;
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, TestMesh.subMeshCount - 1);
            _closeInderectBuffer.SetData(new uint[5] { TestMesh.GetIndexCount(subMeshIndex), 0, TestMesh.GetIndexStart(subMeshIndex), TestMesh.GetBaseVertex(subMeshIndex), 0 });

            // Initialize materials
            InitializeMaterial();

            StartCoroutine(Initlize());
            //StartCoroutine(Rendering());
        }

        public void UpdateSceneCamera(ISceneManagerCamera newCam)
        {
            _sceneCamera = newCam;
            _frustumPlanes = GenerateFrustumPlane();
        }

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
                ComputeShader = Instantiate(ComputeShader),
                SurfaceSize = new Vector3(_mesh.bounds.size.x, _mesh.bounds.size.y, _mesh.bounds.size.z)
            };
            //Initialize(tree);

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

            frustumVector4[5].w = DrawDistance;
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
            static public int Yoffset = Shader.PropertyToID("_Yoffset");

            static public int frustumPlanes = Shader.PropertyToID("_FrustumPlanes");

            static public int minMaxWidthHeight = Shader.PropertyToID("_MinMaxWidthHeight");
            static public int quads = Shader.PropertyToID("_Quads");

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
        private float[] _offsetArray;

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
        public int DrawDistance = 4500;
        private Vector4[] _frustumPlanes;

        // TODO: replace with better solution
        //public CameraManager CameraManager;
        private ISceneManagerCamera _sceneCamera;

        public bool UpdateTree = true;
        public bool DrawTreeShadows = true;
        public float Wind = 0.0f;
        public float Density = 22.127f;

        [Header("****** SHADERS ******")]
        public ComputeShader ComputeShader;
        public Shader TreeShader;
        public float FadeFarValue = 2000;
        public float FadeNearValue = 5;
        public float FadeNearAmount = 5;
        public float FadeFarAmount = 500;

        private Material _treeMaterial;

        private ComputeBuffer _megaBuffer;
        private ComputeBuffer _closeMegaBuffer;
        private ComputeBuffer _inderectBuffer;
        private ComputeBuffer _closeInderectBuffer;

        private Texture2DArray _treeTextures;
        private Transform _roiTransform;
        public bool UseETC2 = false;

        public Mesh TestMesh;
        public Material TestMat;
        private bool _hasRendered;

        // Compute kernel names
        private const string _indirectTreeKernelName = "IndirectGrass";
        private const string _meshTreeGeneratorKernelName = "MeshTreeGenerator";
        private const string _cullKernelName = "TreeCull";

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Texture2DArray Create2DArray(TerrainTextures[] texture, bool etc2)
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
        private float[] GetOffset(TerrainTextures[] textures)
        {
            List<float> offset = new List<float>();

            foreach (TerrainTextures item in textures)
            {
                offset.Add(item.Yoffset);
            }

            return offset.ToArray();
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
        private void Initialize(Trees tree)
        {
            //Init shaders
            //tree.ComputeShader = Instantiate(ComputeShader);

            // Init kernels
            tree.MeshTreeGeneratorKernel = new ComputeKernel(_meshTreeGeneratorKernelName, tree.ComputeShader);
            tree.IndirectTreeKernel = new ComputeKernel(_indirectTreeKernelName, tree.ComputeShader);
            tree.CullKernal = new ComputeKernel(_cullKernelName, tree.ComputeShader);

            tree.SurfaceSize = new Vector3(tree.SurfaceMesh.bounds.size.x, tree.SurfaceMesh.bounds.size.y, tree.SurfaceMesh.bounds.size.z);
            tree.SurfaceTriangles = tree.SurfaceMesh.triangles.Length;

            var maxTreeMesh = (int)Mathf.Ceil((tree.SurfaceSize.x * tree.SurfaceSize.z) / Density) / 2;
            tree.MaxTree = maxTreeMesh > 0 ? maxTreeMesh : 1;

            //Debug.LogWarning("About to add to memory: " + (tree.MaxTree / 1000000.0).ToString("F4") + " mb");
            //PrintTotalBufferSize();

            tree.TreeBuffer = new ComputeBuffer(tree.MaxTree, sizeof(float) * 4, ComputeBufferType.Append);

            var surfaceVertices = tree.SurfaceMesh.vertices;
            var surfaceIndices = tree.SurfaceMesh.GetIndices(0);
            var surfaceUVs = tree.SurfaceMesh.uv;

            //Destroy(tree.SurfaceMesh);

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
        }
        // --- GrassComponent ---
        private void InitializeMaterial()
        {
            string _QuadKernelName = "FindQuad";
            _treeMaterial = new Material(TreeShader);

            // ********************** Grass material **********************
            _treeMaterial.SetBuffer(ShaderID.treeBuffer, _megaBuffer);
            TestMat.SetBuffer("_Buffer", _closeMegaBuffer);

            _treeMaterial.SetTexture(ShaderID.treeTexture, _treeTextures);
            _treeMaterial.SetTexture(ShaderID.perlinNoise, PerlinNoise);
            _treeMaterial.SetTexture(ShaderID.colorVariance, PerlinNoise);

            _treeMaterial.SetVectorArray(ShaderID.minMaxWidthHeight, _minMaxWidthHeight);
            _treeMaterial.SetFloatArray(ShaderID.Yoffset, _offsetArray);
            List<Vector4> quads = new List<Vector4>();
            ComputeBuffer smallestQuad = new ComputeBuffer(1, sizeof(float) * 4, ComputeBufferType.Append);
            ComputeKernel findSmallestQuad = new ComputeKernel(_QuadKernelName, ComputeShader);

            foreach (TerrainTextures terrain in TreeTextures)
            {
                var front = GetBillboardTexture(Sides.Front, terrain.FeatureTexture);
                var side = GetBillboardTexture(Sides.Side, terrain.FeatureTexture);
                var top = GetBillboardTexture(Sides.Top, terrain.FeatureTexture);

                var frontXY = CalcSide(front, findSmallestQuad, smallestQuad);
                var sideXY = CalcSide(side, findSmallestQuad, smallestQuad);
                var topXY = CalcSide(top, findSmallestQuad, smallestQuad);

                quads.Add(frontXY / front.width);
                quads.Add(sideXY / side.width);
                quads.Add(topXY / top.width);
            }

            smallestQuad.SafeRelease();
            _treeMaterial.SetVectorArray(ShaderID.quads, quads.ToArray());
            //Debug.LogFormat("Generated Mesh");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        IEnumerator Initlize()
        {
            while (true)
            {
                if (Camera.main != null || !UpdateTree)
                {
                    _frustumPlanes = GenerateFrustumPlane();

                    for (int i = 0; i < _drawTree.Count; i++)
                    {
                        var tree = _drawTree[i];

                        if (tree.GameObject == null)
                        {
                            //RemoveTree(tree);
                            continue;
                        }

                        if (!tree.GameObject.activeSelf) { continue; }
                        var longestSide = Mathf.Max(tree.SurfaceSize.x, tree.SurfaceSize.z, tree.SurfaceSize.y);
                        _frustumPlanes[5].w = DrawDistance + longestSide * 0.75f;

                        if (!IsInFrustum(tree.GameObject.transform.position, -longestSide * 0.75f))
                        {
                            continue;
                        }

                        //UpdatePlanePos(tree);
                        UpdateShaderValues(tree);

                        if (!tree.Initlized)
                        {
                            tree.Initlized = true;
                            Initialize(tree);

                            tree.MeshTreeGeneratorKernel.SetTexture(ComputeShaderID.splatMap, tree.SplatMap);
                            tree.MeshTreeGeneratorKernel.SetTexture(ComputeShaderID.nodeTexture, tree.NodeTexture);
                            tree.MeshTreeGeneratorKernel.SetTexture(ComputeShaderID.placementMap, tree.PlacementMap);

                            tree.CullKernal.SetBuffer(ComputeShaderID.cullOutBuffer, _megaBuffer);
                            tree.CullKernal.SetBuffer(ComputeShaderID.closeBuffer, _closeMegaBuffer);

                            tree.MeshTreeGeneratorKernel.SetBuffer(ComputeShaderID.terrainBuffer, tree.TreeBuffer);                 // tree generator output
                            tree.TreeBuffer.SetCounterValue(0);
                            var GenX = Mathf.CeilToInt(tree.SurfaceTriangles / 8.0f);

                            tree.MeshTreeGeneratorKernel.Dispatch(GenX, 1, 1);
                            

                            tree.CullKernal.SetBuffer(ComputeShaderID.cullInBuffer, tree.TreeBuffer);

                            tree.SurfaceVertices.SafeRelease();
                            tree.SurfaceIndices.SafeRelease();
                            tree.SurfaceUVs.SafeRelease();

                            _roiTransform = FindFirstNodeParent(tree.GameObject.transform);
                            yield return null;
                        }
                    }
                }
                yield return null;
            }
        }

        private void PrintTotalBufferSize()
        {
            int size = 0;
            int extra = 0;
            foreach (Trees tree in _drawTree)
            {
                if(tree.Initlized)
                {
                    size += tree.MaxTree * sizeof(float) * 4;
                    extra++;
                }
                if(tree.SurfaceVertices != null)
                {
                    size += tree.SurfaceMesh.vertices.Length * sizeof(float) * 3;
                    size += tree.SurfaceMesh.GetIndices(0).Length * sizeof(int);
                    size += tree.SurfaceMesh.uv.Length * sizeof(float) * 2;
                }
            }
            Debug.LogFormat("buffer tree memory: " + (size / 1000000.0).ToString("F4") + " mb buffers: " + extra + " average: " + ((size / 1000000.0)/extra).ToString("F4"));
        }

        private void Render(List<Trees> treeList)
        {
            //PrintTotalBufferSize();
            if (Camera.main != null || !UpdateTree)
            {
                FadeFarAmount = DrawDistance / 3;
                FadeFarValue = DrawDistance - FadeFarAmount;

                _treeMaterial.SetFloat(ShaderID.FadeFar, FadeFarValue);
                _treeMaterial.SetFloat(ShaderID.FadeNear, FadeNearValue);
                _treeMaterial.SetFloat(ShaderID.FadeNearAmount, FadeNearAmount);
                _treeMaterial.SetFloat(ShaderID.FadeFarAmount, FadeFarAmount);


                _megaBuffer.SetCounterValue(0);
                _closeMegaBuffer.SetCounterValue(0);
                _frustumPlanes = GenerateFrustumPlane();

                //int current = 0;
                //int active = _drawTree.Where(p => p.Initlized == true).Count();
                //Debug.LogFormat("active tree buffers: " + (_drawTree.Where(p => p.Initlized == true).Count() * 2).ToString() + " + 2");

                for (int i = 0; i < _drawTree.Count; i++)
                {
                    var tree = _drawTree[i];

                    if (tree.GameObject == null)
                    {
                        RemoveTree(tree);
                        continue;
                    }

                    if (!tree.GameObject.activeSelf) { continue; }
                    var longestSide = Mathf.Max(tree.SurfaceSize.x, tree.SurfaceSize.z, tree.SurfaceSize.y);
                    _frustumPlanes[5].w = DrawDistance + longestSide * 1.25f;

                    if (!IsInFrustum(tree.GameObject.transform.position, -longestSide * 1.25f))
                    {
                        continue;
                    }

                    UpdatePlanePos(tree);
                    UpdateShaderValues(tree);

                    //Debug.Log(Time.unscaledDeltaTime);

                    if (tree.Initlized)
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

                // ********* Update grassmaterial *********
                _treeMaterial.SetFloat(ShaderID.Wind, Wind);
                if (_roiTransform != null)
                {
                    _treeMaterial.SetMatrix(ShaderID.worldToObj, _roiTransform.worldToLocalMatrix);
                    //_treeMaterial.SetMatrix(ShaderID.worldToScreen, _sceneCamera.Camera.worldToCameraMatrix);
                }
                _treeMaterial.SetVector(ShaderID.viewDir, Camera.main.transform.forward);

                var bounds = new Bounds(Vector3.zero, new Vector3(DrawDistance + DrawDistance / 3, DrawDistance + DrawDistance / 3, DrawDistance + DrawDistance / 3) * 1.5f);
                Graphics.DrawProceduralIndirect(_treeMaterial, bounds, MeshTopology.Points, _inderectBuffer, 0, null, null, DrawTreeShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);
                //Graphics.DrawMeshInstancedIndirect(TestMesh, 0, TestMat, bounds, _closeInderectBuffer, 0, null, DrawTreeShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);
            }
        }

        private void UpdateShaderValues(Trees tree)
        {
            tree.ComputeShader.SetInt(ComputeShaderID.terrainResolution, tree.SplatMap.height);
            tree.ComputeShader.SetMatrix(ComputeShaderID.objToWorld, tree.GameObject.transform.localToWorldMatrix);
            tree.ComputeShader.SetFloat(ComputeShaderID.surfaceGridStep, Density);
        }
        private void UpdatePlanePos(Trees tree)
        {
            tree.ComputeShader.SetVectorArray(ComputeShaderID.frustumPlanes, _frustumPlanes);
        }

        enum Sides
        {
            Front = 1 << 0,
            Side = 1 << 1,
            Top = 1 << 2,
        };

        private Texture2D GetBillboardTexture(Sides side, Texture2D billboard)
        {
            Texture2D Image = new Texture2D(billboard.width / 2, billboard.height / 2);

            int sx = 0;
            int sy = 0;

            switch (side)
            {
                case Sides.Front:
                    sx = 0;
                    sy = billboard.height / 2;
                    break;
                case Sides.Side:
                    sx = 0;
                    sy = 0;
                    break;
                case Sides.Top:
                    sx = billboard.width / 2;
                    sy = 0;
                    break;
            }

            List<Color32> TestList = new List<Color32>();
            Color32[] pix = billboard.GetPixels32();


            for (int y = 0; y < billboard.height / 2; y++)
            {
                for (int x = 0; x < billboard.width / 2; x++)
                {
                    TestList.Add(pix[(x + sx) + ((y + sy) * billboard.height)]);
                }
            }


            Image.SetPixels32(TestList.ToArray());
            Image.Apply();

            return Image;
        }
        private Vector4 CalcSide(Texture2D Side, ComputeKernel findSmallestQuad, ComputeBuffer smallestQuad)
        {
            smallestQuad.SetCounterValue(0);

            findSmallestQuad.SetBuffer("SmallestQuad", smallestQuad);

            findSmallestQuad.SetTexture("BillboardPlane", Side);
            ComputeShader.SetInt("BillboardPlaneResolution", Side.width);

            smallestQuad.SetCounterValue(0);

            findSmallestQuad.Dispatch(1, 1, 1);

            Vector4[] quad = new Vector4[1];
            smallestQuad.GetData(quad);

            return quad.First();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Camera_OnPostTraverse()
        {
            if (!_hasRendered)
            {
                Render(_drawTree);
            }
            _hasRendered = true;
        }

        public void LateUpdate()
        {
            if (!_hasRendered)
            {
                Render(_drawTree);
            }
            _hasRendered = false;
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
            //Debug.LogFormat("Trees: Removed " + tree.ID + " From List!");
            tree.TreeBuffer.SafeRelease();

            Destroy(tree.ComputeShader);

            tree.SurfaceVertices.SafeRelease();
            tree.SurfaceIndices.SafeRelease();
            tree.SurfaceUVs.SafeRelease();
        }
    }
}
