Shader "Hidden/DownsampleDepth"
{
    Properties
    {
        _MainTex ("Dummy", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture;
            float4 _MainTex_TexelSize;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float frag(v2f i) : SV_Target
            {
                //float raw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                //return LinearEyeDepth(raw);

                float2 uvBase = i.uv - 0.5 * _MainTex_TexelSize * 2; //shift to cover 2×2 block

                float d00 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uvBase));
                float d10 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uvBase + float2(_MainTex_TexelSize.x, 0)));
                float d01 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uvBase + float2(0, _MainTex_TexelSize.y)));
                float d11 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uvBase + _MainTex_TexelSize.xy));

                return max(max(d00, d10), max(d01, d11));

            }
            ENDCG
        }
    }
}
