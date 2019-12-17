Shader "Terrain/DynamicTerrain/Normals" {
    Properties {
        _MainTex( "Heightmap", 2D ) = "white" {}
    }

    SubShader {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma only_renderers d3d11

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert ( appdata v ) {
                v2f o;
                o.vertex = UnityObjectToClipPos( v.vertex );
                o.uv = v.uv;
                return o;
            }

            sampler2D_float _MainTex;
            float4 _MainTex_TexelSize;
            float3 _TerrainSize;

            float2 frag( v2f i ) : SV_Target {
                // Sample heightmap
                float hb = tex2Dlod( _MainTex, float4( i.uv + float2( -1, +0 ) * _MainTex_TexelSize.xy, 0, 0 ) );
                float ht = tex2Dlod( _MainTex, float4( i.uv + float2( +1, +0 ) * _MainTex_TexelSize.xy, 0, 0 ) );
                float hr = tex2Dlod( _MainTex, float4( i.uv + float2( +0, +1 ) * _MainTex_TexelSize.xy, 0, 0 ) );
                float hl = tex2Dlod( _MainTex, float4( i.uv + float2( +0, -1 ) * _MainTex_TexelSize.xy, 0, 0 ) );

                // Compute (bi)tangent vectors
                float3 bitangent = float3( 0.0f, hr - hl, 2.0f * _MainTex_TexelSize.y * _TerrainSize.z );
                float3 tangent = float3( 2.0f * _MainTex_TexelSize.x * _TerrainSize.x, ht - hb, 0 );

                // Compute normal vector
                float3 normal = normalize( cross( bitangent, tangent ) );

                // Return normal vector
                return normal.xz;
            }

            ENDCG

        }
    }
}
