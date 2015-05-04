Shader "Hidden/ProBuilder/UnlitColor" 
{
	Properties
	{
		_Color ("Color Tint", Color) = (1,1,1,1)   
		_MainTex ("Base (RGB) Alpha (A)", 2D) = "white"
	}

	Category
	{
		Lighting Off
		ZWrite On
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		Offset 0, -1
	//	AlphaTest Greater 0.001
		Fog { Mode Off }
		Tags {"Queue"="Transparent" }

		SubShader
		{
			Pass
			{
				SetTexture [_MainTex]
				{
					ConstantColor [_Color]
					Combine Texture * constant
				}
			}
		}
	}
}