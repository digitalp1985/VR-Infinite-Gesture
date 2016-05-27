Shader "Unlit/Transparent (ChromaKey)" {

	Properties{
		_MainTex("Base(RGB)", 2D) = "white"{}
		_keyingColor("Key Colour", Color) = (1,1,1,1)
		_channelLimits("Channel Limits", Vector) = (0.2, 0.2, 0.2)
		_channelFeathers("Channel Feathers", Vector) = (0,0,0)
		_channelFactors("Channel Factors", Vector) = (1, 1, 1)
	}

	SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Lighting Off
		ZWrite Off
		ZTest Off
		AlphaTest Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
		CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float3 _keyingColor;
			float3 _channelLimits;
			float3 _channelFeathers;
			float3 _channelFactors;

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 keyHSV : TEXCOORD1;
			};

			float3 rgb_to_hsv_no_clip(float3 RGB)
			{
				float3 HSV;

				float minChannel, maxChannel;
				if (RGB.x > RGB.y) {
					maxChannel = RGB.x;
					minChannel = RGB.y;
				}
				else {
					maxChannel = RGB.y;
					minChannel = RGB.x;
				}

				if (RGB.z > maxChannel) maxChannel = RGB.z;
				if (RGB.z < minChannel) minChannel = RGB.z;

				HSV.xy = 0;
				HSV.z = maxChannel;
				float delta = maxChannel - minChannel;             //Delta RGB value
				if (delta != 0) {                    // If gray, leave H  S at zero
					HSV.y = delta / HSV.z;
					float3 delRGB;
					delRGB = (HSV.zzz - RGB + 3 * delta) / (6.0*delta);
					if (RGB.x == HSV.z) HSV.x = delRGB.z - delRGB.y;
					else if (RGB.y == HSV.z) HSV.x = (1.0 / 3.0) + delRGB.x - delRGB.z;
					else if (RGB.z == HSV.z) HSV.x = (2.0 / 3.0) + delRGB.y - delRGB.x;
				}
				return (HSV);
			}

			float3 hsv_to_rgb(float3 HSV)
			{
				float3 RGB = HSV.z;

				float var_h = HSV.x * 6;
				float var_i = floor(var_h);   // Or ... var_i = floor( var_h )
				float var_1 = HSV.z * (1.0 - HSV.y);
				float var_2 = HSV.z * (1.0 - HSV.y * (var_h - var_i));
				float var_3 = HSV.z * (1.0 - HSV.y * (1 - (var_h - var_i)));
				if (var_i == 0) { RGB = float3(HSV.z, var_3, var_1); }
				else if (var_i == 1) { RGB = float3(var_2, HSV.z, var_1); }
				else if (var_i == 2) { RGB = float3(var_1, HSV.z, var_3); }
				else if (var_i == 3) { RGB = float3(var_1, var_2, HSV.z); }
				else if (var_i == 4) { RGB = float3(var_3, var_1, HSV.z); }
				else { RGB = float3(HSV.z, var_1, var_2); }

				return (RGB);
			}

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				o.keyHSV = rgb_to_hsv_no_clip(_keyingColor);
				return o;
			}

			float4 frag(v2f i) : COLOR {				
				float3 input_color = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex)).rgb;

				float3 inputHSV = rgb_to_hsv_no_clip(input_color);
				
				float hueFactor = 1-smoothstep(_channelLimits.x, _channelLimits.x + _channelFeathers.x, min(abs(inputHSV.x - i.keyHSV.x), abs(inputHSV.x - 1 - i.keyHSV.x)));
				float saturationFactor = 1 - smoothstep(_channelLimits.y, _channelLimits.y + _channelFeathers.y, abs(inputHSV.y - i.keyHSV.y));
				float valueFactor = 1 - smoothstep(_channelLimits.z, _channelLimits.z + _channelFeathers.z, abs(inputHSV.z - i.keyHSV.z));

				float alpha = 1 - lerp(1, hueFactor, _channelFactors.x) * lerp(1, saturationFactor, _channelFactors.y) * lerp(1, valueFactor, _channelFactors.z);

				return float4(input_color, alpha);
			}
		ENDCG
		}
	}

	FallBack "Unlit"
}