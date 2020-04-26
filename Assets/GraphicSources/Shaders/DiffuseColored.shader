Shader "Custom/DiffuseColored"
{
    Properties
    {
	    _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
		Pass 
		{
			Tags { "RenderType"="Opaque" }
			Tags {"LightMode"="ForwardBase"}
			LOD 200

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
 
			struct v2f
			{
				float4 objPos : SV_POSITION;
				fixed4 col : COLOR;
			};
 
			fixed4 _Color;

			half lambert(float3 normal)
			{
				// get vertex normal in world space
				half3 worldNormal = UnityObjectToWorldNormal(normal);
				// dot product between normal and light direction for
				// standard diffuse (Lambert) lighting
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				return nl;
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.objPos = UnityObjectToClipPos(v.vertex);
				o.col = lambert(v.normal) * _Color;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return i.col;
			}
			ENDCG
		}
    }
    FallBack "Diffuse"
}
