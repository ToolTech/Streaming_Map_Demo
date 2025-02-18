Shader "Custom/TerrainShader"
{
    Properties
    {
        [NoScaleOffset] _FeatureMap("Splatmap", 2D) = "black" {}
		[NoScaleOffset] _MainTex("Satellite / Coarse texture", 2D) = "white" {}

		_BumpScale("Main Bump scale", float) = 10
		_DetailBumpScale("Detail Bump scale", float) = 10
		_Detail("Texture Detail (tieling)", float) = 0.07
    }
    SubShader
    {
       Name "Deferred"
		Tags
		{ 
			"Queue" = "Geometry" 
			"RenderType" = "Opaque"
			"LightMode" = "Deferred"
		}

		Cull Back
		Blend Off
        LOD 100

        CGINCLUDE

		#include "UnityCG.cginc"
		#include "UnityGBuffer.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"

        #define PI 3.14159265358979323846264338327950

		#pragma multi_compile __ NORMAL_TEXTURES_ON

		#pragma exclude_renderers nomrt addshadow
		#pragma require 2darray

		#pragma target 5.0
		#pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap noshadow 

        struct appdata
        {
            float4 position : POSITION;
			float2 uv : TEXCOORD0;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			uint vertexID : SV_VertexID;  // System value for vertex ID
        };

        struct v2f
        {
            float4 position : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 normal : TEXCOORD2;
			float4 tangent : TEXCOORD3;
			float4 worldPos : TEXCOORD4;
        };

		struct FragmentResult
		{
			float4 albedo : SV_Target0;
			float4 specular : SV_Target1;
			float4 normal : SV_Target2;
			float4 ELLR : SV_Target3;
		};

		// ********** using YIQ to shift **********
		// Using YIQ to shift hue
		float3 HueShift(float3 col, float hueShift, float targetHue)
		{
			// Convert RGB to YIQ
			float3x3 RGB_YIQ = float3x3(
				0.299, 0.587, 0.114,
				0.595716, -0.274453, -0.321263,
				0.211456, -0.522591, 0.311135
			);

			// Convert YIQ to RGB
			float3x3 YIQ_RGB = float3x3(
				1.0, 0.9563, 0.6210,
				1.0, -0.2721, -0.6474,
				1.0, -1.1070, 1.7046
			);

			// Compute the YIQ values from the RGB input
			float3 YIQ = mul(RGB_YIQ, col);
			float hue = atan2(YIQ.z, YIQ.y);

			// Shift hue towards target hue
			float hueDiff = targetHue - hue;

			// Correct hue wrap-around if Q is negative
			if (YIQ.z > 0) {
				if (hueDiff > 3.14159)
					hueDiff -= 2 * 3.14159;
				else if (hueDiff < -3.14159)
					hueDiff += 2 * 3.14159;
			}

			// Apply hue shift without overshooting target hue
			hue += hueShift * sign(hueDiff) * min(abs(hueDiff), abs(hueShift));

			// Ensure hue wraps around properly
			hue = fmod(hue + 2 * 3.14159, 2 * 3.14159);

			// Recompute I and Q based on the new hue and original chroma
			float chroma = length(float2(YIQ.y, YIQ.z));
			float Y = YIQ.x; // Preserving brightness
			float I = chroma * cos(hue);
			float Q = chroma * sin(hue);

			// Convert back to RGB
			float3 shiftYIQ = float3(Y, I, Q);
			return mul(YIQ_RGB, shiftYIQ);
		}

		float3 HueShiftColor(float3 col, float hueShift, float3 targetColor)
		{
			// Convert RGB to YIQ for both source and target colors
			float3x3 RGB_YIQ = float3x3(
				0.299, 0.587, 0.114,
				0.595716, -0.274453, -0.321263,
				0.211456, -0.522591, 0.311135
			);

			float3x3 YIQ_RGB = float3x3(
				1.0, 0.9563, 0.6210,
				1.0, -0.2721, -0.6474,
				1.0, -1.1070, 1.7046
			);

			// Compute the YIQ values from the RGB input and target
			float3 YIQ = mul(RGB_YIQ, col);
			float3 targetYIQ = mul(RGB_YIQ, targetColor);

			// Extract hue and saturation from YIQ
			float hue = atan2(YIQ.z, YIQ.y);
			float targetHue = atan2(targetYIQ.z, targetYIQ.y);
			float saturation = length(float2(YIQ.y, YIQ.z));
			float targetSaturation = length(float2(targetYIQ.y, targetYIQ.z));

			// Avoid shifting hue when saturation is too low (grayscale colors)
			if (saturation > 0.001 && YIQ.z < 0)
			{
				// Compute hue and saturation differences
				float hueDiff = targetHue - hue;
				float satDiff = targetSaturation - saturation;

				// Correct for wrap-around
				if (hueDiff > PI) hueDiff -= 2.0 * PI;
				else if (hueDiff < -PI) hueDiff += 2.0 * PI;

				// Apply hue shift smoothly
				hue += hueShift * hueDiff / (abs(hueDiff) + 1e-5); // Prevent division by zero
				saturation = lerp(saturation, targetSaturation, 0.1); // Smooth transition

				// Ensure hue wraps around properly and saturation is clamped
				hue = fmod(hue + 2.0 * PI, 2.0 * PI);
				saturation = clamp(saturation, 0.0, 1.0);
			}
			else
			{
				// If saturation is too low, avoid shifting and keep I/Q minimal
				hue = 0.0;
				saturation = 0.0;
			}

			// Recompute I and Q based on the new hue and adjusted saturation
			float I = saturation * cos(hue);
			float Q = saturation * sin(hue);
			float Y = YIQ.x;  // Preserve brightness

			// Convert back to RGB
			float3 shiftYIQ = float3(Y, I, Q);
			return mul(YIQ_RGB, shiftYIQ);
		}

		float3 TransformNormal(v2f i, float3 normal)
		{
			float3 faceNormal = normalize(i.normal);
			float3 faceTangent = normalize(i.tangent.xyz);
			float3 faceBinormal = normalize(cross(faceNormal, faceTangent) * (i.tangent.w * unity_WorldTransformParams.w));

			float3 TangentSpaceNormal = normal;// UnpackNormal(normal);

			return faceTangent * TangentSpaceNormal.x + faceBinormal * TangentSpaceNormal.y + faceNormal * TangentSpaceNormal.z;
		}

		sampler2D _FeatureMap;
		sampler2D _MainTex;
	
		float3 _WorldOffset;
		float4 _TargetTerrainColor;

		float _BumpScale, _DetailBumpScale, _Detail;
		float _HueShift;

		StructuredBuffer<int> _MappingBuffer;
		StructuredBuffer<float3> _NormalBuffer;

		int _WaterIndex;
		float3 _Ambient;
		float3 _WindVector;

        UNITY_DECLARE_TEX2DARRAY(_Textures);
		UNITY_DECLARE_TEX2DARRAY(_NormalMaps);

        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert (appdata v)
            {
                v2f o;
				o.position = UnityObjectToClipPos(v.position);
				o.uv = v.uv;
				o.normal = UnityObjectToWorldNormal(v.normal);
				#ifdef NORMAL_TEXTURES_ON
					o.normal = _NormalBuffer[v.vertexID];
				#endif
				o.tangent = float4(UnityObjectToWorldDir(v.tangent), v.tangent.w);
				o.worldPos = mul(unity_ObjectToWorld, v.position);
                return o;
            }

			void frag(v2f i, out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3)
			{
				float3 worldPos = (i.worldPos + _WorldOffset) % 1024;

				uint feature = (uint)(tex2D(_FeatureMap, i.uv) * 255.0);

				float2 textureScrolling = 0;

				float3 satellite = tex2D(_MainTex, i.uv);

				float3 color = HueShiftColor(satellite.rgb, _HueShift, _TargetTerrainColor.rgb);
				

				int mappingIndex = _MappingBuffer[feature];
				if(mappingIndex == 0)
				{
					// invalid pixel skip 
				}
				else
				{
					mappingIndex -= 1;
				}

				if(feature == _WaterIndex)
				{
					textureScrolling = _Time.y * _WindVector.xy * _WindVector.z * 0.0015 + 0.1;
				}

				fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(abs(0.005 * worldPos.xz + textureScrolling ) % 1, mappingIndex));
				float3 finalColor = lerp(satellite.rgb, color.rgb, saturate(col.g * 2));

				fixed3 normalDetail = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalMaps, float3(abs(_Detail * worldPos.xz + textureScrolling) % 1, mappingIndex)), _DetailBumpScale);
				fixed3 normalMain = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_NormalMaps, float3(abs(_Detail * 0.73 * worldPos.xz + textureScrolling) % 1, mappingIndex)), _BumpScale);

				float3 normalBlend = BlendNormals(normalMain, normalDetail);

				if(feature == _WaterIndex)
				{
					finalColor = satellite.rgb; 

					// -------- enhanced Water stuff... --------
					// color = HueShiftColor(satellite.rgb, _HueShift, float3(0.45,0.79,0.95));

					// float3 lightblue = satellite.b * float3(0.45,0.79,0.95);
					// float3 darkblue = satellite.b * float3(0.05,0.36,0.6);
					// float3 waterColor = lerp(darkblue.rgb, lightblue.rgb, saturate(col.g * (_SinTime.z * 0.3 + 0.70) * 3));
					
					// finalColor = lerp(waterColor, satellite.rgb, 0.5);

					// _BumpScale = 3;
					// _DetailBumpScale = 0.75;

					// normalBlend = normalMain;
				}


				float3 finalnormal =TransformNormal(i, normalize(normalBlend));

				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));

				// ************ Set Deferred Buffer ************
				
				#ifdef UNITY_COMPILER_HLSL
					SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
				#else
					SurfaceOutputStandardSpecular o;
				#endif

				float test = color.r > 1 ||  color.g > 1 || color.b > 1;
				float test2 = color.r < 0 ||  color.g < 0 || color.b < 0;

				o.Albedo = finalColor;
				o.Emission = 0.0f;
				o.Alpha = col.a;
				o.Occlusion = 1.0f;
				o.Smoothness = 0.0f;
				o.Specular = 0.0f;
				if(feature == _WaterIndex)
				{
					// -------- enhanced Water stuff... --------
					// o.Specular = _Ambient;
					// o.Smoothness = 0.9;
				}			
				o.Normal = finalnormal;

				// Setup lighting environment
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = 0;
				gi.light.dir = fixed3(0, 1, 0);

				// Call GI (lightmaps/SH/reflections) lighting function
				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = i.worldPos;
				giInput.worldViewDir = worldViewDir;
				giInput.atten = 1;
				giInput.lightmapUV = 0.0;


				#if UNITY_SHOULD_SAMPLE_SH
					//giInput.ambient = IN.sh;
				#else
					giInput.ambient.rgb = 0.0;
				#endif

				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;

				#if defined( UNITY_SPECCUBE_BLENDING ) || defined( UNITY_SPECCUBE_BOX_PROJECTION )
					giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
				#endif

				#ifdef UNITY_SPECCUBE_BOX_PROJECTION
					giInput.boxMax[0] = unity_SpecCube0_BoxMax;
					giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
					giInput.boxMax[1] = unity_SpecCube1_BoxMax;
					giInput.boxMin[1] = unity_SpecCube1_BoxMin;
					giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
				#endif

				// Standard specular global illumination
				LightingStandardSpecular_GI(o, giInput, gi);

				// Call lighting function to output g-buffer
				outEmission = LightingStandardSpecular_Deferred(o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
				outGBuffer2.w = 0;

				#ifndef UNITY_HDR_ON
					outEmission.rgb = exp2(-outEmission.rgb);
				#endif
				
			}

            ENDCG
        }

		// ********* SHADOW CASTER PASS *********	

		UsePass "Standard/SHADOWCASTER"
    }
}
