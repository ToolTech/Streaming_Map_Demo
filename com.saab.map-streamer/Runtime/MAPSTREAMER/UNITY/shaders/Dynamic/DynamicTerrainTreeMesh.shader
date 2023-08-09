//******************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s) 
// herein belong to Saab AB. The program(s) may be used and/or
// copied only with the written permission of Saab AB, or in
// accordance with the terms and conditions stipulated in the
// agreement/contract under which the program(s) have been
// supplied. 
//
//
// Information Class:	COMPANY UNCLASSIFIED
// Defence Secrecy:		NOT CLASSIFIED
// Export Control:		NOT EXPORT CONTROLLED
//
//
// File			: DynamicTerrainGrass.shader
// Module		:
// Description	: Shader Code
// Author		: ALBNI
// Product		: BTA
//
//
// Revision History...
//
// Who	Date	Description
//
//
//******************************************************************************﻿

Shader "Terrain/DynamicTerrain/Tree/Mesh"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_ColorIntensity("Color Intensity", Range(0, 1)) = 1.0
		_Fade("Fade in out", float) = 15.0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Geometry"
				"RenderType" = "Opaque"
			}

			CGINCLUDE

			// ****** Includes ******
			#include "UnityCG.cginc"
			#include "UnityGBuffer.cginc"
			#include "UnityStandardUtils.cginc"
			#include "UnityPBSLighting.cginc"

			#define PI 3.1415926535
			#define PI2 6.283185307

			// ****** Textures ******
			sampler2D _MainTex;
			sampler2D _ColorVariance;

			// ****** Properties ******
			fixed _ColorIntensity;
			half _Fade;
			fixed3 _ViewDir;
			half3 _TerrainSize;
			half4 _MinMaxWidthHeight[16];
			fixed4 _FrustumPlanes[6];						// Frustum planes (6 planes * 4 floats: [ normal.x, normal.y, normal.z, distance ])
			float4x4 _worldToObj;
			//float4x4 _worldToScreen;

			// ****** Grass point cloud ******
			StructuredBuffer<float4> _Buffer;

			// Vertex shader
			uint vert(uint id : SV_VertexID, uint instanceID : SV_InstanceID) : TEXCOORD
			{
				return id + instanceID * 64000;
			}

			ENDCG

			Pass
			{
				Tags
				{
					"LightMode" = "Deferred"
				}

					//Cull ON
					Blend Off

					CGPROGRAM
					// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
					//#pragma exclude_renderers d3d11

					// Shader programs
					#pragma vertex vert
					#pragma fragment frag

					// Compiler definitions
					#pragma target 5.0
					#pragma only_renderers d3d11
					#pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap noshadow 


					// Fragment input
					struct FramentInput
					{
						float4 position : SV_POSITION;
						fixed3 texcoord : TEXCOORD0;
						fixed3 worldNormal : NORMAL;
						fixed4 color : COLOR;
						half4 worldPosition : TEXCOORD1;
						#if UNITY_SHOULD_SAMPLE_SH
							half3 sh : TEXCOORD2;
						#endif
					};

				// Fragment shader
				void frag(FramentInput IN, out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3)
				{
					// Sample texture and multiply color
					fixed4 c = tex2Dlod(_MainTex, half4(IN.texcoord.xy, 1,1));

					const half4x4 thresholdMatrix =
					{
						1, 9, 3, 11,
						13, 5, 15, 7,
						4, 12, 2, 10,
						16, 8, 14, 6
					};

					//float3 screenPos = mul(_worldToScreen, IN.position);

					fixed threshold = thresholdMatrix[IN.position.x % 4][IN.position.y % 4] / 17;

					if (threshold >= IN.color.a)
					{
						discard;
					}

					// Cutoff
					//clip((c.a - _Cutoff));
					c *= IN.color;

					half3 worldPos = IN.worldPosition;

					fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

					#ifdef UNITY_COMPILER_HLSL
						SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
					#else
						SurfaceOutputStandardSpecular o;
					#endif

					o.Albedo = c.rgb * _ColorIntensity;
					o.Emission = 0.0f;
					o.Alpha = c.a;
					o.Occlusion = 1.0f;
					o.Smoothness = 0.0f;
					o.Specular = 0.0f;
					o.Normal = IN.worldNormal; // This is used only for ambient occlusion

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
					giInput.worldPos = worldPos;
					giInput.worldViewDir = worldViewDir;
					giInput.atten = 1;
					giInput.lightmapUV = 0.0;

					#if UNITY_SHOULD_SAMPLE_SH
						giInput.ambient = IN.sh;
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

			Pass
			{
				Name "ShadowCaster"
				Tags
				{
					"LightMode" = "ShadowCaster"
				}
				Cull Off
				Blend Off
				CGPROGRAM

				// Includes
				#include "UnityCG.cginc"

				// Shader programs
				#pragma vertex vert
				#pragma fragment frag

				// Compiler definitions
				#pragma target 5.0
				#pragma multi_compile_shadowcaster
				//#pragma only_renderers d3d11

				struct FramentInput
				{
					fixed3 texcoord : TEXCOORD0;
					V2F_SHADOW_CASTER;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				half4 frag(FramentInput IN) : SV_Target
				{
					// Sample texture and multiply color
					fixed4 c = tex2Dlod(_MainTex, half4(IN.texcoord.xy, 1,1));

				// Cutoff
				//clip((c.a - _Cutoff));

				SHADOW_CASTER_FRAGMENT(IN)
			}
		ENDCG
		}
		}
}
