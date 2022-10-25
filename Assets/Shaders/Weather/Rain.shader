Shader "Weather/Rain"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
            #define UNITY_MATRIX_MVP mul(unity_MatrixVP, unity_ObjectToWorld)

            #pragma vertex vert
            #pragma fragment frag

            inline half3 LinearToGammaSpace(half3 linRGB)
            {
                linRGB = max(linRGB, half3(0.h, 0.h, 0.h));
                // An almost-perfect approximation from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
                return max(1.055h * pow(linRGB, 0.416666667h) - 0.055h, 0.h);
            }

            inline half3 GammaToLinearSpace(half3 sRGB)
            {
                // Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
                return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);
            }

            float4 GammaToLinear(float4 col)
            {
#if UNITY_COLORSPACE_GAMMA
                return col;
#endif
                return float4(LinearToGammaSpace(col.xyz).xyz, col.a);
            }

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
            TEXTURE2D_SAMPLER2D(_NoiseTex, sampler_NoiseTex);
            TEXTURE2D_SAMPLER2D(_RainTex, sampler_RainTex);
            TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
            float _ViewDistance;
            float _Density;
            float4 _Color;
            float3 _Forward;
            float2 _Wind;
            uniform float4x4 _ClipToWorld;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = float4(v.vertex.xy, 0.0, 1.0);
                o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
                o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
                o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);

                float4 clip = float4(o.texcoord.xy * 2 - 1, 0.0, 1.0);
                o.worldDirection = mul(_ClipToWorld, clip) - _WorldSpaceCameraPos;

                return o;
            }

            // ********https://www.shadertoy.com/view/stGfDz ********

            float4 frag(v2f i) : SV_Target
            {
                    float time = unity_DeltaTime.x;
                    float farPlane = _ProjectionParams.z;
                    float2 groundScroll = (_Time * normalize(_Wind) * 0.1);

                    float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                    float nonLinearDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord).r;
                    float linearDepth = Linear01Depth(nonLinearDepth);

                    float3 worldspace = i.worldDirection * LinearEyeDepth(nonLinearDepth) + _WorldSpaceCameraPos;
                    float mask = clamp(dot(worldspace, float3(0, -1, 0)), 0, 1);

                    // ************* Ground Effect *************
                    float2 groundUV = (worldspace.xz / worldspace.y) * 0.01;

                    float4 groundNoise01 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, groundUV + groundScroll);
                    float4 groundNoise02 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, groundUV * 3 + groundScroll);

                    float4 groundnoise = clamp(groundNoise01 * groundNoise02 * 2, 0, 1) * mask;

                    float4 ground = lerp(GammaToLinear(color), GammaToLinear(_Color), groundnoise.a * _Color.a * 0);

                    //return mask;
                    //  ************* Rain Effect *************

                    float4 rainTex = SAMPLE_TEXTURE2D(_RainTex, sampler_RainTex, i.texcoord);

                    float4 rain = lerp(ground, GammaToLinear(_Color), rainTex.a * _Color.a);

#if UNITY_COLORSPACE_GAMMA
                    return rain;
#else
                    return float4(GammaToLinearSpace(rain.xyz).xyz, rain.a);;
#endif

                    // ************* mix rain Effects *************

                    //return lerp(ground, rain, rainTex.a * _Color.a);
            }

            ENDHLSL
        }
    }
}