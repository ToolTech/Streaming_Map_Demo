Shader "Custom/Dynamic/Skybox"
{
    Properties
    {
        [ShowAsVector3] _MoonDir("moon Direction", Vector) = (0,0,0,0)

        _MainLightColor("Sun color", Color) = (1, 1, 1, 1)
        [NoScaleOffset] _HorizonTex("Horizon texture", 2D ) = "gray" {}
        _AmbientColor("Ambient color", Color) = (1, 1, 1, 1)
        _SunRadius("Sun radius", Range(0,45)) = 0.5
        _FlareSize("Flare Size", Range(0,45)) = 45
        _MoonRadius("Moon radius", Range(0,45)) = 0.5
        _MoonExposure("Moon exposure", Range(-16, 16)) = 1

        _StarExposure("Star exposure", Range(-16, 16)) = 6
        _StarPower("Star power", Range(1,100)) = 50
        _StarsColorValue("Stars Color intensity", Range(0,1)) = 0.1

        _StarLatitude("Star latitude", Range(-90, 90)) = 0
        _StarSpeed("Star speed", Float) = 0.001

        _Warp("tile warping", float) = 1

        [NoScaleOffset] _StarCubeMap("Star cube map", Cube) = "black" {}
        [NoScaleOffset] _MoonCubeMap("Moon cube map", Cube) = "black" {}
        [NoScaleOffset] _SunFlare("Sun Flare", 2D) = "black" {}
        [NoScaleOffset] _SunZenithGrad("Sun-Zenith gradient", 2D) = "white" {}
        [NoScaleOffset] _ViewZenithGrad("View-Zenith gradient", 2D) = "white" {}
        [NoScaleOffset] _SunViewGrad("Sun-View gradient", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Background" "PreviewType" = "Skybox" "LightMode" = "ForwardBase"}
        //Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #define PI 3.1415926538
            
           #include "UnityCG.cginc"

            struct Attributes
            {
                float4 vertex       : POSITION;
            };

            struct v2f
            {
                float4 pos          : SV_POSITION;
                float4 worldPos     : TEXCOORD0;
                float4 positionView : TEXCOORD1;
                float4 positionClip : TEXCOORD2;
            };

            v2f Vertex(Attributes v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); 
                o.positionView = mul(UNITY_MATRIX_V, o.worldPos);
                o.positionClip = mul(UNITY_MATRIX_VP, o.worldPos);

                return o;
            }

            float3 _SunDir, _MoonDir, _CamUp, _CamForward;
            float _SunRadius, _MoonRadius, _Warp, _FlareSize;
            float _MoonExposure, _StarExposure, _StarPower, _StarsColorValue;
            float _StarLatitude, _StarSpeed;
            float4x4 _MoonSpaceMatrix;

            float4 _MainLightColor, _AmbientColor;

            samplerCUBE _MoonCubeMap, _StarCubeMap;
            sampler2D _SunFlare;
            sampler2D _SunZenithGrad;
            sampler2D _ViewZenithGrad;
            sampler2D _SunViewGrad;
            sampler2D _HorizonTex;

            float GetRadius(float degree)
            {
                return cos((degree) * 0.0174532925);
            }

            float GetSunMask(float sunViewDot, float sunRadius)
            {
                float stepRadius = sunRadius;
                return step(stepRadius, sunViewDot);
            }

            float3 GetMoonTexture(float3 normal)
            {
                float3 uvw = mul(_MoonSpaceMatrix, float4(normal, 0)).xyz;

                float3x3 correctionMatrix = float3x3(0, -0.2588190451, -0.9659258263,
                    0.08715574275, 0.9622501869, -0.2578341605,
                    0.9961946981, -0.08418598283, 0.02255756611);
                uvw = mul(correctionMatrix, uvw);

                return texCUBE(_MoonCubeMap, uvw).rgb;
            }

            // From Inigo Quilez, https://www.iquilezles.org/www/articles/intersectors/intersectors.htm
            float sphIntersect(float3 rayDir, float3 sphereDir, float radius)
            {
                float b = dot(sphereDir, rayDir);
                float h = -(b + radius);
                if (h < 0.0) 
                    return -1.0;

                h = sqrt(h);
                return -b - h;
            }

            // Construct a rotation matrix that rotates around a particular axis by angle
            // From: https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
            float3x3 AngleAxis3x3(float angle, float3 axis)
            {
                float c, s;
                sincos(angle, s, c);

                float t = 1 - c;
                float x = axis.x;
                float y = axis.y;
                float z = axis.z;

                return float3x3(
                    t * x * x + c, t * x * y - s * z, t * x * z + s * y,
                    t * x * y + s * z, t * y * y + c, t * y * z - s * x,
                    t * x * z - s * y, t * y * z + s * x, t * z * z + c
                    );
            }

            // Rotate the view direction, tilt with latitude, spin with time
            float3 GetStarUVW(float3 viewDir, float latitude, float localSiderealTime)
            {
                // tilt = 0 at the north pole, where latitude = 90 degrees
                float tilt = PI * (latitude - 90) / 180;
                float3x3 tiltRotation = AngleAxis3x3(tilt, float3(1, 0, 0));

                // 0.75 is a texture offset for lST = 0 equals noon
                float spin = (0.75 - localSiderealTime) * 2 * PI;
                float3x3 spinRotation = AngleAxis3x3(spin, float3(0, 1, 0));

                // The order of rotation is important
                float3x3 fullRotation = mul(spinRotation, tiltRotation);

                return mul(fullRotation, viewDir);
            }

            float invLerp(float from, float to, float value) 
            {
                return (value - from) / (to - from);
            }

            float remap(float origFrom, float origTo, float targetFrom, float targetTo, float value) 
            {
                float rel = invLerp(origFrom, origTo, value);
                return lerp(targetFrom, targetTo, rel);
            }

            float2 ConvertToUV(float3 sunNormal, float3 viewDir, float3 up, float size)
            {
                float3 right = cross(up, sunNormal);
                float3 sunUp = cross(sunNormal, right);

                float u = dot(right, viewDir);
                float v = dot(sunUp, viewDir);

                float width = GetRadius(size / 2 - 90);

                u = remap(-width, width, 0, 1, u);
                v = remap(-width, width, 0, 1, v);

                float mask = dot(-sunNormal, viewDir) > GetRadius(size / 2);

                return float2(u, v) * mask;
            }

            float2 RotateUV(float2 uv, float angle)
            {
                // Define the center of the texture (usually the middle point)
                float2 center = float2(0.5, 0.5);

                // Move the uv coordinate so that the rotation is around the center of the texture
                uv -= center;

                // Create the rotation matrix
                float cosAngle = cos(angle);
                float sinAngle = sin(angle);
                float2x2 rotationMatrix = float2x2(cosAngle, -sinAngle, sinAngle, cosAngle);

                // Apply the rotation matrix to the uv coordinates
                uv = mul(uv, rotationMatrix);

                // Move the uv coordinate back to the original space
                uv += center;

                return uv;
            }

            float3 ConvertToBlackAndWhite(float3 color, float weight)
            {
                float grayscale = dot(color, float3(0.299, 0.587, 0.114));
                return lerp(float3(grayscale, grayscale, grayscale), color, weight);
            }

            float4 Fragment(v2f i) : SV_TARGET
            {
                float2 uv = i.worldPos.xz / i.worldPos.y;
                float3 viewDir = normalize(i.worldPos.xyz);
                _SunDir = normalize(_WorldSpaceLightPos0.xyz);
                _MoonDir = normalize(_MoonDir);

                // Main angles
                float groundMask = i.worldPos.y > 0;
                float sunViewDot = dot(_SunDir, viewDir);
                float sunZenithDot = _SunDir.y;
                float viewZenithDot = viewDir.y;
                float sunMoonDot = dot(_SunDir, _MoonDir);
                float moonViewDot = dot(_SunDir, viewDir);

                float sunViewDot01 = (sunViewDot + 1.0) * 0.5;
                float sunZenithDot01 = (sunZenithDot + 1.0) * 0.5;

                // Sky colours
                float3 sunZenithColor = tex2D(_SunZenithGrad, float2(sunZenithDot01, 0.5)).rgb;
                float3 viewZenithColor = tex2D(_ViewZenithGrad, float2(sunZenithDot01, 0.5)).rgb;
                float3 sunViewColor = tex2D(_SunViewGrad, float2(sunZenithDot01, 0.5)).rgb;

                float vzMask = pow(saturate(1.0 - viewZenithDot), 10) * pow(sunViewDot01,0.5) * sunZenithDot01;
                float svMask = pow(saturate(sunViewDot), 30) * groundMask;
                float3 skyColor = sunZenithColor + vzMask * viewZenithColor + svMask * sunViewColor;

                float solarEclipse01 = pow(smoothstep(GetRadius((_SunRadius + _MoonRadius) / 2), 1, sunMoonDot), 4) * moonViewDot;

                // sun flare
                float intensityCurve = -pow(sunZenithDot - 1, 8) + 1;
                float flareSizeMultipy = remap(-1, 1, 1, 1.4, sin(_Time.y)) * intensityCurve;
                float2 sunUv = ConvertToUV(-_SunDir, viewDir, _CamUp, flareSizeMultipy * _FlareSize * pow(dot(_CamForward, _SunDir), 10));
                sunUv = RotateUV(sunUv, cos(_Time.y * 0.4) * PI * 0.020);
                float flare = tex2D(_SunFlare, sunUv).a * 0.07 * (1 - solarEclipse01);

                // The sun
                float sunMask = (GetSunMask(sunViewDot, GetRadius(_SunRadius / 2)) + flare) * groundMask ;
                float bloom = svMask * GetSunMask(sunViewDot, _SunRadius + 0.001) * groundMask * 0.5;
                float3 sunColor = _MainLightColor.rgb * sunMask + (bloom * sunViewColor.rgb);

                // The moon
                float moonIntersect = sphIntersect(viewDir, -_MoonDir, GetRadius(_MoonRadius / 2));
                float moonMask = moonIntersect > -1 ? 1 : 0;

                float3 moonNormal = normalize(_MoonDir - viewDir * moonIntersect);
                float moonNdotL = saturate(dot(moonNormal, -_SunDir));

                float3 moonTexture = GetMoonTexture(moonNormal);
                float3 moonColor = moonMask * moonNdotL * exp2(_MoonExposure) * moonTexture;

                // Solar eclipse
                
                skyColor *= lerp(1, 0.2, solarEclipse01);
                sunColor *= (sunMask * (1 - moonMask)) * lerp(16, 32, solarEclipse01);

                // Lunar eclipse
                float lunarEclipseMask = 1 - step(GetRadius(_SunRadius / 2), -sunViewDot);
                float lunarEclipse01 = smoothstep(GetRadius(_SunRadius / 2), 1.0, -sunMoonDot);
                moonColor *= lerp(lunarEclipseMask, float3(0.76, 0.18, 0.02), lunarEclipse01);

                // Star Map
                float3 starUVW = GetStarUVW(viewDir, _StarLatitude, _Time.y * _StarSpeed % 1);
                float3 starsColor = texCUBE(_StarCubeMap, starUVW).rgb;
                starsColor = ConvertToBlackAndWhite(starsColor, _StarsColorValue);
                float starStrength = (1 - sunViewDot01) * (saturate(-sunZenithDot));
                starsColor = pow(abs(starsColor), _StarPower) ;
                starsColor *= (1 - sunMask) * (1 - moonMask) * starStrength * exp2(_StarExposure);

                // Water plane
                float2 scroll = _Time.z * normalize(float2(1,3)) * 0.005;
                float3 water = tex2D(_HorizonTex, scroll + uv * pow(-viewDir.y, _Warp)).rgb;
                float3 water2 = tex2D(_HorizonTex, scroll * 0.75 + uv * pow(-viewDir.y, _Warp)).rgb;
                
                // sky color result
                float3 col = skyColor + sunColor + moonColor + starsColor;

    return float4(col.xyz * groundMask + ((water + water2 * 0.5) * _AmbientColor) * 0.75 * (1 - groundMask) * clamp(sunZenithDot01, 0.2, 1), 1);
}
            ENDHLSL
        }
    }
}