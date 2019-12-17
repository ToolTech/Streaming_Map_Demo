// Surface shader input
struct SurfInput {
    float2 uv_Splat0 : TEXCOORD0;
    float2 uv_Splat1 : TEXCOORD1;
    float2 uv_Splat2 : TEXCOORD2;
    float2 uv_Splat3 : TEXCOORD3;
#if ( CONTROL_1 )
    float2 uv_Splat4 : TEXCOORD6;
    float2 uv_Splat5 : TEXCOORD7;
    float2 uv_Splat6 : TEXCOORD8;
    float2 uv_Splat7 : TEXCOORD9;
#endif
    float2 tc_Control : TEXCOORD4;  // Not prefixing '_Contorl' with 'uv' allows a tighter packing of interpolators, which is necessary to support directional lightmap.
    UNITY_FOG_COORDS( 5 )
#if defined( SNOW ) || defined( WETNESS ) || defined( RIPPLES )
    float3 viewDir;
    float3 worldNormal;
    float3 worldPos;
    INTERNAL_DATA
#endif
};

// Splatmap blending
void SplatmapMix( SurfInput IN, half4 defaultAlpha0, half4 defaultAlpha1, out half4 splat_control0, out half4 splat_control1, out fixed4 mixedDiffuse, inout fixed3 mixedNormal ) {
    splat_control0 = tex2D( _Control0, IN.tc_Control );
#if ( CONTROL_1 )
    splat_control1 = tex2D( _Control1, IN.tc_Control );
#else
    splat_control1 = 0;
#endif
    half weight = dot( splat_control0, half4( 1, 1, 1, 1 ) );
#if ( CONTROL_1 )
    weight += dot( splat_control1, half4( 1, 1, 1, 1 ) );
#endif
    // Normalize weights before lighting and restore weights in final modifier functions so that the overal
    // lighting result can be correctly weighted.
    splat_control0 /= ( weight + 1e-3f );
#if ( CONTROL_1 )
    splat_control1 /= ( weight + 1e-3f );
#endif

    mixedDiffuse = 0.0f;
    fixed4 mixedNormalPacked = 0.0f;
    
    if ( splat_control0.r > 0.0f ) {
        mixedDiffuse += splat_control0.r * _Splat0.Sample( sampler_Splat0, IN.uv_Splat0 ) * half4( 1.0, 1.0, 1.0, defaultAlpha0.r );
        mixedNormalPacked += splat_control0.r * _Normal0.Sample( sampler_Splat0, IN.uv_Splat0 );
    }

    if ( splat_control0.g > 0.0f ) {
        mixedDiffuse += splat_control0.g * _Splat1.Sample( sampler_Splat0, IN.uv_Splat1 ) * half4( 1.0, 1.0, 1.0, defaultAlpha0.g );
        mixedNormalPacked += splat_control0.g *  _Normal1.Sample( sampler_Splat0, IN.uv_Splat1 );
    }

    if ( splat_control0.b > 0.0f ) {
        mixedDiffuse += splat_control0.b * _Splat2.Sample( sampler_Splat0, IN.uv_Splat2 ) * half4( 1.0, 1.0, 1.0, defaultAlpha0.b );
        mixedNormalPacked += splat_control0.b * _Normal2.Sample( sampler_Splat0, IN.uv_Splat2 );
    }

    if ( splat_control0.a > 0.0f ) {
        mixedDiffuse += splat_control0.a * _Splat3.Sample( sampler_Splat0, IN.uv_Splat3 ) * half4( 1.0, 1.0, 1.0, defaultAlpha0.a );
        mixedNormalPacked += splat_control0.a * _Normal3.Sample( sampler_Splat0, IN.uv_Splat3 );
    }

#if ( CONTROL_1 )
    if ( splat_control1.r > 0.0f ) {
        mixedDiffuse += splat_control1.r * _Splat4.Sample( sampler_Splat0, IN.uv_Splat4 ) * half4( 1.0, 1.0, 1.0, defaultAlpha1.r );
        mixedNormalPacked += splat_control1.r * _Normal4.Sample( sampler_Splat0, IN.uv_Splat4 );
    }

    if ( splat_control1.g > 0.0f ) {
        mixedDiffuse += splat_control1.g * _Splat5.Sample( sampler_Splat0, IN.uv_Splat5 ) * half4( 1.0, 1.0, 1.0, defaultAlpha1.g );
        mixedNormalPacked += splat_control1.g * _Normal5.Sample( sampler_Splat0, IN.uv_Splat5 );
    }

    if ( splat_control1.b > 0.0f ) {
        mixedDiffuse += splat_control1.b * _Splat6.Sample( sampler_Splat0, IN.uv_Splat6 ) * half4( 1.0, 1.0, 1.0, defaultAlpha1.b );
        mixedNormalPacked += splat_control1.b * _Normal6.Sample( sampler_Splat0, IN.uv_Splat6 );
    }

    if ( splat_control1.a > 0.0f ) {
        mixedDiffuse += splat_control1.a * _Splat7.Sample( sampler_Splat0, IN.uv_Splat7 ) * half4( 1.0, 1.0, 1.0, defaultAlpha1.a );
        mixedNormalPacked += splat_control1.a * _Normal7.Sample( sampler_Splat0, IN.uv_Splat7 );
    }
#endif

    mixedNormal = UnpackNormal( mixedNormalPacked );
}

