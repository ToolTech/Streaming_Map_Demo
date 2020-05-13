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
// File			: Blur.shader
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
//******************************************************************************

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Blur"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		// Horizontal
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float2 _BlurSize;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 s = tex2D(_MainTex, i.uv) * 0.38774;
				s += tex2D(_MainTex, i.uv + float2(_BlurSize.x * 2, 0)) * 0.06136;
				s += tex2D(_MainTex, i.uv + float2(_BlurSize.x, 0)) * 0.24477;
				s += tex2D(_MainTex, i.uv + float2(_BlurSize.x * -1, 0)) * 0.24477;
				s += tex2D(_MainTex, i.uv + float2(_BlurSize.x * -2, 0)) * 0.06136;

				return s;
			}
			ENDCG
		}

		// Vertical
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float2 _BlurSize;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 s = tex2D(_MainTex, i.uv) * 0.38774;
				s += tex2D(_MainTex, i.uv + float2(0, _BlurSize.y * 2)) * 0.06136;
				s += tex2D(_MainTex, i.uv + float2(0, _BlurSize.y)) * 0.24477;			
				s += tex2D(_MainTex, i.uv + float2(0, _BlurSize.y * -1)) * 0.24477;
				s += tex2D(_MainTex, i.uv + float2(0, _BlurSize.y * -2)) * 0.06136;

				return s;
			}
			ENDCG
		}
	}
}
