	// ---------------- Varibles ----------------
	fixed4 _TrunkColor;
	half _RandomColor;

	sampler2D _MainTex;
	half _Cutoff;

	half _TreeAmplitude;
	half _WindStength;
	half _WindSpeed;

	struct InstanceData
	{
		// local position
		float3 Point;

		// size of the tree
		float Size;

		// rotation of the tree (heading, pitch, roll)
		float3 Rotation;

		// color of the tree
		float3 Color;

		// Origion.y offset
		float Offset;

		// Plane Offset
		float3 PlaneOffset;
	};

	StructuredBuffer<InstanceData> _buffer;
	float4x4 _LocalToWorld;

	// Tranforms position from object to homogenous space
	inline float4 __UnityObjectToClipPos(in float3 pos)
	{
		// More efficient than computing M*VP matrix product
		return mul(UNITY_MATRIX_VP, mul(_LocalToWorld, float4(pos, 1.0)));
	}

	struct v2g
	{
		float4 pos : SV_POSITION;
		float4 uv : TEXCOORD0;	// size, Heading, Pitch, Roll (will be overiden later)
		float4 uv1 : TEXCOORD1; // Position offset - Plane offsets.xyz

		float3 color: COLOR0;
	};

	struct g2f
	{
		float4 pos : SV_POSITION;
		float3 norm : NORMAL;
		float2 uv : TEXCOORD0;

		float3 diffuseColor : COLOR0;
		fixed3 diff : COLOR1;
	};

	v2g vert(uint id : SV_VertexID)
	{
		v2g OUT;

		InstanceData data = _buffer[id.x];

		OUT.pos = float4(data.Point, 1);
		OUT.uv = float4(data.Size, data.Rotation.xyz);
		OUT.uv1 = float4(data.Offset, data.PlaneOffset.xyz);
		OUT.color = data.Color;

		return OUT;
	}

	void GenerateQuad(inout TriangleStream<g2f> tristream, float3 points[4], float3 color, float2 uv[4], float3 norm)
	{
		g2f OUT;
		float3 faceNormal = cross(points[1] - points[0], points[2] - points[0]);
		for (int i = 0; i < 4; i++)
		{
			OUT.pos = __UnityObjectToClipPos(points[i]);
			OUT.norm = faceNormal;
			OUT.diffuseColor = color;
			OUT.uv = uv[i];

			// ---------------- Light ----------------
			half3 worldNormal = UnityObjectToWorldNormal(norm);
			half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
			OUT.diff = nl * _LightColor0.rgb;

			tristream.Append(OUT);
		}
		tristream.RestartStrip();
	}

	float3 rotate(float degress, float3 vec)
	{
		float3x3 rotMatrix = float3x3(cos(degress), 0, sin(degress), 0, 1, 0, -sin(degress), 0, cos(degress));
		return mul(rotMatrix, vec);
	}

	[maxvertexcount(12)]
	void geom(point v2g IN[1], inout TriangleStream<g2f> triStream)
	{
		float size = IN[0].uv.x;

		float3 _Offset = float3(0, IN[0].uv1.x, 0);
		float3 _normalOffset = IN[0].uv1.yzw; //float3(IN[0].uv1.y, IN[0].uv1.z, IN[0].uv1.w);

		float heading = IN[0].uv.y;
		float pitch = IN[0].uv.z;
		float roll = IN[0].uv.w;

		float3 color = IN[0].color;

		// ---------------- UV1 size/heading ----------------
		half height = size;
		half width = height;
		half angle = heading;

		// ---------------- UV2 pitch/roll ----------------
		float3 norm = float3(pitch, 1, roll);

		// --------------------------------------------
		float3 rot = .5 * (angle * PI);

		float3 perpendicularAngle = rotate(rot, float3(0, 0, 1));
		_Offset.y *= height;

		// ---------------- orgion base ----------------
		float3 base = IN[0].pos.xyz - _Offset;
		float3 top = base + norm * height;
		float3 mid = base + norm * (height / 2);
		// ---------------- orgion mid ----------------
		mid = IN[0].pos.xyz - _Offset;
		base = mid + norm * (-height / 2);
		top = mid + norm * (height / 2);

		// ----------------------------------------------------------
		float3 forwarddir = normalize(float3(0,0,0) - mul(_LocalToWorld , float4(IN[0].pos.xyz, 1)));
		// ----------------------------------------------------------

		float3 rot90 = rotate(PI / 2, perpendicularAngle);

		// ---------------- get angle ----------------
		float3 dir = norm;
		float newPitch = dot(dir, forwarddir);

		dir = perpendicularAngle;
		float yawRight = dot(dir, forwarddir);

		dir = cross(norm, perpendicularAngle);
		float yawLeft = dot(dir, forwarddir);

		// ---------------- top quad ----------------
		float3 topOffset = mid + norm * (_normalOffset.y * (height / 2));

		float3 topQuad[4] = { topOffset + (rot90 * 0.5 * width) + (perpendicularAngle * 0.5 * width),
			topOffset + (rot90 * 0.5 * width) - (perpendicularAngle * 0.5 * width),
			topOffset - (rot90 * 0.5 * width) + (perpendicularAngle * 0.5 * width),
			topOffset - (rot90 * 0.5 * width) - (perpendicularAngle * 0.5 * width) };

		float2 top_uv[4] = { float2(0.5,0), float2(1,0), float2(0.5,0.5), float2(1,0.5) };
		// ---------------- right quad ----------------
		float3 rightOffset = perpendicularAngle * (_normalOffset.z * (width / 2));;

		float3 rightQuad[4] = { (base + rightOffset) + rot90 * 0.5 * width,
				(base + rightOffset) - rot90 * 0.5 * width,
				(top + rightOffset) + rot90 * 0.5 * width,
				(top + rightOffset) - rot90 * 0.5 * width };

		float2 right_uv[4] = { float2(0.5,0), float2(0,0), float2(0.5,0.5), float2(0,0.5) };
		// ---------------- forward quad ----------------
		float3 forwardOffset = rot90 * (_normalOffset.z * (width/2));

		float3 leftQuad[4] = { ((base + forwardOffset) + perpendicularAngle * 0.5 * width) ,
				((base + forwardOffset) - perpendicularAngle * 0.5 * width) ,
				((top + forwardOffset) + perpendicularAngle * 0.5 * width) ,
				((top + forwardOffset) - perpendicularAngle * 0.5 * width)  };

		float2 forward_uv[4] = { float2(0.5,0.5), float2(0,0.5), float2(0.5,1), float2(0,1) };

		// ---------------- alpha ----------------

		float alphaYawRight = 1;
		float alphaYawLeft = 1;
		float alphaYawTop = 1;

		// ---------------- Render ----------------

		//if (yawRight >= 0.5 || yawRight <= -0.5)
		{
			GenerateQuad(triStream, rightQuad, color, right_uv, norm);
		}
		//if (yawLeft >= 0.5 || yawLeft <= -0.5)
		{
			GenerateQuad(triStream, leftQuad, color, forward_uv, norm);
		}
		if (newPitch >= 0.3 || newPitch <= -0.3)
		{
			GenerateQuad(triStream, topQuad, color, top_uv, norm);
		}
	}

	half3 frag(g2f IN) : SV_TARGET
	{
		// ---------------- leaf texture ----------------
		//fixed4 leaf = tex2D(_MainTex, IN.uv) + tex2D(_Mask, IN.uv) * _LeafColor * (_LeafColorModify * IN.diffuseColor);

		float2 cord = IN.uv;
		cord.x += cord.y * sin(cord.y * _WindStength + _Time.x * _WindSpeed) * _TreeAmplitude;

		// ---------------- trunk texture ----------------
		fixed4 trunk = tex2D(_MainTex, cord) * _TrunkColor;
		//************************ 5 ms (2M) ************************
		trunk = trunk + (trunk * (half4(IN.diffuseColor,1) * _RandomColor));

		clip(trunk.a - _Cutoff);
		// ---------------- lighting color ----------------

		//fixed3 lighting = IN.diff + IN.ambient;
		//trunk.rgb *= lighting;
		

		return trunk.rgba * (IN.diff); //+_LightColor0.rgb);
	}