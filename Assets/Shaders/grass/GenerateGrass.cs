using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Saab.Unity.Core;
using Saab.Unity.Core.ComputeExtension;
using UnityEngine.UI;

public class GenerateGrass : MonoBehaviour
{
    //debug
    public RawImage splatmap;

    public GameObject grassObject;
    public Texture2D grassPrototypes; // grass splat map
    public Texture2D[] grassTexture;
    public Texture2D[] treeTexture;
    public Texture2D perlinNoise;

    public bool drawGrassShadows = true;
    public float GrassDensity = 0.28f;
    public float TreeDensity = 0.28f;
    public bool Deform;

    private struct ComputeShaderID
    {
        // Buffers
        static public int frustumTiles = Shader.PropertyToID("frustumTiles");
        static public int gpuToCpuBuffer = Shader.PropertyToID("gpuToCpuBuffer");
        static public int grassBuffer = Shader.PropertyToID("grassPoints");
        static public int treeBuffer = Shader.PropertyToID("treePoints");
        static public int indirectBuffer = Shader.PropertyToID("indirectBuffer");
        static public int indirectBufferTree = Shader.PropertyToID("indirectBufferTree");
        static public int surfaceVertices = Shader.PropertyToID("surfaceVertices");
        static public int surfaceIndices = Shader.PropertyToID("surfaceIndices");
        static public int surfaceUVs = Shader.PropertyToID("surfaceUVs");

        // Textures
        static public int heightmap = Shader.PropertyToID("heightmap");
        static public int uniformNoise = Shader.PropertyToID("uniformNoise");
        static public int grassTexture = Shader.PropertyToID("grassTexture");
        static public int splatMap = Shader.PropertyToID("splatMap");
        static public int heightmapRW = Shader.PropertyToID("heightmapRW");
        static public int sparseHeightmap = Shader.PropertyToID("sparseHeightmap");
        static public int tilesAABBMinHeight = Shader.PropertyToID("tilesAABBMinHeight");
        static public int tilesAABBMaxHeight = Shader.PropertyToID("tilesAABBMaxHeight");
        static public int tilesFrustumCullingGrass = Shader.PropertyToID("tilesFrustumCullingGrass");
        static public int heightmapDecal = Shader.PropertyToID("heightmapDecal");

        // Scalars & vectors
        static public int frustumPlanes = Shader.PropertyToID("frustumPlanes");
        static public int grassDistanceFade = Shader.PropertyToID("grassDistanceFade");
        static public int rectMinMax = Shader.PropertyToID("rectMinMax");
        static public int deformationIntensity = Shader.PropertyToID("deformationIntensity");
        static public int terrainSize = Shader.PropertyToID("terrainSize");
        static public int terrainOffset = Shader.PropertyToID("terrainOffset");
        static public int tileSize = Shader.PropertyToID("tileSize");
        static public int heightmapResolution = Shader.PropertyToID("terrainHeightMapResolution");
        static public int grassResolution = Shader.PropertyToID("grassResolution");
        static public int grassTypes = Shader.PropertyToID("grassTypes");
        static public int grassTexelSize = Shader.PropertyToID("grassTexelSize");
        static public int sparseHeightmapResolution = Shader.PropertyToID("sparseHeightmapResolution");
        static public int heightmapTileResolution = Shader.PropertyToID("heightmapTileResolution");
        static public int terrainSizeInTiles = Shader.PropertyToID("terrainSizeInTiles");
        static public int tileMinMax = Shader.PropertyToID("tileMinMax");
        static public int colliderTileResolution = Shader.PropertyToID("colliderTileResolution");
        static public int surfaceIndicesSize = Shader.PropertyToID("surfaceIndicesSize");
        static public int surfaceGridStep = Shader.PropertyToID("surfaceGridStep");
        static public int surfaceGridStepTree = Shader.PropertyToID("surfaceGridStepTree");

        // Matrices
        static public int heightmapDecalMatrix = Shader.PropertyToID("heightmapDecalMatrix");
        static public int frustumCullingMatrix = Shader.PropertyToID("frustumCullingMatrix");
    }

