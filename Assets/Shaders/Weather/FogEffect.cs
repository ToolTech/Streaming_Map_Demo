using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Random = UnityEngine.Random;

[Serializable]
[PostProcess(typeof(FogRenderer), PostProcessEvent.AfterStack, "Weather/Fog")]
public sealed class Fog : PostProcessEffectSettings
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
    public FloatParameter MaxDensity = new FloatParameter { value = 0.95f };
   
    [Tooltip("Fog view distance.")]
    public FloatParameter ViewDistance = new FloatParameter { value = 3000 };
    [Tooltip("Fog Height.")]
    public FloatParameter FogHeight = new FloatParameter { value = 200 };
    [Tooltip("Fog Color")]
    public ColorParameter Color = new ColorParameter { value = UnityEngine.Color.grey };
}

public sealed class FogRenderer : PostProcessEffectRenderer<Fog>
{
    private int kernel;
    private ComputeShader noiseCompute;

    public override void Render(PostProcessRenderContext context)
    {
        //noiseCompute = settings.NoiseCompute;
        var sheet = context.propertySheets.Get(Shader.Find("Weather/Fog"));
        if(sheet == null)
        {
            Debug.LogWarning("could not find shader");

            return;
        }
           
        sheet.properties.SetFloat("_Density", settings.Density);

        sheet.properties.SetFloat("_MinDensity", settings.MinDensity);
        sheet.properties.SetFloat("_MaxDensity", settings.MaxDensity);
        sheet.properties.SetFloat("_ViewDistance", settings.ViewDistance);
        sheet.properties.SetVector("_Forward", Camera.current.transform.forward);
        sheet.properties.SetFloat("_FogHeight", settings.FogHeight);
        
        sheet.properties.SetColor("_Color", settings.Color);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
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