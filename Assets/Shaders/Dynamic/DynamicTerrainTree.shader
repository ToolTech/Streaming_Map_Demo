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

Shader "Terrain/DynamicTerrain/Tree"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2DArray) = "white" {}
		_NormalTex("Normal", 2DArray) = "bump" {}
		_Cutoff("Cutoff", float) = 0.62
		_TextureWaving("Grass Texture Waving ", float) = 0.01
		_ColorIntensity("Color Intensity", Range(0, 1)) = 1
		_FadeNear("Fade Near value", float) = 100.0
		_FadeFar("Fade Far value", float) = 500.0
		_FadeNearAmount("Fade Near Amount", float) = 50
		_FadeFarAmount("Fade Near Amount", float) = 200
	}

		SubShader
		{
			Tags
			{
				"Queue" = "AlphaTest"
				"RenderType" = "Opaque"
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
			UNITY_DECLARE_TEX2DARRAY(_NormalTex);
			sampler2D _PerlinNoise;
			sampler2D _ColorVariance;

			// ****** Properties ******
			fixed _Cutoff;
			fixed _ColorIntensity;
			half _TextureWaving;

			half _FadeNear;
			half _FadeFar;
			half _FadeNearAmount;
			half _FadeFarAmount;
			half _Yoffset[16];

			fixed3 _ViewDir;
			half3 _TerrainSize;

			half4 _MinMaxWidthHeight[16];
			half4 _Quads[24];
			fixed4 _FrustumPlanes[6];						// Frustum planes (6 planes * 4 floats: [ normal.x, normal.y, normal.z, distance ])
			float4x4 _worldToObj;
			//float4x4 _worldToScreen;

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
			inline bool GemerateGeometry(in uint p, inout half4 grassPosition, inout half4 displacement, inout half4 displacementx, inout half4 size, inout half tilt, inout half tiltx, inout fixed4 bottomColor, inout fixed4 topColor, inout float2 uvDistortion, inout fixed2 textureWaving, inout int index)
			{
				// Get grass position from compute buffer
				grassPosition = _PointBuffer[p];
				half4 objPos = mul(_worldToObj, half4(grassPosition.xyz, 1));
				half2 _uv = objPos.xz;
				half4 uv = half4(_uv.xy, 1, 1);

				//half4 colorVar = tex2Dlod(_ColorVariance, uv);
				//fixed offset = 0.75f;
				//colorVar = half4(colorVar.x + offset > 1 ? 1 : colorVar.x + offset, colorVar.y + offset > 1 ? 1 : colorVar.y + offset, colorVar.z + offset > 1 ? 1 : colorVar.z + offset, 0);

				// Sample perlin noise
				fixed4 perlinNoise = tex2Dlod(_PerlinNoise, uv);
				half3 hsv = RGBToHSV(perlinNoise.xyz);

				// Grass "random" number (0.0 - 1.0)
				half random = frac(hsv.x);
				//random = 1;

				// Grass index
				index = (int)grassPosition.w;

				// Grass color
				//fixed4 color = fixed4(colorVar.xyz, 1.0);
				//color = fixed4(1.0, 1.0, 1.0, 1.0) * fixed4(_LightColor0.rgb.xyz, 1.0);

				// Grass size
				half4 minMaxWidthHeight = _MinMaxWidthHeight[index];
				half2 s = lerp(minMaxWidthHeight.xz, minMaxWidthHeight.yw, random);

				size = half4(s.x, s.y, s.x, random);
				uvDistortion = ((uint) (grassPosition.w * 1000.0f)) % 2 ? fixed2(1.0f, 0) : fixed2(0.0f, 1.0f);

				// Wind
				textureWaving = fixed2(sin(_Time.w + PI * grassPosition.w), cos(_Time.w + PI * grassPosition.x)) * _TextureWaving;

				// Generate grass quad
				grassPosition = half4(grassPosition.xyz, 1.0f);

				// Top vertices
				topColor = fixed4(1,1,1,1);
				bottomColor = topColor;
				half sin;
				half cos;

				sincos(random * PI2, sin, cos);
				displacement = fixed4(sin, 0, cos, 0) * 0.5 * size.y;
				sincos(random * PI2 + PI / 2, sin, cos);
				displacementx = fixed4(sin, 0, cos, 0) * 0.5 * size.y;

				fixed3 normal = normalize(lerp(displacementx, fixed3(0, 1, 0), 0.75));
				fixed3 normalx = normalize(lerp(displacement, fixed3(0, 1, 0), 0.75));

				//tilt = dot(float3(0, 1, 0), -_ViewDir) * saturate(cameraDistance / 3);

				fixed3 lookview = normalize(fixed3(0,0,0) - grassPosition.xyz);
				lookview.y = 0;

				tilt = dot(lookview, normal);
				tiltx = dot(lookview, normalx);

				//topColor.a = 1 - abs(tilt * 2);

				//bottomColor = topColor * fixed4(0.75f, 0.75f, 0.75f, 1.0f);
				return true;
			}

			ENDCG

			Pass
			{
				Tags
				{
					"LightMode" = "Deferred"
				}

				Cull Off
				Blend Off

				CGPROGRAM
				// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
				//#pragma exclude_renderers d3d11

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
					float4 position : SV_POSITION;
					fixed3 texcoord : TEXCOORD0;
					fixed3 worldNormal : NORMAL;
					fixed4 color : COLOR;
					half4 worldPosition : TEXCOORD1;
					#if UNITY_SHOULD_SAMPLE_SH
						half3 sh : TEXCOORD2;
					#endif
				};

				inline void AppendVertex(inout TriangleStream<FramentInput> triStream, half4 worldPosition, fixed4 displacement, fixed3 worldNormal, fixed3 uv, fixed4 color)
				{
					FramentInput o;
					o.position = mul(UNITY_MATRIX_VP, worldPosition + displacement);
					o.worldPosition = worldPosition;
					o.worldNormal = worldNormal;
					o.texcoord = uv;
					o.color = color;
					#if UNITY_SHOULD_SAMPLE_SH
						o.sh = 0;
						o.sh = ShadeSHPerVertex(worldNormal, o.sh);
					#endif
					triStream.Append(o);
				}

				// Geometry shader
				[maxvertexcount(12)]
				void geom(point uint p[1] : TEXCOORD, inout TriangleStream<FramentInput> triStream)
				{
					// Initialize fragment input
					FramentInput o;
					UNITY_INITIALIZE_OUTPUT(FramentInput, o);

					int index;
					half tilt;
					half tiltx;
					half4 grassPosition;
					half4 size;
					fixed2 uvDistortion, textureWaving;
					fixed4 displacement, displacementx;
					fixed4 bottomColor, topColor;

					grassPosition = 0;
					displacement = 0;
					displacementx = 0;
					size = 0;
					tilt = 0;
					tiltx = 0;
					bottomColor = 0;
					topColor = 0;
					uvDistortion = 0;
					textureWaving = 0;
					index = 0;

					fixed3 upNormal = fixed3(0, 1, 0);

					// Generate grass mesh
					if (GemerateGeometry(p[0], grassPosition, displacement, displacementx, size, tilt, tiltx, bottomColor, topColor, uvDistortion, textureWaving, index))
					{
						half dist = distance(grassPosition.xyz, fixed3(0, 0, 0));

						half fadeAmount = _FadeNearAmount;
						half maxdist = 30000;
						half fade = _FadeNear;
						half transparency = 0;

						grassPosition.y -= _Yoffset[index] * size.y;

						fixed yaw = dot(normalize(grassPosition + size.y / 2), upNormal);

						if (dist < fade)
						{
							fade = 1 - (abs(fade - dist) / fadeAmount);
							//fade = ((dist - (fadeDist - fadeAmount)) / fadeAmount);
							//if (fade < 0) { fade = 0; }
						}
						if (dist > _FadeFar)
						{
							half value = 1 - ((dist - _FadeFar) / _FadeFarAmount);
							fade = value < 1.0 ? value : 0.0;
						}
						// ************ side ************

						if (tiltx <= 1 && tiltx >= -1)
						{
							if (dist < maxdist)
							{
								transparency = (abs(tilt) * 2) > 1 ? 1 : (abs(tilt) * 2);
								topColor.a = transparency < fade ? transparency : fade;
								bottomColor.a = topColor.a;
							}

							half4 side = _Quads[index * 3 + 1];

							// Top vertices
							AppendVertex(triStream, grassPosition, displacement * ((0.5 - side.x) * 2) + half4(0, size.y, 0, 0), upNormal, fixed3((side.x * 0.5) + textureWaving.x, 0.5, index), topColor);
							AppendVertex(triStream, grassPosition, -displacement * ((side.y - 0.5) * 2) + half4(0, size.y, 0, 0), upNormal, fixed3((side.y * 0.5) + textureWaving.y, 0.5, index), topColor);

							// Bottom vertices
							AppendVertex(triStream, grassPosition, displacement * ((0.5 - side.x) * 2), upNormal, fixed3((side.x * 0.5), 0, index), bottomColor);
							AppendVertex(triStream, grassPosition, -displacement * ((side.y - 0.5) * 2), upNormal, fixed3((side.y * 0.5), 0, index), bottomColor);
						}

						// ************ Front ************

						if (tilt <= 1 && tilt >= -1)
						{
							if (dist < maxdist)
							{
								transparency = (abs(tiltx) * 2) > 1 ? 1 : (abs(tiltx) * 2);
								topColor.a = transparency < fade ? transparency : fade;
								bottomColor.a = topColor.a;
							}

							triStream.RestartStrip();

							half4 front = _Quads[index * 3];

							// Top vertices (crossed)
							AppendVertex(triStream, grassPosition, displacementx * ((0.5 - front.x) * 2) + half4(0, size.y, 0, 0), upNormal, fixed3((front.x * 0.5) + textureWaving.x, 1, index), topColor);
							AppendVertex(triStream, grassPosition, -displacementx * ((front.y - 0.5) * 2) + half4(0, size.y, 0, 0), upNormal, fixed3((front.y * 0.5) + textureWaving.y, 1, index), topColor);

							// Bottom vertices (crossed)
							AppendVertex(triStream, grassPosition, displacementx * ((0.5 - front.x) * 2), upNormal, fixed3((front.x * 0.5), 0.5, index), bottomColor);
							AppendVertex(triStream, grassPosition, -displacementx * ((front.y - 0.5) * 2), upNormal, fixed3((front.y * 0.5), 0.5, index), bottomColor);
						}

						// ************ Top ************

						if (yaw < 0)
						{
							if (dist < maxdist)
							{
								transparency = (abs(yaw * 4)) > 1 ? 1 : (abs(yaw* 4));
								topColor.a = transparency < fade ? transparency : fade;
								bottomColor.a = topColor.a;
							}

							triStream.RestartStrip();

							half4 Top = _Quads[index * 3 + 2];

							// Top vertices (crossed)
							AppendVertex(triStream, grassPosition, displacementx * ((0.5 - Top.x) * 2) + half4(0, size.y / 2, 0, 0) + (displacement * ((Top.w - 0.5) * 2)), upNormal, fixed3((0.5 * Top.x) + 0.5, Top.w * 0.5, index), topColor);
							AppendVertex(triStream, grassPosition, -displacementx * ((Top.y - 0.5) * 2) + half4(0, size.y / 2, 0, 0) + (displacement * ((Top.w - 0.5) * 2)), upNormal, fixed3((0.5 * Top.y) + 0.5, Top.w * 0.5, index), topColor);

							// Bottom vertices (crossed)
							AppendVertex(triStream, grassPosition, displacementx * ((0.5 - Top.x) * 2) + half4(0, size.y / 2, 0, 0) - (displacement * ((0.5 - Top.z) * 2)), upNormal, fixed3((0.5 * Top.x) + 0.5, 0.5 * Top.z, index), bottomColor);
							AppendVertex(triStream, grassPosition, -displacementx * ((Top.y - 0.5) * 2) + half4(0, size.y / 2, 0, 0) - (displacement * ((0.5 - Top.z) * 2)), upNormal, fixed3((0.5 * Top.y) + 0.5, 0.5 * Top.z, index), bottomColor);
						}
					}
				}

				// Fragment shader
				void frag(FramentInput IN, out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3)
				{
					// Sample texture and multiply color
					fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, IN.texcoord);
					fixed3 n = UNITY_SAMPLE_TEX2DARRAY(_NormalTex, IN.texcoord);

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
					clip((c.a - _Cutoff));
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
					//o.Normal = n;
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
					#pragma geometry geom
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

					inline void AppendVertex(inout TriangleStream<FramentInput> triStream, half4 worldPosition, fixed4 displacement, fixed3 worldNormal, fixed3 uv, fixed4 color)
					{
						FramentInput o;
						UNITY_INITIALIZE_OUTPUT(FramentInput, o);
						o.texcoord = uv;
						o.pos = mul(UNITY_MATRIX_VP, worldPosition + displacement);
						triStream.Append(o);
					}

					// Geometry shader
					[maxvertexcount(4)]
					void geom(point uint p[1] : TEXCOORD, inout TriangleStream<FramentInput> triStream)
					{
						// Initialize fragment input
						FramentInput o;
						UNITY_INITIALIZE_OUTPUT(FramentInput, o);

						int index;
						half tilt;
						half tiltx;
						half4 grassPosition;
						half4 size;
						fixed2 uvDistortion, textureWaving;
						fixed4 displacement, displacementx;
						fixed4 bottomColor, topColor;

						grassPosition = 0;
						displacement = 0;
						displacementx = 0;
						size = 0;
						tilt = 0;
						tiltx = 0;
						bottomColor = 0;
						topColor = 0;
						uvDistortion = 0;
						textureWaving = 0;
						index = 0;

						fixed3 upNormal = fixed3(0, 1, 0);

						// Generate grass mesh
						if (GemerateGeometry(p[0], grassPosition, displacement, displacementx, size, tilt, tiltx, bottomColor, topColor, uvDistortion, textureWaving, index))
						{
							half dist = distance(grassPosition.xyz, fixed3(0, 0, 0));
							grassPosition.y -= _Yoffset[index] * size.y;

							half yaw = dot(normalize(grassPosition + size.y / 2), upNormal);

							// ************ Side ************
							if (tiltx <= 0.5 && tiltx >= -0.5)
							{
								half4 side = _Quads[index * 3 + 1];

								// Top vertices
								AppendVertex(triStream, grassPosition, displacement * ((0.5 - side.x) * 2) + half4(0, size.y, 0, 0), upNormal, fixed3((side.x * 0.5) + textureWaving.x, 0.5, index), topColor);
								AppendVertex(triStream, grassPosition, -displacement * ((side.y - 0.5) * 2) + half4(0, size.y, 0, 0), upNormal, fixed3((side.y * 0.5) + textureWaving.y, 0.5, index), topColor);

								// Bottom vertices
								AppendVertex(triStream, grassPosition, displacement * ((0.5 - side.x) * 2), upNormal, fixed3((side.x * 0.5), 0, index), bottomColor);
								AppendVertex(triStream, grassPosition, -displacement * ((side.y - 0.5) * 2), upNormal, fixed3((side.y * 0.5), 0, index), bottomColor);
							}
							else // ************ Front ************
							{
								//triStream.RestartStrip();
								half4 front = _Quads[index * 3];

								// Top vertices (crossed)
								AppendVertex(triStream, grassPosition, displacementx * ((0.5 - front.x) * 2) + half4(0, size.y, 0, 0), displacementx, fixed3((front.x * 0.5) + textureWaving.x, 1, index), topColor);
								AppendVertex(triStream, grassPosition, -displacementx * ((front.y - 0.5) * 2) + half4(0, size.y, 0, 0), -displacementx, fixed3((front.y * 0.5) + textureWaving.y, 1, index), topColor);

								// Bottom vertices (crossed)
								AppendVertex(triStream, grassPosition, displacementx * ((0.5 - front.x) * 2), displacementx, fixed3((front.x * 0.5), 0.5, index), bottomColor);
								AppendVertex(triStream, grassPosition, -displacementx * ((front.y - 0.5) * 2), -displacementx, fixed3((front.y * 0.5), 0.5, index), bottomColor);
							}

						}
					}

					half4 frag(FramentInput IN) : SV_Target
					{
						// Sample texture and multiply color
						fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, IN.texcoord);

					// Cutoff
					clip((c.a - _Cutoff));

					SHADOW_CASTER_FRAGMENT(IN)
				}
			ENDCG
			}
		}
}
