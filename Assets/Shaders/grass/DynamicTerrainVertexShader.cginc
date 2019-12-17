// Vertex structure
struct Vertex {
    float4 vertex : POSITION;
    float4 tangent : TANGENT;
    float3 normal : NORMAL;
    float2 texcoord : TEXCOORD0;
    uint instanceID : SV_InstanceID;
};

// Vertex shader
Vertex vert( Vertex v ) {
    Vertex o;

    // Get the position of current tile
    float4 tilePositionScale = _PositionBuffer[ v.instanceID ];

    // Transform vertex and texture coordinates
    o.vertex = float4( v.vertex * tilePositionScale.w + tilePositionScale.xyz, 1 );
    o.texcoord = ( ( o.vertex.xz - _TerrainOffset.xz ) / _TerrainSize.xz );

    // Output other vertex attributes
    o.tangent = v.tangent;
    o.normal = v.normal;
    o.instanceID = v.instanceID;
    return o;
}