// Hull shader - constant phase
UnityTessellationFactors hull_ConstPhase( InputPatch<Vertex, 3> v ) {
    UnityTessellationFactors o;

    // Compute tessellation factors
    float4 tessellationFactors = UnityEdgeLengthBasedTess( v[ 0 ].vertex, v[ 1 ].vertex, v[ 2 ].vertex, _EdgeSize );

    o.edge[ 0 ] = tessellationFactors.x;
    o.edge[ 1 ] = tessellationFactors.y;
    o.edge[ 2 ] = tessellationFactors.z;
    o.inside = tessellationFactors.w;
    return o;
}

// Hull shader - control point phase
[UNITY_domain( "tri" )]
[UNITY_partitioning( "fractional_even" )]
[UNITY_outputtopology( "triangle_cw" )]
[UNITY_patchconstantfunc( "hull_ConstPhase" )]
[UNITY_outputcontrolpoints( 3 )]
Vertex hull( InputPatch<Vertex, 3> v, uint id : SV_OutputControlPointID ) {
    return v[ id ];
}