Shader "Tutorial/R03 - Solid Color Adjustable"
{
	// Properties pass variables from the material inspector to the shader
	Properties
	{
		_Color("Color", Color) = (0,0,0,1)
	}

	// Write multiple subshaders and later tell unity which one to pick.
	SubShader
	{
		// Passes are drawn consecutively.
		Pass
		{
			Tags
			{
				"RenderType" = "Opaque" // Render is completely opaque
				"Queue" = "Geometry"	// Draw with other opaque objects
			}

			CGPROGRAM
			#include "unityCG.cginc"

			#pragma vertex Vert
			#pragma fragment frag

			fixed4 _Color;

			struct appdata
			{
				float4 vertex : POSITION; // POSITION is an attribute identifying what vertex will act as a surrogate for.
			};

			struct v2f
			{
				float4 position : SV_POSITION; // SV_POSITION is position relative to the screen
			};

			v2f Vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return _Color;
			}

			ENDCG
		}
	}
}