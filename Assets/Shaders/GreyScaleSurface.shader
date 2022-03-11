Shader "CustomSurface/GreyScale"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _EffectAmount("Effect Amount", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" "ForceNoShadowCasting"="True"}
        ZWrite Off
        Cull off
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf NoLighting alpha nolightmap 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        float _EffectAmount;

        struct Input
        {
            float2 uv_MainTex;
        };

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten) 
        {
            return fixed4(s.Albedo * 0.5f, s.Alpha);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            c.rgb = lerp(c.rgb, dot(c.rgb, float3(0.3, 0.59, 0.11)), _EffectAmount);

            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    //FallBack "Diffuse"
}
