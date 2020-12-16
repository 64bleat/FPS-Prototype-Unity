Shader "Pixelation"
{
	Properties
	{
		[Toggle] _bBloomEnabled("Enable Bloom", Float) = 1
		[Toggle] _bRescaleEnabled("Rescale Resolution", Float) = 1
		_Height("Vertical Resolution", Float) = 480
		_Width("Horizontal Resolution", Float) = 854
		[Toggle] _HSVMode("Convert to HSV", Float) = 0
		[Toggle] _VGAMode("Convert to VGA", Float) = 0
		[Toggle] _GSCMode("Greyscale Mode", Float) = 0
		_BitsR("Red/Hue Bits", Int) = 4
		_BitsG("Green/Saturation Bits", Int) = 3
		_BitsB("Blue/Value Bits", Int) = 2
		_BitsD("Dither Bits", Int) = 4
		_BitsF("Fog Bits", Int) = 4
		_DitherSpread("Dither Spread", Range(0, 1)) = 0.5
		
		//Bichrome
		[Toggle]_bBichromeEnabled("Enable Bichrome", Float) = 0
		_BichromeDark("Bichrome Dark", Color) = (0,0,0,1)
		_BichromeLight("Bichrome Light", Color) = (1,1,1,1)

		//Fog
		[Toggle]_bEnableFog("Enable Fog", Float) = 0
		_FogColor("Fog Color", Color) = (0, 0, 0, 1)
		_FogStart("Fog Start", Float) = 3
		_FogEnd("Fog End", Float) = 10

		//Distance Coloring
		[Toggle] _bDistanceColorEnabled("Enable Distance Color", Float) = 0
		_DistanceColor("DistanceColor", Color) = (1, 1, 1, 1)
		_DistanceColorStart("Distance Color Start", Float) = 3
		_DistanceColorEnd("Distance Color End", Float) = 10
		
		//invert
		_bInvert("Invert", Range(-5.0, 6.0)) = 0

		//Contrast
		_Contrast("Contrast", Range(-1, 1)) = 0.5

		[HideInInspector]
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			ZWrite On
			Tags { "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" "IgnoreProjector" = "True" }
			NAME "PIXELSCREEN"
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			//internal
			uniform sampler2D_float _CameraDepthTexture;

			sampler2D _MainTex;
			//sampler2D _Source;

			//float CameraVariables
			float _FOV;
			float _AspectRatio;
			float _NearPlane;
			float _FarPlane;

			//pixelation
			float _bRescaleEnabled;
			float _HSVMode;
			float _VGAMode;
			float _GSCMode;
			float _Height;
			float _Width;
			float _DitherSpread;

			//Bits
			uint _BitsR;
			uint _BitsG;
			uint _BitsB;
			uint _BitsD;
			uint _BitsF;

			//bloom
			float _bBloomEnabled;

			//inversion
			float _bInvert;

			//bichrome
			float _bBichromeEnabled;
			float4 _BichromeDark;
			float4 _BichromeLight;

			//fog
			float _bEnableFog;
			float4 _FogColor;
			float _FogStart;
			float _FogEnd;

			//Distance Color
			float _bDistanceColorEnabled;
			float4 _DistanceColor;
			float _DistanceColorStart;
			float _DistanceColorEnd;


			float _Contrast;


			struct appdata
			{
				//float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				return o;
			}

			float4 thresh(float4 c, float thresh)
			{
				if (c.r < thresh)
					c.r = 0;
				else
					c.r = c.r - thresh;

				if (c.g < thresh)
					c.g = 0;
				else
					c.g = c.g - thresh;

				if (c.b < thresh)
					c.b = 0;
				else
					c.b = c.b - thresh;


				return c;
			}


			fixed4 frag(v2f i) : COLOR
			{
				float dither[16] = { 2, 28, 12, 22, 20, 16, 26, 8, 6, 24, 4, 18, 31, 14, 30, 10};
				float2 resolution = { _Width, _Height };
				uint2 pixels = floor(i.uv * resolution);
				float bitsD = pow(2, _BitsD);
				float ditherThresh = bitsD > 0 ? dither[pixels.x % 4 + (pixels.y % 4) * 4] / 32 : 0.5;
				float depth;
				float2 uv = (_bRescaleEnabled ? pixels / resolution : i.uv);
				float4 c = tex2D(_MainTex, uv);
				float3 colors = { pow(2, _BitsR) - 1, pow(2, _BitsG) - 1, pow(2, _BitsB) -1};
				uint dp = (2 << _BitsF) - 1;
				float lum = max(c[0], max(c[1], c[2]));

				matrix <int, 1, 2> bleat;

				ditherThresh = 1 / bitsD / 2 + ditherThresh * (1 - 1 / bitsD / 2);
				ditherThresh = pow(round(ditherThresh * bitsD) / bitsD, tan(3.14059 / 2 * _DitherSpread));

				// Bloom
				if (_bBloomEnabled != 0)
				{
					float threshVal = 0.7;
					float2 j = float2((float)pixels.x / resolution.x, (float)pixels.y / resolution.y);

					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal));

					j = float2((float)pixels.x / resolution.x, (float)pixels.y / resolution.y + 1.0 / resolution.y);
					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal) * 0.5);
					j = float2((float)pixels.x / resolution.x, (float)pixels.y / resolution.y - 1.0 / resolution.y);
					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal) * 0.5);
					j = float2((float)pixels.x / resolution.x, (float)pixels.y / resolution.y + 2.0 / resolution.y);
					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal) * 0.2);
					j = float2((float)pixels.x / resolution.x, (float)pixels.y / resolution.y - 2.0 / resolution.y);
					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal) * 0.2);
					j = float2((float)pixels.x / resolution.x, (float)pixels.y / resolution.y + 3.0 / resolution.y);
					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal) * 0.1);
					j = float2((float)pixels.x / resolution.x, (float)pixels.y / resolution.y - 3.0 / resolution.y);
					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal) * 0.1);

					j = float2((float)pixels.x / resolution.x + 1.0 / resolution.x, (float)pixels.y / resolution.y);
					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal) * 0.5);
					j = float2((float)pixels.x / resolution.x - 1.0 / resolution.x, (float)pixels.y / resolution.y);
					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal) * 0.5);
					j = float2((float)pixels.x / resolution.x + 2.0 / resolution.x, (float)pixels.y / resolution.y);
					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal) * 0.2);
					j = float2((float)pixels.x / resolution.x - 2.0 / resolution.x, (float)pixels.y / resolution.y);
					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal) * 0.2);
					j = float2((float)pixels.x / resolution.x + 3.0 / resolution.x, (float)pixels.y / resolution.y);
					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal) * 0.1);
					j = float2((float)pixels.x / resolution.x - 3.0 / resolution.x, (float)pixels.y / resolution.y);
					c = max(c, thresh(pow(tex2D(_MainTex, j), 1), threshVal)* 0.1);
				}

				//greyscale
				if (_GSCMode != 0)
					c.r = c.g = c.b = max(c.r, max(c.g, c.b));
				
				//screen depth
				{
					float screenDist = atan(length(float2((uv.x - 0.5) * _AspectRatio, (uv.y - 0.5))));

					depth = tex2D(_CameraDepthTexture, uv);
					depth = _FarPlane / (_FarPlane - _NearPlane) * (_NearPlane / depth) / cos(_FOV * screenDist);
				}

				//Bichrome
				if (_bBichromeEnabled != 0)
				{
					float4 mix = lerp(_BichromeDark, _BichromeLight, max(c.r, max(c.g, c.b)));

					c.rgb = lerp(c.rgb, mix.rgb, mix.a);
				}
					
				//INVERT
				if (_bInvert != 0)
					c.rgb = lerp(c.rgb, 1.0 - c.rgb, _bInvert);

				//Distance Color
				if (_bDistanceColorEnabled != 0)
				{
					float distance = (clamp(depth, _DistanceColorStart, _DistanceColorEnd) - _DistanceColorStart) / (_DistanceColorEnd - _DistanceColorStart) * _DistanceColor.a;

					c.rgb = lerp(c.rgb, _DistanceColor * max(c.r, max(c.g, c.b)), distance);
				}

				//FOG
				if (_bEnableFog)
				{
					float distance = (clamp(depth, _FogStart, _FogEnd) - _FogStart) / (_FogEnd - _FogStart) * _FogColor.a;

					c.rgb = lerp(c.rgb, _FogColor, distance);
				}



				//CONTRAST
				c *= pow(c, - _Contrast * c * 5);
				
				if (_VGAMode) //VGA MODE!
				{
					float lum;

					//increase contrast
					//if (c.r + c.g + c.b > 0.01)
						//c.rgb += (1 - pow(c.rgb, 0.25)) * 0.2 + (pow(c.rgb, 2)) * 0.35;

					//lum = pow((c.r + c.g + c.b) / 3, 0.75);
					lum = max(c.r, max(c.g, c.b));

					if (ceil(lum * 4) - lum * 4 >= ditherThresh)
						lum -= 0.25;

					if (lum < 0.25)
						c = float4(0, 0, 0, 1);
					else if (lum < 0.5)
						c = float4(1, 1 / 3, 1, 1);
					else if (lum < 0.75)
						c = float4(1 / 3, 1, 1, 1);
					else if (lum < 500)
						c = float4(1, 1, 1, 1);
					else
						c = float4(0, 0, 0, 1);

					c.rgb *= 0.75;
				}
				else
				{
					// HSV CONVERSION ========================================================
					if (_HSVMode)
					{
						float cMax = max(c.r, max(c.g, c.b));
						float cMin = min(c.r, min(c.g, c.b));
						float delta = cMax - cMin;
						float4 hsv = { 0, 0, 0, 1 };

						if (delta == 0)
							hsv[0] = 0;
						else if (c.r == cMax)
							hsv[0] = 60 * (((c.g - c.b) / delta) % 6);
						else if (c.g == cMax)
							hsv[0] = 60 * ((c.b - c.r) / delta + 2);
						else
							hsv[0] = 60 * ((c.r - c.g) / delta + 4);

						hsv[0] /= 360;

						if (cMax == 0)
							hsv[1] = 0;
						else
							hsv[1] = delta / cMax;

						hsv[2] = cMax;

						c = hsv;
					}

					// COLOR CLAMP ============================================
					{
						float4 x;

						c.r = (ceil(x = c.r * colors.r) - x > ditherThresh ? floor(x) : ceil(x)) / colors[0];
						c.g = (ceil(x = c.g * colors.g) - x > ditherThresh ? floor(x) : ceil(x)) / colors[1];
						c.b = (ceil(x = c.b * colors.b) - x > ditherThresh ? floor(x) : ceil(x)) / colors[2];
					}

					// HSV DECONVERSION ======================================================
					if (_HSVMode)
					{
						float C, X, m;

						c[0] *= 360;

						C = c[2] * c[1];
						X = C * (1 - abs((c[0] / 60) % 2 - 1));
						m = c[2] - C;

						if (c[0] < 60)
							c = float4(C, X, 0, 1);
						else if (c[0] < 120)
							c = float4(X, C, 0, 1);
						else if (c[0] < 180)
							c = float4(0, C, X, 1);
						else if (c[0] < 240)
							c = float4(0, X, C, 1);
						else if (c[0] < 300)
							c = float4(X, 0, C, 1);
						else
							c = float4(C, 0, X, 1);

						c += float4(m, m, m, 0);
					}
				}

				return c;
			}



			ENDCG
		}
	}
}