Shader "Billboards/ShadedCutout"
{
   Properties 
   {
		_MainTex("Diffuse", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1) 
		_SpecColor ("Specular Color", Color) = (1,1,1,1) 
		_Shininess ("Shininess", Float) = 10
   }
   CGINCLUDE
		#include "UnityCG.cginc"
		sampler2D _MainTex;
		float4 _MainTex_ST;
   		uniform float4 _Color; 
		uniform float4 _SpecColor; 
		uniform float _Shininess;
		uniform float4 _LightColor0; // color of light source (from "Lighting.cginc")

   		struct vertexInput 
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct vertexOutput 
		{
			float4 pos : SV_POSITION;
			float4 posWorld : TEXCOORD2;
			float3 normalDir : TEXCOORD1;
			float2 uv : TEXCOORD0;
		};

		vertexInput BillboardTransform(vertexInput v)
		{
			vertexInput o;

			#if defined(USING_STEREO_MATRICES)
				float3 cameraPos = lerp(unity_StereoWorldSpaceCameraPos[0], unity_StereoWorldSpaceCameraPos[1], 0.5);
			#else
				float3 cameraPos = _WorldSpaceCameraPos;
			#endif

			float3 forward = normalize(cameraPos - mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz);
			float3 right = cross(forward, float3(0, 1, 0));
			float yawCamera = atan2(right.x, forward.x) - UNITY_PI / 2;//Add 90 for quads to face towards camera
			float s, c;
			sincos(yawCamera, s, c);
			float3x3 transposed = transpose((float3x3)unity_ObjectToWorld);
			float3 scale = float3(length(transposed[0]), length(transposed[1]), length(transposed[2]));

			float3x3 newBasis = float3x3(
				float3(c * scale.x, 0, s * scale.z),
				float3(0, scale.y, 0),
				float3(-s * scale.x, 0, c * scale.z)
				);//Rotate yaw to point towards camera, and scale by transform.scale

			float4x4 objectToWorld = unity_ObjectToWorld;
			//Overwrite basis vectors so the object rotation isn't taken into account
			objectToWorld[0].xyz = newBasis[0];
			objectToWorld[1].xyz = newBasis[1];
			objectToWorld[2].xyz = newBasis[2];
			//Now just normal MVP multiply, but with the new objectToWorld injected in place of matrix M
			o.vertex = mul(unity_WorldToObject, mul(objectToWorld, v.vertex));
			o.normal = mul(unity_WorldToObject, mul(objectToWorld, float4(v.normal, 0)));
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);

			return o;
		}

		vertexOutput vert(vertexInput input) 
		{
			input = BillboardTransform(input);
				
			vertexOutput output;
			output.posWorld = mul(unity_ObjectToWorld, input.vertex);
			output.normalDir = input.normal;
			output.pos = UnityObjectToClipPos(input.vertex);
			output.uv = input.uv;

			return output;
		}

		float4 Lighting(vertexOutput input)
		{
			float3 normalDirection = normalize(input.normalDir);
			float3 viewDirection = normalize(_WorldSpaceCameraPos - input.posWorld.xyz);
			float3 lightDirection;
			float attenuation;
 
			if (0.0 == _WorldSpaceLightPos0.w) // directional light?
			{
				attenuation = 1.0; // no attenuation
				lightDirection = normalize(_WorldSpaceLightPos0.xyz);
			} 
			else // point or spot light
			{
				float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - input.posWorld.xyz;
				float distance = length(vertexToLightSource);

				attenuation = 1.0 / distance; // linear attenuation 
				lightDirection = normalize(vertexToLightSource);
			}
 
			float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0, dot(normalDirection, lightDirection));
			float3 specularReflection;

			if (dot(normalDirection, lightDirection) < 0.0) // light source on the wrong side?
				specularReflection = float3(0.0, 0.0, 0.0);	// no specular reflection
			else											// light source on the right side
				specularReflection = attenuation * _LightColor0.rgb 
					* _SpecColor.rgb * pow(max(0.0, dot(
					reflect(-lightDirection, normalDirection), 
					viewDirection)), _Shininess);

			// no ambient lighting in this pass
			return float4((diffuseReflection + specularReflection), 1.0);  
		}
   ENDCG
   SubShader 
   {
      Pass	// pass for ambient light and first light source
	  {	
         Tags { "LightMode" = "ForwardBase" } 
         CGPROGRAM
			 #pragma vertex vert  
			 #pragma fragment frag 
			 #include "UnityCG.cginc"

			 //uniform float4 _LightColor0; // color of light source (from "Lighting.cginc")

			 float4 frag(vertexOutput input) : COLOR
			 {
				return Lighting(input) * UNITY_LIGHTMODEL_AMBIENT * tex2D(_MainTex, input.uv);
			 }
         ENDCG
      }
      Pass // pass for additional light sources
	  {	
         Tags { "LightMode" = "ForwardAdd" }  
         Blend One One // additive blending 
         CGPROGRAM
			 #pragma vertex vert  
			 #pragma fragment frag 
			 #include "UnityCG.cginc"

			 //uniform float4 _LightColor0; // color of light source (from "Lighting.cginc")
 
			float4 frag(vertexOutput input) : COLOR
			{
				 return Lighting(input) * tex2D(_MainTex, input.uv);
			}
         ENDCG
      }
   }
   //Fallback "Specular"
}