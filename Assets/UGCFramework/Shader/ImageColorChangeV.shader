Shader "MyShader/ImageColorChangeV"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
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
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		ColorMask [_ColorMask]
		
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
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};
	
			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};
	
			v2f o;

			v2f vert (appdata_t v)
			{
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color;
				return o;
			}

			float3 RGBConvertToHSV(float3 rgb)
			{
			    float R = rgb.x,G = rgb.y,B = rgb.z;
			    float3 hsv;
			    float max1=max(R,max(G,B));
			    float min1=min(R,min(G,B));
			    if (R == max1) 
			    {
			        hsv.x = (G-B)/(max1-min1);
			    }
			    if (G == max1) 
			    {
			        hsv.x = 2 + (B-R)/(max1-min1);
			        }
			    if (B == max1) 
			    {
			        hsv.x = 4 + (R-G)/(max1-min1);
			        }
			    hsv.x = hsv.x * 60.0;   
			    if (hsv.x < 0) 
			        hsv.x = hsv.x + 360;
			    hsv.z=max1;
			    hsv.y=(max1-min1)/max1;
			    return hsv;
			}

			float3 HSVConvertToRGB(float3 hsv)
			{
			    float R,G,B;
			    if( hsv.y == 0 )
			    {
			        R=G=B=hsv.z;
			    }
			    else
			    {
			        hsv.x = hsv.x/60.0; 
			        int i = (int)hsv.x;
			        float f = hsv.x - (float)i;
			        float a = hsv.z * ( 1 - hsv.y );
			        float b = hsv.z * ( 1 - hsv.y * f );
			        float c = hsv.z * ( 1 - hsv.y * (1 - f ) );
			        if(i == 0)
			        {
			             R = hsv.z; G = c; B = a;
			        }
					else if(i==1)
					{
						R = b; G = hsv.z; B = a; 
					}
					else if(i==2)
					{
						 R = a; G = hsv.z; B = c; 
					}
					else if(i==3)
					{
						R = a; G = b; B = hsv.z; 
					}
					else if(i==4)
					{
						R = c; G = a; B = hsv.z; 
					}
					else
					{
						R = hsv.z; G = a; B = b; 
					}
			    }
			    return float3(R,G,B);
			}
				
			float3 colorHSV;  
			float3 colorHSV2;
			fixed4 frag (v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);  
				
				if(i.color.x == 0 && i.color.y == 0 && i.color.z == 0)
				{
					float grey = dot(col.rgb, float3(0.299, 0.587, 0.114));  
					col.rgb = float3(grey, grey, grey);  
				}
				else
				{
					colorHSV.xyz = RGBConvertToHSV(col.rgb);
					colorHSV2.xyz = RGBConvertToHSV(i.color);
					if((col.r == col.g && col.g == col.b))
					{
						col = col;
					}
					else if(colorHSV2.y == 0)
					{
						col *= i.color;
					}
					else
					{
						colorHSV.x = colorHSV2.x;
						colorHSV.y *= colorHSV2.y;                   
						if(abs(col.r - col.g) > 0.2 || abs(col.g - col.b) > 0.2)
                        {
                            colorHSV.z = colorHSV2.z;
                        }
						col.rgb = HSVConvertToRGB(colorHSV.xyz);
					}
				}
				col.a *= i.color.a;
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