    private struct ShaderID
    {
        // Bufers
        static public int frustumCullingBuffer = Shader.PropertyToID("_PositionBuffer");
        static public int grassBuffer = Shader.PropertyToID("_GrassBuffer");
        static public int treeBuffer = Shader.PropertyToID("_GrassBuffer");
        static public int indirectGrassBuffer = Shader.PropertyToID("_IndirectGrassBuffer");

        // Textures
        static public int heightMap = Shader.PropertyToID("_HeightMap");
        static public int normalMap = Shader.PropertyToID("_NormalMap");
        static public int grassTexture = Shader.PropertyToID("_MainTexGrass");
        static public int deformedHeightMap = Shader.PropertyToID("_SparseHeightMap");
        static public int perlinNoise = Shader.PropertyToID("_PerlinNoise");

        static public int[] controlMaps = new int[] {
                                                            Shader.PropertyToID( "_Control0" ),
                                                            Shader.PropertyToID( "_Control1" ),
                                                        };

        static public int[] splatMaps = new int[] {
                                                            Shader.PropertyToID( "_Splat0" ),
                                                            Shader.PropertyToID( "_Splat1" ),
                                                            Shader.PropertyToID( "_Splat2" ),
                                                            Shader.PropertyToID( "_Splat3" ),
                                                            Shader.PropertyToID( "_Splat4" ),
                                                            Shader.PropertyToID( "_Splat5" ),
                                                            Shader.PropertyToID( "_Splat6" ),
                                                            Shader.PropertyToID( "_Splat7" ),
                                                        };

        static public int[] normalSplatMaps = new int[] {
                                                            Shader.PropertyToID( "_Normal0" ),
                                                            Shader.PropertyToID( "_Normal1" ),
                                                            Shader.PropertyToID( "_Normal2" ),
                                                            Shader.PropertyToID( "_Normal3" ),
                                                            Shader.PropertyToID( "_Normal4" ),
                                                            Shader.PropertyToID( "_Normal5" ),
                                                            Shader.PropertyToID( "_Normal6" ),
                                                            Shader.PropertyToID( "_Normal7" ),
                                                        };

        // Scalars & vectors
        static public int tesselationEdgeSize = Shader.PropertyToID("_EdgeSize");
        static public int terrainSize = Shader.PropertyToID("_TerrainSize");
        static public int windDirection = Shader.PropertyToID("_WindDirection");
        static public int windForce = Shader.PropertyToID("_WindForce");
        static public int terrainOffset = Shader.PropertyToID("_TerrainOffset");
        static public int heightMapResolution = Shader.PropertyToID("_HeightMapResolution");
        static public int sparseHeightMapResolution = Shader.PropertyToID("_SparseHeightMapResolution");
        static public int minMaxWidthHeight = Shader.PropertyToID("_MinMaxWidthHeight");
        static public int noiseSpread = Shader.PropertyToID("_NoiseSpread");
        static public int grassHealthyColor = Shader.PropertyToID("_GrassHealthyColor");
        static public int grassDryColor = Shader.PropertyToID("_GrassDryColor");
        static public int grassWavingTint = Shader.PropertyToID("_WavingTint");
        static public int grassWaveAndDistance = Shader.PropertyToID("_WaveAndDistance");
        static public int viewDir = Shader.PropertyToID("_ViewDir");

        static public int[] metalic = new int[] {
                                                            Shader.PropertyToID( "_Metallic0" ),
                                                            Shader.PropertyToID( "_Metallic1" ),
                                                            Shader.PropertyToID( "_Metallic2" ),
                                                            Shader.PropertyToID( "_Metallic3" ),
                                                            Shader.PropertyToID( "_Metallic4" ),
                                                            Shader.PropertyToID( "_Metallic5" ),
                                                            Shader.PropertyToID( "_Metallic6" ),
                                                            Shader.PropertyToID( "_Metallic7" ),
                                                        };
        static public int[] smoothness = new int[] {
                                                            Shader.PropertyToID( "_Smoothness0" ),
                                                            Shader.PropertyToID( "_Smoothness1" ),
                                                            Shader.PropertyToID( "_Smoothness2" ),
                                                            Shader.PropertyToID( "_Smoothness3" ),
                                                            Shader.PropertyToID( "_Smoothness4" ),
                                                            Shader.PropertyToID( "_Smoothness5" ),
                                                            Shader.PropertyToID( "_Smoothness6" ),
                                                            Shader.PropertyToID( "_Smoothness7" ),
                                                        };
    }

