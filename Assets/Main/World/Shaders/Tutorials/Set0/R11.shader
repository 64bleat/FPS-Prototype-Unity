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

			// NOISE
			float rand3dTo1d(float3 position, float3 dotDir = float3(12.9898, 78.233, 37.719))
			{
				float3 smallValue = sin(position);
				float random = dot(smallValue, dotDir);
				random = frac(sin(random) * 143758.5453);
				return random;
			}

			float3 rand3dTo3d(float3 value)
			{
				return float3(
					rand3dTo1d(value, float3(12.989, 78.233, 37.719)),
					rand3dTo1d(value, float3(39.346, 11.135, 83.155)),
					rand3dTo1d(value, float3(73.156, 52.235, 09.151)));
			}

			// Frac SMoothing
			float smoothFrac(float value)
			{
				float low = value * value;
				float high = 1 - (1 - value) * (1 - value);
				return lerp(low, high, value);
			}

			float3 smoothFrac3(float3 value)
			{
				return float3(smoothFrac(value.x), smoothFrac(value.y), smoothFrac(value.z));
			}

			float3 ValueNoise3d(float3 value)
			{
				float interpolatorX = smoothFrac(frac(value.x));
				float interpolatorY = smoothFrac(frac(value.y));
				float interpolatorZ = smoothFrac(frac(value.z));

				float3 cellNoiseZ[2];
				[unroll]
				for(int z=0;z<=1;z++)
				{
					float3 cellNoiseY[2];

					[unroll]
					for(int y=0;y<=1;y++)
					{
						float3 cellNoiseX[2];

						[unroll]
						for(int x=0;x<=1;x++)
						{
							float3 cell = floor(value) + float3(x, y, z);

							cellNoiseX[x] = rand3dTo3d(cell);
						}

						cellNoiseY[y] = lerp(cellNoiseX[0], cellNoiseX[1], interpolatorX);
					}

					cellNoiseZ[z] = lerp(cellNoiseY[0], cellNoiseY[1], interpolatorY);
				}

				float3 noise = lerp(cellNoiseZ[0], cellNoiseZ[1], interpolatorZ);

				return noise;
			}

			float3 perlinNoise(float3 value)
			{
				float3 fraction = frac(value);

				float interpolatorX = smoothFrac(fraction.x);
				float interpolatorY = smoothFrac(fraction.y);
				float interpolatorZ = smoothFrac(fraction.z);

				float3 cellNoiseZ[2];

				[unroll]
				for(int z=0;z<=1;z++)
				{
					float3 cellNoiseY[2];
					[unroll]

					for(int y=0;y<=1;y++)
					{
						float3 cellNoiseX[2];

						[unroll]
						for(int x=0;x<=1;x++)
						{
							float3 cell = floor(value) + float3(x, y, z);
							float3 cellDirection = rand3dTo3d(cell) * 2 - 1;
							float3 compareVector = fraction - float3(x, y, z);

							cellNoiseX[x] = dot(cellDirection, compareVector);
						}

						cellNoiseY[y] = lerp(cellNoiseX[0], cellNoiseX[1], interpolatorX);
					}

					cellNoiseZ[z] = lerp(cellNoiseY[0], cellNoiseY[1], interpolatorY);
				}

				float3 noise = lerp(cellNoiseZ[0], cellNoiseZ[1], interpolatorZ);

				return noise * 0.5 + 0.5;
			}

			float3 perlinNoise3(float3 value)
			{
				float3 fraction = frac(value);

				float interpolatorX = smoothFrac(fraction.x);
				float interpolatorY = smoothFrac(fraction.y);
				float interpolatorZ = smoothFrac(fraction.z);

				float3 cellNoiseZ[2];

				[unroll]
				for(int z=0;z<=1;z++)
				{
					float3 cellNoiseY[2];
					[unroll]

					for(int y=0;y<=1;y++)
					{
						float3 cellNoiseX[2];

						[unroll]
						for(int x=0;x<=1;x++)
						{
							float3 cell = floor(value) + float3(x, y, z);
							float3 cellDirection = rand3dTo3d(cell) * 2 - 1;

							cellNoiseX[x] = cellDirection;
						}

						cellNoiseY[y] = lerp(cellNoiseX[0], cellNoiseX[1], interpolatorX);
					}

					cellNoiseZ[z] = lerp(cellNoiseY[0], cellNoiseY[1], interpolatorY);
				}

				float3 noise = lerp(cellNoiseZ[0], cellNoiseZ[1], interpolatorZ);

				return noise * 0.5 + 0.5;
			}

			float3 voronoiNoise(float3 value)
			{
				float3 baseCell = floor(value);

				//first pass to find the closest cell
				float minDistToCell = 10;
				float3 toClosestCell;
				float3 closestCell;
				[unroll]
				for(int x1=-1; x1<=1; x1++){
					[unroll]
					for(int y1=-1; y1<=1; y1++){
						[unroll]
						for(int z1=-1; z1<=1; z1++){
							float3 cell = baseCell + float3(x1, y1, z1);
							float3 cellPosition = cell + rand3dTo3d(cell);
							float3 toCell = cellPosition - value;
							float distToCell = length(toCell);
							if(distToCell < minDistToCell){
								minDistToCell = distToCell;
								closestCell = cell;
								toClosestCell = toCell;
							}
						}
					}
				}

				//second pass to find the distance to the closest edge
				float minEdgeDistance = 10;
				[unroll]
				for(int x2=-1; x2<=1; x2++){
					[unroll]
					for(int y2=-1; y2<=1; y2++){
						[unroll]
						for(int z2=-1; z2<=1; z2++){
							float3 cell = baseCell + float3(x2, y2, z2);
							float3 cellPosition = cell + rand3dTo3d(cell);
							float3 toCell = cellPosition - value;

							float3 diffToClosestCell = abs(closestCell - cell);
							bool isClosestCell = diffToClosestCell.x + diffToClosestCell.y + diffToClosestCell.z < 0.1;
							if(!isClosestCell){
								float3 toCenter = (toClosestCell + toCell) * 0.5;
								float3 cellDifference = normalize(toCell - toClosestCell);
								float edgeDistance = dot(toCenter, cellDifference);
								minEdgeDistance = min(minEdgeDistance, edgeDistance);
							}
						}
					}
				}

				float random = rand3dTo1d(closestCell);
				return float3(minDistToCell, random, minEdgeDistance);
			}


			fixed4 frag(v2f i) : SV_TARGET
			{
				i.worldPos *= _Scale;
				//float3 raw = perlinNoise3(i.worldPos + float3(0,1,0) * _Time[0] + (ValueNoise3d(i.worldPos + _SinTime[0]) * 2 - 1) * 5);
				float3 raw = voronoiNoise(i.worldPos + _Time[0]);
				//raw = step(frac(raw * 23), frac(raw * 59));
				//raw = normalize(raw);
				return fixed4(raw.xxx * raw.zzz, 1);
			}

			ENDCG
		}
	}
	FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}
