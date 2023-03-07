void AppendVertex(inout FS_INPUT pin, float3 wp, float3 center, float radius, float3 uv, float3 normal, float3 color)
{
	pin.pos = UnityObjectToClipPos(wp);
	pin.wp = wp;
	pin.center = center;
	pin.radius = radius;
	pin.tex0 = uv;
	pin.normal = normal;
	pin.color = color;
}

float Random(float input, float mutator = 0.546)
{
	float random = frac(sin(input + mutator) * 142375.554353);
	return random;
}

float Random2D(float2 input, float mutator = 0.546)
{
	float i = (input.x * 13.1071) + (input.y * 65.537);
	float random = frac(sin(i + mutator) * 142375.554353);
	return random;
}

int WeightedRandom(float random, float height)
{
	float weightSum = 0;
	for (int index = 0; index < _foliageCount; index++)
	{
		float heightWeight = _foliageData[index].MaxMin.x > height ? 0 : 1;
		weightSum += _foliageData[index].Weight * heightWeight;
	}

	float r = random * weightSum;

	for (int i = 0; i < _foliageCount; i++)
	{
		float heightWeight = _foliageData[i].MaxMin.x > height ? 0 : 1;
		r -= (_foliageData[i].Weight * heightWeight);
		if (r >= 0)
			continue;

		return i;
	}
	return -1;
}

// Geometry Shader Billboard
[maxvertexcount(4)]
void Billboard(point uint p[1] : TEXCOORD, inout TriangleStream<FS_INPUT> triStream)
{
	// ********************* point cloud data  ********************* //

	float3 pos = _PointBuffer[p[0]].Position;
	float3 color = _PointBuffer[p[0]].Color;
	float height = _PointBuffer[p[0]].Height;
	float random = _PointBuffer[p[0]].Random;

	// ********************* MaxMin height  ********************* //

	float foliageFactor = 0.2f;
	float maxHorizonHeight = height * (1 + foliageFactor);
	float minHorizonHeight = height * (1 - foliageFactor);

	// ********************* foliage type data  ********************* //

	int type = floor(random * _foliageCount);
	type = WeightedRandom(random, height);
	if (type < 0)
		return;

	float2 minMaxHeight = _foliageData[type].MaxMin;
	float2 offset = _foliageData[type].Offset;
	float weight = _foliageData[type].Weight;

	// ********************* ***************  ********************* //
	float minh = min(minMaxHeight.y, maxHorizonHeight);
	float maxh = max(minMaxHeight.x, minHorizonHeight);
	float foliageHeight = minMaxHeight.x + ((minh - maxh) * random);
	
	height = foliageHeight + _AdditiveSize;
	pos.y -= offset.y * height;		// offset to handle Roots

	float3 up = float3(0, 1, 0);
	float3 look = float3(1, 1, 1);

	float halfS = 0.5f * height;
	float3 center = pos + up * halfS;

#ifdef SHADOW_BILLBOARD
	look = _WorldSpaceLightPos0;
#else
	look = _WorldSpaceCameraPos - pos;
#endif

	look.y = 0;
	look = normalize(look);
	float3 right = cross(up, look);

	float4 v[4];
	v[0] = float4(pos + halfS * right, 1.0f);
	v[1] = float4(pos + halfS * right + height * up, 1.0f);
	v[2] = float4(pos - halfS * right, 1.0f);
	v[3] = float4(pos - halfS * right + height * up, 1.0f);

	float3 uv0 = float3(1.0f, 0.0f, type);
	float3 uv1 = float3(1.0f, 1.0f, type);
	float3 uv2 = float3(0.0f, 0.0f, type);
	float3 uv3 = float3(0.0f, 1.0f, type);

	#ifdef CROSSBOARD
		uv0 = float3(0.5f, 0.0f, type);
		uv1 = float3(0.5f, 0.5f, type);
		uv2 = float3(0.0f, 0.0f, type);
		uv3 = float3(0.0f, 0.5f, type);
	#endif

	FS_INPUT pIn;

	AppendVertex(pIn, v[0], center, halfS, uv0, -look, color);
	triStream.Append(pIn);

	AppendVertex(pIn, v[1], center, halfS, uv1, -look, color);
	triStream.Append(pIn);

	AppendVertex(pIn, v[2], center, halfS, uv2, -look, color);
	triStream.Append(pIn);

	AppendVertex(pIn, v[3], center, halfS, uv3, -look, color);
	triStream.Append(pIn);
}

