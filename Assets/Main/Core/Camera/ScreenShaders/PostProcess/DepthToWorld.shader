// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/CameraDepthToWorldPosition"
{
    Properties
    {

    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
	uniform sampler2D_float _CameraDepthTexture;

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
		float2 uv : TEXCOORD0;
		float3 worldDirection : TEXCOORD1;
		float4 vertex : SV_POSITION;
    };

    float4x4 clipToWorld;

    v2f vert (appdata v)
    {
		v2f o;

		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;

		float4 clip = float4(o.vertex.xy, 0.0, 1.0);
		o.worldDirection = mul(clipToWorld, clip) - _WorldSpaceCameraPos;

		return o;
    }
    ENDCG

    SubShader
    {
        Cull Off ZWrite OFF ZTest Off

        Pass
        {
            Name "WorldPosition"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i) : SV_Target
            {
				float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
				float linearDepth = LinearEyeDepth(rawDepth);
                float3 localPosition = i.worldDirection * linearDepth;
				float3 worldPosition = localPosition + _WorldSpaceCameraPos;
                float depth = length(localPosition);

                return float4(worldPosition, depth);
            }
            ENDCG
        }

                Pass
        {
            Name "WorldPositionDebug"
            BLEND SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag (v2f i) : SV_Target
            {
				float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv.xy);
				float linearDepth = LinearEyeDepth(rawDepth);
                float3 localPosition = i.worldDirection * linearDepth;
				float3 worldPosition = localPosition + _WorldSpaceCameraPos;
                float depth = length(localPosition);

                float d = min(1, frac(depth) + 0.001);
				float4 col = float4(frac(worldPosition), d);

				return float4(pow((max(col, 1 - col) - 0.5) * 2, 5).rgb, pow(1 / (depth / 10 + 1), 3));// * (max(d, 1 - d) - 0.5) * 2;
            }
            ENDCG
        }
    }
}
