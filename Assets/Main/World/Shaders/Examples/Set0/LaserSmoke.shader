Shader "Hidden/LaserSmoke"
{
	Properties
	{
		_Scale("Scale", Float) = 1
	}

	SubShader
	{
		Tags{ "RenderType"="Transparent" "Queue"="Transparent"}
		Blend One One
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "Noise.cginc"

			#pragma vertex vert
			#pragma fragment frag

			float _Scale;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float3 worldPos : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex) * _Scale;
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET
			{
				i.worldPos *= _Scale;
				float time = _Time[0] * 0.1;
				float sintime = sin(time);
				float3 randScale = noise3d3d(i.worldPos + time) * 2 - 1;
				float3 random = noise3d3d(i.worldPos * randScale + sintime + time * 10);
				random *= noise3d3d(random);

				float3 raw = noise3d3d(i.worldPos + (random * 2 - 1) * 5);

				//raw = step(raw, 0.5) - step(raw, 0.8);//step(frac(raw * 23), frac(raw * 59));
				//raw = normalize(raw);
				raw = sin(raw * 200 * random) * pow(raw * 2 - 1, 2) * 2;
				raw = max(float3(0,0,0), raw);

				return float4(0, raw.y, 0, raw.y);

				// Voronoi
				//float3 raw = voronoi3dInfo(i.worldPos + _Time[0]);
				return fixed4(raw.xxx * raw.zzz, 1);
			}

			ENDCG
		}
	}
	FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}
