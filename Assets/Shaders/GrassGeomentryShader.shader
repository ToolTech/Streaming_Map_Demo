// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Low Poly Shader developed as part of World of Zero: http://youtube.com/worldofzerodevelopment
// Based upon the example at: http://www.battlemaze.com/?p=153

Shader "Custom/Grass Geometry Shader" {
	Properties{
		_BaseColor("Base Color", Color) = (1,0,0,1)
		_SecondaryColor("Secondary Color", Color) = (0,0,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		[Toggle(FILL_WITH_RED)]
		_Billbord("Billbord", Float) = 0
		[Toggle(Billbord)]
		_Plane("Plane", Float) = 0
		[Toggle(Lod)]
		_UseLod("Use Lod", Float) = 0
		_LodFadeOut("fade Out distance", Float) = 200
		_LodOut("Lod Out distance", Float) = 300
		_Cutoff("Cutoff", Range(0,1)) = 0.25
		_GrassHeight("Grass Height", Float) = 0.5
		_GrassWidth("Grass Width", Float) = 0.5
		_RandomWidth("Random Width Amount", Range(0,1)) = 0.4
		_RandomHeight("Random Height Amount", Range(0,1)) = 0.3
		_RandomColor("Random Color Amount", Range(0,1)) = 0.4
		_AngleAmount("Angle Amount", Range(0,1)) = 0.4
		_WindSpeed("Wind Speed", Float) = 100
		_WindStength("Wind Strength", Float) = 0.05
	}
		SubShader{
		Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" "LightMode" = "ForwardBase"}
		LOD 200
		Cull Off

		Pass
		{
			
			//Cull Back

			CGPROGRAM
			#define PI 3.1415926535897932384626433832795
			#include "UnityCG.cginc" 
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			//#pragma alphatest : _Cutoff

			// Use shader model 4.0 target, we need geometry shader support
			#pragma target 5.0

			sampler2D _MainTex;

			struct v2g
			{
				float4 pos : SV_POSITION;
				float3 norm : NORMAL;
				float2 uv : TEXCOORD0;
				//float3 color : TEXCOORD1;
				float4 color: TEXCOORD1;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float3 norm : NORMAL;
				float2 uv : TEXCOORD0;
				float4 diffuseColor : TEXCOORD1;

				fixed3 diff : COLOR0;
				fixed3 ambient : COLOR1;
				//float3 specularColor : TEXCOORD2;
			};

			fixed4 _BaseColor;
			fixed4 _SecondaryColor;
			bool _Billbord;
			bool _Plane;
			bool _UseLod;
			half _RandomWidth;
			half _RandomHeight;
			half _RandomColor;
			half _GrassHeight;
			half _GrassWidth;
			half _Cutoff;
			half _AngleAmount;
			half _WindStength;
			half _WindSpeed;
			half _LodOut;
			half _LodFadeOut;

			v2g vert(appdata_full v)
			{
				//float3 base = v.vertex.xyz;
				v2g OUT;
				OUT.pos = v.vertex;
				OUT.norm = v.normal;
				OUT.uv = v.texcoord;
				OUT.color = v.color;
				//OUT.color = tex2Dlod(_MainTex, v.texcoord).rgb;
				return OUT;
			}

			float3 rotate(float degress, float3 vec)
			{
				float3x3 rotMatrix = float3x3(cos(degress), 0, sin(degress), 0, 1, 0, -sin(degress), 0, cos(degress));
				return mul(rotMatrix, vec);
			}

			void GenerateQuad(inout TriangleStream<g2f> tristream, float3 points[4], float4 color, float2 uv[4], float3 norm)
			{
				g2f OUT;
				float3 faceNormal = cross(points[1] - points[0], points[2] - points[0]);
				for (int i = 0; i < 4; i++)
				{
					OUT.pos = UnityObjectToClipPos(points[i]);
					OUT.norm = faceNormal;
					OUT.diffuseColor = color;
					OUT.uv = uv[i];

					// ---------------- Light ----------------
					half3 worldNormal = UnityObjectToWorldNormal(norm);
					half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					OUT.diff = nl * _LightColor0.rgb;
					OUT.ambient = ShadeSH9(half4(worldNormal, 1));

					tristream.Append(OUT);
				}
				tristream.RestartStrip();
			}

			[maxvertexcount(24)]
			void geom(point v2g IN[1], inout TriangleStream<g2f> triStream)
			{
				float dist = length(ObjSpaceViewDir(IN[0].pos));
				float4 color = IN[0].color;

				if (dist < _LodOut || !_UseLod)
				{

					if (dist > _LodFadeOut && _UseLod)
					{
						float fade = (_LodOut - dist) / (_LodOut - _LodFadeOut);
						color.a = fade;
					}

					float3 lightPosition = _WorldSpaceLightPos0;

					// ---------------- Camera Vectors ----------------
					float3 rightdir = UNITY_MATRIX_IT_MV[0].xyz;
					float3 updir = UNITY_MATRIX_IT_MV[1].xyz;
					float3 forwarddir = UNITY_MATRIX_IT_MV[2].xyz;
					float3 camPos = _WorldSpaceCameraPos.xyz;

					float3 norm = IN[0].norm;

					if (_Billbord == 0)
					{
						updir = norm;
					}


					half height = _GrassHeight * (1 + (_RandomHeight * color.g));
					half width = _GrassWidth * (1 + (_RandomWidth * color.g));
					half angle = color.b;

					float3 rot = (angle * 180);

					float3 perpendicularAngle = rotate(rot, float3(0, 0, 1));
					float3 faceNormal = cross(perpendicularAngle, norm);

					float3 base = IN[0].pos.xyz;
					float3 top = IN[0].pos.xyz + updir * height;

					// basic wind

					float3 wind = float3(sin(_Time.x * _WindSpeed + base.x) + sin(_Time.x * _WindSpeed + base.z * 2) + sin(_Time.x * _WindSpeed * 0.1 + base.x), 0,
						cos(_Time.x * _WindSpeed + base.x * 2) + cos(_Time.x * _WindSpeed + base.z));
					top += wind * _WindStength;

					// TODO: make better names
					float3 rot90 = rotate(PI / 1.3, perpendicularAngle);
					float3 rot60 = rotate(PI/4, perpendicularAngle);
					float3 rot30 = rotate(PI/1.6, perpendicularAngle);

					g2f OUT;

					// ---------------- get angle ----------------
					float3 dir = norm;
					float pitch = dot(dir, forwarddir);

					dir = perpendicularAngle;
					float yawRight = dot(dir, forwarddir);

					dir = cross(norm, perpendicularAngle);
					float yawLeft = dot(dir, forwarddir);



					float2 uv[4] = { float2(1,0), float2(0,0), float2(1,1), float2(0,1) };

					if (_Plane == 0)
					{

						// ---------------- forward quad ----------------
						float3 forwardQuad[4] = { base + perpendicularAngle * 0.5 * width,
								base - perpendicularAngle * 0.5 * width,
								(top + rot30 * _AngleAmount) + perpendicularAngle * 0.5 * width,
								(top + rot30 * _AngleAmount) - perpendicularAngle * 0.5 * width };

						GenerateQuad(triStream, forwardQuad, color, uv, norm);

						// ---------------- quad 60 ----------------
						float3 quad1[4] = { base + rot60 * 0.5 * width,
								base - rot60 * 0.5 * width,
								(top - rot90 * _AngleAmount) + rot60 * 0.5 * width,
								(top - rot90 * _AngleAmount) - rot60 * 0.5 * width };

						GenerateQuad(triStream, quad1, color, uv, norm);

						// ---------------- quad 30 ----------------
						float3 quad2[4] = { base + rot30 * 0.5 * width,
								base - rot30 * 0.5 * width,
								(top - perpendicularAngle * _AngleAmount) + rot30 * 0.5 * width,
								(top - perpendicularAngle * _AngleAmount) - rot30 * 0.5 * width };

						GenerateQuad(triStream, quad2, color, uv, norm);
					}
					else
					{
						// ---------------- forward quad ----------------
						float3 Quad[4] = { base + perpendicularAngle * 0.5 * width,
								base - perpendicularAngle * 0.5 * width,
								top + perpendicularAngle * 0.5 * width,
								top - perpendicularAngle * 0.5 * width };

						GenerateQuad(triStream, Quad, color, uv, norm);
					}
				}
			}

			half4 frag(g2f IN) : COLOR
			{
				fixed4 c = tex2D(_MainTex, IN.uv);
				clip((c.a * IN.diffuseColor.a) - _Cutoff);

				fixed4 lighting = float4(IN.diff + IN.ambient, 1.0);



				return (c * _BaseColor) * lighting + (c * _SecondaryColor * (IN.diffuseColor.r * _RandomColor));
			}

			ENDCG

			}
		}
}