void SplatmapFinalGBuffer( SurfInput IN, SurfaceOutputStandardSpecular o, inout half4 outGBuffer0, inout half4 outGBuffer1, inout half4 outGBuffer2, inout half4 emission ) {
    UnityStandardDataApplyWeightToGbuffer( outGBuffer0, outGBuffer1, outGBuffer2, o.Alpha );
    emission *= o.Alpha;
}

// Surface function
void surf( SurfInput IN, inout SurfaceOutputStandardSpecular o ) {
    half4 splat_control0;
    half4 splat_control1;
    half weight;
    fixed4 mixedDiffuse;
    half4 defaultSmoothness0 = half4( _Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3 );
    half4 defaultSmoothness1 = half4( _Smoothness4, _Smoothness5, _Smoothness6, _Smoothness7 );
    SplatmapMix( IN, defaultSmoothness0, defaultSmoothness1, splat_control0, splat_control1, mixedDiffuse, o.Normal );
    o.Albedo = mixedDiffuse.rgb;
    o.Alpha = 1.0f;
    o.Smoothness = mixedDiffuse.a;
    float metallic = dot( splat_control0, half4( _Metallic0, _Metallic1, _Metallic2, _Metallic3 ) );
    metallic += dot( splat_control1, half4( _Metallic4, _Metallic5, _Metallic6, _Metallic7 ) );

    // Metalic to specular
    half oneMinusReflectivity;
    o.Albedo = DiffuseAndSpecularFromMetallic( o.Albedo, metallic, o.Specular, oneMinusReflectivity );
    o.Albedo /= oneMinusReflectivity;


    // #if defined( SNOW ) || defined( WETNESS ) || defined( RIPPLES )
    //    WeatherDataInput weatherDataInput;
    //    UNITY_INITIALIZE_OUTPUT( WeatherDataInput, weatherDataInput );
    //    weatherDataInput.texCoord = IN.tc_Control.xy;
    //    weatherDataInput.viewDir = IN.viewDir;
    //    weatherDataInput.worldPos = IN.worldPos.xyz;
    //    weatherDataInput.height = 0.25f;
    //    weatherDataInput.mixmapValue = float2( 1.0f, 0.0f );
    //    weatherDataInput.worldNormalFace = WorldNormalVector( IN, float3( 0.0f, 0.0f, 1.0f ) );
    //#if defined( SNOW )
    //    weatherDataInput.worldNormal = WorldNormalVector( IN, lerp( o.Normal, float3( 0.0f, 0.0f, 1.0f ), saturate( ( _SnowAmount * _SnowAccumulation.y + _SnowAccumulation.x ) * 0.5f ) ) );
    //#endif
    //    weatherDataInput.puddleMaskValue = 1.0f;
    //    weatherDataInput.uniqueSnowMaskValue = 1.0f;
    //
    //    ApplyWeatherSurfaceEffects( weatherDataInput, o.Albedo, o.Specular, o.Smoothness, o.Occlusion, o.Emission, o.Normal );
    //#endif
}