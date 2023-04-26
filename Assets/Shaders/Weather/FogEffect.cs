using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(FogEffectRenderer), PostProcessEvent.AfterStack, "Weather/Fog")]
public sealed class FogEffect : PostProcessEffectSettings
{
    //public ParameterOverride<ComputeShader> NoiseCompute = new ParameterOverride<ComputeShader>();
    //public ParameterOverride<RenderTexture> Texture3D = new ParameterOverride<RenderTexture>();

    //public IntParameter Resolution = new IntParameter { value = 128 };
    //public IntParameter NumberOfCells = new IntParameter { value = 10 };

    [Range(0, 1), Tooltip("Fog density.")]
    public FloatParameter Density = new FloatParameter { value = 1f };

    [Range(0, 1), Tooltip("min Fog density.")]
    public FloatParameter MinDensity = new FloatParameter { value = 0.0f };

    [Range(0, 1), Tooltip("max Fog density.")]
    public FloatParameter MaxDensity = new FloatParameter { value = 1f };
   
    [Tooltip("Fog view distance.")]
    public FloatParameter ViewDistance = new FloatParameter { value = 10000 };

    [Tooltip("Fog Height.")]
    public FloatParameter FogHeight = new FloatParameter { value = 200 };

    [Tooltip("Fog Color")]
    public ColorParameter Color = new ColorParameter { value = UnityEngine.Color.white };
}

[ExecuteAlways]
public sealed class FogEffectRenderer : PostProcessEffectRenderer<FogEffect>
{
    public override void Render(PostProcessRenderContext context)
    {
        if (context == null)
            return;

        var sheet = context.propertySheets.Get(Shader.Find("Weather/Fog"));
        if(sheet == null)
        {
            Debug.LogWarning("could not find shader");
            return;
        }

        var camera = context.camera;
        var p = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);// Unity flips its 'Y' vector depending on if its in VR, Editor view or game view etc... (facepalm)
        p[2, 3] = p[3, 2] = 0.0f;
        p[3, 3] = 1.0f;
        var clipToWorld = Matrix4x4.Inverse(p * camera.worldToCameraMatrix) * Matrix4x4.TRS(new Vector3(0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
        sheet.properties.SetMatrix("clipToWorld", clipToWorld);
        sheet.properties.SetFloat("_Density", settings.Density);

        sheet.properties.SetFloat("_MinDensity", settings.MinDensity);
        sheet.properties.SetFloat("_MaxDensity", settings.MaxDensity);
        sheet.properties.SetFloat("_ViewDistance", settings.ViewDistance);
        sheet.properties.SetVector("_Forward", context.camera.transform.forward);
        sheet.properties.SetFloat("_FogHeight", settings.FogHeight);
        
        sheet.properties.SetColor("_Color", settings.Color);

        var ambientIntensity = (Vector3.Dot(RenderSettings.sun.transform.forward, Vector3.down) + 1) / 2;
        sheet.properties.SetFloat("_Ambient", ambientIntensity);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}