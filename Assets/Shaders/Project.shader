Shader "Custom/ProjectorAdditiveTint" 
{
    Properties
    {
        _Color("Tint Color", Color) = (1,1,1,1)
        _ShadowTex("Cookie", 2D) = "gray" {}
    }

    Subshader
    {
            Tags {"Queue" = "Transparent+10" "RenderType" = "Transparent"}


            Pass 
            {
                //ZWrite Off
                //Offset -1, -1
                ColorMask RGB
                Blend SrcAlpha One // Additive blending

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct v2f 
                {
                    float4 uvShadow : TEXCOORD0;
                    float4 pos : SV_POSITION;
                };

                float4x4 unity_Projector;
                float4x4 unity_ProjectorClip;

                v2f vert(float4 vertex : POSITION)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(vertex);
                    o.uvShadow = mul(unity_Projector, vertex);
                    return o;
                }

                sampler2D _ShadowTex;
                fixed4 _Color;

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 texCookie = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
                    clip(1 + i.uvShadow.z);
                    clip(1 - i.uvShadow.z);
                    return _Color * texCookie.a;
                }
                ENDCG
            }
    }
}