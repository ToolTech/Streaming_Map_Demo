Shader "Custom/Foliage/Billboard"
{
	Properties
	{
		_MainTexArray("Tex2DArray (RGB)", 2DArray) = "white" {}
		_NormalMapArray("Normal Map", 2DArray) = "bump" {}
		_AdditiveSize("Additive foliage Size", Range(0, 30)) = 0
		[MaterialToggle] _isToggled("Up Normals", Float) = 0
	}

		SubShader
		{
			CGINCLUDE

			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityGBuffer.cginc"
			#include "UnityStandardUtils.cginc"
			#include "UnityPBSLighting.cginc"

			struct FS_INPUT
			{
				float4	pos		: POSITION;
				float3	wp		: TEXCOORD2;
				float3  center	: POSITION1;
				float	radius	: POSITION2;
				float3	tex0	: TEXCOORD0;
				float3	normal	: NORMAL;
				float3	color	: TEXCOORD1;
			};

			struct FoliagePoint
			{
				float3 Position;
				float3 Color;
				float Height;
				float Random;
			};

			struct FoliageShaderData
			{
				float2 MaxMin;
				float2 Offset;
				float Weight;
			};

			uint _foliageCount;
			float _AdditiveSize;
			float _isToggled;

			UNITY_DECLARE_TEX2DARRAY(_MainTexArray);
			UNITY_DECLARE_TEX2DARRAY(_NormalMapArray);

			// ****** foliage point cloud ******
			StructuredBuffer<FoliagePoint> _PointBuffer;
			StructuredBuffer<FoliageShaderData> _foliageData;

			// Vertex shader
			uint vert(uint id : SV_VertexID, uint instanceID : SV_InstanceID) : TEXCOORD
			{
				return id + instanceID * 64000;
			}

			// Alpha Blended weight
			float AlphaWeight(float4 color, float depth)
			{
				float weight = max(min(1.0, max(max(color.r, color.g), color.b) * color.a), color.a) *
					clamp(0.03 / (0.00001 + pow(depth / 200, 4.0)), 0.01, 3000);
				return weight;
			}

			float3 CylinderProjectedNormal(FS_INPUT i)
			{
				float3 CamToPixelDir = normalize(i.wp - _WorldSpaceCameraPos);
				float3 CamToCenter = (i.center - _WorldSpaceCameraPos);
				float CenterProjection = dot(CamToPixelDir, CamToCenter);
				float3 RightAnglePoint = _WorldSpaceCameraPos + CamToPixelDir * CenterProjection;
				float CenterDistance = distance(i.center, RightAnglePoint);
				float RightAngleDistance = sqrt(i.radius * i.radius - CenterDistance * CenterDistance);
				float3 ProjectedPoint = RightAnglePoint - CamToPixelDir * RightAngleDistance;

				return normalize(ProjectedPoint - i.center);
			}

			ENDCG

			// ********* Opaque alpha cutoff - Deferred  *********
			Pass
			{
				Name "Deferred"
				Tags
				{ 
					"Queue" = "AlphaTest" 
					"RenderType" = "Opaque"
					"LightMode" = "Deferred"
				}

				Cull Off
				Blend Off
				CGPROGRAM
				#include "Geometry.cginc" 
				#pragma geometry Crossboard

				#pragma target 5.0
				#pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap noshadow 
				
				void frag(FS_INPUT i, out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3)
				{
					fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, i.tex0);
					//half3 tnormal = UnpackNormal(tex2D(_NormalMap, i.tex0));

					clip(col.a - 0.5);
					col.rgb = col.rgb * 0.7f + i.color.rgb * 0.3f;

					fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.wp));
					float depth = i.pos.z / i.pos.w;
					
					float3 LookDir = normalize(_WorldSpaceCameraPos - i.wp);
					float blend = dot(i.normal, LookDir);

					// *********** sphere normals ************* 
					float3 cylinderNormal = CylinderProjectedNormal(i);
					float3 finalNormal = cylinderNormal;

					if (_isToggled)
						finalNormal = float3(0, 1, 0);

					const half4x4 thresholdMatrix =
					{
						1, 9, 3, 11,
						13, 5, 15, 7,
						4, 12, 2, 10,
						16, 8, 14, 6
					};
					const half4x4 invThresholdMatrix =
					{
						16, 8, 14, 6,
						4, 12, 2, 10,
						13, 5, 15, 7,
						1, 9, 3, 11
					};

					fixed threshold;

					if (depth % 2 == 0)
					{
						threshold = invThresholdMatrix[i.pos.x % 4][i.pos.y % 4] / 17;
					}
					else
					{
						threshold = thresholdMatrix[i.pos.x % 4][i.pos.y % 4] / 17;
					}

					float power = 3;
					if (threshold >= (-pow(1 - abs(blend), power) + 1) * (1 - i.normal.y) + (i.normal.y) * pow(abs(blend), power))
					{
						//discard;
					}

					#ifdef UNITY_COMPILER_HLSL
						SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
					#else
						SurfaceOutputStandardSpecular o;
					#endif

					o.Albedo = col.rgb; //normalize(trueNormal * sign(blend)).rgb;
					o.Emission = 0.0f;
					o.Alpha = col.a;
					o.Occlusion = 1.0f;
					o.Smoothness = 0.0f;
					o.Specular = 0.0f;
					o.Normal = finalNormal; // This is used only for ambient occlusion

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
					giInput.worldPos = i.wp;
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
			Pass
			{
				Name "ShadowCaster"
				Tags { "LightMode" = "ShadowCaster" }
				//ColorMask 0				// won't write color to frame buffer
				//Cull back

				CGPROGRAM
				#define	SHADOW_BILLBOARD
				#define	CROSSBOARD
				#include "Geometry.cginc" 

				#pragma multi_compile_shadowcaster
				#pragma multi_compile_instancing	// allow instanced shadow pass for most of the shaders
				#pragma geometry Billboard

				// Fragment Shader 
				float4 frag(FS_INPUT i) : COLOR
				{
					fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, i.tex0);
					//fixed4 col = tex2D(_MainTex, i.tex0);
					clip(col.a - 0.5);
					return float4(0,0,0,0);
				}
				ENDCG
			}
	}
}
