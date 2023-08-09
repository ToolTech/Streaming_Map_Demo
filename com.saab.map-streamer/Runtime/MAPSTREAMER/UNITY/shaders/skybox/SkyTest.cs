using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SkyTest : MonoBehaviour
{
    public Material Sky;
    public float Speed = 2f;
    [Range(0, 1)]
    public float ShadowStrength = 0.8f;

    public Light SunLight;
    public Light MoonLight;
    public Gradient SkyColor;
    public Gradient HorizonColor;

    public Gradient MoonLightColor;

    void RotateSun(float speed)
    {
        SunLight.transform.RotateAroundLocal(new Vector3(0.8f, 1f, 0f).normalized, speed * Time.deltaTime);
    }

    private void Update()
    {
        RotateSun(Speed);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var sky = CreateTexture(SkyColor);
        var horizon = CreateTexture(HorizonColor);
        var moonCol = CreateTexture(MoonLightColor);

        // Color Gradients
        Sky.SetTexture("_SunZenithGrad", sky);
        Sky.SetTexture("_ViewZenithGrad", horizon);
        Sky.SetTexture("_SunViewGrad", horizon);

        float sunZenithDot = -SunLight.transform.forward.y;
        float moonZenithDot = -MoonLight.transform.forward.y;

        SunLight.intensity = -Mathf.Pow(sunZenithDot - 1, 8) + 1;
        SunLight.shadowStrength = SunLight.intensity * ShadowStrength;

        MoonLight.intensity = (-Mathf.Pow(moonZenithDot - 1, 8) + 1) * (1 - SunLight.intensity) * 0.4f;
        MoonLight.shadowStrength = MoonLight.intensity * ShadowStrength;

        MoonLight.color = moonCol.GetPixel(Mathf.RoundToInt(moonCol.width * moonZenithDot), 1);
        var skycol = sky.GetPixel(Mathf.RoundToInt(sky.width * sunZenithDot), 1);
        var ambient = SunLight.color * SunLight.intensity * 0.5f + MoonLight.color * MoonLight.intensity + skycol;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambient;

        // Sun
        Sky.SetVector("_SunDir", -SunLight.transform.forward);
        Sky.SetVector("_CamUp", Camera.main.transform.up);
        Sky.SetVector("_CamForward", Camera.main.transform.forward);

        // Moon
        Sky.SetVector("_MoonDir", -MoonLight.transform.forward);
        Sky.SetMatrix("_MoonSpaceMatrix", new Matrix4x4(-MoonLight.transform.forward, -MoonLight.transform.up, -MoonLight.transform.right, Vector4.zero).transpose);
    }

    Texture2D CreateTexture(Gradient gradient, int width = 128, int height = 1)
    {
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            float t = (float)x / (width - 1); // Normalized position along the gradient
            Color color = gradient.Evaluate(t);
            texture.SetPixel(x, 0, color);
            texture.wrapMode = TextureWrapMode.Clamp;
        }

        texture.Apply();

        return texture;
    }
}
