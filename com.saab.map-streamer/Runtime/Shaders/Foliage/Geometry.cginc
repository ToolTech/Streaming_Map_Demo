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
	float i = (input.x * 131.071) + (input.y * 655.37);
	return Random(i, mutator);
}

int WeightedRandomHeight(float random, float height)
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

		if (heightWeight == 0)
			return -1;

		return i;
	}
	return -1;
}

int WeightedRandom(float random)
{
	float weightSum = 0;
	for (int index = 0; index < _foliageCount; index++)
	{
		weightSum += _foliageData[index].Weight;
	}

	float r = random * weightSum;

	for (int i = 0; i < _foliageCount; i++)
	{
		r -= _foliageData[i].Weight;
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
	type = WeightedRandomHeight(random, height);
	if (type < 0)
		return;

	float2 minMaxHeight = _foliageData[type].MaxMin;
	float2 offset = _foliageData[type].Offset;
	float weight = _foliageData[type].Weight;

	// ********************* ***************  ********************* //

	float foliageHeight = minMaxHeight.x + ((minMaxHeight.y - minMaxHeight.x) * Random(random, 0.321f));
	foliageHeight = clamp(foliageHeight, minMaxHeight.x, height);
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

// Geometry Shader CrossBoard
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
	type = WeightedRandomHeight(random, height);
	if (type < 0)
		return;

	float2 minMaxHeight = _foliageData[type].MaxMin;
	float2 offset = _foliageData[type].Offset;
	float weight = _foliageData[type].Weight;

	// ********************* ***************  ********************* //

	float foliageHeight = minMaxHeight.x + ((minMaxHeight.y - minMaxHeight.x) * Random(random, 0.321f));
	foliageHeight = clamp(foliageHeight, minMaxHeight.x, height);
	height = foliageHeight + _AdditiveSize;
	pos.y -= offset.y * height;		// offset to handle Roots

	float3 up = normalize(float3(0, 1, 0));
	float angle = (Random(random, 0.131f) * 2 - 1) * 3.141592;
	float3 randDir = float3(cos(angle), 0, sin(angle));

	float3 right = randDir;
	float3 front = normalize(-cross(up, right));

	float halfS = 0.5f * height;
	float3 center = pos + up * halfS;

	float3 wind = float3(0, 0, 0);

	if (length(_Wind.xy) > 0)
	{
		float2 windDir = normalize(_Wind.xy);
		float rand = Random(random, 0.181);
		float curve = ((cos(_Time * _Wind.z * (1 + rand * 0.1))) * 0.5 + 0.5) * 0.5;
		wind = float3(windDir.x, 0, windDir.y) * curve * foliageHeight * 0.02;
	}
	// disable wind
	//wind *= 0;

	// *********** front points ***********

	float4 f[4];
	f[0] = float4(pos + halfS * right, 1.0f);
	f[1] = float4(pos + halfS * right + height * up + wind, 1.0f);
	f[2] = float4(pos - halfS * right, 1.0f);
	f[3] = float4(pos - halfS * right + height * up + wind, 1.0f);

	// *********** Right points ***********

	float4 r[4];
	r[0] = float4(pos + halfS * front, 1.0f);
	r[1] = float4(pos + halfS * front + height * up + wind, 1.0f);
	r[2] = float4(pos - halfS * front, 1.0f);
	r[3] = float4(pos - halfS * front + height * up + wind, 1.0f);

	// *********** Top points ***********

	float4 t[4];
	t[0] = float4(center + halfS * right - halfS * front + wind * 0.5, 1.0f);
	t[1] = float4(center + halfS * right + halfS * front + wind * 0.5, 1.0f);
	t[2] = float4(center - halfS * right - halfS * front + wind * 0.5, 1.0f);
	t[3] = float4(center - halfS * right + halfS * front + wind * 0.5, 1.0f);

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

// Geometry Shader Grass
[maxvertexcount(48)]
void Grass(point uint p[1] : TEXCOORD, inout TriangleStream<FS_INPUT> triStream)
{
	// ********************* point cloud data  ********************* //

	float3 pos = _PointBuffer[p[0]].Position;
	float3 color = _PointBuffer[p[0]].Color;
	float height = _PointBuffer[p[0]].Height;
	float random = _PointBuffer[p[0]].Random;

	// ********************* foliage type data  ********************* //

	int type = floor(random * _foliageCount);	// calculate the foliage (index) to place down
	type = WeightedRandom(random);

	float2 minMaxHeight = _foliageData[type].MaxMin;
	float2 offset = _foliageData[type].Offset;
	float weight = _foliageData[type].Weight;

	// *********** billboard ***********
	float3 up = normalize(float3(0, 1, 0));
	float3 center = pos;

	//float distance = clamp((length(_WorldSpaceCameraPos - pos) / 20), 0, 1);

	for (uint i = 0; i < 12; i++)
	{
		float rand = Random(random + i, 0.912);
		float angle = (rand * 2 - 1) * 3.141592;
		float3 randDir = float3(cos(angle), 0, sin(angle));
		float3 pos = center + randDir * rand * 1.5;

		float rand02 = Random(random + i, 0.643);
		angle = (rand02 * 2 - 1) * 3.141592;
		randDir = float3(cos(angle), 0, sin(angle));

		float3 camDir = _WorldSpaceCameraPos - pos;
		float blend = normalize(camDir).y;
		camDir = normalize(camDir);

		float3 billboardRight = cross(up, camDir);
		float rotateDir = sign(dot(billboardRight, randDir));

		float3 right = lerp(billboardRight * rotateDir, randDir, blend);
		float3 front = normalize(-cross(up, right));

		float side = sign(dot(camDir, front));

		float foliageHeight = minMaxHeight.x + ((minMaxHeight.y - minMaxHeight.x) * Random(random, 0.321f));
		float h = foliageHeight;
		float halfS = 0.5f * h;
		pos.y -= offset.y * h;					// offset to handle Roots

		float3 wind = float3(0, 0, 0);

		if (length(_Wind.xy) > 0)
		{
			float2 windDir = normalize(_Wind.xy);
			float curve = ((cos(_Time * _Wind.z * (1 + rand * 0.03))) + 1) * 0.3;
			wind = float3(windDir.x, 0, windDir.y) * curve * h;
		}

		float4 f[4];
		// right bottom
		f[0] = float4(pos + halfS * right, 1.0f);
		// right top
		f[1] = float4(pos + halfS * right + h * up + wind, 1.0f);
		// left bottom
		f[2] = float4(pos - halfS * right, 1.0f);
		// left top
		f[3] = float4(pos - halfS * right + h * up + wind, 1.0f);

		FS_INPUT pIn;

		// right bottom
		AppendVertex(pIn, f[0], center, height, float3(1.0f, 0.0f, type), normalize(front * side + right + up * 0.5), color);
		triStream.Append(pIn);
		// right top
		AppendVertex(pIn, f[1], center, height, float3(1.0f + (rand * 0.2f), 1.0f + (rand02 * 0.2f), type), normalize(up + right), color);
		triStream.Append(pIn);
		// left bottom
		AppendVertex(pIn, f[2], center, height, float3(0.0f, 0.0f, type), normalize(front * side - right + up * 0.5), color);
		triStream.Append(pIn);
		// left top
		AppendVertex(pIn, f[3], center, height, float3(0.0f + (rand02 * 0.2f), 1.0f + (rand * 0.2f), type), normalize(up + right), color);
		triStream.Append(pIn);

		triStream.RestartStrip();
	}
}

[maxvertexcount(48)]
void geo(point uint p[1] : TEXCOORD, inout TriangleStream<FS_INPUT> triStream)
{
#ifdef CROSSBOARD_ON
	Crossboard(p, triStream);
#else
	Grass(p, triStream);
#endif
}
