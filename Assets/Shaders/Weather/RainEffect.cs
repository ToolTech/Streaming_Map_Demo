using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(RainRenderer), PostProcessEvent.AfterStack, "Weather/Rain")]
public sealed class Rain : PostProcessEffectSettings
{
    [Tooltip("ComputeShader for Generating rain texture")]
    public ParameterOverride<ComputeShader> RainShader = new ParameterOverride<ComputeShader> { value = null };

    [Tooltip("Noise Texture")]
    public TextureParameter Noise = new TextureParameter { value = null };

    [Range(0, 10000), Tooltip("Rain density.")]
    public IntParameter Density = new IntParameter { value = 2000 };

    [Tooltip("Rain Color")]
    public ColorParameter Color = new ColorParameter { value = new Color(0.8f, 0.8f, 0.8f, 0.5f) };

    [Range(5, 50), Tooltip("Rain speed.")]
    public FloatParameter Speed = new FloatParameter { value = 8 };

    [Range(0, 10), Tooltip("Rain Fade.")]
    public FloatParameter Fade = new FloatParameter { value = 5 };

    [Tooltip("Wind")]
    public Vector2Parameter Wind = new Vector2Parameter { value = Vector2.zero };
}

public sealed class RainRenderer : PostProcessEffectRenderer<Rain>
{
    private int _kernel;
    private int _kernelParticle;
    private RenderTexture _outputTexture;
    private RenderTexture _inputTexture;

    private ComputeBuffer _rainBuffer;
    private Matrix4x4 _worldToClip;

    private float _fov;
    private Vector2Int _screenSize; 

    public override void Init()
    {
        Setup();
    }

    private void Setup()
    {
        _screenSize = new Vector2Int(Screen.width, Screen.height);

        _outputTexture = new RenderTexture(Screen.width, Screen.height, 24);
        _outputTexture.enableRandomWrite = true;
        _outputTexture.Create();

        _inputTexture = new RenderTexture(Screen.width, Screen.height, 24);
        _inputTexture.enableRandomWrite = true;
        _inputTexture.Create();

        ParticleSetup();
    }

