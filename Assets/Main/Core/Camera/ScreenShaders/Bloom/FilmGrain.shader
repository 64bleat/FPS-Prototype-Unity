Shader "Hidden/FilmGrain"
{
    Properties
    {
    }
    SubShader
    {
        Cull Off
		ZWrite Off
		ZTest Always

        Pass
        {
			Name "FilmGrain"
			Blend One One
            CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				float _Grain;
				float3 _Seed;

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				float rand3dTo1d(float3 value, float3 dotDir = float3(12.9898, 78.233, 37.719))
				{
					return frac(sin(dot(sin(value), dotDir)) * 19375.39472);
				}

				v2f vert (appdata v)
				{
					v2f i;

					i.pos = UnityObjectToClipPos(v.vertex);
					i.uv = v.uv;

					return i;
				}

				float4 frag (v2f i) : SV_Target
				{
					float3 rgb;

					i.uv += _SinTime[0];
					rgb.r = rand3dTo1d(i.uv.xyy, _Seed);
					rgb.g = rand3dTo1d(i.uv.xyy, _Seed + 1);
					rgb.b = rand3dTo1d(i.uv.xyy, _Seed + 2);
					rgb -= 0.5f;
					rgb *= _Grain;

					return float4(rgb, 1);
				}
            ENDCG
        }
    }
}
