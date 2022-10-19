Shader "Custom/Weather/Rain"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color (RGBA)", Color) = (1, 1, 1, 1) // add _Color property
        _size("Offset Texture", Float) = 2.0
        _speed("Texture scroll", Vector) = (0, 0, 0, 0)
        _offsetSpeed("Offset scroll", Vector) = (0,0,0,0)
        _intensity("intensity", Float) = 1.0
    }
        SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        //Cull front
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _MainTex_ST;
            float4 _speed;
            float4 _offsetSpeed;
            float _size;
            float _intensity;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col1 = tex2D(_MainTex, i.uv + _speed * _Time) * _intensity;
                fixed4 col2 = tex2D(_MainTex, i.uv * _size + _offsetSpeed * _Time) * _intensity;
                fixed4 col3 = tex2D(_MainTex, i.uv / _size + _speed * _Time / 2) * _intensity;

                fixed4 col = col1 * col2 * col3;
                col.a = col.r;
                return col * _Color;
            }
        ENDCG
    }
    }
}
