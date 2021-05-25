Shader "Tutorial/6: Cg silhouette enhancement" 
{
	Properties
	{
	   [HDR] _Color("Color", Color) = (1, 1, 1, 0.5)
	   _Intensity("Intensity", Float) = 1
	}
	SubShader
	{
	//Tags { "Queue" = "Transparent" } 
		Pass 
		{
			//Tags { "Queue" = "Transparent" } 
			//Blend OneMinusDstColor One
			//Cull Back
			ZWrite On
			CGPROGRAM
				#include "UnityCG.cginc"
				#pragma vertex vert  
				#pragma fragment frag 

				uniform float4 _Color; // define shader property for shaders
				uniform float _Intensity;

				struct vertexInput 
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
				};

				struct vertexOutput 
				{
					float4 pos : SV_POSITION;
					float3 normal : TEXCOORD;
					float3 viewDir : TEXCOORD1;
				};

				vertexOutput vert(vertexInput input)
				{
					vertexOutput output;

					float4x4 modelMatrix = unity_ObjectToWorld;
					float4x4 modelMatrixInverse = unity_WorldToObject;

					output.normal = normalize(mul(float4(input.normal, 0.0), modelMatrixInverse).xyz);
					output.viewDir = normalize(_WorldSpaceCameraPos - mul(modelMatrix, input.vertex).xyz);
					output.pos = UnityObjectToClipPos(input.vertex);

					return output;
				}

				float4 frag(vertexOutput input) : COLOR
				{
					float3 normalDirection = normalize(input.normal);
					float3 viewDirection = normalize(input.viewDir);
					float viewDot = (1.0 + dot(normalize(viewDirection + float3(0, 0.75, 0)), normalDirection)) / 2;
					viewDot = max(0, (pow(viewDot, 2) + abs(cos(acos(pow(abs(viewDot), 2.0 - viewDot)) * 10 * (1.0 - viewDot) + _Time[1]))));

					float newOpacity = pow(min(1.0, _Color.a / viewDot), _Intensity);

					return float4(_Color.rgb * newOpacity, newOpacity);
				}
			ENDCG
		}
	}
}