    public Matrix4x4 GetClipToWorld(Camera camera)
    {
        var p = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);// Unity flips its 'Y' vector depending on if its in VR, Editor view or game view etc... (facepalm)
        p[2, 3] = p[3, 2] = 0.0f;
        p[3, 3] = 1.0f;
        _worldToClip = (p * camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
        return Matrix4x4.Inverse(p * camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
    }

    public void ParticleSetup()
    {
        _kernel = settings.RainShader.value.FindKernel("CSRain");
        _kernelParticle = settings.RainShader.value.FindKernel("CSRainParticles");

        settings.RainShader.value.SetTexture(_kernel, "Result", _outputTexture);
        settings.RainShader.value.SetTexture(_kernel, "Input", _inputTexture);

        _rainBuffer = new ComputeBuffer(10000, sizeof(float) * 3, ComputeBufferType.Raw);
        settings.RainShader.value.SetTexture(_kernelParticle, "Result", _outputTexture);
        settings.RainShader.value.SetBuffer(_kernelParticle, "RainBuffer", _rainBuffer);

        GeneratePoints();
    }

    private void GeneratePoints()
    {
        var setup = settings.RainShader.value.FindKernel("CSCreateParticles");
        settings.RainShader.value.SetBuffer(setup, "RainBuffer", _rainBuffer);

        int threads = settings.Density.value > 10 ? settings.Density.value / 10 : 1;

        if (settings.Density > 0)
            settings.RainShader.value.Dispatch(setup, threads, 1, 1);
    }

    public override void Render(PostProcessRenderContext context)
    {
        if (_screenSize.x != Screen.width || _screenSize.y != Screen.height)
            Setup();
        if(context.camera.fieldOfView != _fov)
        {
            GeneratePoints();
        }

        _fov = context.camera.fieldOfView;
        var clipToWorld = GetClipToWorld(context.camera);
        Matrix4x4 world2Screen = context.camera.projectionMatrix * context.camera.worldToCameraMatrix;

        settings.RainShader.value.SetVector("Resolution", new Vector2(Screen.width, Screen.height));
        settings.RainShader.value.SetInt("ParticlesNum", settings.Density.value);
        settings.RainShader.value.SetFloat("Speed", settings.Speed.value);
        settings.RainShader.value.SetFloat("Fade", settings.Fade.value);
        settings.RainShader.value.SetVector("Wind", settings.Wind.value);
        settings.RainShader.value.SetTextureFromGlobal(_kernel, "DepthTexture", "_CameraDepthTexture");
        settings.RainShader.value.SetTextureFromGlobal(_kernelParticle, "DepthTexture", "_CameraDepthTexture");
        settings.RainShader.value.SetMatrix("ClipToWorld", clipToWorld);
        settings.RainShader.value.SetMatrix("WorldToScreen", world2Screen);

        int threads = settings.Density.value > 10 ? settings.Density.value / 10 : 1;

        //RenderTexture rt = RenderTexture.active;
        //RenderTexture.active = _outputTexture;
        //GL.Clear(true, true, Color.clear);
        //RenderTexture.active = rt;

        _rainBuffer.SetCounterValue(0);
        settings.RainShader.value.SetBuffer(_kernelParticle, "RainBuffer", _rainBuffer);
        settings.RainShader.value.Dispatch(_kernelParticle, threads, 1, 1);

        settings.RainShader.value.Dispatch(_kernel, _outputTexture.width / 8, _outputTexture.height / 8, 1);

        var sheet = context.propertySheets.Get(Shader.Find("Weather/Rain"));
        if (sheet == null)
        {
            Debug.LogWarning("could not find shader");
            return;
        }

        sheet.properties.SetMatrix("_ClipToWorld", clipToWorld);

        sheet.properties.SetFloat("_Density", settings.Density);
        sheet.properties.SetVector("_Forward", context.camera.transform.forward);
        sheet.properties.SetColor("_Color", settings.Color);
        sheet.properties.SetVector("_Wind", settings.Wind);

        var imageTexture = settings.Noise.value == null ? RuntimeUtilities.whiteTexture : settings.Noise.value;
        sheet.properties.SetTexture("_NoiseTex", imageTexture);
        sheet.properties.SetTexture("_RainTex", _outputTexture);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }

    public override void Release()
    {
        _rainBuffer.SetCounterValue(0);

        _outputTexture.Release();
        _inputTexture.Release();
        _rainBuffer.Release();
    }

    //void CreateWorlyTexture3D()
    //{
    //    kernel = noiseCompute.FindKernel("CSWorley");
    //    var kernelNormal = noiseCompute.FindKernel("CSNormalize");
    //    var numcells2 = Mathf.FloorToInt(settings.NumberOfCells * 0.5f);
    //    var numcells3 = Mathf.FloorToInt(settings.NumberOfCells * 0.25f);

    //    CreateNoisePoints(settings.NumberOfCells, "points");

    //    noiseCompute.SetFloat("density", settings.Density);
    //    noiseCompute.SetInt("numCells", settings.NumberOfCells);
    //    noiseCompute.SetInt("resolution", settings.Resolution);

    //    // Dispatch noise gen kernel
    //    int numThreadGroups = Mathf.CeilToInt(settings.Resolution / (float)8);

    //    var minMaxBuffer = CreateComputeBuffer(new int[] { int.MaxValue, 0 }, sizeof(int), "minMax", kernel);
    //    noiseCompute.SetTexture(kernel, "Result", settings.Texture3D);
    //    noiseCompute.Dispatch(kernel, numThreadGroups, numThreadGroups, numThreadGroups);

    //    noiseCompute.SetBuffer(kernelNormal, "minMax", minMaxBuffer);
    //    noiseCompute.SetTexture(kernelNormal, "Result", settings.Texture3D);
    //    noiseCompute.Dispatch(kernelNormal, numThreadGroups, numThreadGroups, numThreadGroups);
    //}
    //void CreateNoisePoints(int count, string bufferName)
    //{
    //    var points = new Vector3[count * count * count];
    //    float cellSize = 1f / count;

    //    for (int x = 0; x < count; x++)
    //    {
    //        for (int y = 0; y < count; y++)
    //        {
    //            for (int z = 0; z < count; z++)
    //            {
    //                var offset = new Vector3(Random.value, Random.value, Random.value);
    //                var pos = (new Vector3(x, y, z) + offset) * cellSize;
    //                int index = x + count * (y + z * count);

    //                points[index] = pos;
    //            }
    //        }
    //    }

    //    var kernel = noiseCompute.FindKernel("CSWorley");
    //    CreateComputeBuffer(points, sizeof(float) * 3, bufferName, kernel);
    //}
    //ComputeBuffer CreateComputeBuffer(System.Array data, int byteSize, string bufferName, int kernel = 0, ComputeBuffer buffer = null)
    //{
    //    if (buffer == null)
    //        buffer = new ComputeBuffer(data.Length, byteSize, ComputeBufferType.Raw);

    //    //buffersToRelease.Add(buffer);
    //    buffer.SetData(data);
    //    noiseCompute.SetBuffer(kernel, bufferName, buffer);

    //    return buffer;
    //}
}