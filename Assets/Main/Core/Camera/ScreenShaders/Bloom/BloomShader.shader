Shader "ScreenShaders/BloomShader"
{
    Properties
    {
       [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
    }

	CGINCLUDE
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _SourceTex;
		float4 _MainTex_TexelSize;
		float4 _Filter;
		float _Intensity;
		float _Scale;

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

		v2f vert (appdata v)
		{
			v2f i;

			i.pos = UnityObjectToClipPos(v.vertex);
			i.uv = v.uv;

			return i;
		}

		float3 Prefilter(float3 c)
		{
			float brightness = max(c.r, max(c.g, c.b));
			float soft = brightness - _Filter.y;
			soft = clamp(soft, 0, _Filter.z);
			soft = soft * soft * _Filter.w;
			float contribution = max(soft, brightness - _Filter.x) / max(brightness, 0.00001);

			return c * contribution;
			
		}

		float3 Sample(float2 uv)
		{
			return tex2D(_MainTex, uv).rgb;
		}

		float3 SampleBox(float2 uv, float delta)
		{
			float4 o = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
			float3 s = Sample(uv);
			float distance;
			int i; 

			[unroll]
			for(i = 1; i <= 4; i++)
			{
				distance = max(0, pow(i, 1.5) * _Scale);
				s = max(s, pow((
					Sample(uv + o.xy * distance) + 
					Sample(uv + o.zy * distance) + 
					Sample(uv + o.xw * distance) + 
					Sample(uv + o.zw * distance)) / 4 / (i + 0.2), 0.92 + i * 0.06));
			}

			return s;
		}
	ENDCG

    SubShader
    {
        Cull Off
		ZWrite Off
		ZTest Always

		Pass
        {
			Name "DownblendPrefilter" 
            CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				float4 frag (v2f i) : SV_TARGET
				{
					return float4(Prefilter(SampleBox(i.uv, 1)), 1);
				}
            ENDCG
        }

        Pass
        {
			Name "Downblend"
            CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				float4 frag (v2f i) : SV_TARGET
				{
					return float4(SampleBox(i.uv, 1), 1);
				}
            ENDCG
        }

		Pass 
        {
			Name "Upblend"
			Blend One One
            CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				float4 frag (v2f i) : SV_TARGET
				{
					return float4(SampleBox(i.uv, 0.5), 1);
				}
            ENDCG
        }

		Pass
		{
			Name "ApplyBloom"
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				float4 frag (v2f i) : SV_TARGET
				{
					float4 c = tex2D(_SourceTex, i.uv);

					c.rgb += _Intensity * SampleBox(i.uv, 0.5);

					c.rgb = lerp(c.rgb, c.rgb / max(max(c.r, max(c.g, c.b)), 1), 0.85);

					return c;
				}
            ENDCG
		}

		Pass
		{
			Name "DebugBloomPass"
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				float4 frag(v2f i) : SV_TARGET
				{
					return float4(_Intensity * SampleBox(i.uv, 0.5), 1);
				}
			ENDCG
		}

		Pass
        {
			Name "BoxUpCoolPass"
			Blend One One
            CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				float4 frag (v2f i) : SV_TARGET
				{
					float4 bloom = float4(SampleBox(i.uv, 0.5), 1);

					return max(0, bloom - pow(bloom, _Scale));
				}
            ENDCG
        }
    }
}
