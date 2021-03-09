
#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

	#include "UnityPBSLighting.cginc"
	#include "AutoLight.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float4 worldPos : TEXCOORD1;
		float3 normal : TEXCOORD2;

		#if defined(BINORMAL_PER_FRAGMENT)
			float4 tangent : TEXCOORD3;
		#else
			float3 tangent : TEXCOORD3;
			float3 binormal : TEXCOORD4;
		#endif

		UNITY_SHADOW_COORDS(5)

		#if defined(VERTEXLIGHT_ON)
			float3 vertexLightColor : TEXCOORD6;
		#endif
	};

	void ComputeVertexLightColor(inout v2f i)
	{
		#if defined(VERTEXLIGHT_ON)
			i.vertexLightColor = Shade4PointLights
			(
				unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
				unity_LightColor[0].rgb, unity_LightColor[1].rgb,
				unity_LightColor[2].rgb, unity_LightColor[3].rgb,
				unity_4LightAtten0, i.worldPos, i.normal
			);
		#endif
	}

	float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign) 
	{
		return cross(normal, tangent.xyz) * (binormalSign * unity_WorldTransformParams.w);
	}

	v2f vert(appdata v)
	{
		v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		o.uv = v.uv;

		o.pos = UnityObjectToClipPos(v.vertex);
		o.worldPos = mul(unity_ObjectToWorld, v.vertex);

		o.normal = UnityObjectToWorldNormal(v.normal);

		#if defined(BINORMAL_PER_FRAGMENT)
			o.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
		#else
			o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
			o.binormal = CreateBinormal(o.normal, o.tangent, v.tangent.w);
		#endif

		UNITY_TRANSFER_SHADOW(o, v.uv);
		ComputeVertexLightColor(o);
		return o;
	}

	sampler2D _MainTex;
	sampler2D _PlacementTex;
	sampler2D _NormalMap;

	UNITY_DECLARE_TEX2DARRAY(_DetailTexs);
	UNITY_DECLARE_TEX2DARRAY(_NormalTexs);

	float BumpDetail[10];

	float4 _SpecularTint;
	float _Smoothness;
	float _Metallic;
	float _BumpScale;
	float _FadeAmount;
	float _Fade;
	float2 _Tiling;
	float2 _DefaultTiling;

	float FadeAmount(float4 worldPos)
	{
		float dist = distance(worldPos, _WorldSpaceCameraPos);
		float fade = 0;

		if (dist > _Fade && dist < (_Fade + _FadeAmount))
			fade = ((dist - _Fade) / _FadeAmount);
		else if (dist > (_Fade + _FadeAmount))
			fade = 1;

		return fade;
	}

	void InitializeFragmentNormal(inout v2f i, float3 normal)
	{
		float3 tangentSpaceNormal = normal;

		//float3 binormal = cross(i.normal, i.tangent.xyz) * (i.tangent.w * unity_WorldTransformParams.w);

		#if defined(BINORMAL_PER_FRAGMENT)
			float3 binormal = cross(i.normal, i.tangent.xyz) * (i.tangent.w * unity_WorldTransformParams.w);
			//float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
		#else
			float3 binormal = i.binormal;
		#endif

		i.normal = normalize
		(
			tangentSpaceNormal.x * i.tangent +
			tangentSpaceNormal.y * binormal +
			tangentSpaceNormal.z * i.normal
		);
	}

	float4 DetailTexturesMix(v2f i, out float3 normal, out bool undefined)
	{
		undefined = false;
		fixed4 placement = tex2D(_PlacementTex, i.uv);

		// *************************************************************** //
		uint value = (uint)(placement.x * 255) | (uint)(placement.y * 255) << 8 | (uint)(placement.z * 255) << 16;

		bool Grassland = (value >> 0) & 1 || (value >> 1) & 1;
		bool Buildings = (value >> 2) & 1 || (value >> 3) & 1;
		bool vegetation = (value >> 4) & 1 || (value >> 5) & 1;
		bool Water = (value >> 6) & 1 || (value >> 7) & 1;

		bool DirtRoad = (value >> 8) & 1 || (value >> 9) & 1;
		bool Barren = (value >> 10) & 1 || (value >> 11) & 1;
		bool ManMadeGround = (value >> 12) & 1 || (value >> 13) & 1;
		bool AsphaltRoad = (value >> 14) & 1 || (value >> 15) & 1;

		bool Railway = (value >> 16) & 1 || (value >> 17) & 1;
		bool Runway = (value >> 18) & 1 || (value >> 19) & 1;
		bool Crops = (value >> 20) & 1 || (value >> 21) & 1;

		// *************************************************************** //

		float2 uv = i.uv * _Tiling;

		// *************************************************************** //

		// Get all indexces to mix
		normal = float4(1, 1, 1, 1);
		float totalNormal = 0;
		float maxNormal = -1;
		int returnindex = 0;
		float3 defaultNormal = tex2D(_NormalMap, i.uv * _DefaultTiling).xyz;
		float3 terrainNormal = defaultNormal;

		for (uint x = 0; x <= 20; x += 2)
		{
			if ((value >> x) & 1 || (value >> x + 1) & 1)
			{
				uint index = x / 2;
				fixed4 norm = UNITY_SAMPLE_TEX2DARRAY(_NormalTexs, float3(uv % 1, index));
				float normLength = dot(norm, terrainNormal);

				if (maxNormal < normLength)
				{
					maxNormal = normLength;
					returnindex = index;
				}

				totalNormal += normLength;
			}
		}

		if (totalNormal == 0)
			undefined = true;

		float fade = FadeAmount(i.worldPos);

		float4 returnNormal = float4(defaultNormal.xyz, 1);
		normal = UnpackScaleNormal(returnNormal, _BumpScale);
		float4 returncolor = UNITY_SAMPLE_TEX2DARRAY(_DetailTexs, float3(uv % 1, returnindex));

		if (!undefined)
		{
			float4 normalMix = UNITY_SAMPLE_TEX2DARRAY(_NormalTexs, float3(uv % 1, returnindex));
			float bump = BumpDetail[returnindex];
			normalMix.xyz = UnpackScaleNormal(normalMix, bump * (1 - fade));

			float3 blend = BlendNormals(normal.xyz, normalMix.xyz);

		#ifdef BLEND
			normal = normalMix.xyz;
		#else
			normal = blend;
		#endif
		}

		return returncolor;
	}

	UnityIndirect CreateIndirectLight(v2f i, float3 viewDir)
	{
		UnityIndirect indirectLight;
		indirectLight.diffuse = 0;
		indirectLight.specular = 0;

		#if defined(VERTEXLIGHT_ON)
			indirectLight.diffuse = i.vertexLightColor;
		#endif

		#if defined(FORWARD_BASE_PASS)
			indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
			float3 reflectionDir = reflect(-viewDir, i.normal);
			float4 envSample = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflectionDir);
			indirectLight.specular = DecodeHDR(envSample, unity_SpecCube0_HDR);
		#endif

		return indirectLight;
	}

	UnityLight CreateLight(v2f i)
	{
		UnityLight light;

		#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
			light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
		#else
			light.dir = _WorldSpaceLightPos0.xyz;
		#endif

		UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz);

		light.color = _LightColor0.rgb * attenuation;
		light.ndotl = DotClamped(i.normal, light.dir);

		return light;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		float3 normal;
		bool undefined;
		fixed4 detail = DetailTexturesMix(i, normal, undefined);

		// *************************************************************** //

		fixed4 col = tex2D(_MainTex, i.uv);
		float fade = FadeAmount(i.worldPos);
		fixed4 result = col;

		if (!undefined)
		{
			result = (col * fade + detail * (1 - fade));
		}

		InitializeFragmentNormal(i, normal);

		// *************************************************************** //

		float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

		float3 specularTint;
		float oneMinusReflectivity;
		float3 albedo = DiffuseAndSpecularFromMetallic(result.rgb, _Metallic, specularTint, oneMinusReflectivity);

		return UNITY_BRDF_PBS(
			albedo, specularTint,
			oneMinusReflectivity, _Smoothness,
			i.normal, viewDir,
			CreateLight(i), CreateIndirectLight(i, viewDir)
		);
	}
#endif