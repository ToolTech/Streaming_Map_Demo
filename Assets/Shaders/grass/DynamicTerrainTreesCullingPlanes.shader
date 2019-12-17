Shader "DynamicTerrain/Trees/CullingPlanes" {
    SubShader {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"

            int _ShadowCascade;

            struct v2f {
                float4 pos : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            // Fullscreen quad based on VertexID
            // Draws 1 "fullscreen" triangle, requires 3 vertices: DrawProcedural( MeshTopology.Triangles, 3 )
            v2f vert( uint id: SV_VertexID ) {
                v2f output;
                output.pos = float4( float2( ( id << 1 ) & 2, id & 2 ) * 2.0f - 1.0f, 0.0f, 1.0f );
                output.screenPos = output.pos;
                return output;
            }

            fixed4 frag( v2f input ) : SV_Target {
                float2 screenUV = ( input.screenPos.xy + 1 ) / 2;
                screenUV.y = 1 - screenUV.y;
                int2 pixelPos = screenUV * float2( 6.0f, 4.0f ); // (six planes,  four cascades)
                int planeIndex = pixelPos.x;
                int cascades = 3;

                if ( _ShadowCascade != pixelPos.y ) {
                    discard;
                }

                // TODO: extending near/far plane by the factor of 1.25 helps with "long" shadows but it should be computed somehow better
                float4 plane = unity_CameraWorldClipPlanes[ planeIndex ];
                // if ( planeIndex >= 4 ) {
                //     float center = unity_CameraWorldClipPlanes[ 4 ].w + ( - unity_CameraWorldClipPlanes[ 5 ].w - unity_CameraWorldClipPlanes[ 4 ].w ) / 2.0f;
                //     plane.w = center + 1.25f * ( plane.w - center );
                // }

                return plane;
            }

            ENDCG

        }
    }
}
