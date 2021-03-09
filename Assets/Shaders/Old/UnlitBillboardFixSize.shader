Shader "Unlit/Vector(BillboardFixedSize)"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (-1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			Pass
			{
			CGPROGRAM
				#pragma vertex VectorVert
				#pragma fragment SpriteFrag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile _ PIXELSNAP_ON
				#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
				#include "UnitySprites.cginc"

				v2f VectorVert(appdata_t IN)
				{
					v2f OUT;

					UNITY_SETUP_INSTANCE_ID(IN);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

					// extract world pivot position from object to world transform matrix
					float3 worldPos = unity_ObjectToWorld._m03_m13_m23;

					// extract x and y scale from object to world transform matrix
					float2 scale = float2(
						length(unity_ObjectToWorld._m00_m10_m20),
						length(unity_ObjectToWorld._m01_m11_m21)
						);

					// transform pivot position into view space
					float4 viewPos = mul(UNITY_MATRIX_V, float4(worldPos, 1.0));

					// Adjust scale by distance
					float z = -viewPos.z;
					scale = lerp(scale, scale * 0.25, clamp(z / 5000, 0, 1));

					// apply transform scale to xy vertex positions
					float2 vertex = IN.vertex.xy * scale;

					// multiply by view depth for constant view size scaling
					vertex *= -viewPos.z;


					// divide by perspective projection matrix [1][1] if you don't want camera FOV to displayed size
					// the * 0.5 is to make a default quad with a scale of 1 be exactly the height of the view

					vertex /= UNITY_MATRIX_P._m11 * float2(-0.5, 0.5);

					// along with the perspective projection divide by screen height if you want the scale to be in screen pixels
					// vertex /= _ScreenParams.y;


					// add vertex positions to view position pivot
					viewPos.xy += vertex;

					// transform into clip space
					OUT.vertex = mul(UNITY_MATRIX_P, viewPos);

					OUT.texcoord = IN.texcoord;

					#ifdef UNITY_COLORSPACE_GAMMA
					fixed4 color = IN.color;
					#else
					fixed4 color = fixed4(GammaToLinearSpace(IN.color.rgb), IN.color.a);
					#endif

					OUT.color = color * _Color * _RendererColor;

					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}
			ENDCG
			}
		}
}
