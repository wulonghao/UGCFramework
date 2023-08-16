Shader "MyShader/ImageColorChangeV"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Offset -1, -1
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float4 _MainTex_ST;

			struct appdata_t
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				half4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv[5] : TEXCOORD0;
				half4 color : COLOR;
			};

			v2f o;

			v2f vert(appdata_t v)
			{
				o.vertex = UnityObjectToClipPos(v.vertex);
				float2 uv = v.texcoord;
				o.uv[0] = uv + _MainTex_TexelSize.xy * half2(0, 0); 
				o.uv[1] = uv + _MainTex_TexelSize.xy * half2(0, -1);
				o.uv[2] = uv + _MainTex_TexelSize.xy * half2(-1, 0);
				o.uv[3] = uv + _MainTex_TexelSize.xy * half2(1, 0);
				o.uv[4] = uv + _MainTex_TexelSize.xy * half2(0, 1);
				o.color = v.color;
				return o;
			}

			half3 CalculateAlphaProductAround(v2f i)
			{
				half3 rgbProduct = half3(1, 1, 1);
				[unroll] for (int j = 0; j < 5; j++)
				{
					rgbProduct *= tex2D(_MainTex, i.uv[j]).rgb;
				}
				return rgbProduct;
			}

			half3 RGBConvertToHSV(half3 RGB)
			{
				half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				half4 P = lerp(half4(RGB.bg, K.wz), half4(RGB.gb, K.xy), step(RGB.b, RGB.g));
				half4 Q = lerp(half4(P.xyw, RGB.r), half4(RGB.r, P.yzx), step(P.x, RGB.r));
				half D = Q.x - min(Q.w, Q.y);
				half E = 1e-6;
				return half3(abs(Q.z + (Q.w - Q.y) / (6.0 * D + E)), D / (Q.x + E), Q.x);
			}

			half3 HSVConvertToRGB(half3 HSV)
			{
				half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				half3 P = abs(frac(HSV.xxx + K.xyz) * 6.0 - K.www);
				return HSV.z * lerp(K.xxx, saturate(P - K.xxx), HSV.y);
			}

			half3 colorHSV;
			half3 colorHSV2;
			half4 frag(v2f i) : COLOR
			{
				half4 col = tex2D(_MainTex, i.uv[0]);

				if (!any(i.color.rgb))
				{
					half grey = dot(col.rgb, half3(0.299, 0.587, 0.114));
					col.rgb = half3(grey, grey, grey);
					col.a *= i.color.a;
				}
				else
				{
					colorHSV.xyz = RGBConvertToHSV(col.rgb);
					colorHSV2.xyz = RGBConvertToHSV(i.color);
					if ((col.r == col.g && col.r == col.b))
					{
						col.a *= i.color.a;
					}
					else if (colorHSV2.y == 0)
					{
						half3 hsv = RGBConvertToHSV(CalculateAlphaProductAround(i));
						col.rgb = lerp(col.rgb * i.color.rgb, col.rgb, saturate((1 - hsv.y) * 5));
					}
					else
					{
						half selfY = colorHSV.y;
						half selfZ = colorHSV.z;
						colorHSV.x = colorHSV2.x;
						colorHSV.y *= colorHSV2.y;
						colorHSV.z *= colorHSV2.z;
						half3 hsv = RGBConvertToHSV(CalculateAlphaProductAround(i));
						col.rgb = lerp(HSVConvertToRGB(colorHSV.xyz), HSVConvertToRGB(half3(colorHSV.x, selfY, selfZ)), saturate((1 - hsv.y) * 5));
						col.a *= i.color.a;
					}

				}
				return col;
			}
			ENDCG
		}
	}

	SubShader
	{
		LOD 100

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			Offset -1, -1
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMaterial AmbientAndDiffuse
			
			SetTexture [_MainTex]
			{
				Combine Texture * Primary
			}
		}
	}
}
