using Saab.Core;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GlowPrePass : MonoBehaviour
{
    
    private static RenderTexture PrePass;
    private static RenderTexture Blurred;

    private Material _blurMat;

    [SerializeField] private bool _update;
    [SerializeField] private Camera _parentCam;

    void OnEnable()
    {
        PrePass = new RenderTexture(Screen.width, Screen.height, 24);
        PrePass.antiAliasing = QualitySettings.antiAliasing;
        Blurred = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);

        var camera = GetComponent<Camera>();

        var glowShader = Shader.Find("Hidden/GlowReplace");
        camera.targetTexture = PrePass;
        camera.SetReplacementShader(glowShader, "Glowable");
        Shader.SetGlobalTexture("_GlowPrePassTex", PrePass);
        Shader.SetGlobalTexture("_GlowBlurredTex", Blurred);

        _blurMat = new Material(Shader.Find("Hidden/Blur"));
        _blurMat.SetVector("_BlurSize", new Vector2(Blurred.texelSize.x * 1.5f, Blurred.texelSize.y * 1.5f));

        camera.farClipPlane = BtaApplication.GetConfigValue(@"ForceBelonging/FarClipPlane", 500);
        camera.nearClipPlane = BtaApplication.GetConfigValue(@"ForceBelonging/NearClipPlane", 50);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst);

        Graphics.SetRenderTarget(Blurred);
        GL.Clear(false, true, Color.clear);

        Graphics.Blit(src, Blurred);

        for (int i = 0; i < 4; i++)
        {
            var temp = RenderTexture.GetTemporary(Blurred.width, Blurred.height);
            Graphics.Blit(Blurred, temp, _blurMat, 0);
            Graphics.Blit(temp, Blurred, _blurMat, 1);
            RenderTexture.ReleaseTemporary(temp);
        }
    }
}
