// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Tutorial/5: Cg shader using discard" 
{
	SubShader
	{
		Pass 
		{
			Cull Back // turn off triangle culling, alternatives are:
			// Cull Back (or nothing): cull only back faces 
			// Cull Front : cull only front faces

			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag 

			struct vertexInput 
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct vertexOutput 
			{
				float4 pos : SV_POSITION;
				float4 posInObjectCoords : TEXCOORD0;
				float3 normal : NORMAL;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;
				output.pos = UnityObjectToClipPos(input.vertex);
				output.posInObjectCoords = input.vertex;
				output.normal = input.normal;

				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				if (input.posInObjectCoords.y > 0)
					discard; // drop the fragment if y coordinate > 0\

				return float4(input.normal[0] / 2 + 0.5, input.normal[1] / 2 + 0.5, input.normal[2] / 2 + 0.5, 1.0); // green
			}

			ENDCG
		}

		Pass //reverse pass with flipped normals
		{
			Cull Off

			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag 

			struct vertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float4 posInObjectCoords : TEXCOORD0;
				float3 normal : NORMAL;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;
				output.pos = UnityObjectToClipPos(input.vertex);
				output.posInObjectCoords = input.vertex;
				output.normal = input.normal;

				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				//if (input.posInObjectCoords.y > 0)
					//discard;

				return float4(-input.normal[0] / 2 + 0.5, -input.normal[1] / 2 + 0.5, -input.normal[2] / 2 + 0.5, 1.0); // green
			}

			ENDCG
		}
	}
}