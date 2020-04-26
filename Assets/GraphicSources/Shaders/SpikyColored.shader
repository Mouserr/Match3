Shader "Custom/SpikyColored"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_ColorCoef ("Color Coef", float) = 0.2
        _SpikeSize ("Spike Size", float) = 1
    }
    SubShader
    {
		Pass {
			Tags { "RenderType"="Opaque" }
			Tags {"LightMode"="ForwardBase"}
			LOD 200

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
 
			struct v2g
			{
				float4 objPos : SV_POSITION;
				float3 normal : NORMAL;
				fixed4 col : COLOR;
			};
 
			struct g2f
			{
				float4 worldPos : SV_POSITION;
				fixed4 col : COLOR;
			};

			fixed4 _Color;
			float _ColorCoef;
			float _SpikeSize;

			v2g vert (appdata v)
			{
				v2g o;
				o.objPos = v.vertex;
				o.normal = v.normal;
				o.col = _Color;
				return o;
			}

			void addTriangle(inout TriangleStream<g2f> tristream, float4 center, fixed4 centerColor, v2g base1, v2g base2)
			{
				g2f o;
				o.worldPos = UnityObjectToClipPos(base1.objPos);
				o.col = base1.col * (1 - _ColorCoef);
				tristream.Append(o);
 
				o.worldPos = UnityObjectToClipPos(center);
				o.col = centerColor * (1 + _ColorCoef);

				tristream.Append(o);
 
				o.worldPos = UnityObjectToClipPos(base2.objPos);
				o.col = base2.col * (1 - _ColorCoef);
				tristream.Append(o);

				tristream.RestartStrip();
			}

			float3 getFaceNormal(triangle v2g input[3])
			{
				return normalize(cross(input[1].objPos - input[0].objPos, input[2].objPos - input[0].objPos));
			}

			void addSpikeOnBigEdge(triangle v2g input[3], inout TriangleStream<g2f> tristream)
			{
				float3 faceNormal = getFaceNormal(input);

				// determine which lateral side is the longest
				float edge0 = distance(input[1].objPos, input[2].objPos);
				float edge1 = distance(input[2].objPos, input[0].objPos);
				float edge2 = distance(input[1].objPos, input[0].objPos);

				float4 centralPos;
				fixed4 centralColor;
		
				if (step(edge1, edge2) * step(edge0, edge2) == 1)
				{
					centralPos = lerp(input[1].objPos, input[0].objPos, 0.5f);
					centralColor = lerp(input[1].col, input[0].col, 0.5f);
					centralPos += float4(faceNormal,0) * _SpikeSize;
					addTriangle(tristream, centralPos, centralColor, input[2], input[1]);
					addTriangle(tristream, centralPos, centralColor, input[0], input[2]);
				}
				else if (step(edge0, edge1) * step(edge2, edge1) == 1)
				{
					centralPos = lerp(input[2].objPos, input[0].objPos, 0.5f);
					centralColor = lerp(input[2].col, input[0].col, 0.5f);
					centralPos += float4(faceNormal,0) * _SpikeSize;
					addTriangle(tristream, centralPos, centralColor, input[1], input[0]);
					addTriangle(tristream, centralPos, centralColor, input[2], input[1]);
				}
				else
				{
					centralPos = lerp(input[1].objPos, input[2].objPos, 0.5f);
					centralColor = lerp(input[1].col, input[2].col, 0.5f);
					centralPos += float4(faceNormal,0) * _SpikeSize;
					addTriangle(tristream, centralPos, centralColor, input[1], input[0]);
					addTriangle(tristream, centralPos, centralColor, input[0], input[2]);
				}

				addTriangle(tristream, input[0].objPos, input[0].col, input[2], input[1]);
			}

			void addSpikeInCenter(triangle v2g input[3], inout TriangleStream<g2f> tristream)
			{
				float3 faceNormal = getFaceNormal(input);

				float edge0 = distance(input[1].objPos, input[2].objPos);
				float edge1 = distance(input[2].objPos, input[0].objPos);
				float edge2 = distance(input[1].objPos, input[0].objPos);
				
				float4 centralPos = lerp(input[1].objPos, input[2].objPos, 0.5f);
				centralPos = lerp(input[0].objPos, centralPos, 0.5f);
				float4 centralColor = lerp(input[1].col, input[2].col, 0.5f);
				centralColor = lerp(input[0].col, centralColor, 0.5f);
				centralPos += float4(faceNormal,0) * _SpikeSize;

				for(uint i = 0; i < 3; i++)
				{
					uint nexti = (i+1)%3;
					addTriangle(tristream, centralPos, centralColor, input[nexti], input[i]);
				}
			}

			[maxvertexcount(12)]
			void geom (triangle v2g input[3], inout TriangleStream<g2f> tristream)
			{
				addSpikeOnBigEdge(input, tristream);
				//addSpikeInCenter(input, tristream);
			}

			fixed4 frag (g2f i) : SV_Target
			{
				return i.col;
			}
			ENDCG
		}
    }
    FallBack "Diffuse"
}