[maxvertexcount(12)]
void Crossboard(point uint p[1] : TEXCOORD, inout TriangleStream<FS_INPUT> triStream)
{
	// ********************* point cloud data  ********************* //

	float3 pos = _PointBuffer[p[0]].Position;
	float3 color = _PointBuffer[p[0]].Color;
	float height = _PointBuffer[p[0]].Height;
	float random = _PointBuffer[p[0]].Random;

	// ********************* MaxMin height  ********************* //

	float foliageFactor = 0.2f;
	float maxHorizonHeight = height * (1 + foliageFactor);
	float minHorizonHeight = height * (1 - foliageFactor);

	// ********************* foliage type data  ********************* //

	int type = floor(random * _foliageCount);	// calculate the foliage (index) to place down
	type = WeightedRandom(random, height);
	if (type < 0)
		return;

	float2 minMaxHeight = _foliageData[type].MaxMin;
	float2 offset = _foliageData[type].Offset;
	float weight = _foliageData[type].Weight;

	// ********************* ***************  ********************* //

	float minh = min(minMaxHeight.y, maxHorizonHeight);
	float maxh = max(minMaxHeight.x, minHorizonHeight);
	float foliageHeight = minMaxHeight.x + ((minh - maxh) * random);
	height = foliageHeight + _AdditiveSize;
	pos.y -= offset.y * height;		// offset to handle Roots

	float3 up = normalize(float3(0, 1, 0));
	float angle = (Random(random) * 2 - 1) * 3.141592;
	float3 randDir = float3(cos(angle), 0, sin(angle));

	float3 right = randDir; 
	float3 front = normalize(-cross(up, right));

	float halfS = 0.5f * height;
	float3 center = pos + up * halfS;

	// *********** front points ***********

	float4 f[4];
	f[0] = float4(pos + halfS * right, 1.0f);
	f[1] = float4(pos + halfS * right + height * up, 1.0f);
	f[2] = float4(pos - halfS * right, 1.0f);
	f[3] = float4(pos - halfS * right + height * up, 1.0f);

	// *********** Right points ***********

	float4 r[4];
	r[0] = float4(pos + halfS * front, 1.0f);
	r[1] = float4(pos + halfS * front + height * up, 1.0f);
	r[2] = float4(pos - halfS * front, 1.0f);
	r[3] = float4(pos - halfS * front + height * up, 1.0f);

	// *********** Top points ***********

	float4 t[4];
	t[0] = float4(center + halfS * right - halfS * front, 1.0f);
	t[1] = float4(center + halfS * right + halfS * front, 1.0f);
	t[2] = float4(center - halfS * right - halfS * front, 1.0f);
	t[3] = float4(center - halfS * right + halfS * front, 1.0f);

	// *********** Crossboards ***********

	FS_INPUT pIn;

	// *********** front face ***********

	AppendVertex(pIn, f[0], center, halfS, float3(0.5f, 0.0f, type), normalize(front), color);
	triStream.Append(pIn);

	AppendVertex(pIn, f[1], center, halfS, float3(0.5f, 0.5f, type), normalize(front), color);
	triStream.Append(pIn);

	AppendVertex(pIn, f[2], center, halfS, float3(0.0f, 0.0f, type), normalize(front), color);
	triStream.Append(pIn);

	AppendVertex(pIn, f[3], center, halfS, float3(0.0f, 0.5f, type), normalize(front), color);
	triStream.Append(pIn);

	// *********** right face ***********

	triStream.RestartStrip();

	AppendVertex(pIn, r[0], center, halfS, float3(0.5f, 0.5f, type), normalize(right), color);
	triStream.Append(pIn);

	AppendVertex(pIn, r[1], center, halfS, float3(0.5f, 1.0f, type), normalize(right), color);
	triStream.Append(pIn);

	AppendVertex(pIn, r[2], center, halfS, float3(0.0f, 0.5f, type), normalize(right), color);
	triStream.Append(pIn);

	AppendVertex(pIn, r[3], center, halfS, float3(0.0f, 1.0f, type), normalize(right), color);
	triStream.Append(pIn);

	// *********** top face ***********

	triStream.RestartStrip();

	AppendVertex(pIn, t[0], center, halfS, float3(1.0f, 0.0f, type), normalize(up), color);
	triStream.Append(pIn);

	AppendVertex(pIn, t[1], center, halfS, float3(1.0f, 0.5f, type), normalize(up), color);
	triStream.Append(pIn);

	AppendVertex(pIn, t[2], center, halfS, float3(0.5f, 0.0f, type), normalize(up), color);
	triStream.Append(pIn);

	AppendVertex(pIn, t[3], center, halfS, float3(0.5f, 0.5f, type), normalize(up), color);
	triStream.Append(pIn);
}