    //Compute shader
    //private static ComputeShader _computeShader;
    public ComputeShader _computeShader;
    private const string _computeShaderName = "Shaders/DynamicTerrainCompute";

    //Compute kernels
    private ComputeKernel _grassGeneratorKernel;
    private ComputeKernel _meshGrassGeneratorKernel;
    private ComputeKernel _meshTreeGeneratorKernel;
    private ComputeKernel _indirectGrassKernel;

    // Compute kernel names
    private const string _indirectGrassKernelName = "IndirectGrass";
    private const string _grassGeneratorKernelName = "GrassGenerator";
    private const string _meshGrassGeneratorKernelName = "MeshGrassGenerator";
    private const string _meshTreeGeneratorKernelName = "MeshTreeGenerator";

    //Compute buffers
    private ComputeBuffer _argsBufferGrass;
    private ComputeBuffer _argsBufferTree;
    private ComputeBuffer _grassBuffer;
    private ComputeBuffer _treeBuffer;
    private ComputeBuffer _surfaceVertices;
    private ComputeBuffer _surfaceIndices;
    private ComputeBuffer _surfaceUVs;

    // Indirect arguments
    private uint[] _indirectArguments = new uint[6] { 0, 0, 0, 0, 0, 0 };

    // Shader
    public Shader _grassShader;
    private const string _grassShaderName = "Terrain/DynamicTerrain/Grass";
    public Shader _treeShader;

    // Materials
    private Material _grassMaterial;
    private Material _treeMaterial;

    private Mesh _pointMeshGrass;
    private Mesh _pointMeshTree;
    private Vector3 _planeCenter;
    private Vector3 _planeSize;
    private Vector3 _planeOffset;
    //private int _terrainTilesX;
    //private int _terrainTilesY;
    private int _grassTypes = 1;
    private int _treeTypes = 1;
    private Color[] _grassHealthyColors;
    private Color[] _grassDryColors;
    private Vector4[] _minMaxWidthHeight;
    private Vector4[] _minMaxWidthHeight2;
    private float[] _noiseSpread;
    private Color _wavingGrassTint;
    private bool updateGrass = true;
    private Mesh _surfaceMesh;
    private int _grassCount;

    private void Awake()
    {
        var plane = grassObject.GetComponent<MeshFilter>().mesh;
        _surfaceMesh = Instantiate(plane);
        DeformPlane(ref _surfaceMesh, grassObject.transform.localScale, Deform);
        DeformPlane(ref plane, Vector3.one, Deform);
        UpdatePlanePos();
    }

    private void UpdatePlanePos()
    {
        _planeCenter = _surfaceMesh.bounds.center + grassObject.transform.localPosition;
        _planeSize = Vector3.Scale(_surfaceMesh.bounds.size, grassObject.transform.localScale);
        _planeOffset = grassObject.transform.position;

        //_planeOffset = Vector3.Scale(_surfaceMesh.bounds.min, grassObject.transform.localScale) + grassObject.transform.localPosition;
        //_terrainTilesX = Mathf.Max(1, Mathf.FloorToInt(_planeSize.x / terrainTileSize));
        //_terrainTilesY = Mathf.Max(1, Mathf.FloorToInt(_planeSize.z / terrainTileSize));
    }

    private static void DeformPlane(ref Mesh mesh, Vector3 scale, bool deform)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        for (var i = 0; i < vertices.Length; i++)
        {
            if (deform)
            {
                vertices[i] += normals[i] * Mathf.Sin(i) * 1f;
            }
            vertices[i] = Vector3.Scale(vertices[i], scale);
        }

