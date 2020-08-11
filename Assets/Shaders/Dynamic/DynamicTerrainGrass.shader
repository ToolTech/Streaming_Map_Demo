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

Shader "Terrain/DynamicTerrain/Grass"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2DArray) = "white" {}
		_Cutoff("Cutoff", float) = 0.38
		_TextureWaving("Grass Texture Waving ", float) = 0.01
		_ColorIntensity("Color Intensity", Range(0, 1)) = 0.85
	}

		SubShader
		{
			Tags
			{
				"Queue" = "AlphaTest"
				"RenderType" = "Opaque"
				// can interfere with render order.
				"DisableBatching" = "True"
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
			UNITY_DECLARE_TEX2DARRAY(_MainTex);
			sampler2D _PerlinNoise;

			// ****** Properties ******
			float _Cutoff;
			float _ColorIntensity;
			float _TextureWaving;
			fixed3 _ViewDir;
			half3 _TerrainSize;
			half4 _MinMaxWidthHeight[16];
			fixed4 _FrustumPlanes[6];						// Frustum planes (6 planes * 4 floats: [ normal.x, normal.y, normal.z, distance ])
			float4x4 _worldToObj;

			// ****** Grass point cloud ******
			StructuredBuffer<float4> _PointBuffer;

			// Vertex shader
			uint vert(uint id : SV_VertexID, uint instanceID : SV_InstanceID) : TEXCOORD
			{
				return id + instanceID * 64000;
			}

			inline fixed2 MirrorCoordinates(fixed2 uv)
			{
				fixed2 t = frac(uv * 0.5f) * 2.0f;
				fixed2 length = fixed2(1.0f, 1.0f);
				return length - abs(t - length);
			}

			inline half3 RGBToHSV(half3 RGB)
			{
				half R = (RGB.x >= 1.0 ? 255 : RGB.x * 256.0);
				half G = (RGB.y >= 1.0 ? 255 : RGB.y * 256.0);
				half B = (RGB.z >= 1.0 ? 255 : RGB.z * 256.0);

				half h = 0, s;

				half v = max(max(R, G), B);
				half Cmin = min(min(R, G), B);

				half delta = v - Cmin;

				if (v == 0.0) { s = 0; }
				else { s = delta / v; }

				if (s == 0) { h = 0.0; }
				else
				{
					if (R == v)
					{
						h = (G - B) / delta;
					}
					else if (G == v)
					{
						h = 2 + (B - R) / delta;
					}
					else if (B == v)
					{
						h = 4 + (R - G) / delta;
					}

					h *= 60;
					if (h < 0.0) { h = h + 360; }
				}

				return half3(h, s, (v / 255));
			}

			// Grass mesh generation
			inline bool GemerateGeometry(in uint p, inout half4 grassPosition, inout half4 displacement, inout half4 displacementx, inout half4 size, inout half tilt, inout half2 uvDistortion, inout half2 textureWaving, inout half index)
			{
				// Get grass position from compute buffer
				grassPosition = _PointBuffer[p];
				half4 objPos = mul(_worldToObj, half4(grassPosition.xyz, 1));
				half2 _uv = objPos.xz;
				half4 uv = half4(MirrorCoordinates(_uv.xy), 1, 1);

				// To calculate tilt
				half cameraDistance = distance(_WorldSpaceCameraPos.xyz, grassPosition.xyz);

				// Sample perlin noise
				half4 perlinNoise = tex2Dlod(_PerlinNoise, uv);
				half3 hsv = RGBToHSV(perlinNoise.xyz);

				// Grass "random" number (0.0 - 1.0)
				fixed random = frac(hsv.x);

				// Grass index
				index = (half)grassPosition.w;

				// Grass size
				half4 minMaxWidthHeight = _MinMaxWidthHeight[index];
				half2 s = lerp(minMaxWidthHeight.xz, minMaxWidthHeight.yw, random);

				size = half4(s.x, s.y, s.x, random);
				uvDistortion = ((uint) (grassPosition.w * 1000.0f)) % 2 ? fixed2(1.0f, 0) : fixed2(0.0f, 1.0f);

				// Wind
				textureWaving = fixed2(sin(_Time.w + PI * grassPosition.w), cos(_Time.w + PI * grassPosition.x)) * _TextureWaving;

				// Generate grass quad
				grassPosition = half4(grassPosition.xyz, 1.0f);

				half sin;
				half cos;

				sincos(random * PI2, sin, cos);
				displacement = half4(sin, 0, cos, 0) * 0.5 * size.y;
				sincos(random * PI2 + PI / 2, sin, cos);
				displacementx = half4(sin, 0, cos, 0) * 0.5 * size.y;

				tilt = dot(half3(0, 1, 0), -_ViewDir) * saturate(cameraDistance / 3);

				return true;
			}

			ENDCG

			Pass
			{
				Name "GeometryShaderGrass"
				Tags
				{
					"LightMode" = "Deferred"
				}

				Cull Off
				Blend Off

				CGPROGRAM

				// Shader programs
				#pragma vertex vert
				#pragma geometry geom
				#pragma fragment frag

				// Compiler definitions
				#pragma target 5.0
				//#pragma only_renderers d3d11
				#pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap noshadow 

				// Fragment input
				struct FramentInput
				{
					half4 position : SV_POSITION;
					fixed3 texcoord : TEXCOORD0;
					#if UNITY_SHOULD_SAMPLE_SH
						half3 sh : TEXCOORD1;
					#endif
				};

				inline void AppendVertex(inout TriangleStream<FramentInput> triStream, half4 worldPosition, half4 displacement, fixed3 uv)
				{
					FramentInput o;
					o.position = mul(UNITY_MATRIX_VP, worldPosition + displacement);
					o.texcoord = uv;
					#if UNITY_SHOULD_SAMPLE_SH
						o.sh = 0;
						o.sh = ShadeSHPerVertex(fixed3(0,1,0), o.sh);
					#endif
					triStream.Append(o);
				}

				// Geometry shader
				[maxvertexcount(8)]
				void geom(point uint p[1] : TEXCOORD, inout TriangleStream<FramentInput> triStream)
				{
					// Initialize fragment input
					FramentInput o;
					UNITY_INITIALIZE_OUTPUT(FramentInput, o);

					int index;
					half tilt;
					half4 grassPosition;
					half4 size;
					fixed2 uvDistortion, textureWaving;
					fixed4 displacement, displacementx;

					grassPosition = 0;
					displacement = 0;
					displacementx = 0;
					size = 0;
					tilt = 0;
					uvDistortion = 0;
					textureWaving = 0;
					index = 0;

					// Generate grass mesh
					if (GemerateGeometry(p[0], grassPosition, displacement, displacementx, size, tilt, uvDistortion, textureWaving, index))
					{
						//float cameraDistance = distance(_WorldSpaceCameraPos.xyz, grassPosition.xyz);
						//float3 right = cross(_ViewDir.xyz, float3(0, 1, 0)) * length(displacement);
						//float4 faceCam = float4(right.xyz, displacement.w);

						// Top vertices
						//AppendVertex(triStream, grassPosition, half4(0, size.y * 2, 0, 0) + tilt, fixed3(0.5f+ textureWaving.x, 2.0f, index));

						AppendVertex(triStream, grassPosition, displacement + float4(0, size.y, 0, 0) + displacementx * tilt, float3(uvDistortion.x + textureWaving.x, 1.0f, index));
						AppendVertex(triStream, grassPosition, -displacement + float4(0, size.y, 0, 0) - displacementx * tilt, float3(uvDistortion.y + textureWaving.y, 1.0f, index));

						// Bottom vertices
						AppendVertex(triStream, grassPosition, displacement, fixed3(uvDistortion.x, 0.0f, index));
						AppendVertex(triStream, grassPosition, -displacement, fixed3(uvDistortion.y, 0.0f, index));

						triStream.RestartStrip();

						//AppendVertex(triStream, grassPosition, half4(0, size.y * 2, 0, 0) + tilt, fixed3(0.5f + textureWaving.x, 2.0f, index));

						// Top vertices (crossed)
						AppendVertex(triStream, grassPosition, displacementx + float4(0, size.y, 0, 0) - displacement * tilt, float3(uvDistortion.x + textureWaving.x, 1.0f, index));
						AppendVertex(triStream, grassPosition, -displacementx + float4(0, size.y, 0, 0) - displacement * tilt, float3(uvDistortion.y + textureWaving.y, 1.0f, index));

						// Bottom vertices (crossed)
						AppendVertex(triStream, grassPosition, displacementx, fixed3(uvDistortion.x, 0.0f, index));
						AppendVertex(triStream, grassPosition, -displacementx, fixed3(uvDistortion.y, 0.0f, index));
					}
				}

				// Fragment shader
				void frag(FramentInput IN, out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3)
				{
					// Sample texture and multiply color
					fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, IN.texcoord);

					// Cutoff
					clip(c.a - _Cutoff);

					//c *= IN.color;
					c *= fixed4(0.75, 0.75, 0.75, 1);

					half3 worldPos = IN.position;

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
					o.Normal = fixed3(0, 1, 0); // This is used only for ambient occlusion

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
				#pragma geometry geom
				#pragma fragment frag

				// Compiler definitions
				#pragma target 5.0
				#pragma multi_compile_shadowcaster
				//#pragma only_renderers d3d11

				struct FramentInput
				{
					half3 texcoord : TEXCOORD0;
					V2F_SHADOW_CASTER;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				inline void AppendVertex(inout TriangleStream<FramentInput> triStream, half4 worldPosition, half4 displacement, fixed3 uv)
				{
					FramentInput o;
					UNITY_INITIALIZE_OUTPUT(FramentInput, o);
					
					o.texcoord = uv;
					o.pos = mul(UNITY_MATRIX_VP, worldPosition + displacement);
					triStream.Append(o);
				}

				// Geometry shader
				[maxvertexcount(8)]
				void geom(point uint p[1] : TEXCOORD, inout TriangleStream<FramentInput> triStream)
				{
					// Initialize fragment input
					FramentInput o;
					UNITY_INITIALIZE_OUTPUT(FramentInput, o);

					int index;
					half tilt;
					half4 grassPosition;
					half4 size;
					fixed2 uvDistortion, textureWaving;
					fixed4 displacement, displacementx;

					grassPosition = 0;
					displacement = 0;
					displacementx = 0;
					size = 0;
					tilt = 0;
					uvDistortion = 0;
					textureWaving = 0;
					index = 0;

					// Generate grass mesh
					if (GemerateGeometry(p[0], grassPosition, displacement, displacementx, size, tilt, uvDistortion, textureWaving, index))
					{
						//float cameraDistance = distance(_WorldSpaceCameraPos.xyz, grassPosition.xyz);
						//float3 right = cross(_ViewDir.xyz, float3(0, 1, 0)) * length(displacement);
						//float4 faceCam = float4(right.xyz, displacement.w);

						//AppendVertex(triStream, grassPosition, half4(0, size.y * 2, 0, 0) + tilt, fixed3(0.5f + textureWaving.x, 2.0f, index));

						// Top vertices
						AppendVertex(triStream, grassPosition, displacement + float4(0, size.y, 0, 0) + displacementx * tilt, float3(uvDistortion.x + textureWaving.x, 1.0f, index));
						AppendVertex(triStream, grassPosition, -displacement + float4(0, size.y, 0, 0) - displacementx * tilt, float3(uvDistortion.y + textureWaving.y, 1.0f, index));

						// Bottom vertices
						AppendVertex(triStream, grassPosition, displacement, fixed3(uvDistortion.x, 0.0f, index));
						AppendVertex(triStream, grassPosition, -displacement, fixed3(uvDistortion.y, 0.0f, index));

						triStream.RestartStrip();

						//AppendVertex(triStream, grassPosition, half4(0, size.y * 2, 0, 0) + tilt, fixed3(0.5f + textureWaving.x, 2.0f, index));

						// Top vertices (crossed)
						AppendVertex(triStream, grassPosition, displacementx + float4(0, size.y, 0, 0) - displacement * tilt, float3(uvDistortion.x + textureWaving.x, 1.0f, index));
						AppendVertex(triStream, grassPosition, -displacementx + float4(0, size.y, 0, 0) - displacement * tilt, float3(uvDistortion.y + textureWaving.y, 1.0f, index));

						// Bottom vertices (crossed)
						AppendVertex(triStream, grassPosition, displacementx, fixed3(uvDistortion.x, 0.0f, index));
						AppendVertex(triStream, grassPosition, -displacementx, fixed3(uvDistortion.y, 0.0f, index));
					}
				}

				half4 frag(FramentInput IN) : SV_Target
				{
					// Sample texture and multiply color
					fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, IN.texcoord);
				// Cutoff
				clip(c.a - _Cutoff);
				SHADOW_CASTER_FRAGMENT(IN)
			}
			ENDCG
		}
		}
}
