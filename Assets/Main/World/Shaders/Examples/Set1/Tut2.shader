Shader "Tutorial/2: Cg shader for RGB cube" 
{
	SubShader
	{
		Pass 
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			struct vertexOutput 
			{
				float4 position : SV_POSITION;
				nointerpolation float4 color : TEXCOORD0; //nointerpolation prevents vertex values from interpolating along a surface.
			};

			vertexOutput vert(float4 vertexPos : POSITION)
			{
				vertexOutput output;

				output.position = UnityObjectToClipPos(vertexPos);
				output.color = vertexPos + float4(0.5, 0.5, 0.5, 0.0);
				// Here the vertex shader writes output data
				// to the output structure. We add 0.5 to the 
				// x, y, and z coordinates, because the 
				// coordinates of the cube are between -0.5 and
				// 0.5 but we need them between 0.0 and 1.0. 
				return output;
			}

			float4 frag(vertexOutput input) : COLOR // fragment shader
			{
				return input.color;
				// Here the fragment shader returns the "color" input 
				// parameter with semantic TEXCOORD0 as nameless
				// output parameter with semantic COLOR.
			}

			ENDCG
		}
	}
}