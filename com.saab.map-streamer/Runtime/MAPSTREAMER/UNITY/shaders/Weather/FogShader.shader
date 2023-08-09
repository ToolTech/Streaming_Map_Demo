Shader "Weather/NewFog"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            //#include "../../../Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
            #include "Library/PackageCache/com.unity.postprocessing@3.2.2/PostProcessing/Shaders/StdLib.hlsl"
          
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord  : TEXCOORD0;
                float2 texcoordStereo  : TEXCOORD1;
                float3 worldDirection  : TEXCOORD2;
            };

            TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
            TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
            float _ViewDistance;
            float _Density;
            float _MinDensity;
            float _MaxDensity;
            float _FogHeight;
            float4 _Color;
            float3 _Forward;
            uniform float4x4 clipToWorld;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = float4(v.vertex.xy, 0.0, 1.0);
                o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
                o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
                o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);

                float4 clip = float4(o.texcoord.xy * 2 - 1, 0.0, 1.0);
                o.worldDirection = mul(clipToWorld, clip).xyz - _WorldSpaceCameraPos;

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                    float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                    float nonLinearDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord).r;
                    float linearDepth = Linear01Depth(nonLinearDepth);

                    float farPlane = _ProjectionParams.z;

                    float3 worldspace = i.worldDirection * LinearEyeDepth(nonLinearDepth) + _WorldSpaceCameraPos;
                    float mask = 1 - abs(dot(normalize(worldspace), float3(0, 1, 0)));

                    //return mask;
                    //return float4(coord.xyz * linearDepth * _ViewDistance, 0);

                    float3 plane = float3(0, -1, 0);
                    float denominator = dot(normalize(worldspace), plane);
                    //return denominator;
                    float distPlane = dot(float3(0, _FogHeight, 0) - _WorldSpaceCameraPos, plane) / denominator;
                    distPlane = clamp(distPlane, 0, farPlane);

                    distPlane = denominator <= 0 ? distPlane : farPlane;
                    float dist = linearDepth * farPlane;

                    dist = min(dist, distPlane);

                    float depth = 1 - clamp(_ViewDistance / dist, 0, 1);

                    depth = clamp(depth + _MinDensity, 0, _MaxDensity) * (_Density * mask);
                    return _Color * depth + (color * (1 - depth));
            }

            ENDHLSL
        }
    }
}
