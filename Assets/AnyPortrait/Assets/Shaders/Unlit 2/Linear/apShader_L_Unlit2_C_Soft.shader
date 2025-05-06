/*
*	Copyright (c) RainyRizzle Inc. All rights reserved
*	Contact to : www.rainyrizzle.com , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express permission of [Seungjik Lee] of [RainyRizzle team].
*
*	It is illegal to download files from other than the Unity Asset Store and RainyRizzle homepage.
*	In that case, the act could be subject to legal sanctions.
*/

Shader "AnyPortrait/Unlit (v2)/Linear/SoftAdditive Clipped"
{
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)	// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Main Texture (RGBA)", 2D) = "white" {}			// Main Texture controlled by AnyPortrait

		//Clipped
		_MaskTex("Mask Texture (A)", 2D) = "white" {}				// Mask Texture for clipping Rendering (controlled by AnyPortrait)
		_MaskScreenSpaceOffset("Mask Screen Space Offset (XY_Scale)", Vector) = (0, 0, 0, 1)	// Mask Texture's Transform Offset (controlled by AnyPortrait)
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane" }
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend OneMinusDstColor One//Soft Add <
		ZWrite Off
		LOD 200

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
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

				//Screen Pos (Clipped)
				float4 screenPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _Color;

			//Clipped
			sampler2D _MaskTex;
			float4 _MaskScreenSpaceOffset;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				//Screen Pos (Clipped)
				o.screenPos = ComputeScreenPos(o.vertex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				//col.rgb *= _Color.rgb * 2.0f;//Gamma
				col.rgb *= _Color.rgb * 4.595f;//Linear : pow(2, 2.2) = 4.595
				col.rgb = pow(col.rgb, 2.2f);//Linear

				col.a *= _Color.a;


				//-------------------------------------------
				// Clipped
				float2 screenUV = i.screenPos.xy / max(i.screenPos.w, 0.0001f);

				screenUV -= float2(0.5f, 0.5f);

				screenUV.x *= _MaskScreenSpaceOffset.z;
				screenUV.y *= _MaskScreenSpaceOffset.w;
				screenUV.x += _MaskScreenSpaceOffset.x * _MaskScreenSpaceOffset.z;
				screenUV.y += _MaskScreenSpaceOffset.y * _MaskScreenSpaceOffset.w;

				screenUV += float2(0.5f, 0.5f);

				col.a *= tex2D(_MaskTex, screenUV).r;
				//-------------------------------------------


				//Additive
				col.rgb *= col.a;
				col.a = 1.0f;

				return col;
			}
			ENDCG
		}
	}
}
