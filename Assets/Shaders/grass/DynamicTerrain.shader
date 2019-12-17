Shader "Terrain/DynamicTerrain" {
    Properties{
        [HideInInspector] _Control0( "Control0 (RGBA)", 2D ) = "red" {}
        [HideInInspector] _Control1( "Control1 (RGBA)", 2D ) = "black" {}
        [HideInInspector] _Splat0( "Layer 0 (R)", 2D ) = "white" {}
        [HideInInspector] _Splat1( "Layer 1 (G)", 2D ) = "white" {}
        [HideInInspector] _Splat2( "Layer 2 (B)", 2D ) = "white" {}
        [HideInInspector] _Splat3( "Layer 3 (A)", 2D ) = "white" {}
        [HideInInspector] _Splat4( "Layer 4 (R)", 2D ) = "white" {}
        [HideInInspector] _Splat5( "Layer 5 (G)", 2D ) = "white" {}
        [HideInInspector] _Splat6( "Layer 6 (B)", 2D ) = "white" {}
        [HideInInspector] _Splat7( "Layer 7 (A)", 2D ) = "white" {}
        [HideInInspector] _Normal0( "Normal 0 (R)", 2D ) = "bump" {}
        [HideInInspector] _Normal1( "Normal 1 (G)", 2D ) = "bump" {}
        [HideInInspector] _Normal2( "Normal 2 (B)", 2D ) = "bump" {}
        [HideInInspector] _Normal3( "Normal 3 (A)", 2D ) = "bump" {}
        [HideInInspector] _Normal4( "Normal 4 (R)", 2D ) = "bump" {}
        [HideInInspector] _Normal5( "Normal 5 (G)", 2D ) = "bump" {}
        [HideInInspector] _Normal6( "Normal 6 (B)", 2D ) = "bump" {}
        [HideInInspector] _Normal7( "Normal 7 (A)", 2D ) = "bump" {}
        [HideInInspector][Gamma] _Metallic0( "Metallic 0", Range( 0.0, 1.0 ) ) = 0.0
        [HideInInspector][Gamma] _Metallic1( "Metallic 1", Range( 0.0, 1.0 ) ) = 0.0
        [HideInInspector][Gamma] _Metallic2( "Metallic 2", Range( 0.0, 1.0 ) ) = 0.0
        [HideInInspector][Gamma] _Metallic3( "Metallic 3", Range( 0.0, 1.0 ) ) = 0.0
        [HideInInspector][Gamma] _Metallic4( "Metallic 4", Range( 0.0, 1.0 ) ) = 0.0
        [HideInInspector][Gamma] _Metallic5( "Metallic 5", Range( 0.0, 1.0 ) ) = 0.0
        [HideInInspector][Gamma] _Metallic6( "Metallic 6", Range( 0.0, 1.0 ) ) = 0.0
        [HideInInspector][Gamma] _Metallic7( "Metallic 7", Range( 0.0, 1.0 ) ) = 0.0
        [HideInInspector] _Smoothness0( "Smoothness 0", Range( 0.0, 1.0 ) ) = 1.0
        [HideInInspector] _Smoothness1( "Smoothness 1", Range( 0.0, 1.0 ) ) = 1.0
        [HideInInspector] _Smoothness2( "Smoothness 2", Range( 0.0, 1.0 ) ) = 1.0
        [HideInInspector] _Smoothness3( "Smoothness 3", Range( 0.0, 1.0 ) ) = 1.0
        [HideInInspector] _Smoothness4( "Smoothness 4", Range( 0.0, 1.0 ) ) = 1.0
        [HideInInspector] _Smoothness5( "Smoothness 5", Range( 0.0, 1.0 ) ) = 1.0
        [HideInInspector] _Smoothness6( "Smoothness 6", Range( 0.0, 1.0 ) ) = 1.0
        [HideInInspector] _Smoothness7( "Smoothness 7", Range( 0.0, 1.0 ) ) = 1.0
    }
    SubShader {
        Tags {
            "Queue" = "Geometry+500"
            "RenderType" = "Opaque"
        }

        CGINCLUDE

        // Splatmap macro definition
        #define SPLAT_MAP( index )     \
            texture2D _Splat##index;   \
            texture2D _Normal##index;  \
            float4 _Splat##index##_ST; \
            half _Metallic##index;     \
            half _Smoothness##index;

        #define SPLAT_0 SPLAT_COUNT_1 || SPLAT_COUNT_2 || SPLAT_COUNT_3 || SPLAT_COUNT_4 || SPLAT_COUNT_5 || SPLAT_COUNT_6 || SPLAT_COUNT_7 || SPLAT_COUNT_8
        #define SPLAT_1 SPLAT_COUNT_2 || SPLAT_COUNT_3 || SPLAT_COUNT_4 || SPLAT_COUNT_5 || SPLAT_COUNT_6 || SPLAT_COUNT_7 || SPLAT_COUNT_8
        #define SPLAT_2 SPLAT_COUNT_3 || SPLAT_COUNT_4 || SPLAT_COUNT_5 || SPLAT_COUNT_6 || SPLAT_COUNT_7 || SPLAT_COUNT_8
        #define SPLAT_3 SPLAT_COUNT_4 || SPLAT_COUNT_5 || SPLAT_COUNT_6 || SPLAT_COUNT_7 || SPLAT_COUNT_8
        #define SPLAT_4 SPLAT_COUNT_5 || SPLAT_COUNT_6 || SPLAT_COUNT_7 || SPLAT_COUNT_8
        #define SPLAT_5 SPLAT_COUNT_6 || SPLAT_COUNT_7 || SPLAT_COUNT_8
        #define SPLAT_6 SPLAT_COUNT_7 || SPLAT_COUNT_8
        #define SPLAT_7 SPLAT_COUNT_8

        #define CONTROL_0 SPLAT_2
        #define CONTROL_1 SPLAT_5

        // Splatmaps
        SPLAT_MAP( 0 )
        SPLAT_MAP( 1 )
        SPLAT_MAP( 2 )
        SPLAT_MAP( 3 )
        SPLAT_MAP( 4 )
        SPLAT_MAP( 5 )
        SPLAT_MAP( 6 )
        SPLAT_MAP( 7 )

        // Splatmap sampler
        SamplerState sampler_Splat0;

        // First control texture
        sampler2D _Control0;
        float4 _Control0_ST;

        // Second control texture
        sampler2D _Control1;

        // Height and normal map
        sampler2D_float _HeightMap;
        sampler2D_float _SparseHeightMap;
        sampler2D _NormalMap;

        // Variables
        float _EdgeSize;
        float2 _HeightMapResolution;
        float2 _SparseHeightMapResolution;
        float3 _TerrainSize;
        float3 _TerrainOffset;

        // Positions (xyz) and scales (w) of tiles in the camera frustum
        StructuredBuffer<float4> _PositionBuffer;

        ENDCG

        Pass {
            Name "FORWARD"
            Tags {
                "LightMode" = "ForwardBase"
            }

            CGPROGRAM

            // Compile directives
            #pragma vertex vert
            #pragma hull hull
            #pragma domain domain
            #pragma fragment frag
            #pragma target 5.0
            #pragma multi_compile_fog
            #pragma multi_compile SPLAT_COUNT_1 SPLAT_COUNT_2 SPLAT_COUNT_3 SPLAT_COUNT_4 SPLAT_COUNT_5 SPLAT_COUNT_6 SPLAT_COUNT_7 SPLAT_COUNT_8
            #pragma multi_compile_fwdbase
            #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
            #define UNITY_PASS_FORWARDBASE
            #include "HLSLSupport.cginc"
            #include "UnityShaderVariables.cginc"
            #include "UnityShaderUtilities.cginc"
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "Tessellation.cginc"
            #include "UnityPBSLighting.cginc"

            #include "DynamicTerrainSurfaceShader.cginc"
            #include "DynamicTerrainVertexShader.cginc"
            #include "DynamicTerrainHullShader.cginc"

            // Domain to fragment structure
            struct DomainToFragment {
                float4 pos : SV_POSITION;
                float4 pack0 : TEXCOORD0;  // _Splat0 _Splat1
                float4 pack1 : TEXCOORD1;  // _Splat2 _Splat3
                float4 pack2 : TEXCOORD10; // _Splat4 _Splat5
                float4 pack3 : TEXCOORD11; // _Splat6 _Splat7
                float4 tSpace0 : TEXCOORD2;
                float4 tSpace1 : TEXCOORD3;
                float4 tSpace2 : TEXCOORD4;

                #if ( defined( FOG_LINEAR ) || defined( FOG_EXP ) || defined( FOG_EXP2 ) )
                    float4 custompack0 : TEXCOORD5; // tc_Control fogCoord
                #else
                    float2 custompack0 : TEXCOORD5; // tc_Control
                #endif

                float3 worldPos : TEXCOORD9;

                #if UNITY_SHOULD_SAMPLE_SH
                    half3 sh : TEXCOORD6; // SH
                #endif
                UNITY_SHADOW_COORDS( 7 )
                #if SHADER_TARGET >= 30
                    float4 lmap : TEXCOORD8;
                #endif
            };

            // Domain shader
            [UNITY_domain( "tri" )]
            DomainToFragment domain( UnityTessellationFactors tessFactors, const OutputPatch<Vertex, 3> controlPoints, float3 bary : SV_DomainLocation ) {
                // New vertex data from tesselator
                float4 newPosition = controlPoints[ 0 ].vertex * bary.x + controlPoints[ 1 ].vertex * bary.y + controlPoints[ 2 ].vertex * bary.z;
                float2 newTexcoord = controlPoints[ 0 ].texcoord * bary.x + controlPoints[ 1 ].texcoord * bary.y + controlPoints[ 2 ].texcoord * bary.z;

                // Vertex-to-fragment output
                DomainToFragment o;
                UNITY_INITIALIZE_OUTPUT( DomainToFragment, o );

                // Compute texture coordinates
                float4 uv = float4( newTexcoord /* ( 1.0f - 1.0f / _HeightMapResolution )*/ + 1.0f / _HeightMapResolution / 2.0f, 0, 0 );
                float4 uv2 = float4( newTexcoord * ( 1.0f - 1.0f / ( _SparseHeightMapResolution ) ) + 1.0f / ( _SparseHeightMapResolution ) / 2.0f, 0, 0 );

                // Sample terrain height and displace
                float height = tex2Dlod( _HeightMap, uv );
                height += tex2Dlod( _SparseHeightMap, uv2 );
                newPosition.y = height;

                // New vertex output
                o.pos = mul( UNITY_MATRIX_VP, newPosition );
                o.pack0.xy = TRANSFORM_TEX( newTexcoord, _Splat0 );
                o.pack0.zw = TRANSFORM_TEX( newTexcoord, _Splat1 );
                o.pack1.xy = TRANSFORM_TEX( newTexcoord, _Splat2 );
                o.pack1.zw = TRANSFORM_TEX( newTexcoord, _Splat3 );
                o.pack2.xy = TRANSFORM_TEX( newTexcoord, _Splat4 );
                o.pack2.zw = TRANSFORM_TEX( newTexcoord, _Splat5 );
                o.pack3.xy = TRANSFORM_TEX( newTexcoord, _Splat6 );
                o.pack3.zw = TRANSFORM_TEX( newTexcoord, _Splat7 );
                o.custompack0.xy = TRANSFORM_TEX( newTexcoord, _Control0 );
                #if ( defined( FOG_LINEAR ) || defined( FOG_EXP ) || defined( FOG_EXP2 ) )
                    o.custompack0.z = o.pos.z;
                #endif

                // Unpack normal vector
                float3 newNormal = 0;// tex2Dlod(_NormalMap, uv);
                newNormal.xz = newNormal.xz * 2 - 1; // Y is always positive in <0,1>
                newNormal = normalize( newNormal );
                float4 newTangent = float4( cross( newNormal, float3( 0, 0, 1 ) ), -1 );

                float3 worldPos = newPosition;
                o.worldPos = worldPos;
                fixed3 worldNormal = UnityObjectToWorldNormal( newNormal );
                fixed3 worldTangent = UnityObjectToWorldDir( newTangent.xyz );
                fixed tangentSign = newTangent.w * unity_WorldTransformParams.w;
                fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
                o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
                o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
                o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
                #ifdef DYNAMICLIGHTMAP_ON
                    o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                #ifdef LIGHTMAP_ON
                    o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                #endif

                #ifndef LIGHTMAP_ON
                #if UNITY_SHOULD_SAMPLE_SH
                    o.sh = 0;
                #ifdef VERTEXLIGHT_ON
                    o.sh += Shade4PointLights(
                        unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                        unity_LightColor[ 0 ].rgb, unity_LightColor[ 1 ].rgb, unity_LightColor[ 2 ].rgb, unity_LightColor[ 3 ].rgb,
                        unity_4LightAtten0, worldPos, worldNormal );
                #endif
                    o.sh = ShadeSHPerVertex( worldNormal, o.sh );
                #endif
                #endif

                // Pass shadow coordinates to pixel shader
                UNITY_TRANSFER_SHADOW( o, v.texcoord1.xy );

                return o;
            }

            fixed4 frag( DomainToFragment IN ) : SV_Target {
                return float4( 1, 0, 1, 1 ); 
                // Prepare and unpack data
                SurfInput surfIN;
                UNITY_INITIALIZE_OUTPUT( SurfInput, surfIN );
                surfIN.uv_Splat0.x = 1.0;
                surfIN.uv_Splat1.x = 1.0;
                surfIN.uv_Splat2.x = 1.0;
                surfIN.uv_Splat3.x = 1.0;
#if ( CONTROL_1 )
                surfIN.uv_Splat4.x = 1.0;
                surfIN.uv_Splat5.x = 1.0;
                surfIN.uv_Splat6.x = 1.0;
                surfIN.uv_Splat7.x = 1.0;
#endif
                surfIN.tc_Control.x = 1.0;
                surfIN.uv_Splat0 = IN.pack0.xy;
                surfIN.uv_Splat1 = IN.pack0.zw;
                surfIN.uv_Splat2 = IN.pack1.xy;
                surfIN.uv_Splat3 = IN.pack1.zw;
#if ( CONTROL_1 )
                surfIN.uv_Splat4 = IN.pack2.xy;
                surfIN.uv_Splat5 = IN.pack2.zw;
                surfIN.uv_Splat6 = IN.pack3.xy;
                surfIN.uv_Splat7 = IN.pack3.zw;
#endif
                surfIN.tc_Control = IN.custompack0.xy;
                #if ( defined( FOG_LINEAR ) || defined( FOG_EXP ) || defined( FOG_EXP2 ) )
                    surfIN.fogCoord = IN.custompack0.zw;
                #endif
                    float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
                #ifndef USING_DIRECTIONAL_LIGHT
                    fixed3 lightDir = normalize( UnityWorldSpaceLightDir( worldPos ) );
                #else
                    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                #endif
                    fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
                #ifdef UNITY_COMPILER_HLSL
                    SurfaceOutputStandardSpecular o = ( SurfaceOutputStandardSpecular ) 0;
                #else
                    SurfaceOutputStandardSpecular o;
                #endif
                o.Albedo = 0.0;
                o.Emission = 0.0;
                o.Alpha = 0.0;
                o.Occlusion = 1.0;
                fixed3 normalWorldVertex = fixed3( 0, 0, 1 );

                // Call surface function
                surf( surfIN, o );

                // Compute lighting & shadowing factor
                UNITY_LIGHT_ATTENUATION( atten, IN, worldPos )
                fixed4 c = 0;
                fixed3 worldN;
                worldN.x = dot( IN.tSpace0.xyz, o.Normal );
                worldN.y = dot( IN.tSpace1.xyz, o.Normal );
                worldN.z = dot( IN.tSpace2.xyz, o.Normal );
                o.Normal = worldN;

                // Setup lighting environment
                UnityGI gi;
                UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
                gi.indirect.diffuse = 0;
                gi.indirect.specular = 0;
                gi.light.color = _LightColor0.rgb;
                gi.light.dir = lightDir;
                // Call GI (lightmaps/SH/reflections) lighting function
                UnityGIInput giInput;
                UNITY_INITIALIZE_OUTPUT( UnityGIInput, giInput );
                giInput.light = gi.light;
                giInput.worldPos = worldPos;
                giInput.worldViewDir = worldViewDir;
                giInput.atten = atten;
                #if defined( LIGHTMAP_ON ) || defined( DYNAMICLIGHTMAP_ON )
                    giInput.lightmapUV = IN.lmap;
                #else
                    giInput.lightmapUV = 0.0;
                #endif
                #if UNITY_SHOULD_SAMPLE_SH
                    giInput.ambient = IN.sh;
                #else
                    giInput.ambient.rgb = 0.0;
                #endif
                    giInput.probeHDR[ 0 ] = unity_SpecCube0_HDR;
                    giInput.probeHDR[ 1 ] = unity_SpecCube1_HDR;
                #if defined( UNITY_SPECCUBE_BLENDING ) || defined( UNITY_SPECCUBE_BOX_PROJECTION )
                    giInput.boxMin[ 0 ] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                #endif
                #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                    giInput.boxMax[ 0 ] = unity_SpecCube0_BoxMax;
                    giInput.probePosition[ 0 ] = unity_SpecCube0_ProbePosition;
                    giInput.boxMax[ 1 ] = unity_SpecCube1_BoxMax;
                    giInput.boxMin[ 1 ] = unity_SpecCube1_BoxMin;
                    giInput.probePosition[ 1 ] = unity_SpecCube1_ProbePosition;
                #endif
                LightingStandardSpecular_GI( o, giInput, gi );

                // Call lighting function
                c += LightingStandardSpecular( o, worldViewDir, gi );
                c *= o.Alpha;
                UNITY_APPLY_FOG( IN.fogCoord, c );
                UNITY_OPAQUE_ALPHA( c.a );
                return c;
            }

            ENDCG

        }

        Pass {
            Name "DEFERRED"
            Tags {
                "LightMode" = "Deferred"
            }

            CGPROGRAM

            // Compile directives
            #pragma vertex vert
            #pragma hull hull
            #pragma domain domain
            #pragma fragment frag
            #pragma target 5.0
            #pragma multi_compile_fog
            #pragma multi_compile_prepassfinal
            #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
            #define UNITY_PASS_DEFERRED
            #include "HLSLSupport.cginc"
            #include "UnityShaderVariables.cginc"
            #include "UnityShaderUtilities.cginc"
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Tessellation.cginc"
            #include "UnityPBSLighting.cginc"

            #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
            #define WorldReflectionVector( data, normal ) reflect ( data.worldRefl, half3( dot( data.internalSurfaceTtoW0, normal ), dot( data.internalSurfaceTtoW1, normal ), dot( data.internalSurfaceTtoW2, normal ) ) )
            #define WorldNormalVector( data, normal ) fixed3( dot( data.internalSurfaceTtoW0, normal ), dot( data.internalSurfaceTtoW1, normal ), dot( data.internalSurfaceTtoW2, normal ) )

            // Dynamic terrain shaders
            #include "DynamicTerrainSurfaceShader.cginc"
            #include "DynamicTerrainVertexShader.cginc"
            #include "DynamicTerrainHullShader.cginc"


            // Domain to fragment structure
            struct DomainToFragment {
                float4 pos : SV_POSITION;
                float4 pack0 : TEXCOORD0;  // _Splat0 _Splat1
                float4 pack1 : TEXCOORD1;  // _Splat2 _Splat3
                float4 pack2 : TEXCOORD10; // _Splat4 _Splat5
                float4 pack3 : TEXCOORD11; // _Splat6 _Splat7
                float4 tSpace0 : TEXCOORD2;
                float4 tSpace1 : TEXCOORD3;
                float4 tSpace2 : TEXCOORD4;
                float2 custompack0 : TEXCOORD8; // tc_Control
                float3 worldPos : TEXCOORD9;

                #ifndef DIRLIGHTMAP_OFF
                    half3 viewDir : TEXCOORD5;
                #endif
                float4 lmap : TEXCOORD6;
                #ifndef LIGHTMAP_ON
                #if UNITY_SHOULD_SAMPLE_SH
                    half3 sh : TEXCOORD7; // SH
                #endif
                #else
                #ifdef DIRLIGHTMAP_OFF
                    float4 lmapFadePos : TEXCOORD7;
                #endif
                #endif
            };


            // Domain shader
            [UNITY_domain( "tri" )]
            DomainToFragment domain( UnityTessellationFactors tessFactors, const OutputPatch<Vertex, 3> controlPoints, float3 bary : SV_DomainLocation ) {
                // New vertex data from tesselator
                float4 newPosition = controlPoints[ 0 ].vertex * bary.x + controlPoints[ 1 ].vertex * bary.y + controlPoints[ 2 ].vertex * bary.z;
                float2 newTexcoord = controlPoints[ 0 ].texcoord * bary.x + controlPoints[ 1 ].texcoord * bary.y + controlPoints[ 2 ].texcoord * bary.z;

                // Vertex-to-fragment output
                DomainToFragment o;
                UNITY_INITIALIZE_OUTPUT( DomainToFragment, o );

                // Compute texture coordinates
                float4 uv = float4( newTexcoord * ( 1.0f - 1.0f / _HeightMapResolution ) + 1.0f / _HeightMapResolution / 2.0f, 0, 0 );
                float4 uv2 = float4( newTexcoord * ( 1.0f - 1.0f / _SparseHeightMapResolution ) + 1.0f / _SparseHeightMapResolution / 2.0f, 0, 0 );

                // Sample terrain height and displace
                float height = tex2Dlod( _HeightMap, uv );
                height += tex2Dlod( _SparseHeightMap, uv2 );
                newPosition.y = height;

                // New vertex output
                o.pos = mul( UNITY_MATRIX_VP, newPosition );
                o.pack0.xy = TRANSFORM_TEX( newTexcoord, _Splat0 );
                o.pack0.zw = TRANSFORM_TEX( newTexcoord, _Splat1 );
                o.pack1.xy = TRANSFORM_TEX( newTexcoord, _Splat2 );
                o.pack1.zw = TRANSFORM_TEX( newTexcoord, _Splat3 );
                o.pack2.xy = TRANSFORM_TEX( newTexcoord, _Splat4 );
                o.pack2.zw = TRANSFORM_TEX( newTexcoord, _Splat5 );
                o.pack3.xy = TRANSFORM_TEX( newTexcoord, _Splat6 );
                o.pack3.zw = TRANSFORM_TEX( newTexcoord, _Splat7 );
                o.custompack0.xy = TRANSFORM_TEX( newTexcoord, _Control0 );

                // Sample neighboring heights
                float hb = tex2Dlod( _SparseHeightMap, float4( uv2 + float2( -1,  0 ) / _SparseHeightMapResolution, 0, 0 ) );
                float ht = tex2Dlod( _SparseHeightMap, float4( uv2 + float2(  1,  0 ) / _SparseHeightMapResolution, 0, 0 ) );
                float hr = tex2Dlod( _SparseHeightMap, float4( uv2 + float2(  0,  1 ) / _SparseHeightMapResolution, 0, 0 ) );
                float hl = tex2Dlod( _SparseHeightMap, float4( uv2 + float2(  0, -1 ) / _SparseHeightMapResolution, 0, 0 ) );

                // TODO: remove sampling heightmap -> use normalmap
                hb += tex2Dlod( _HeightMap, float4( uv2 + float2( -1,  0 ) / _SparseHeightMapResolution, 0, 0 ) );
                ht += tex2Dlod( _HeightMap, float4( uv2 + float2(  1,  0 ) / _SparseHeightMapResolution, 0, 0 ) );
                hr += tex2Dlod( _HeightMap, float4( uv2 + float2(  0,  1 ) / _SparseHeightMapResolution, 0, 0 ) );
                hl += tex2Dlod( _HeightMap, float4( uv2 + float2(  0, -1 ) / _SparseHeightMapResolution, 0, 0 ) );

                // Sample normalmap
                // float3 terrainNormal;
                // terrainNormal.xz = tex2Dlod( _NormalMap, uv );
                // terrainNormal.y = sqrt( 1 - terrainNormal.x * terrainNormal.x - terrainNormal.z * terrainNormal.z );

                // Compute (bi)tangent vectors
                float3 bitangent = float3( 0.0f, hr - hl, 2.0f / _SparseHeightMapResolution.y * _TerrainSize.z );
                float3 tangent = float3( 2.0f / _SparseHeightMapResolution.x * _TerrainSize.x, ht - hb, 0 );

                // Compute normal vector
                float3 newNormal = normalize( cross( bitangent, tangent ) );

                float4 newTangent = float4( cross( newNormal, float3( 0, 0, 1 ) ), -1 );

                float3 worldPos = newPosition;
                o.worldPos = worldPos;
                fixed3 worldNormal = UnityObjectToWorldNormal( newNormal );
                fixed3 worldTangent = UnityObjectToWorldDir( newTangent.xyz );
                fixed tangentSign = newTangent.w * unity_WorldTransformParams.w;
                fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
                o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
                o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
                o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
                float3 viewDirForLight = UnityWorldSpaceViewDir( worldPos );
                #ifndef DIRLIGHTMAP_OFF
                    o.viewDir.x = dot( viewDirForLight, worldTangent );
                    o.viewDir.y = dot( viewDirForLight, worldBinormal );
                    o.viewDir.z = dot( viewDirForLight, worldNormal );
                #endif
                    o.lmap.zw = 0;
                #ifdef LIGHTMAP_ON
                    o.lmap.xy = newTexcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                #ifdef DIRLIGHTMAP_OFF
                    o.lmapFadePos.xyz = ( newPosition / -unity_ShadowFadeCenterAndType.xyz ) * unity_ShadowFadeCenterAndType.w;
                    o.lmapFadePos.w = ( -UnityObjectToViewPos( newPosition ).z ) * ( 1.0 - unity_ShadowFadeCenterAndType.w );
                #endif
                #else
                    o.lmap.xy = 0;
                    #if UNITY_SHOULD_SAMPLE_SH
                        o.sh = 0;
                        o.sh = ShadeSHPerVertex( worldNormal, o.sh );
                    #endif
                #endif
                return o;
            }

            // Fragment shader
            void frag( DomainToFragment IN, 
                out half4 outGBuffer0 : SV_Target0,
                out half4 outGBuffer1 : SV_Target1,
                out half4 outGBuffer2 : SV_Target2,
                out half4 outEmission : SV_Target3
                #if defined( SHADOWS_SHADOWMASK ) && ( UNITY_ALLOWED_MRT_COUNT > 4 )
                    , out half4 outShadowMask : SV_Target4
                #endif
            ) {
                // Prepare and unpack data
                SurfInput surfIN;
                UNITY_INITIALIZE_OUTPUT( SurfInput, surfIN );
                surfIN.uv_Splat0.x = 1.0;
                surfIN.uv_Splat1.x = 1.0;
                surfIN.uv_Splat2.x = 1.0;
                surfIN.uv_Splat3.x = 1.0;
#if ( CONTROL_1 )
                surfIN.uv_Splat4.x = 1.0;
                surfIN.uv_Splat5.x = 1.0;
                surfIN.uv_Splat6.x = 1.0;
                surfIN.uv_Splat7.x = 1.0;
#endif
                surfIN.tc_Control.x = 1.0;
                surfIN.uv_Splat0 = IN.pack0.xy;
                surfIN.uv_Splat1 = IN.pack0.zw;
                surfIN.uv_Splat2 = IN.pack1.xy;
                surfIN.uv_Splat3 = IN.pack1.zw;
#if ( CONTROL_1 )
                surfIN.uv_Splat4 = IN.pack2.xy;
                surfIN.uv_Splat5 = IN.pack2.zw;
                surfIN.uv_Splat6 = IN.pack3.xy;
                surfIN.uv_Splat7 = IN.pack3.zw;
#endif
                surfIN.tc_Control = IN.custompack0.xy;
                float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);

                #if defined( SNOW ) || defined( WETNESS )
                    surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
                    surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
                    surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
                    surfIN.worldPos = worldPos;
                    surfIN.worldNormal.x = 1.0;
                    surfIN.viewDir.x = 1.0;
                #endif

                #ifndef USING_DIRECTIONAL_LIGHT
                    fixed3 lightDir = normalize( UnityWorldSpaceLightDir( worldPos ) );
                #else
                    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
                #endif
                    fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
                #ifdef UNITY_COMPILER_HLSL
                    SurfaceOutputStandardSpecular o = ( SurfaceOutputStandardSpecular ) 0;
                #else
                    SurfaceOutputStandardSpecular o;
                #endif
                o.Albedo = 0.0;
                o.Emission = 0.0;
                o.Alpha = 0.0;
                o.Occlusion = 1.0;
                fixed3 normalWorldVertex = fixed3( 0, 0, 1 );

                // Call surface function
                surf( surfIN, o );
                fixed3 originalNormal = o.Normal;
                fixed3 worldN;
                worldN.x = dot( IN.tSpace0.xyz, o.Normal );
                worldN.y = dot( IN.tSpace1.xyz, o.Normal );
                worldN.z = dot( IN.tSpace2.xyz, o.Normal );
                o.Normal = worldN;
                half atten = 1;

                // Setup lighting environment
                UnityGI gi;
                UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
                gi.indirect.diffuse = 0;
                gi.indirect.specular = 0;
                gi.light.color = 0;
                gi.light.dir = half3( 0, 1, 0 );
                // Call GI (lightmaps/SH/reflections) lighting function
                UnityGIInput giInput;
                UNITY_INITIALIZE_OUTPUT( UnityGIInput, giInput );
                giInput.light = gi.light;
                giInput.worldPos = worldPos;
                giInput.worldViewDir = worldViewDir;
                giInput.atten = atten;
                #if defined( LIGHTMAP_ON ) || defined( DYNAMICLIGHTMAP_ON )
                    giInput.lightmapUV = IN.lmap;
                #else
                    giInput.lightmapUV = 0.0;
                #endif
                #if UNITY_SHOULD_SAMPLE_SH
                    giInput.ambient = IN.sh;
                #else
                    giInput.ambient.rgb = 0.0;
                #endif
                    giInput.probeHDR[ 0 ] = unity_SpecCube0_HDR;
                    giInput.probeHDR[ 1 ] = unity_SpecCube1_HDR;
                #if defined( UNITY_SPECCUBE_BLENDING ) || defined( UNITY_SPECCUBE_BOX_PROJECTION )
                    giInput.boxMin[ 0 ] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
                #endif
                #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                    giInput.boxMax[ 0 ] = unity_SpecCube0_BoxMax;
                    giInput.probePosition[ 0 ] = unity_SpecCube0_ProbePosition;
                    giInput.boxMax[ 1 ] = unity_SpecCube1_BoxMax;
                    giInput.boxMin[ 1 ] = unity_SpecCube1_BoxMin;
                    giInput.probePosition[ 1 ] = unity_SpecCube1_ProbePosition;
                #endif
                LightingStandardSpecular_GI( o, giInput, gi );

                // Call lighting function to output g-buffer
                outEmission = LightingStandardSpecular_Deferred(o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
                #if defined( SHADOWS_SHADOWMASK ) && ( UNITY_ALLOWED_MRT_COUNT > 4 )
                    outShadowMask = UnityGetRawBakedOcclusions( IN.lmap.xy, float3( 0, 0, 0 ) );
                #endif
                #ifndef UNITY_HDR_ON
                    outEmission.rgb = exp2( -outEmission.rgb );
                #endif
                SplatmapFinalGBuffer( surfIN, o, outGBuffer0, outGBuffer1, outGBuffer2, outEmission );
            }

            ENDCG

        }
        /*
        Pass{
            Name "ShadowCaster"
            Tags {
                "LightMode" = "ShadowCaster"
            }

            CGPROGRAM

            // Compile directives
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f {
                V2F_SHADOW_CASTER;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert( appdata_base v ) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID( v );
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
                TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
                return o;
            }

            float4 frag( v2f i ) : SV_Target {
                SHADOW_CASTER_FRAGMENT( i )
            }

            ENDCG

        }
        */
    }

}