Shader "Instanced/Trees/Opaque/Compute/Crossboard" 
{
	Properties
	{
		_TrunkColor("Color Trunk", Color) = (1,1,1,1)
		_RandomColor("Random Color Amount", Range(0,1)) = 1

		_MainTex("Texture", 2D) = "white" {}
		_Cutoff("Alpha Cutoff", Range(0,1)) = 0.6

		// TODO: move to a computeShader which send back a splatmap with the wind info.
		_TreeAmplitude("Wind Amplitude", Float) = 0.001
		_WindSpeed("Wind Speed", Float) = 40
		_WindStength("Wind Strength", Float) = 10

	}
	SubShader
	{
		Cull Off
		LOD 200

		Tags
		{
			"Queue" = "Geometry" 
			"RenderType" = "TransparentCutout" 
			"LightMode" = "ForwardBase" 
			//"LightMode" = "Deferred"
			//"IgnoreProjector" = "True" 
			//"DisableBatching" = "True"
		}
		Pass
		{
			//Name "FORWARD_BASE"

			Cull Off
			//ZWrite On
			Blend Off

			CGPROGRAM

			#include "UnityCG.cginc" 

			#define PI 3.1415926535897932384626433832795
			#include "Lighting.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom

			#pragma only_renderers d3d11
			#pragma target 5.0

			#include "Opaque_Crossboard_compute.cginc"

			ENDCG
		}
	}
}