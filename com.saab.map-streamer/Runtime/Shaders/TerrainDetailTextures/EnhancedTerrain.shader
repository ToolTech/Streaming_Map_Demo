/* 
 * Copyright (C) SAAB AB
 *
 * All rights, including the copyright, to the computer program(s) 
 * herein belong to Saab AB. The program(s) may be used and/or
 * copied only with the written permission of Saab AB, or in
 * accordance with the terms and conditions stipulated in the
 * agreement/contract under which the program(s) have been
 * supplied. 
 * 
 * Information Class:          COMPANY RESTRICTED
 * Defence Secrecy:            UNCLASSIFIED
 * Export Control:             NOT EXPORT CONTROLLED
 */

Shader "Custom/EnhancedTerrain"
{
	Properties
	{
		_FeatureMap("Splatmap", 2D) = "white" {}
		_SplatMapDimensions("Splatmap Dimensions", Vector) = (512, 512, 0, 0)
		_MainTex("Satellite / Coarse texture", 2D) = "white" {}
		_Textures("Textures", 2DArray) = "" {}
		_NormalMaps("Normal maps", 2DArray) = "" {}
		_HeightMaps("Height maps", 2DArray) = "" {}
		_RoughnessMaps("Roughness maps", 2DArray) = "" {}
		_Smoothness("Smoothness", Range(0.0, 1.0)) = 0
		_RoughnessFallback("RoughnessFallback", Range(0.0, 1.0)) = 1
		_HueShiftInclusion("HueShiftInclusion", Range(0.0, 1.0)) = 1
		_SecondaryNormalIntensity("SecondaryNormalIntensity", Range(0.0, 1.0)) = 1
		_TertiaryNormalIntensity("TertiaryNormalIntensity", Range(0.0, 1.0)) = 1

		_DetailTextureFadeStart("Detail texture fade start distance", Range(0.0, 10000.0)) = 1
		_DetailTextureFadeZoneLength("Detail texture fade zone length", Range(0.0, 100.0)) = 1
	}

	SubShader
	{
		Pass
		{
			Tags{ "LightMode" = "Deferred" }
			CGPROGRAM

			#pragma vertex VS
			#pragma fragment FS

			#pragma exclude_renderers nomrt
			#pragma require 2darray

			#pragma multi_compile __ DETAIL_TEXTURES_ON

			#pragma multi_compile_prepassfinal

			#define DEFERRED_PASS
			#define PI 3.14159265358979323846264338327950

			#include "UnityStandardUtils.cginc"
			#include "UnityPBSLighting.cginc"

			sampler2D _MainTex;
			float _Smoothness;

#ifdef DETAIL_TEXTURES_ON
			sampler2D _FeatureMap;
			float4 _FeatureMap_ST;
			float _DetailTextureFadeStart;
			float _DetailTextureFadeZoneLength;

			StructuredBuffer<int> _MappingBuffer;

			float2 _SplatMapDimensions;

			UNITY_DECLARE_TEX2DARRAY(_Textures);
			UNITY_DECLARE_TEX2DARRAY(_NormalMaps);
			UNITY_DECLARE_TEX2DARRAY(_HeightMaps);
			UNITY_DECLARE_TEX2DARRAY(_RoughnessMaps);

			float _RoughnessFallback;
			float _HueShiftInclusion;

			float _SecondaryNormalIntensity;
			float _TertiaryNormalIntensity;
#endif

			struct VertexData
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
				float4 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct Interpolators
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 featureUV : TEXCOORD1;
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

			Interpolators VS(VertexData v)
			{
				Interpolators i;

				i.position = UnityObjectToClipPos(v.position);
#ifdef DETAIL_TEXTURES_ON
				i.uv = TRANSFORM_TEX(v.uv, _FeatureMap);
				i.uv *= 2;
#endif

				i.featureUV = v.uv;
				i.normal = UnityObjectToWorldNormal(v.normal);
				i.tangent = float4(UnityObjectToWorldDir(v.tangent), v.tangent.w);
				i.worldPos = mul(unity_ObjectToWorld, v.position);

				return i;
			}

#ifdef DETAIL_TEXTURES_ON
			void SampleSplatMap(uint input, float2 uvCoord, float4 fallbackAlbedo, out float4 albedo, out float4 normal, out float height, out float roughness)
			{
				float textureIndex = _MappingBuffer[input] - 1;

				float3 defaultAlbedo = fallbackAlbedo;
				float4 defaultNormal = float4(1, 0.5, 1, 0.5);
				float defaultHeight = 0.5;
				float defaultRoughness = _RoughnessFallback;

				//validSample = Use detail texture
				//invalidSample = Use default values/satellite image
				bool validSample = (textureIndex != -1);
				bool invalidSample = !validSample;

				float4 norm1 = UNITY_SAMPLE_TEX2DARRAY(_NormalMaps, float3(uvCoord, textureIndex));
				float4 norm2 = UNITY_SAMPLE_TEX2DARRAY(_NormalMaps, float3(uvCoord * 2, textureIndex));
				float4 norm3 = UNITY_SAMPLE_TEX2DARRAY(_NormalMaps, float3(uvCoord * 4, textureIndex));

				float4 normalCombination = normalize(norm1 + (norm2 * 2 * _SecondaryNormalIntensity) + (norm3 * 4 * _TertiaryNormalIntensity));

				albedo = float4(
					validSample * UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(uvCoord, textureIndex)).xyz +
					invalidSample * defaultAlbedo,
					1
				);

				normal =
					validSample		* normalCombination +
					invalidSample	* defaultNormal;

				height =
					validSample		* UNITY_SAMPLE_TEX2DARRAY(_HeightMaps, float3(uvCoord, textureIndex)) +
					invalidSample	* defaultHeight;

				roughness =
					validSample		* UNITY_SAMPLE_TEX2DARRAY(_RoughnessMaps, float3(uvCoord, textureIndex)) +
					invalidSample	* defaultRoughness;
			}

			void InterpolateSamplesHeight(
				float2 interpolationFactor,
				float4 texs[4],
				float4 norms[4],
				float heights[4],
				float roughnesses[4],
				out float4 albedo,
				out float4 normal,
				out float roughness)
			{
				float4 scalarIntermediate1 = lerp(float4(1, 0, 0, 0), float4(0, 1, 0, 0), interpolationFactor.x);
				float4 scalarIntermediate2 = lerp(float4(0, 0, 1, 0), float4(0, 0, 0, 1), interpolationFactor.x);

				float4 scalars = lerp(scalarIntermediate1, scalarIntermediate2, interpolationFactor.y);
				float4 height = float4(heights[0], heights[1], heights[2], heights[3]) * scalars;

				float4 strengths = float4(
					(height.y <= height.x) && (height.z <= height.x) && (height.w <= height.x),
					(height.x <= height.y) && (height.z <= height.y) && (height.w <= height.y),
					(height.x <= height.z) && (height.y <= height.z) && (height.w <= height.z),
					(height.x <= height.w) && (height.y <= height.w) && (height.z <= height.w)
				);

				strengths = length(strengths) == 0 ? float4(0, 0, 0, 1) : strengths;

				albedo = texs[0] * strengths.x + texs[1] * strengths.y + texs[2] * strengths.z + texs[3] * strengths.w;
				normal = norms[0] * strengths.x + norms[1] * strengths.y + norms[2] * strengths.z + norms[3] * strengths.w;
				height = heights[0] * strengths.x + heights[1] * strengths.y + heights[2] * strengths.z + heights[3] * strengths.w;
				roughness = roughnesses[0] * strengths.x + roughnesses[1] * strengths.y + roughnesses[2] * strengths.z + roughnesses[3] * strengths.w;
			}

			float3 TransformNormal(Interpolators i, float4 normal)
			{
				float3 faceNormal = normalize(i.normal);
				float3 faceTangent = normalize(i.tangent.xyz);
				float3 faceBinormal = normalize(cross(faceNormal, faceTangent) * (i.tangent.w * unity_WorldTransformParams.w));

				float3 TangentSpaceNormal = UnpackNormal(normal);

				return faceTangent * TangentSpaceNormal.x + faceBinormal * TangentSpaceNormal.y + faceNormal * TangentSpaceNormal.z;
			}

			// ********** using YIQ to shift **********
			float3 HueShift(float3 col, float hueShift, float targetHue)
			{
				float3x3 RGB_YIQ = float3x3(
					0.299, 0.587, 0.114,
					0.5959, -0.2746, -0.3213,
					0.2115, -0.5227, 0.3112
				);

				float3x3 YIQ_RGB = float3x3(
					1, 0.956, 0.619,
					1, -0.272, -0.647,
					1, -1.106, 1.703
				);

				float brightness = 0;
				float saturation = 1.5;

				float3 YIQ = mul(RGB_YIQ, col);
				float hue = atan2(YIQ.z, YIQ.y);

				float shiftAmount = 1 - pow(saturate(YIQ.z), 10);
				hueShift *= shiftAmount;

				float hueDiff = targetHue - hue;

				if (hueDiff > 0 || hueDiff < PI)
					hueShift *= -1;

				hue += hueShift;

				// black and white
				float chroma = length(float2(YIQ.y, YIQ.z)) * saturation;

				float Y = YIQ.x + brightness;
				float I = chroma * cos(hue);
				float Q = chroma * sin(hue);

				float3 shiftYIQ = float3(Y, I, Q);

				return mul(YIQ_RGB, shiftYIQ);
			}
#endif

			FragmentResult BuildFragmentResult(float4 detail, float3 specularTint, float smoothnessFinal, Interpolators i, float3 worldSpaceNormal)
			{
				FragmentResult result = (FragmentResult)0;

				result.albedo = detail;
				result.specular = float4(specularTint * smoothnessFinal, smoothnessFinal);

#ifdef UNITY_COMPILER_HLSL
				SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
#else
				SurfaceOutputStandardSpecular o;
#endif

				// Setup lighting environment
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = 0;
				gi.light.dir = fixed3(0, 1, 0);

				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));

				// Call GI (lightmaps/SH/reflections) lighting function
				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = i.worldPos;
				giInput.worldViewDir = worldViewDir;
				giInput.atten = 1;
				giInput.lightmapUV = 0.0;

				o.Albedo = result.albedo.rgb;
				o.Emission = 0.0f;
				o.Alpha = result.albedo.a;
				o.Occlusion = 1.0f;
				o.Smoothness = 0.0f;
				o.Specular = result.specular;

				o.Normal = worldSpaceNormal;

				// Standard specular global illumination
				LightingStandardSpecular_GI(o, giInput, gi);

				result.ELLR = LightingStandardSpecular_Deferred(o, worldViewDir, gi, result.albedo, result.specular, result.normal);
#ifndef UNITY_HDR_ON
				result.ELLR.rgb = exp2(-result.ELLR.rgb);
#endif
				return result;
			}

			float2 SnapToBottomLeftFeatureTexel(float2 featureCoordinate, float2 texelCoordinate, float2 texelSize)
			{
				float2 texelFraction = frac(texelCoordinate);
				float2 texelCenterOffset = texelFraction - 0.5;
				
				return float2(
					featureCoordinate.x + (
						texelCenterOffset.x < 0 ?
						-texelSize.x :
						0
					),
					featureCoordinate.y + (
						texelCenterOffset.y < 0 ?
						-texelSize.y :
						0
					)
				);
			}

			FragmentResult FS(Interpolators i)
			{
#ifdef DETAIL_TEXTURES_ON
				float2 texelSize = float2(1 / _SplatMapDimensions.x, 1 / _SplatMapDimensions.y);
				float2 texelCoordinate = _SplatMapDimensions.xy * i.featureUV.xy;

				float2 bottomLeftTexel = SnapToBottomLeftFeatureTexel(i.featureUV, texelCoordinate, texelSize);

				float splat1 = tex2D(_FeatureMap, bottomLeftTexel).r * 255;
				float splat2 = tex2D(_FeatureMap, bottomLeftTexel + float2(texelSize.x, 0)).r * 255;
				float splat3 = tex2D(_FeatureMap, bottomLeftTexel + float2(0, texelSize.y)).r * 255;
				float splat4 = tex2D(_FeatureMap, bottomLeftTexel + float2(texelSize.x, texelSize.y)).r * 255;

				float4 albedoSamples[4];
				float4 normalSamples[4];
				float heightSamples[4];
				float roughnessSamples[4];
#endif

				float4 coarseColor = tex2D(_MainTex, i.featureUV);

#ifdef DETAIL_TEXTURES_ON
				SampleSplatMap((uint)splat1, i.uv, float4(1, 0, 1, 1), albedoSamples[0], normalSamples[0], heightSamples[0], roughnessSamples[0]);
				SampleSplatMap((uint)splat2, i.uv, float4(1, 0, 1, 1), albedoSamples[1], normalSamples[1], heightSamples[1], roughnessSamples[1]);
				SampleSplatMap((uint)splat3, i.uv, float4(1, 0, 1, 1), albedoSamples[2], normalSamples[2], heightSamples[2], roughnessSamples[2]);
				SampleSplatMap((uint)splat4, i.uv, float4(1, 0, 1, 1), albedoSamples[3], normalSamples[3], heightSamples[3], roughnessSamples[3]);

				float2 interpolationFactor = frac(texelCoordinate - 0.5);

				float4 albedo;
				float4 normal;
				float roughnessFinal;

				InterpolateSamplesHeight(interpolationFactor, albedoSamples, normalSamples, heightSamples, roughnessSamples, albedo, normal, roughnessFinal);

				float smoothnessFinal = _Smoothness * (1 - roughnessFinal);

				//Tangentspace -> Worldspace
				float3 worldSpaceNormal = TransformNormal(i, normal);

				// ********* Hue shift *********
				float4 hue = float4(HueShift(coarseColor, PI / 4, 0), 1);
				float4 detail = lerp(lerp(coarseColor, hue, albedo.g), coarseColor, 1 - _HueShiftInclusion);

				return BuildFragmentResult(detail, _LightColor0.rgb, smoothnessFinal, i, worldSpaceNormal);
#else
				return BuildFragmentResult(coarseColor, _LightColor0.rgb, _Smoothness, i, float3(0, 1, 0));
#endif
			}

			ENDCG
		}
	}
}