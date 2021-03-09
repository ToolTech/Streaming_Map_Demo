
Shader "Custom/Terrain"
{
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		[NoScaleOffset] _NormalMap("Default Normal", 2D) = "bump" {}
		_DefaultTiling("Default Tiling + Offset", Vector) = (30,30,0,0)
		[NoScaleOffset] _PlacementTex("Placement", 2D) = "white" {}

		_BumpScale("Bump Scale", Float) = 0.6
		_Tiling("Detail Tiling + Offset", Vector) = (200,200,0,0)
		_Smoothness("Smoothness", Range(0, 1)) = 0.1
		[Gamma] _Metallic("Metallic", Range(0, 1)) = 0.0

		_Fade("Fade", float) = 10
		_FadeAmount("Fade Amount", float) = 10

		[Toggle(BLEND)]
		_Blend("Don't Blend with terrain", Float) = 0

		[NoScaleOffset] _DetailTexs("Detail Textures", 2DArray) = "white" {}
		[NoScaleOffset] _NormalTexs("Normal Textures", 2DArray) = "bump" {}
	}
		SubShader
	{
		Pass
		{
			Tags 
			{
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM

			// Compiler definitions
			#pragma target 3.0

			//#pragma multi_compile_fwdbase
			#pragma multi_compile _ SHADOWS_SCREEN
			#pragma multi_compile _ VERTEXLIGHT_ON

			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature BLEND

			#define FORWARD_BASE_PASS
			#define BINORMAL_PER_FRAGMENT

			#include "TerrainPass.cginc"

			ENDCG
		}

		Pass
		{
			Tags
			{
				"LightMode" = "ForwardAdd"
			}

			Blend One One
			ZWrite Off

			CGPROGRAM

			// Compiler definitions
			#pragma target 3.0

			//#pragma multi_compile DIRECTIONAL POINT SPOT
			#pragma multi_compile_fwdadd_fullshadows

			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature BLEND

			#define BINORMAL_PER_FRAGMENT

			#include "TerrainPass.cginc"

			ENDCG
		}

		Pass 
		{
			Tags 
			{
				"LightMode" = "ShadowCaster"
			}

			//ZWrite Off

			CGPROGRAM

			#pragma target 3.0

			#pragma multi_compile_shadowcaster

			#pragma vertex MyShadowVertexProgram
			#pragma fragment MyShadowFragmentProgram

			#if !defined(MY_SHADOWS_INCLUDED)
			#define MY_SHADOWS_INCLUDED

			#include "UnityCG.cginc"

			struct VertexData 
			{
				float4 position : POSITION;
				float3 normal : NORMAL;
			};

				#if defined(SHADOWS_CUBE)
					struct Interpolators 
					{
						float4 position : SV_POSITION;
						float3 lightVec : TEXCOORD0;
					};

					Interpolators MyShadowVertexProgram(VertexData v) 
					{
						Interpolators i;
						i.position = UnityObjectToClipPos(v.position);
						i.lightVec = mul(unity_ObjectToWorld, v.position).xyz - _LightPositionRange.xyz;
						return i;
					}

					float4 MyShadowFragmentProgram(Interpolators i) : SV_TARGET
					{
						float depth = length(i.lightVec) + unity_LightShadowBias.x;
						depth *= _LightPositionRange.w;
						return UnityEncodeCubeShadowDepth(depth);
					}
				#else
					float4 MyShadowVertexProgram(VertexData v) : SV_POSITION 
					{
						float4 position = UnityClipSpaceShadowCasterPos(v.position.xyz, v.normal);
						return UnityApplyLinearShadowBias(position);
					}

					half4 MyShadowFragmentProgram() : SV_TARGET 
					{
						return 0;
					}
				#endif
			#endif

			ENDCG
		}

	}
	//Fallback "Diffuse"
}
