// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Tutorial/7: Cg per-vertex diffuse lighting" 
{
	Properties
	{
	   _Color("Diffuse Material Color", Color) = (1,1,1,1)
	}
		
	SubShader
	{
		//this pass does not loop.
		Pass 
		{
			// make sure that all uniforms are correctly set
			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert  
			#pragma fragment frag 

			#include "UnityCG.cginc"

			// color of light source (from "Lighting.cginc")
			uniform float4 _LightColor0;

			// define shader property for shaders
			uniform float4 _Color; 

			struct vertexInput 
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct vertexOutput 
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;
				float4x4 modelMatrix = unity_ObjectToWorld;
				float4x4 modelMatrixInverse = unity_WorldToObject;
				float3 normalDirection = normalize(mul(float4(input.normal, 0.0), modelMatrixInverse).xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 diffuseReflection = _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection));

				output.col = float4(diffuseReflection, 1.0);
				output.pos = UnityObjectToClipPos(input.vertex);

				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				return input.col;
			}

				ENDCG
		}

		//This pass loops for every light.
		Pass
			{
					Tags {"LightMode" = "ForwardAdd"}
					Blend One One //additive blending

					CGPROGRAM

	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"

					//color of light source
					uniform float4 _LightColor0; 
					uniform float4 _Color;

					struct vertexInput
					{
						float4 vertex : POSITION;
						float3 normal : NORMAL;
						float3 color : COLOR;
					};

					struct vertexOutput
					{
						float4 pos : SV_POSITION;
						float4 col : COLOR;
					};

					vertexOutput vert(vertexInput input)
					{
						vertexOutput output;

						float4x4 modelMatrix = unity_ObjectToWorld;
						float4x4 modelMatrixInverse = unity_WorldToObject;
						float3 normalDirection = normalize(mul(float4(input.normal, 0.0), modelMatrixInverse).xyz);
						float3 lightDirection;
						float3 diffuseReflection;
						float attenuation;

						if (_WorldSpaceLightPos0.w == 0.0)
						{
							attenuation = 1.0;
							normalDirection = normalize(mul(float4(input.normal, 0.0), modelMatrixInverse).xyz);
						}
						else
						{
							float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - mul(modelMatrix, input.vertex).xyz;
							float distance = length(vertexToLightSource);

							attenuation = 1.0 / distance;

							lightDirection = normalize(_WorldSpaceLightPos0.xyz - mul(unity_ObjectToWorld, input.vertex).xyz);
						}

						diffuseReflection = input.color * _LightColor0.rgb * attenuation * _Color.rgb * max(0.0, dot(normalDirection, lightDirection));

						output.col = float4(diffuseReflection, 1.0);
						output.pos = UnityObjectToClipPos(input.vertex);

						return output;
					}

					float4 frag(vertexOutput input) : COLOR
					{
						return input.col;
					}

					ENDCG
		}
	}
	Fallback "Diffuse"
}