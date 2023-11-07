using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyTest : MonoBehaviour
{
    public Material Sky;
    public float Speed = 1f;
    [Range(0, 1)]
    public float ShadowStrength = 0.7f;

    public Light SunLight;
    public Light MoonLight;
    public Gradient SkyColor = new Gradient()
    {
        // The number of keys must be specified in this array initialiser
        colorKeys = new GradientColorKey[3]
        {
            // Add your colour and specify the stop point
            new GradientColorKey(new Color(0.05f, 0.05f, 0.05f), 0.0f),
            new GradientColorKey(new Color(0.16f, 0.26f, 0.4f), 0.5f),
            new GradientColorKey(new Color(0.38f, 0.69f, 0.85f), 0.6f)
        },
        // This sets the alpha to 1 at both ends of the gradient
        alphaKeys = new GradientAlphaKey[2]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        }
    };

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
