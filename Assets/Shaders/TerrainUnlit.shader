Shader "Unlit/TerrainUnlit"
{
    Properties
    {
        _MainTex("First Texture", 2D) = "white" {}
        _SecondTex("Second Texture", 2D) = "white" {}
        _ThirdTex("Third Texture", 2D) = "white" {}
        _FadeMain("Fade Main Value", Range(0, 1)) = 0
        _FadeSecond("Fade Second  Value", Range(0, 1)) = 0
        _FadeThird("Fade Third Value", Range(0, 1)) = 0
        _Alpha("Alpha Value", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest+100" "RenderType"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha        

        // Front Pass
        Pass
        {
            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
        // make fog work
        #pragma multi_compile_fog

        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            UNITY_FOG_COORDS(1)
            float4 vertex : SV_POSITION;
        };

        sampler2D _MainTex;
        sampler2D _SecondTex;
        sampler2D _ThirdTex;

        float _FadeMain;
        float _FadeSecond;
        float _FadeThird;

        float _Alpha;

        float4 _MainTex_ST;

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            UNITY_TRANSFER_FOG(o,o.vertex);
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            // sample the texture
            fixed4 col01 = tex2D(_MainTex, i.uv) * _FadeMain;
            fixed4 col02 = tex2D(_SecondTex, i.uv) * _FadeSecond;
            fixed4 col03 = tex2D(_ThirdTex, i.uv) * _FadeThird;
            // apply fog
            fixed4 col = col01 + col02 + col03;
            UNITY_APPLY_FOG(i.fogCoord, col);
            return float4(col.rgb, _Alpha);
        }
        ENDCG
    }

           
        // Back Pass
        Pass
        {
            ZWrite Off
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SecondTex;
            sampler2D _ThirdTex;

            float _FadeMain;
            float _FadeSecond;
            float _FadeThird;

            float _Alpha;

            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col01 = tex2D(_MainTex, i.uv) * _FadeMain;
                fixed4 col02 = tex2D(_SecondTex, i.uv) * _FadeSecond;
                fixed4 col03 = tex2D(_ThirdTex, i.uv) * _FadeThird;
                // apply fog
                fixed4 col = col01 + col02 + col03;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return float4(col.rgb, _Alpha);
            }
            ENDCG
        }
    }
}
