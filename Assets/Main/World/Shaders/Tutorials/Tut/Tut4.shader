// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Tutorial/4: Cg shading in world space" 
{
	SubShader
	{
		Pass 
		{
			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag 

			// uniform float4x4 unity_ObjectToWorld; 
			// automatic definition of a Unity-specific uniform parameter

			struct vertexInput 
			{
				float4 vertex : POSITION;
			};

			struct vertexOutput 
			{
				float4 pos : SV_POSITION;
				float4 posGlobal : TEXCOORD0;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				output.pos = UnityObjectToClipPos(input.vertex); //local position
				output.posGlobal = mul(unity_ObjectToWorld, input.vertex); //global position
				// transformation of input.vertex from object 
				// coordinates to world coordinates;

				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				float dist = distance(input.posGlobal, float4(0.0, 0.0, 0.0, 1.0));
				dist *= 40;
				return float4(sin(dist) / 2 + 0.5, sin(dist + 1) / 2 + 0.5, sin(dist + 2), 1) / 2 + 0.5;
			}	

			ENDCG
		}
	}
}