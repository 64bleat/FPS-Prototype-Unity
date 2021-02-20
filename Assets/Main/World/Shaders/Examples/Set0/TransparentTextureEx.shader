Shader "Hidden/TransparencyEx"
{
	Properties
	{
		_Color("Color", Color) = (0,0,0,1)
		_MainTex("Texture", 2D) =  "white" {}
	}
	SubShader
	{
		Tags{ "RenderType"="Transparent" "Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		BlendOp Add
		ZWrite Off
		Pass
		{
			CGPROGRAM
			#include "unityCG.cginc"

			#pragma vertex Vert
			#pragma fragment frag

			fixed4 _Color;
			sampler2D _MainTex;

			struct appdata
			{
				float4 vertex : POSITION;	// POSITION is an attribute identifying what vertex will act as a surrogate for.
				float2 uv : TEXCOORD0;		// TEXCOORD0 is usually texture coordinates
			};

			struct v2f
			{
				float4 position : SV_POSITION; // SV_POSITION is position relative to the screen
				float2 uv : TEXCOORD0;
			};

			v2f Vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv) * _Color; // Get the color at the pixel uv coordinates and color it.
			}

			ENDCG
		}
	}
}