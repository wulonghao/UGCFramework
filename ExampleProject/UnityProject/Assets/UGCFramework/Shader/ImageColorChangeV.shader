Shader "MyShader/ImageColorChangeV"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
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

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}
		ColorMask[_ColorMask]

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
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv[5] : TEXCOORD0;
				float4 color : COLOR;
			};

			v2f o;

			v2f vert(appdata_t v)
			{
				o.vertex = UnityObjectToClipPos(v.vertex);
				half2 uv = v.texcoord;
				o.uv[0] = uv + _MainTex_TexelSize.xy * half2(0, 0); 
				o.uv[1] = uv + _MainTex_TexelSize.xy * half2(0, -1);
				o.uv[2] = uv + _MainTex_TexelSize.xy * half2(-1, 0);
				o.uv[3] = uv + _MainTex_TexelSize.xy * half2(1, 0);
				o.uv[4] = uv + _MainTex_TexelSize.xy * half2(0, 1);
				o.color = v.color;
				return o;
			}

			float3 CalculateAlphaProductAround(v2f i)
			{
				float3 rgbProduct = float3(1, 1, 1);
				[unroll] for (int j = 0; j < 5; j++)
				{
					rgbProduct *= tex2D(_MainTex, i.uv[j]).rgb;
				}
				return rgbProduct;
			}

			float3 RGBConvertToHSV(float3 rgb)
			{
				float R = rgb.x, G = rgb.y, B = rgb.z;
				float3 hsv;
				float max1 = max(R, max(G, B));
				float min1 = min(R, min(G, B));
				if (max1 == min1)
				{
					hsv.x = 0;
					hsv.y = 0;
					hsv.z = max1;
				}
				else
				{
					if (R == max1)
					{
						hsv.x = (G - B) / (max1 - min1);
					}
					if (G == max1)
					{
						hsv.x = 2 + (B - R) / (max1 - min1);
					}
					if (B == max1)
					{
						hsv.x = 4 + (R - G) / (max1 - min1);
					}
					hsv.x = hsv.x * 60.0;
					if (hsv.x < 0)
						hsv.x = hsv.x + 360;
					hsv.z = max1;
					hsv.y = (max1 - min1) / max1;
				}
				return hsv;
			}

			float3 HSVConvertToRGB(float3 hsv)
			{
				float R, G, B;
				if (hsv.y == 0)
				{
					R = G = B = hsv.z;
				}
				else
				{
					hsv.x = hsv.x * 0.0166667;// 1/60
					int i = (int)hsv.x;
					float f = hsv.x - (float)i;
					float a = hsv.z * (1 - hsv.y);
					float b = hsv.z * (1 - hsv.y * f);
					float c = hsv.z * (1 - hsv.y * (1 - f));
					if (i == 0)
					{
						R = hsv.z; G = c; B = a;
					}
					else if (i == 1)
					{
						R = b; G = hsv.z; B = a;
					}
					else if (i == 2)
					{
						R = a; G = hsv.z; B = c;
					}
					else if (i == 3)
					{
						R = a; G = b; B = hsv.z;
					}
					else if (i == 4)
					{
						R = c; G = a; B = hsv.z;
					}
					else
					{
						R = hsv.z; G = a; B = b;
					}
				}
				return float3(R, G, B);
			}

			float3 colorHSV;
			float3 colorHSV2;
			float4 frag(v2f i) : COLOR
			{
				float4 col = tex2D(_MainTex, i.uv[0]);

				if (i.color.x == 0 && i.color.y == 0 && i.color.z == 0)
				{
					float grey = dot(col.rgb, float3(0.299, 0.587, 0.114));
					col.rgb = float3(grey, grey, grey);
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
						float3 uvs = RGBConvertToHSV(CalculateAlphaProductAround(i));
						col.rgb = lerp(col.rgb * i.color.rgb, col.rgb, saturate((1 - uvs.y) * 5));
					}
					else
					{
						float selfY = colorHSV.y;
						float selfZ = colorHSV.z;
						colorHSV.x = colorHSV2.x;
						colorHSV.y *= colorHSV2.y;
						colorHSV.z *= colorHSV2.z;
						float3 uvs = RGBConvertToHSV(CalculateAlphaProductAround(i));
						col.rgb = lerp(HSVConvertToRGB(colorHSV.xyz), HSVConvertToRGB(float3(colorHSV.x, selfY, selfZ)), saturate((1 - uvs.y) * 5));
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
