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
// File			: Opaque_Crossboard_compute.shader
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

Shader "Instanced/Trees/Opaque/Compute/Crossboard" 
{
	Properties
	{
		_TrunkColor("Color Trunk", Color) = (1,1,1,1)
		_RandomColor("Random Color Amount", Range(0,1)) = 1

		_MainTex("Texture", 2D) = "white" {}
		_Cutoff("Alpha Cutoff", Range(0,2)) = 0.6

		[Toggle(LOD)]
		_UseLod("Use Lod", Float) = 1

		// settings for the distance to lod out a object is based on its size.
		// t.ex. a obeject with size 1 will lod out at a distance of 200 m (with the settings below)
		// and a object with size 10 will instead lod out at a distance of 2000m.
		_LodFar("Far LodOut Distance", Float) = 6000
		_LodNear("Near LodOut Distance", Float) = 0 //100  

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
			"Queue" = "AlphaTest" 
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
			
		//	Blend SrcAlpha OneMinusSrcAlpha

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