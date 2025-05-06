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

Shader "AnyPortrait/Unlit (v2)/Linear/AlphaBlend"
{
	Properties
	{
		_Color("2X Color (RGBA Mul)", Color) = (0.5, 0.5, 0.5, 1.0)	// Main Color (2X Multiply) controlled by AnyPortrait
		_MainTex("Main Texture (RGBA)", 2D) = "white" {}			// Main Texture controlled by AnyPortrait
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "PreviewType" = "Plane" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 200

		Pass
		{
			Tags {"LightMode"="ForwardBase"}
			ZWrite Off
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
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				//col.rgb *= _Color.rgb * 2.0f;//Gamma
				col.rgb *= _Color.rgb * 4.595f;//Linear : pow(2, 2.2) = 4.595
				col.rgb = pow(col.rgb, 2.2f);//Linear

				col.a *= _Color.a;

				return col;
			}
			ENDCG
		}

		Pass
        {
            Tags {"LightMode"="ShadowCaster"}
			ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

            struct v2f { 
				float2 uv : TEXCOORD0;
                V2F_SHADOW_CASTER;
            };


			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _Color;

            v2f vert(appdata_base v)
            {
                v2f o;

                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

                return o;
            }


            float4 frag(v2f i) : SV_Target
            {
				fixed4 col = tex2D(_MainTex, i.uv);
				col.a *= _Color.a;

				if(col.a < 0.05f)
				{
					discard;
				}

                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
	}
}