        mesh.vertices = vertices;
    }

    private void Start()
    {
        Initialize();
    }

    private void LateUpdate()
    {
        Render();
    }

    private void OnDestroy()
    {
        SafeRelease();
    }

    private void Initialize()
    {
        _grassTypes = grassTexture.Length;
        _treeTypes = treeTexture.Length;

        if (perlinNoise == null)
        {
            perlinNoise = Resources.Load("Textures/PerlinNoiseRGB") as Texture2D;
        }

        //Init shaders
        //_computeShader = _computeShader ?? Resources.Load(_computeShaderName) as ComputeShader;

        _computeShader = Instantiate(_computeShader);
        _grassShader = _grassShader ?? Shader.Find(_grassShaderName);

        // Init kernels
        //_meshGrassGeneratorKernel = new ComputeKernel(_grassGeneratorKernelName, _computeShader);
        _meshGrassGeneratorKernel = new ComputeKernel(_meshGrassGeneratorKernelName, _computeShader);
        _meshTreeGeneratorKernel = new ComputeKernel(_meshTreeGeneratorKernelName, _computeShader);
        //_initializeKernel = new ComputeKernel(_initializeKernelName, _computeShader);
        _indirectGrassKernel = new ComputeKernel(_indirectGrassKernelName, _computeShader);

        // Initialize buffers
        _argsBufferGrass = new ComputeBuffer(_indirectArguments.Length, sizeof(uint), ComputeBufferType.IndirectArguments);
        _argsBufferTree = new ComputeBuffer(_indirectArguments.Length, sizeof(uint), ComputeBufferType.IndirectArguments);
        _grassBuffer = new ComputeBuffer(2000000, sizeof(float) * 4, ComputeBufferType.Append); // 2M grass billboards
        _treeBuffer = new ComputeBuffer(2000000, sizeof(float) * 4, ComputeBufferType.Append); // 2M grass billboards

        var surfaceVertices = _surfaceMesh.vertices;
        var surfaceIndices = _surfaceMesh.GetIndices(0);
        var surfaceUVs = _surfaceMesh.uv;

        //var tex = grassPrototypes;
        //var color = tex.GetPixelBilinear(surfaceUVs[0].x, surfaceUVs[0].y);

        _surfaceVertices = new ComputeBuffer(surfaceVertices.Length, sizeof(float) * 3, ComputeBufferType.Default);
        _surfaceIndices = new ComputeBuffer(surfaceIndices.Length, sizeof(int), ComputeBufferType.Default);
        _surfaceUVs = new ComputeBuffer(surfaceUVs.Length, sizeof(float) * 2, ComputeBufferType.Default);

        // fill surface vertices
        _surfaceVertices.SetData(surfaceVertices);
        _meshGrassGeneratorKernel.SetBuffer(ComputeShaderID.surfaceVertices, _surfaceVertices);
        _meshTreeGeneratorKernel.SetBuffer(ComputeShaderID.surfaceVertices, _surfaceVertices);
        _surfaceIndices.SetData(surfaceIndices);
        _meshGrassGeneratorKernel.SetBuffer(ComputeShaderID.surfaceIndices, _surfaceIndices);
        _meshTreeGeneratorKernel.SetBuffer(ComputeShaderID.surfaceIndices, _surfaceIndices);
        _surfaceUVs.SetData(surfaceUVs);
        _meshGrassGeneratorKernel.SetBuffer(ComputeShaderID.surfaceUVs, _surfaceUVs);
        _meshTreeGeneratorKernel.SetBuffer(ComputeShaderID.surfaceUVs, _surfaceUVs);

        _computeShader.SetInt(ComputeShaderID.surfaceIndicesSize, surfaceIndices.Length);

        // Create buffer for frustum culling tiles
        //_frustumTilesBuffer = new ComputeBuffer(_terrainTilesX * _terrainTilesY, sizeof(float) * 4, ComputeBufferType.Append);

        // Grass shader
        _grassMaterial = new Material(_grassShader);
        _treeMaterial = new Material(_treeShader);

        // Intialize Grass
        InitializeGrass();

        // Set static resources for compute shader
        //UpdateShaderValues();

        // Initialize AABB and height map
        //_initializeKernel.Dispatch(Mathf.CeilToInt(Mathf.Max(terrainTilesX, heightmapResolution) / 8.0f), Mathf.CeilToInt(Mathf.Max(terrainTilesY, heightmapResolution) / 8.0f), 1);
        //_initializeKernel.Dispatch(Mathf.CeilToInt(_terrainTilesX / 8.0f), Mathf.CeilToInt(_terrainTilesY / 8.0f), 1);

        // Initialize materials
        InitializeMaterials();
    }

    private void InitializeGrass()
    {
        var grassTypes = grassTexture.Length;
        _grassHealthyColors = new Color[grassTypes];
        _grassDryColors = new Color[grassTypes];
        _minMaxWidthHeight = new Vector4[grassTypes];
        _minMaxWidthHeight2 = new Vector4[grassTypes];
        _noiseSpread = new float[grassTypes];

        // TODO Dont use hardcoded values
        SetupGrassWidthHeight();
        SetupGrassHealtyColor();
        SetupGrassDryColor();
        SetupGrassNoiceSpread();

        _wavingGrassTint = new Color(0.063f, 0.651f, 0.000f, 0.000f);
        //_wavingGrassTint = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        GenerateMesh();
    }

    private void SetupGrassWidthHeight()
    {
        if (_minMaxWidthHeight.Length > 0)
        {
            _minMaxWidthHeight[0] = new Vector4(0.8f, 1.0f, 0.3f, 0.9f);
        }
        if (_minMaxWidthHeight.Length > 1)
        {
            _minMaxWidthHeight[1] = new Vector4(0.7f, 1.0f, 0.3f, 0.4f);
        }
        if (_minMaxWidthHeight.Length > 2)
        {
            _minMaxWidthHeight[2] = new Vector4(0.4f, 0.5f, 0.3f, 0.7f);
        }
        if (_minMaxWidthHeight.Length > 3)
        {
            _minMaxWidthHeight[3] = new Vector4(0.8f, 1.5f, 0.8f, 1.5f);
        }
        if (_minMaxWidthHeight.Length > 4)
        {
            _minMaxWidthHeight[4] = new Vector4(0.7f, 1.0f, 0.3f, 0.4f);
        }
        if (_minMaxWidthHeight.Length > 5)
        {
            _minMaxWidthHeight[5] = new Vector4(0.7f, 1.0f, 0.3f, 2.4f);
        }
        if (_minMaxWidthHeight.Length > 6)
        {
            _minMaxWidthHeight[6] = new Vector4(0.4f, 0.5f, 0.3f, 0.7f);
        }
        if (_minMaxWidthHeight.Length > 7)
        {
            _minMaxWidthHeight[7] = new Vector4(0.4f, 0.5f, 0.3f, 0.7f);
        }

        if (_minMaxWidthHeight2.Length > 0)
        {
            _minMaxWidthHeight2[0] = new Vector4(4.8f, 20.0f, 4.3f, 19.9f);
        }
        if (_minMaxWidthHeight2.Length > 1)
        {
            _minMaxWidthHeight2[1] = new Vector4(4.7f, 20.0f, 4.3f, 19.4f);
        }
        if (_minMaxWidthHeight2.Length > 2)
        {
            _minMaxWidthHeight2[2] = new Vector4(4.4f, 19.5f, 4.3f, 19.7f);
        }
        if (_minMaxWidthHeight2.Length > 3)
        {
            _minMaxWidthHeight2[3] = new Vector4(4.8f, 20.5f, 4.8f, 20.5f);
        }
    }

    private void SetupGrassHealtyColor()
    {
        if (_grassHealthyColors.Length > 0)
        {
            _grassHealthyColors[0] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }
        if (_grassHealthyColors.Length > 1)
        {
            _grassHealthyColors[1] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }
        if (_grassHealthyColors.Length > 2)
        {
            _grassHealthyColors[2] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }
        if (_grassHealthyColors.Length > 3)
        {
            _grassHealthyColors[3] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }
        if (_grassHealthyColors.Length > 4)
        {
            _grassHealthyColors[4] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }
        if (_grassHealthyColors.Length > 5)
        {
            _grassHealthyColors[5] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }
        if (_grassHealthyColors.Length > 6)
        {
            _grassHealthyColors[6] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }
        if (_grassHealthyColors.Length > 7)
        {
            _grassHealthyColors[7] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        }
    }

    private void SetupGrassDryColor()
    {
        if (_grassDryColors.Length > 0)
        {
            _grassDryColors[0] = new Vector4(0.890f, 0.890f, 0.890f, 1.0f);
        }
        if (_grassDryColors.Length > 1)
        {
            _grassDryColors[1] = new Vector4(0.801f, 0.831f, 0.831f, 1.0f);
        }
        if (_grassDryColors.Length > 2)
        {
            _grassDryColors[2] = new Vector4(0.853f, 0.853f, 0.853f, 1.0f);
        }
        if (_grassDryColors.Length > 3)
        {
            _grassDryColors[3] = new Vector4(0.838f, 0.838f, 0.838f, 1.0f);
        }
        if (_grassDryColors.Length > 4)
        {
            _grassDryColors[4] = new Vector4(0.838f, 0.838f, 0.838f, 1.0f);
        }
        if (_grassDryColors.Length > 5)
        {
            _grassDryColors[5] = new Vector4(0.838f, 0.838f, 0.838f, 1.0f);
        }
        if (_grassDryColors.Length > 6)
        {
            _grassDryColors[6] = new Vector4(0.838f, 0.838f, 0.838f, 1.0f);
        }
        if (_grassDryColors.Length > 7)
        {
            _grassDryColors[7] = new Vector4(0.838f, 0.838f, 0.838f, 1.0f);
        }
    }
    private void SetupGrassNoiceSpread()
    {
        for (int i = 0; i < _noiseSpread.Length; i++)
        {
            _noiseSpread[i] = 0.1f;
        }
    }

    private void InitializeMaterials()
    {
        // ********************** Grass material **********************
        _grassMaterial.SetBuffer(ShaderID.grassBuffer, _grassBuffer);
        _grassMaterial.SetBuffer(ShaderID.indirectGrassBuffer, _argsBufferGrass);

        Texture2D[] grassTextures = grassTexture;//new Texture2D[1] { grassTexture };
        _grassMaterial.SetTexture(ShaderID.grassTexture, Create2DArray(grassTextures));
        //_grassMaterial.SetTexture(ShaderID.heightMap, heightmap);
        //_grassMaterial.SetTexture(ShaderID.normalMap, normalmap); // Hardcoded to 0,1,0 in shader for now. Use terrain normals
        _grassMaterial.SetVector(ShaderID.terrainSize, _planeSize); //terrainSize);

        _grassMaterial.SetColorArray(ShaderID.grassHealthyColor, _grassHealthyColors);
        _grassMaterial.SetColorArray(ShaderID.grassDryColor, _grassDryColors);
        _grassMaterial.SetFloatArray(ShaderID.noiseSpread, _noiseSpread);
        _grassMaterial.SetVectorArray(ShaderID.minMaxWidthHeight, _minMaxWidthHeight);

        _grassMaterial.SetColor(ShaderID.grassWavingTint, _wavingGrassTint);
        _grassMaterial.SetVector(ShaderID.terrainOffset, _planeOffset);//transform.position );
        _grassMaterial.SetTexture(ShaderID.perlinNoise, perlinNoise);

        // ********************** Tree material **********************

        _treeMaterial.SetBuffer(ShaderID.grassBuffer, _treeBuffer);
        _treeMaterial.SetBuffer(ShaderID.indirectGrassBuffer, _argsBufferTree);

        Texture2D[] TreeTextures = treeTexture;//new Texture2D[1] { grassTexture };
        _treeMaterial.SetTexture(ShaderID.grassTexture, Create2DArray(treeTexture));
        //_grassMaterial.SetTexture(ShaderID.heightMap, heightmap);
        //_grassMaterial.SetTexture(ShaderID.normalMap, normalmap); // Hardcoded to 0,1,0 in shader for now. Use terrain normals
        _treeMaterial.SetVector(ShaderID.terrainSize, _planeSize); //terrainSize);

        _treeMaterial.SetColorArray(ShaderID.grassHealthyColor, _grassHealthyColors);
        _treeMaterial.SetColorArray(ShaderID.grassDryColor, _grassDryColors);
        _treeMaterial.SetFloatArray(ShaderID.noiseSpread, _noiseSpread);
        _treeMaterial.SetVectorArray(ShaderID.minMaxWidthHeight, _minMaxWidthHeight2);

        _treeMaterial.SetColor(ShaderID.grassWavingTint, _wavingGrassTint);
        _treeMaterial.SetVector(ShaderID.terrainOffset, _planeOffset);//transform.position );
        _treeMaterial.SetTexture(ShaderID.perlinNoise, perlinNoise);
    }

    private static Texture2DArray Create2DArray(Texture2D[] texture)
    {
        var textureCount = texture.Length;

        var textureResolution = Math.Max(texture.Max(item => item.width), texture.Max(item => item.height));

        int[] availableGrassResolutions = new int[] {
            64, 128, 256, 512, 1024, 2048, 4096
        };

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
            Graphics.Blit(texture[i], temporaryGrassRenderTexture);
            Texture2D temporaryGrassTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.ARGB32, true);
            RenderTexture.active = temporaryGrassRenderTexture;
            temporaryGrassTexture.ReadPixels(new Rect(0, 0, temporaryGrassTexture.width, temporaryGrassTexture.height), 0, 0);
            RenderTexture.active = null;
            temporaryGrassTexture.Apply(true);
            temporaryGrassTexture.Compress(true);

            //TexToFile(temporaryGrassTexture, Application.dataPath + "/../grassTextureArraySaved_" + i + ".png");

            Graphics.CopyTexture(temporaryGrassTexture, 0, textureArray, i);
            DestroyImmediate(temporaryGrassTexture);
        }
        textureArray.Apply(false, true);

        DestroyImmediate(temporaryGrassRenderTexture);

        return textureArray;
    }
    private void UpdateShaderValues()
    {
        _computeShader.SetVector(ComputeShaderID.terrainSize, _planeSize);//terrainSize);
        //_computeShader.SetFloat(ComputeShaderID.tileSize, terrainTileSize);
        _computeShader.SetVector(ComputeShaderID.terrainOffset, _planeOffset);//transform.position);
        _computeShader.SetInt(ComputeShaderID.grassResolution, grassPrototypes.height);
        _computeShader.SetInt(ComputeShaderID.grassTypes, _grassTypes);

        var grassTexSize = new Vector2(_planeSize.x, _planeSize.z) / grassPrototypes.width;
        _computeShader.SetVector(ComputeShaderID.grassTexelSize, grassTexSize);

        _computeShader.SetFloat(ComputeShaderID.surfaceGridStep, GrassDensity);
        _computeShader.SetFloat(ComputeShaderID.surfaceGridStepTree, TreeDensity);
        _meshGrassGeneratorKernel.SetTexture(ComputeShaderID.splatMap, grassPrototypes);
        _meshTreeGeneratorKernel.SetTexture(ComputeShaderID.splatMap, grassPrototypes);

        splatmap.texture = (Texture)grassPrototypes;

        _meshGrassGeneratorKernel.SetTexture(ComputeShaderID.uniformNoise, NoiseTexture.UniformRGBA128x128);
        _meshTreeGeneratorKernel.SetTexture(ComputeShaderID.uniformNoise, NoiseTexture.UniformRGBA128x128);
        //_meshGrassGeneratorKernel.SetTexture(ComputeShaderID.heightmap, heightmap);       
        _meshGrassGeneratorKernel.SetBuffer(ComputeShaderID.grassBuffer, _grassBuffer); // Grass generator output
        _meshTreeGeneratorKernel.SetBuffer(ComputeShaderID.treeBuffer, _treeBuffer); // Grass generator output

        _indirectGrassKernel.SetBuffer(ComputeShaderID.indirectBuffer, _argsBufferGrass);
        _indirectGrassKernel.SetBuffer(ComputeShaderID.indirectBufferTree, _argsBufferTree);

        _grassMaterial.SetVector(ShaderID.grassWaveAndDistance, new Vector4(Time.time / 20.0f, 0.4f, 1.9f, 9400));
        _grassMaterial.SetVector(ShaderID.viewDir, Camera.main.transform.forward);
    }

    private void GenerateMesh()
    {
        var indices = new int[64000];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
        }

        var bounds = new Bounds(_surfaceMesh.bounds.center,
                                Vector3.Scale(_surfaceMesh.bounds.size, grassObject.transform.localScale));
        // Initialize point mesh grass
        _pointMeshGrass = new Mesh()
        {
            vertices = new Vector3[indices.Length],
            bounds = bounds
        };
        _pointMeshGrass.SetIndices(indices, MeshTopology.Points, 0);
        // Initialize point mesh trees
        _pointMeshTree = new Mesh()
        {
            vertices = new Vector3[indices.Length],
            bounds = bounds
        };
        _pointMeshTree.SetIndices(indices, MeshTopology.Points, 0);

        // Initialize indirect arguments for indirect instanced rendering of grass
        _indirectArguments[0] = _pointMeshGrass.GetIndexCount(0);
        _indirectArguments[1] = 0;
        _argsBufferGrass.SetData(_indirectArguments);
        _argsBufferTree.SetData(_indirectArguments);
    }

    private void Render()
    {
        if (!Camera.main)
        {
            return;
        }

        // Set resources for compute shader
        UpdateShaderValues();

        //InitializeMaterials();
        UpdatePlanePos();
        _grassMaterial.SetVector(ShaderID.terrainOffset, _planeOffset);

        var b = _pointMeshGrass.bounds;
        b.center = _planeCenter;
        b.size = _planeSize;
        _pointMeshGrass.bounds = b;
        _pointMeshTree.bounds = b;

        if (updateGrass)
        {
            //_computeShader.SetMatrix(ComputeShaderID.frustumCullingMatrix, ExtendedFrustum(1.0f));

            // Reset grass buffer and run generation
            _grassBuffer.SetCounterValue(0);

            _meshGrassGeneratorKernel.Dispatch(Mathf.CeilToInt(_surfaceMesh.triangles.Length / 3), 1, 1);
            ComputeBuffer.CopyCount(_grassBuffer, _argsBufferGrass, 4 * 5);
            _indirectGrassKernel.Dispatch(1, 1, 1);
            Graphics.DrawMeshInstancedIndirect(_pointMeshGrass, 0, _grassMaterial, _pointMeshGrass.bounds, _argsBufferGrass, 0, null, drawGrassShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);

            // Reset grass buffer and run generation
            _treeBuffer.SetCounterValue(0);

            _meshTreeGeneratorKernel.Dispatch(Mathf.CeilToInt(_surfaceMesh.triangles.Length / 3), 1, 1);
            ComputeBuffer.CopyCount(_treeBuffer, _argsBufferTree, 4 * 5);
            _indirectGrassKernel.Dispatch(1, 1, 1);
            Graphics.DrawMeshInstancedIndirect(_pointMeshTree, 0, _treeMaterial, _pointMeshTree.bounds, _argsBufferTree, 0, null, drawGrassShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off);
            //updateGrass = false;
        }
    }

    private void SafeRelease()
    {
        // Release compute buffers
        _argsBufferGrass.SafeRelease();
        _argsBufferTree.SafeRelease();

        _grassBuffer.SafeRelease();
        _treeBuffer.SafeRelease();

        _surfaceVertices.SafeRelease();
        _surfaceIndices.SafeRelease();
        _surfaceUVs.SafeRelease();
    }
}
