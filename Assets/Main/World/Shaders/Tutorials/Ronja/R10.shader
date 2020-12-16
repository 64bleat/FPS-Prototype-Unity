Shader "Tutorial/R10 - Checker"
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

			float rand3dTo1d(float3 value, float3 dotDir = float3(12.9898, 78.233, 37.719))
			{
				float3 smallValue = sin(value);
				float random = dot(smallValue, dotDir);
				random = frac(sin(random) * 143758.5453);
				return random;
			}

			inline float easeIn(float interpolator)
			{
				return interpolator * interpolator;
			}


			float easeOut(float interpolator)
			{
				return 1 - easeIn(1 - interpolator);
			}

			float easeInOut(float interpolator)
			{
				float easeInValue = easeIn(interpolator);
				float easeOutValue = easeOut(interpolator);
				return lerp(easeInValue, easeOutValue, interpolator);
			}


			float ValueNoise3d(float3 value)
			{
				float interpolatorX = easeInOut(frac(value.x));
				float interpolatorY = easeInOut(frac(value.y));
				float interpolatorZ = easeInOut(frac(value.z));

				float cellNoiseZ[2];
				[unroll]
				for(int z=0;z<=1;z++)
				{
					float cellNoiseY[2];

					[unroll]
					for(int y=0;y<=1;y++)
					{
						float cellNoiseX[2];

						[unroll]
						for(int x=0;x<=1;x++)
						{
							float3 cell = floor(value) + float3(x, y, z);
							cellNoiseX[x] = rand3dTo1d(cell);
						}

						cellNoiseY[y] = lerp(cellNoiseX[0], cellNoiseX[1], interpolatorX);
					}

					cellNoiseZ[z] = lerp(cellNoiseY[0], cellNoiseY[1], interpolatorY);
				}

				float noise = lerp(cellNoiseZ[0], cellNoiseZ[1], interpolatorZ);

				return noise;
			}

			fixed4 frag(v2f i) : SV_TARGET
			{
				i.worldPos *= _Scale;				
				//float chessboard = floor(i.worldPos.x) + floor(i.worldPos.y) + floor(i.worldPos.z);
				//chessboard = frac(chessboard * 0.5);
				//chessboard *= 2;
				return ValueNoise3d(i.worldPos);//rand3dTo1d(floor(i.worldPos));
			}

			


			ENDCG
		}
	}
	FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}
