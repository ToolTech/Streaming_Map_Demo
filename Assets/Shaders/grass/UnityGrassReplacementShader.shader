Shader "Hidden/TerrainEngine/Details/BillboardWavingDoublePass" {
    Properties {
        _WavingTint( "Fade Color", Color ) = ( .7,.6,.5, 0 )
        _MainTex( "Albedo (RGB)", 2D ) = "white" {}
        _WaveAndDistance( "Wave and distance", Vector ) = ( 12, 3.6, 1, 1 )
        _Cutoff( "Cutoff", float ) = 0.38
        _ColorIntensity( "Color Intensity", Range( 0, 1 ) ) = 1
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
        }

        Cull Off

        CGPROGRAM

        #pragma surface surf StandardSpecular vertex:vert alphatest:_Cutoff 
        #include "TerrainEngine.cginc"

        sampler2D _MainTex;
        float _ColorIntensity;
        
        void vert( inout appdata_full v ) {
            float4 color = v.color;
            WavingGrassBillboardVert( v );
            v.color = color;
        }


        struct Input {
            float2 uv_MainTex;
            fixed4 color : COLOR;
        };

        void surf( Input IN, inout SurfaceOutputStandardSpecular o ) {
            fixed4 c = tex2D( _MainTex, IN.uv_MainTex );
            o.Albedo = c.rgb * IN.color.rgb;
            o.Smoothness = 0;
            o.Specular = 0;
            o.Alpha = c.a;
        }

    ENDCG

    }
}