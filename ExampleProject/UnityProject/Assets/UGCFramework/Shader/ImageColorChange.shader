Shader "MyShader/ImageColorChange"
{
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
			float4 _MainTex_ST;
	
			struct appdata_t
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};
	
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};
	
			v2f o;

			v2f vert (appdata_t v)
			{
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color;
				return o;
			}

			float3 RGBConvertToHSV(float3 RGB)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 P = lerp(float4(RGB.bg, K.wz), float4(RGB.gb, K.xy), step(RGB.b, RGB.g));
				float4 Q = lerp(float4(P.xyw, RGB.r), float4(RGB.r, P.yzx), step(P.x, RGB.r));
				float D = Q.x - min(Q.w, Q.y);
				float E = 1e-6;
				return float3(abs(Q.z + (Q.w - Q.y) / (6.0 * D + E)), D / (Q.x + E), Q.x);
			}

			float3 HSVConvertToRGB(float3 HSV)
			{
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 P = abs(frac(HSV.xxx + K.xyz) * 6.0 - K.www);
				return HSV.z * lerp(K.xxx, saturate(P - K.xxx), HSV.y);
			}
				
			float3 colorHSV;  
			float3 colorHSV2;
			float4 frag (v2f i) : COLOR
			{
				float4 col = tex2D(_MainTex, i.texcoord);

				if (!any(i.color.rgb))
				{
					float grey = dot(col.rgb, float3(0.299, 0.587, 0.114));  
					col.rgb = float3(grey, grey, grey);
					col.a *= i.color.a;
				}
				else
				{
					colorHSV.xyz = RGBConvertToHSV(col.rgb);
					colorHSV2.xyz = RGBConvertToHSV(i.color);
					if((col.r == col.g && col.r == col.b))
					{
						col.a *= i.color.a;
					}
					else if(colorHSV2.y == 0)
					{
						col *= i.color;
					}
					else
					{
						colorHSV.x = colorHSV2.x;
						col.rgb = HSVConvertToRGB(colorHSV.xyz);
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
