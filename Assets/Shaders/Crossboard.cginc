// ---------------- Varibles ----------------
fixed4 _TrunkColor;
fixed4 _TrunkColorModify;
fixed4 _LeafColor;
fixed4 _LeafColorModify;
half _RandomColor;
sampler2D _Mask;
sampler2D _MainTex;
half _Cutoff;
half _Height;
float3 _Offset;
half _Width;
half _RandomWidth;
half _RandomHeight;
bool _UseLod;
half _LodFarFade;
half _LodFar;
half _LodNearFade;
half _LodNear;
half _WindStength;
half _WindSpeed;

	struct v2g
	{
		float4 pos : SV_POSITION;
		float3 norm : NORMAL;
		float2 uv : TEXCOORD0;

		float2 uv1 : TEXCOORD1; //Size, Heading
		float2 uv2 : TEXCOORD2; //Pitch, Roll

		float4 color: TEXCOORD3;
	};

	struct g2f
	{
		float4 pos : SV_POSITION;
		float3 norm : NORMAL;
		float2 uv : TEXCOORD0;

		fixed3 diff : COLOR0;
		fixed3 ambient : COLOR1;

		float4 diffuseColor : TEXCOORD5;

		//float2 uv1 : TEXCOORD1; //Size, Heading
		//float2 uv2 : TEXCOORD2; //Pitch, Roll
	};

	v2g vert(appdata_full v)
	{
		v2g OUT;
		OUT.pos = v.vertex;
		OUT.norm = v.normal;
		OUT.uv = v.texcoord;
		OUT.uv1 = v.texcoord1;
		OUT.uv2 = v.texcoord2;

		OUT.color = v.color.rgba;

		return OUT;
	}

	void GenerateQuad(inout TriangleStream<g2f> tristream, float3 points[4], float4 color, float2 uv[4], float3 norm)
	{
		g2f OUT;
		float3 faceNormal = cross(points[1] - points[0], points[2] - points[0]);
		for (int i = 0; i < 4; i++)
		{
			OUT.pos = UnityObjectToClipPos(points[i]);
			OUT.norm = faceNormal;
			OUT.diffuseColor = color;
			OUT.uv = uv[i];
			// ---------------- Light ----------------
			half3 worldNormal = UnityObjectToWorldNormal(norm);
			half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
			OUT.diff = nl * _LightColor0.rgb;
			OUT.ambient = ShadeSH9(half4(worldNormal, 1));

			tristream.Append(OUT);
		}
		tristream.RestartStrip();
	}

	float3 rotate(float degress, float3 vec)
	{
		float3x3 rotMatrix = float3x3(cos(degress), 0, sin(degress), 0, 1, 0, -sin(degress), 0, cos(degress));
		return mul(rotMatrix, vec);
	}

	[maxvertexcount(24)]
	void geom(point v2g IN[1], inout TriangleStream<g2f> triStream)
	{
		// ---------------- Camera Vectors ----------------
		float3 rightdir = UNITY_MATRIX_IT_MV[0].xyz;
		float3 updir = UNITY_MATRIX_IT_MV[1].xyz;
		float3 forwarddir = UNITY_MATRIX_IT_MV[2].xyz;
		float3 camPos = _WorldSpaceCameraPos.xyz;

		//float3 norm = IN[0].norm;

		float size = IN[0].uv1.x;
		float heading = IN[0].uv1.y;

		float pitch = IN[0].uv2.x;
		float roll = IN[0].uv2.x;

		float4 color = IN[0].color;

		// ---------------- UV1 size/heading ----------------
		half height = _Height * (1 + (_RandomHeight * size));
		half width = _Width * (1 + (_RandomWidth * size));
		// heading 
		half angle = heading;

		// ---------------- UV2 pitch/roll ----------------
		float3 norm = float3(pitch, 1, roll);

		//half height = _Height * (1 + (_RandomHeight * color.g));
		//half width = _Width * (1 + (_RandomWidth * color.g));

		//half angle = color.b;

		float3 rot = .5 * (angle * PI);

		float3 perpendicularAngle = rotate(rot, float3(0, 0, 1));
		_Offset.y *= height;
		float3 base = IN[0].pos.xyz - _Offset;
		float3 top = base + norm * height;
		float3 mid = base + norm * (height / 2);

		// ----------------------------------------------------------
		forwarddir = normalize(ObjSpaceViewDir(IN[0].pos));
		// ----------------------------------------------------------

		float dist = length(ObjSpaceViewDir(IN[0].pos));
		float fade = color.a;

		float LodFar = _LodFar * (height / _Height);
		float LodFarFade = _LodFarFade * (height / _Height);

		if ((dist < LodFar && dist > _LodNear) || !_UseLod)
		{
			if (dist > LodFarFade && _UseLod)
			{
				fade = (LodFar - dist) / (LodFar - LodFarFade);
			}

			if (dist < _LodNearFade && _UseLod)
			{
				fade = (_LodNear - dist) / (_LodNear - _LodNearFade);
			}

			color.a = fade;
			// ---------------- basic wind ----------------
			float3 wind = float3(sin(_Time.x * _WindSpeed + base.x) + sin(_Time.x * _WindSpeed + base.z * 2) + sin(_Time.x * _WindSpeed * 0.1 + base.x), 0,
				cos(_Time.x * _WindSpeed + base.x * 2) + cos(_Time.x * _WindSpeed + base.z));
			top += wind * _WindStength;

			float3 rot90 = rotate(PI / 2, perpendicularAngle);

			// ---------------- get angle ----------------
			float3 dir = norm;
			float pitch = dot(dir, forwarddir);

			dir = perpendicularAngle;
			float yawRight = dot(dir, forwarddir);

			dir = cross(norm, perpendicularAngle);
			float yawLeft = dot(dir, forwarddir);

			// ---------------- top quad ----------------
			float3 topQuad[4] = { mid + (rot90 * 0.5 * width) + (perpendicularAngle * 0.5 * width),
			mid + (rot90 * 0.5 * width) - (perpendicularAngle * 0.5 * width),
			mid - (rot90 * 0.5 * width) + (perpendicularAngle * 0.5 * width),
			mid - (rot90 * 0.5 * width) - (perpendicularAngle * 0.5 * width) };

			float2 top_uv[4] = { float2(0.5,0), float2(1,0), float2(0.5,0.5), float2(1,0.5) };
			// ---------------- right quad ----------------
			float3 rightQuad[4] = { base + rot90 * 0.5 * width,
					base - rot90 * 0.5 * width,
					top + rot90 * 0.5 * width,
					top - rot90 * 0.5 * width };

			float2 right_uv[4] = { float2(0.5,0), float2(0,0), float2(0.5,0.5), float2(0,0.5) };
			// ---------------- forward quad ----------------
			float3 leftQuad[4] = { base + perpendicularAngle * 0.5 * width,
					base - perpendicularAngle * 0.5 * width,
					top + perpendicularAngle * 0.5 * width,
					top - perpendicularAngle * 0.5 * width };

			float2 forward_uv[4] = { float2(0.5,0.5), float2(0,0.5), float2(0.5,1), float2(0,1) };

			// ---------------- alpha ----------------
			float aTop;
			float aRight;
			float aForward;

			if (yawRight > 0 || yawRight < -0)
			{
				aRight = 1 - (1 - abs(yawRight));
				color.a = aRight * fade;
				GenerateQuad(triStream, rightQuad, color, right_uv, norm);
			}
			if (yawLeft > 0 || yawLeft < -0)
			{
				aForward = 1 - (1 - abs(yawLeft));
				color.a = aForward * fade;
				GenerateQuad(triStream, leftQuad, color, forward_uv, norm);
			}
			if (pitch >= 0 || pitch <= -0)
			{
				aTop = 1 - (1 - abs(pitch));
				color.a = aTop * fade;
				GenerateQuad(triStream, topQuad, color, top_uv, norm);

			}
		}
	}

	half4 frag(g2f IN) : COLOR
	{
		// ---------------- leaf texture ----------------
		fixed4 leaf = tex2D(_MainTex, IN.uv) + tex2D(_Mask, IN.uv) * _LeafColor * (_LeafColorModify * (IN.diffuseColor.r * _RandomColor));

		// ---------------- trunk texture ----------------
		fixed4 trunk = tex2D(_MainTex, IN.uv) * _TrunkColor;
		trunk = trunk + (trunk * _TrunkColorModify * (IN.diffuseColor.r * _RandomColor));

		clip(trunk.a - _Cutoff);

		//clip((trunk.a * IN.diffuseColor.a) -_Cutoff);

		fixed4 colorOut = trunk;

		/*if (IN.diffuseColor.a > 1) { colorOut.a = 1; }
		else { colorOut.a = IN.diffuseColor.a; }*/

		colorOut.a = IN.diffuseColor.a;

		// ---------------- lighting color ----------------
		fixed3 lighting = IN.diff + IN.ambient;
		colorOut.rgb *= lighting;
		//colorOut.a = 0;

		return colorOut;
	}