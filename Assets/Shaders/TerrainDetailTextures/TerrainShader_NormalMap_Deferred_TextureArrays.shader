Shader "Custom/TerrainShader_NormalMap_Deferred_TextureArrays"
{
	Properties
	{
		_SplatTex("Splatmap", 2D) = "white" {}
		_SplatMapDimensions("Splatmap Dimensions", Vector) = (512, 512, 0, 0)
		_CoarseTexture("Coarse texture", 2D) = "white" {}
		_Textures("Textures", 2DArray) = "" {}
		_NormalMaps("Normal maps", 2DArray) = "" {}
		_HeightMaps("Height maps", 2DArray) = "" {}
		_RoughnessMaps("Roughness maps", 2DArray) = "" {}
		_Smoothness("Smoothness", Range(0.0, 1.0)) = 1
		_SplatVisualization("Splat visualization", Range(0.0, 1.0)) = 1

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

			#pragma multi_compile_prepassfinal

			#define DEFERRED_PASS

			#include "UnityStandardUtils.cginc"
			#include "UnityPBSLighting.cginc"

			sampler2D _SplatTex;
			sampler2D _CoarseTexture;
			float4 _SplatTex_ST;
			float _SplatVisualization;
			float _DetailTextureFadeStart;
			float _DetailTextureFadeZoneLength;

			float2 _SplatMapDimensions;

			UNITY_DECLARE_TEX2DARRAY(_Textures);
			UNITY_DECLARE_TEX2DARRAY(_NormalMaps);
			UNITY_DECLARE_TEX2DARRAY(_HeightMaps);
			UNITY_DECLARE_TEX2DARRAY(_RoughnessMaps);

			float _Smoothness;

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
				float2 uvSplat : TEXCOORD1;
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
				i.uv = TRANSFORM_TEX(v.uv, _SplatTex);
				i.uv *= 2;
				i.uvSplat = v.uv;

				i.normal = UnityObjectToWorldNormal(v.normal);
				i.tangent = float4(UnityObjectToWorldDir(v.tangent), v.tangent.w);

				i.worldPos = mul(unity_ObjectToWorld, v.position);

				return i;
			}

			void SampleSplatMap(uint input, float2 uvCoord, float4 fallbackAlbedo, out float4 albedo, out float4 normal, out float height, out float roughness)
			{
				bool undefined = input == 1;
				bool building = input == 6;
				bool manmadeObject = input == 8;
				bool grass = input == 21;
				bool grassBarren = input == 22;
				bool vegetation = input == 40;
				bool vegetationOverRoad = input == 47;
				bool vegetationOverBuilding = input == 48;
				bool vegetationOverBridge = input == 49;
				bool waterUnspecified = input == 60;
				bool waterSwimmingpool = input == 65;
				bool manmadeSurface = input == 80;
				bool manmadeSurfacePavedRoad = input == 81;
				bool manmadeSurfaceDirtRoad = input == 83;
				bool manmadeSurfaceBridge = input == 86;
				bool manmadeSurfaceRailBridge = input == 89;
				bool manmadeSurfaceRail = input == 90;
				bool manmadeSurfaceRunway = input == 92;

				float textureIndex = (
					undefined +
					building * 2 +
					manmadeObject * 3 +
					grass * 4 +
					grassBarren * 5 +
					vegetation * 6 +
					vegetationOverRoad * 7 +
					vegetationOverBuilding * 8 +
					vegetationOverBridge * 9 +
					waterUnspecified * 10 +
					waterSwimmingpool * 11 +
					manmadeSurface * 12 +
					manmadeSurfacePavedRoad * 13 +
					manmadeSurfaceDirtRoad * 14 +
					manmadeSurfaceBridge * 15 +
					manmadeSurfaceRailBridge * 16 +
					manmadeSurfaceRail * 17 +
					manmadeSurfaceRunway * 18
				) - 1;

				float3 defaultAlbedo = fallbackAlbedo; //Magenta
				float4 defaultNormal = float4(1, 0.5, 1, 0.5);
				float defaultHeight = 0.5;
				float defaultRoughness = 0;

				bool validSample = (textureIndex != -1);

				albedo = float4(
					UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(uvCoord, textureIndex)).xyz * validSample +
					defaultAlbedo * (1 - validSample),
					1
				);

				float4 norm1 = UNITY_SAMPLE_TEX2DARRAY(_NormalMaps, float3(uvCoord, textureIndex));
				float4 norm2 = UNITY_SAMPLE_TEX2DARRAY(_NormalMaps, float3(uvCoord * 2, textureIndex));
				float4 norm3 = UNITY_SAMPLE_TEX2DARRAY(_NormalMaps, float3(uvCoord * 4, textureIndex));

				float4 normFinal = normalize(norm1 + norm2 * 2 + norm3 * 4);

				normal = normFinal * validSample +
					defaultNormal * (1 - validSample);

				height = UNITY_SAMPLE_TEX2DARRAY(_HeightMaps, float3(uvCoord, textureIndex)) * validSample +
					defaultHeight * (1 - validSample);

				roughness = UNITY_SAMPLE_TEX2DARRAY(_RoughnessMaps, float3(uvCoord, textureIndex)) * validSample +
					defaultRoughness * (1 - validSample);
			}

			void InterpolateSamplesHeight(
				float2 t,
				float4 texs[4],
				float4 norms[4],
				float heights[4],
				float roughnesses[4],
				out float4 albedo,
				out float4 normal,
				out float roughness)
			{
				float4 scalarIntermediate1 = lerp(float4(1, 0, 0, 0), float4(0, 1, 0, 0), t.x);
				float4 scalarIntermediate2 = lerp(float4(0, 0, 1, 0), float4(0, 0, 0, 1), t.x);

				float4 scalars = lerp(scalarIntermediate1, scalarIntermediate2, t.y);
				float4 height = float4(heights[0], heights[1], heights[2], heights[3]) * scalars;

				bool onDiagonal = (abs(scalars.r - scalars.a) < 0.01) || (abs(scalars.g - scalars.b) < 0.01);
				bool onEdge = (abs(t.x - 0.5) < 0.01) || (abs(t.y - 0.5) < 0.01);

				float4 strengths = float4(
					(height.y <= height.x) && (height.z <= height.x) && (height.w <= height.x),
					(height.x <= height.y) && (height.z <= height.y) && (height.w <= height.y),
					(height.x <= height.z) && (height.y <= height.z) && (height.w <= height.z),
					(height.x <= height.w) && (height.y <= height.w) && (height.z <= height.w)
				);

				strengths = length(strengths) == 0 ? float4(0, 0, 0, 1) : strengths;

				albedo = texs[0] * strengths.x + texs[1] * strengths.y + texs[2] * strengths.z + texs[3] * strengths.w;// +float4(0, 1, 0, 1) * onDiagonal + float4(t.x * t.y, 0, 0, 1) * onEdge;
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

			FragmentResult FS(Interpolators i)
			{
				float2 texelCoordinate = _SplatMapDimensions.xy * i.uvSplat.xy;
				float2 texelFraction = frac(texelCoordinate);
				float2 texelCenterOffset = texelFraction - 0.5;

				float2 texelSize = float2(1 / _SplatMapDimensions.x, 1 / _SplatMapDimensions.y);

				float2 minCoord = float2(
					i.uvSplat.x + (
						texelCenterOffset.x < 0 ?
							-texelSize.x :
							0
					),
					i.uvSplat.y + (
						texelCenterOffset.y < 0 ?
							-texelSize.y :
							0
					)
				);

				float splat1 = tex2D(_SplatTex, minCoord).r * 255;
				float splat2 = tex2D(_SplatTex, minCoord + float2(texelSize.x, 0)).r * 255;
				float splat3 = tex2D(_SplatTex, minCoord + float2(0, texelSize.y)).r * 255;
				float splat4 = tex2D(_SplatTex, minCoord + float2(texelSize.x, texelSize.y)).r * 255;

				float4 texs[4];
				float4 norms[4];
				float heights[4];
				float roughnesses[4];

				float4 coarseColor = tex2D(_CoarseTexture, i.uvSplat);

				SampleSplatMap((uint)splat1, i.uv, coarseColor, texs[0], norms[0], heights[0], roughnesses[0]);
				SampleSplatMap((uint)splat2, i.uv, coarseColor, texs[1], norms[1], heights[1], roughnesses[1]);
				SampleSplatMap((uint)splat3, i.uv, coarseColor, texs[2], norms[2], heights[2], roughnesses[2]);
				SampleSplatMap((uint)splat4, i.uv, coarseColor, texs[3], norms[3], heights[3], roughnesses[3]);

				float2 t = frac(texelCoordinate - 0.5);

				float4 albedo;
				float4 normal;
				float roughnessFinal;

				InterpolateSamplesHeight(t, texs, norms, heights, roughnesses, albedo, normal, roughnessFinal);

				//Tangentspace -> Worldspace
				float3 worldSpaceNormal = TransformNormal(i, normal);

				float3 specularTint = _LightColor0.rgb;

				FragmentResult result = (FragmentResult)0;

				float smoothnessFinal = _Smoothness * (1 - roughnessFinal);

				float pixelDistance = distance(i.worldPos, _WorldSpaceCameraPos);

				float detailInclusion = 1 - saturate(pixelDistance * (1 / _DetailTextureFadeZoneLength) - (_DetailTextureFadeStart / _DetailTextureFadeZoneLength));;

				result.albedo = coarseColor;//(float4(albedo.rgb, 0) * detailInclusion + coarseColor * _SplatVisualization) / (1 + detailInclusion);// 
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
				//o.Normal = n;

				o.Normal = worldSpaceNormal; // This is used only for ambient occlusion

				// Standard specular global illumination
				LightingStandardSpecular_GI(o, giInput, gi);

				result.ELLR = LightingStandardSpecular_Deferred(o, worldViewDir, gi, result.albedo, result.specular, result.normal);
				#ifndef UNITY_HDR_ON
					result.ELLR.rgb = exp2(-result.ELLR.rgb);
				#endif

				return result;
			}

			ENDCG
		}
	}
}