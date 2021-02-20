Shader "Tutorial/1: Cg basic shader"  // defines the name of the shader 
{
	SubShader // Unity chooses the subshader that fits the GPU best
	{
	   Pass  // some shaders require multiple passes
		{
			CGPROGRAM // here begins the part in Unity's Cg
			#pragma vertex vert // this specifies the vert function as the vertex shader 
			#pragma fragment frag // this specifies the frag function as the fragment shader

			// vertex shader 
			// this line transforms the vertex input parameter 
			// and returns it as a nameless vertex output parameter 
			// Updated outdated mul() function with UnityObjectToClipPos() function
			float4 vert(float4 vertexPos : POSITION) : SV_POSITION
			{
				//return UnityObjectToClipPos(vertexPos);
				return UnityObjectToClipPos(float4(1 - vertexPos[1] * 50, 1, 1, 1.0) * vertexPos);
			}

			// fragment shader
			// this fragment shader returns a nameless fragment
			// output parameter (with semantic COLOR) that is set to
			// opaque red (red = 1, green = 0, blue = 0, alpha = 1)
			float4 frag(void) : COLOR 
			{
				return float4(1.0, 0.0, 0.0, 1.0);
			}
			ENDCG // here ends the part in Cg 
		}
	}
}