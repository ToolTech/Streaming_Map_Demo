using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//namespace Saab.Foundation.Unity.MapStreamer.Modules

public class SkyModule : MonoBehaviour
{
    public Material Sky;
    private Material _newSkyMat;

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
    public Gradient HorizonColor = new Gradient()
    {
        // The number of keys must be specified in this array initialiser
        colorKeys = new GradientColorKey[5]
        {
            // Add your colour and specify the stop point
            new GradientColorKey(new Color(0.05f, 0.05f, 0.05f), 0.0f),
            new GradientColorKey(new Color(0.15f, 0.21f, 0.35f), 0.4f),
            new GradientColorKey(new Color(0.97f, 0.45f, 0.01f), 0.5f),
            new GradientColorKey(new Color(0.40f, 0.72f, 0.20f), 0.6f),
            new GradientColorKey(new Color(0.50f, 0.75f, 0.99f), 0.7f)
        },
        // This sets the alpha to 1 at both ends of the gradient
        alphaKeys = new GradientAlphaKey[2]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        }
    };
    public Gradient MoonLightColor = new Gradient()
    {
        // The number of keys must be specified in this array initialiser
        colorKeys = new GradientColorKey[2]
        {
            // Add your colour and specify the stop point
            new GradientColorKey(new Color(0.16f, 0.16f, 0.16f), 0.0f),
            new GradientColorKey(new Color(0.20f, 0.20f, 0.25f), 1f)
        },
        // This sets the alpha to 1 at both ends of the gradient
        alphaKeys = new GradientAlphaKey[2]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        }
    };

    private Texture2D _moonCol;
    private Texture2D _sky;

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

    private void OnValidate()
    {
        if(_newSkyMat != null)
            Setup();
    }

    private void Setup()
    {
        _sky = CreateTexture(SkyColor);
        var horizon = CreateTexture(HorizonColor);
        _moonCol = CreateTexture(MoonLightColor);

        // Color Gradients
        _newSkyMat.SetTexture("_SunZenithGrad", _sky);
        _newSkyMat.SetTexture("_ViewZenithGrad", horizon);
        _newSkyMat.SetTexture("_SunViewGrad", horizon);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
    }

    // Start is called before the first frame update
    void Start()
    {
        _newSkyMat = new Material(Sky.shader);
        _newSkyMat.CopyPropertiesFromMaterial(Sky);

        RenderSettings.skybox = _newSkyMat;

        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        float sunZenithDot = -SunLight.transform.forward.y;
        float moonZenithDot = -MoonLight.transform.forward.y;

        SunLight.intensity = -Mathf.Pow(sunZenithDot - 1, 8) + 1;
        SunLight.shadowStrength = SunLight.intensity * ShadowStrength;

        MoonLight.intensity = (-Mathf.Pow(moonZenithDot - 1, 8) + 1) * (1 - SunLight.intensity) * 0.4f;
        MoonLight.shadowStrength = MoonLight.intensity * ShadowStrength;

        MoonLight.color = _moonCol.GetPixel(Mathf.RoundToInt(_moonCol.width * moonZenithDot), 1);
        var skycol = _sky.GetPixel(Mathf.RoundToInt(_sky.width * sunZenithDot), 1);
        var ambient = SunLight.color * SunLight.intensity * 0.5f + MoonLight.color * MoonLight.intensity + skycol;
        RenderSettings.ambientLight = ambient;

        _newSkyMat.SetColor("_AmbientColor", ambient);

        // Sun
        _newSkyMat.SetVector("_SunDir", -SunLight.transform.forward);
        _newSkyMat.SetVector("_CamUp", Camera.main.transform.up);
        _newSkyMat.SetVector("_CamForward", Camera.main.transform.forward);

        // Moon
        _newSkyMat.SetVector("_MoonDir", -MoonLight.transform.forward);
        _newSkyMat.SetMatrix("_MoonSpaceMatrix", new Matrix4x4(-MoonLight.transform.forward, -MoonLight.transform.up, -MoonLight.transform.right, Vector4.zero).transpose);
    }
}

