Shader "Unlit/SkyBox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NightTex("Star Texture", 2D) = "white" {}
        _SkyBlendPower("Sky Blend", Range(0.05,1)) = 0.3
        _GroundBlendPower("Ground Blend", Range(0.05,1)) = 0.3
        _CloudBlendPower("Cloud Blend", Range(0.05,1)) = 0.3
        _SkyColor("Sky color", Color) = (0, 0.5, 1, 1)
        _HorizonColor("Horizon color", Color) = (1, 1, 1, 1)
        _GroundColor("Ground color", Color) = (0.5, 0.5, 0.5, 1)
        _CloudColor("Cloud color", Color) = (1, 1, 1, 1)
        _CloudScale("Cloud Scale", Range(0.01,0.5)) = 1
        _CloudDensity("Cloud Density", Range(0,5)) = 1
        _CloudIntesity("Cloud Intesity", Range(0,10)) = 1
        [ShowAsVector2] _Wind("Wind Direction", Vector) = (0,0,0,0)
        _SunSize("Sun Size", Range(0,1)) = 0.03
        _SunIntensity("Sun intesity", Range(1,5)) = 1.5
        _SunAura("Sun Aura", Range(1,25)) = 2
        _SunriseColor("Sunrise color", Color) = (1, 1, 1, 1)
        _Warp("warp", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Background" "PreviewType"="Skybox" "LightMode"="ForwardBase"}
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _NightTex;

            float4 _MainTex_ST;
            float _SkyBlendPower;
            float _GroundBlendPower;
            float _CloudBlendPower;
            float _CloudScale;
            float _CloudDensity;
            float _CloudIntesity;
            float _SunIntensity;
            float _SunSize;
            float _SunAura;

            float2 _Wind;
            float _Warp;

            float4 _SkyColor;
            float4 _HorizonColor;
            float4 _GroundColor;
            float4 _CloudColor;
            float4 _SunriseColor;

            float3 _SunDir;
            float3 _moonDir;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float GetSunMask(float sunViewDot, float sunRadius)
            {
                float stepRadius = 1 - sunRadius * sunRadius;
                return step(stepRadius, sunViewDot);
            }

            float Random(float value, float mutator = 0.546)
            {
                float random = frac(sin(value + mutator) * 143758.5453);
                return random;
            }

            float4 GammaToLinear(float4 col)
            {
#if UNITY_COLORSPACE_GAMMA
                return col;
#endif
                return float4(LinearToGammaSpace(col.xyz).xyz, col.a);
            }

            float2 Unity_RadialShear_float(float2 UV, float2 Center, float Strength)
            {
                float2 delta = UV - Center;
                float delta2 = dot(delta.xy, delta.xy);
                float2 delta_offset = delta2 * Strength;
                return UV + float2(delta.y, -delta.x) * delta_offset;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                float pos = clamp(normalize(i.worldPos).g, -1, 1);

                float maximum = max(pos, 0);
                float minimum = min(0, pos);

                float sky = pow(maximum, _SkyBlendPower);
                float ground = pow(minimum * -1, _GroundBlendPower);
                float horizon = (1 - (sky + ground));

                float scroll = (_Time * -_Wind);

                float2 uv = i.worldPos.xz / i.worldPos.y;
                uv = uv * pow(pos, _Warp);

                fixed4 col = GammaToLinear(tex2D(_MainTex, uv * _CloudScale + scroll));
                fixed4 col1 = GammaToLinear(tex2D(_MainTex, uv * 0.5 * _CloudScale  + scroll));
                fixed4 col2 = GammaToLinear(tex2D(_MainTex, uv * 2 *  _CloudScale + scroll));

                fixed4 text = clamp((col * col1 * 2 * col2 * 4) * _CloudDensity, 0, 1);
                //text = pow(text, _CloudIntesity);

                float sunDot = ((dot(_WorldSpaceLightPos0.xyz, normalize(i.worldPos)) + 1) / 2);
                float sunScattering = lerp((sunDot+1)/2, _SunIntensity, pow(sunDot, 50) * _SunIntensity);

                float nightsky = ((dot(_WorldSpaceLightPos0.xyz, float3(0,1,0)) + 1) / 2);
                sunScattering = (nightsky) + pow(sunDot, _SunAura) * _SunIntensity;

                float4 cloud = GammaToLinear(_CloudColor) * pow(maximum, _CloudBlendPower) * text;
                cloud.a = pow(cloud.a, _CloudIntesity);

                float sunMask = GetSunMask(sunDot, _SunSize);
                float halo = clamp(pow(sunDot, 4/ (_SunSize * _SunSize)) , 0.01, 1);
                float sun = lerp(GetSunMask(sunDot, _SunSize * 0.2), GetSunMask(sunDot, _SunSize), halo);

                // dont show sun behind clouds
                sun *= clamp(pow((1-cloud.a)+0.3, 3), 0, 1);
                // dont show sun on ground
                sun *= (1 - clamp(pow(minimum * -1, 0.03), 0, 1));

                float sunRise = ((1 - abs(pos)) * pow(sunDot, _SunAura));
                //return sunRise ;
                sunRise = 0;

                fixed4 skynight = GammaToLinear(tex2D(_NightTex, uv)) * sky * 3;
                //return clamp(1 - (nightsky + sunDot), 0, 1);

                float4 skycol = lerp(GammaToLinear(_SkyColor), skynight, clamp(1 - (nightsky + sunDot), 0, 1));

                float4 skylerp = lerp(sky * skycol, sun * _SunIntensity, sun);
                float4 horizonlerp = lerp(horizon * GammaToLinear(_HorizonColor * nightsky), horizon * GammaToLinear(_SunriseColor * nightsky), sunRise);

                float4 skybox = skylerp + horizonlerp + ground * GammaToLinear(_GroundColor);

                float4 result = float4(lerp(skybox, (cloud * col2 * 1) * sunScattering, cloud.a).xyz, 1);
                
                result = lerp(result, result * _SunriseColor, sunRise);

            #if UNITY_COLORSPACE_GAMMA
                return result;
            #else
                return float4(GammaToLinearSpace(result.xyz).xyz, result.a);;
            #endif
            }
            ENDCG
        }
    }
}
