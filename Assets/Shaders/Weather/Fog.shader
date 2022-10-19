Shader "Weather/Fog"
{
	HLSLINCLUDE

	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
	//#include "UnityCG.cginc"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
	float _ViewDistance;
	float _Density;
	float _MinDensity;
	float _MaxDensity;
	float _CloudHeight;
	float4 _Color;
	float3 _Forward;
	float4x4 UnityWorldSpaceViewDir;

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
		float nonLinearDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord).r;
		float linearDepth = Linear01Depth(nonLinearDepth);

		float farPlane = _ProjectionParams.z;

		float3 dir = float3(i.texcoord.x - 0.5, i.texcoord.y - 0.5, 0);
		float3 coord = normalize(dir + _Forward);
		float mask = 1 - abs(dot(coord, float3(0, 1, 0)));

		float angle = pow(dot(_Forward, float3(0, 1, 0)), 2);
		angle = clamp(angle, 0, 0.5);

		float dist = linearDepth * farPlane;
		float depth = 1 - clamp(_ViewDistance / dist, 0, 1 );

		depth = clamp(depth + _MinDensity, 0, _MaxDensity) * (_Density * mask);
		return _Color * depth + (color * (1 - depth));
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment Frag

			ENDHLSL
		}
	}
}