Shader "Weather/NewFog"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;          
                float2 uvStereo : TEXCOORD1;
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
            float4x4 UnityWorldSpaceViewDir;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                o.uv = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
                o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif

                o.uvStereo = TransformStereoScreenSpaceTex(o.uv, 1.0);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float nonLinearDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv).r;
                float linearDepth = Linear01Depth(nonLinearDepth);

                float farPlane = _ProjectionParams.z;

                float3 dir = float3(i.uv.x - 0.5, i.uv.y - 0.5, 0);
                float3 coord = normalize(dir + _Forward);

                float mask = 1 - abs(dot(coord, float3(0, 1, 0)));

                /*float3 camPos = float3(0, 0, 0);
                float denominator = abs(dot(coord, float3(0, 1, 0)));
                float distance = dot(float3(0, _FogHeight, 0) - camPos, float3(0, 1, 0)) / denominator;
                float linearDistance = distance / farPlane;

                linearDepth = min(linearDepth, linearDistance);*/

                float angle = pow(dot(_Forward, float3(0, 1, 0)), 2);  //abs(dot(float3(0, -1, 0), _Forward));
                angle = 0; // clamp(angle, 0, 0.5);

                float dist = linearDepth * farPlane;
                float depth = 1 - clamp(_ViewDistance / dist, 0, 1);

                //return 1 - angle;
                //return i.vertex.xyz; // abs(dot(i.vertex.xyz, float3(0, 1, 0));
                //return abs(dot(float2(0,1), i.vertex.xy));//dist < 3000 ? depth : 0;

                depth = mask * clamp(depth + _MinDensity, 0, _MaxDensity) * (_Density * (1 - angle));
                //return angle;
                return _Color * depth + (color * (1 - depth));
            }

            ENDHLSL
        }
    }
}
