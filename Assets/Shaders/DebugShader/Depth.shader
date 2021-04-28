Shader "Render Depth"
{
	Properties
	{
		[MaterialToggle] _isToggled("Hue Toggle", Float) = 0
		_MainTex("Texture", 2D) = "white"
		_Distance("Draw Distance", float) = 2000.0
		_Min("Min depth", float) = 0.0
		_Max("Max depth", float) = 1.0
	}

		SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Cull Off
		//ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			// the depth texture
			sampler2D _MainTex;
			sampler2D _LastCameraDepthTexture;
			float _isToggled;
			float _Distance;

			float _Min;
			float _Max;

			//the object data that's put into the vertex shader
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			//the data that's used to generate fragments and can be read by the fragment shader
			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// the vertex shader
			v2f vert(appdata v)
			{
				v2f o;
				//convert the vertex positions from object space to clip space so they can be rendered
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			//the fragment shader
			fixed4 frag(v2f i) : SV_TARGET
			{
				// get depth from depth texture
				float depth = tex2D(_LastCameraDepthTexture, i.uv).r;
				// linear depth between camera and far clipping plane
				depth = Linear01Depth(depth);
				// change linear depth to between camera and DrawDistance
				float farClip = _ProjectionParams.z;

				depth = (depth * farClip) / _Distance;
				depth = depth > 1 ? 1 : depth;

				if (depth > _Max || depth < _Min)
				{
					return 1;
				}

				if (_isToggled == 1)
				{
					float2 uv = (0.5, depth);
					return tex2D(_MainTex, uv).rgba;
				}
				else
				{
					return depth;
				}
			}
		ENDCG
		}
	}
}