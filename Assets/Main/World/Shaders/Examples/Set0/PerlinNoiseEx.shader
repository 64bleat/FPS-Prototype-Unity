Shader "Hidden/PerlinNoiseEx"
{
	Properties
	{
		_Scale("Scale", Float) = 1
	}

	SubShader
	{
		Tags{ "RenderType"="Opaque" "Queue"="Geometry"}
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

				float noise1d = noise3d1d(i.worldPos);
				return float4(noise1d, noise1d, noise1d, 1);


				//float chessboard = floor(i.worldPos.x) + floor(i.worldPos.y) + floor(i.worldPos.z);
				//chessboard = frac(chessboard * 0.5);
				//chessboard *= 2;
				//return rand3d1d(floor(i.worldPos));
			}
			ENDCG
		}
	}
	FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}
