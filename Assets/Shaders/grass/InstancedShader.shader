Shader "Instanced/InstancedShader" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_FadeFar("fade Far value", float) = 50
		_FadeNear("fade Near value", float) = 20
		_FadeDistance("Fade Distance", float) = 50
	}
		SubShader{

			Pass {

			//Tags {"LightMode" = "Deferred"}

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#pragma target 4.5

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "AutoLight.cginc"

			sampler2D _MainTex;
			half _FadeFar;
			half _FadeNear;
			half _FadeDistance;

		#if SHADER_TARGET >= 45
			StructuredBuffer<float4> _Buffer;
		#endif

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 worldPos : TEXCOORD4;
				float2 uv_MainTex : TEXCOORD0;
				float3 ambient : TEXCOORD1;
				float3 diffuse : TEXCOORD2;
				float3 color : TEXCOORD3;
				SHADOW_COORDS(4)
			};

			void rotate2D(inout float2 v, float r)
			{
				float s, c;
				sincos(r, s, c);
				v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
			}

			v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
			{
			#if SHADER_TARGET >= 45
				float4 data = _Buffer[instanceID];
			#else
				float4 data = 0;
			#endif

				/*float rotation = data.w * data.w * _Time.x * 0.5f;
				rotate2D(data.xz, rotation);*/

				float3 localPosition = v.vertex.xyz;
				float3 worldPosition = data.xyz + localPosition;
				float3 worldNormal = v.normal;



				float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
				float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
				float3 diffuse = (ndotl * _LightColor0.rgb);
				float3 color = v.color;

				v2f o;
				o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
				o.worldPos = float4(worldPosition, 1.0f);
				o.uv_MainTex = v.texcoord;
				o.ambient = ambient;
				o.diffuse = diffuse;
				o.color = color;
				TRANSFER_SHADOW(o)
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				const half4x4 thresholdMatrix =
				{
					16, 8, 14, 6,
					4, 12, 2, 10,
					13, 5, 15, 7,
					1, 9, 3, 11
				};

				fixed threshold = thresholdMatrix[i.pos.x % 4][i.pos.y % 4] / 17;
				half dist = distance(i.worldPos.xyz, fixed3(0, 0, 0));

				half fade = _FadeFar;

				if (dist > fade)
				{
					fade = 1 - ((dist - fade) / _FadeDistance);
					//fade = dist/fade;
				}
				if (dist < _FadeNear)
				{
					fade = dist / _FadeNear;
				}

				if (threshold >= fade)
				{
					discard;
				}

				fixed shadow = SHADOW_ATTENUATION(i);
				fixed4 albedo = tex2D(_MainTex, i.uv_MainTex);
				float3 lighting = i.diffuse * shadow + i.ambient;
				fixed4 output = fixed4(albedo.rgb * i.color * lighting, albedo.w);
				UNITY_APPLY_FOG(i.fogCoord, output);
				return output;
			}

			ENDCG
		}
		}
}