//Made By Stefan Jovanović - Mobile Optimized Version

//Donate on my Ko-Fi page: https://ko-fi.com/stefanjo

//Twitter: https://twitter.com/SJ1ovGD
//Reddit: https://www.reddit.com/user/sjovanovic3107
//Unity Asset Store: https://assetstore.unity.com/publishers/32235
//Itch.io: https://stefanjo.itch.io/

Shader "Custom/WaterShader_Mobile"
{
	Properties
	{
		_MainTex("텍스처", 2D) = "white" {}
		_DisplacementTex("변위 텍스처", 2D) = "white" {}
		_DisplacementSpeedX("변위 속도 X", Float) = 0.03
		_DisplacementSpeedY("변위 속도 Y", Float) = 0.0
		_DisplacementAmountDivider("변위량 분할값", Float) = 60
		_Tint("틴트", Color) = (1,1,1,1)
		_FoamThreshold("폼 임계값", Float) = 0.022
		_EdgeFoamThreshold("에지 폼 임계값", Float) = 0.005
		_FoamAlpha("폼 투명도", Range(0,1)) = 1
		[Toggle(ENABLE_VERTEX_DISPLACEMENT)]
		_VertexDisplacement("정점 변위 사용 (고품질)", Float) = 0
		_VertexDisplacementAmount("정점 변위량", Range(0,0.1)) = 0.02
	}
	SubShader
	{
		Tags 
		{ 
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True"
		}
		LOD 100

		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma shader_feature ENABLE_VERTEX_DISPLACEMENT
			#pragma target 3.0

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _DisplacementTex;
			float4 _MainTex_ST;
			float4 _Tint;
			float _DisplacementSpeedX;
			float _DisplacementSpeedY;
			float _DisplacementAmountDivider;
			float _FoamThreshold;
			float _FoamAlpha;
			float _EdgeFoamThreshold;
			float _VertexDisplacementAmount;

			v2f vert(appdata v)
			{
				v2f o;
				
				#ifdef ENABLE_VERTEX_DISPLACEMENT
					// 단순화된 정점 변위 - 텍스처 샘플링 없이 사인 함수 사용
					float waveHeight = sin(_Time.y * 2 + v.vertex.x * 5) * cos(_Time.y + v.vertex.z * 3) * _VertexDisplacementAmount;
					v.vertex.y += waveHeight;
				#endif
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// 간단한 UV 변위 계산
				float2 uvOffset = _Time.y * float2(_DisplacementSpeedX, _DisplacementSpeedY);
				float2 offset = tex2D(_DisplacementTex, i.uv + uvOffset).rg;
				
				float2 adjusted = i.uv + (offset - 0.5) / _DisplacementAmountDivider;
				fixed4 col = tex2D(_MainTex, adjusted);
				fixed4 colorWithTint = col * _Tint;
				
				// 단순화된 폼(거품) 계산
				float offsetMagnitude = length(offset - 0.5) / _DisplacementAmountDivider;
				float edgeFoam = (i.uv.y < _EdgeFoamThreshold) ? 1 : 0;
				float foam = (offsetMagnitude > _FoamThreshold || edgeFoam > 0) ? _FoamAlpha : 0;
				
				fixed4 finalColor = lerp(colorWithTint, fixed4(1,1,1,1), foam);
				
				// Apply fog
				UNITY_APPLY_FOG(i.fogCoord, finalColor);
				return finalColor;
			}
			ENDCG
		}
	}
	
	// 모바일 폴백 설정
	FallBack "Mobile/Diffuse"
}