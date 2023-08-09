Shader "Unlit/Vector(Billboard)"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
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

					//OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
					//OUT.vertex = UnityObjectToClipPos(OUT.vertex);

					OUT.vertex = mul(UNITY_MATRIX_P,
						mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
						+ float4(IN.vertex.x, IN.vertex.y, 0.0, 0.0)
						* float4(1.0, 1.0, 1.0, 1.0));

					OUT.texcoord = IN.texcoord;


					// extract x and y scale from object to world transform matrix
					float2 scale = float2(
						length(unity_ObjectToWorld._m00_m10_m20),
						length(unity_ObjectToWorld._m01_m11_m21)
						);

					#ifdef UNITY_COLORSPACE_GAMMA
					fixed4 color = IN.color;
					#else
					fixed4 color = fixed4(GammaToLinearSpace(IN.color.rgb), IN.color.a);
					#endif

					OUT.color = color * _Color * _RendererColor;

					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex * scale);
					#endif

					return OUT;
				}
			ENDCG
			}
		}
}
