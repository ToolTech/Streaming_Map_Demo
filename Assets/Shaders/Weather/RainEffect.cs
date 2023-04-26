using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(RainEffectRenderer), PostProcessEvent.AfterStack, "Weather/Rain")]
public sealed class RainEffect : PostProcessEffectSettings
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

    [Range(0, 5), Tooltip("Rain Radius.")]
    public FloatParameter Radius = new FloatParameter { value = 1 };

    [Tooltip("Wind")]
    public Vector2Parameter Wind = new Vector2Parameter { value = Vector2.zero };
}

public sealed class RainEffectRenderer : PostProcessEffectRenderer<RainEffect>
{
    private int _kernel;
    private int _kernelParticle;
    private RenderTexture _outputTexture;

    private ComputeBuffer _rainBuffer;
    private Matrix4x4 _worldToClip;
    private float _fov;
    private Vector2Int _screenSize;
    private int _resolution = 1024;

    public override void Init()
    {
        _outputTexture = new RenderTexture(_resolution, _resolution, 24);
        _outputTexture.enableRandomWrite = true;
        _outputTexture.Create();
        ParticleSetup();

        _screenSize = new Vector2Int(Screen.width, Screen.height);
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

        if(_rainBuffer == null)
            _rainBuffer = new ComputeBuffer(10000, sizeof(float) * 3, ComputeBufferType.Default);

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

        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = _outputTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;
    }

    public override void Render(PostProcessRenderContext context)
    {
        if (context == null) return;

        var height = _resolution;
        var width = _resolution;

        if (_screenSize.x < Screen.width || _screenSize.y < Screen.height)
        {
            GeneratePoints();
        }

        if (context.camera.fieldOfView != _fov)
        {        
            GeneratePoints();
        }

        _screenSize = new Vector2Int(Screen.width, Screen.height);

        _fov = context.camera.fieldOfView;
        var clipToWorld = GetClipToWorld(context.camera);
        Matrix4x4 world2Screen = context.camera.projectionMatrix * context.camera.worldToCameraMatrix;

        settings.RainShader.value.SetVector("Resolution", new Vector2(width, height));
        settings.RainShader.value.SetInt("ParticlesNum", settings.Density.value);
        settings.RainShader.value.SetFloat("Speed", settings.Speed.value);
        settings.RainShader.value.SetFloat("Fade", settings.Fade.value);
        settings.RainShader.value.SetFloat("Radius", settings.Radius.value);
        settings.RainShader.value.SetVector("Wind", settings.Wind.value);
        settings.RainShader.value.SetTextureFromGlobal(_kernel, "DepthTexture", "_CameraDepthTexture");
        settings.RainShader.value.SetTextureFromGlobal(_kernelParticle, "DepthTexture", "_CameraDepthTexture");
        settings.RainShader.value.SetMatrix("ClipToWorld", clipToWorld);
        settings.RainShader.value.SetMatrix("WorldToScreen", world2Screen);

        int threads = settings.Density.value > 10 ? settings.Density.value / 10 : 1;

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
        if(_outputTexture != null)
            _outputTexture.Release();
        //if(_inputTexture != null)
        //    _inputTexture.Release();
        if (_rainBuffer.IsValid())
            _rainBuffer.Release();
    }
}