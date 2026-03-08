Shader "Custom/Slime_Flow_With_ScanningRim"
{
    Properties
    {
        _MainTex ("Slime Texture", 2D) = "white" {}
        _NoiseTex ("Flow Noise", 2D) = "bump" {}
        _Tint ("Slime Color", Color) = (0.2, 1, 0.3, 1)
        
        [Header(Flow Settings)]
        _FlowSpeed ("Flow Speed", Float) = 0.5
        _DistortionStrength ("Distortion", Range(0, 0.2)) = 0.05
        
        [Header(Scanning Rim Settings)]
        _RimColor ("Rim Highlight Color", Color) = (1, 1, 1, 1)
        _RimSpeed ("Rim Move Speed", Float) = 1.5      // 림이 내려오는 속도
        _RimWidth ("Rim Width", Range(0.01, 0.5)) = 0.1 // 림의 두께
        _RimInterval ("Rim Interval", Float) = 3.0     // 림 사이의 간격 (높을수록 드문드문)
        _RimIntensity ("Rim Intensity", Float) = 2.0   // 밝기 강도
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 noiseUV : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _Tint;
            float4 _RimColor;
            float _FlowSpeed, _DistortionStrength;
            float _RimSpeed, _RimWidth, _RimInterval, _RimIntensity;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.noiseUV = v.uv - _Time.y * _FlowSpeed; // 노이즈 흐름
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 1. 왜곡 계산
                float2 noise = tex2D(_NoiseTex, i.noiseUV).rg * 2.0 - 1.0;
                float2 distortedUV = i.uv + noise * _DistortionStrength;

                // 2. 메인 컬러
                fixed4 col = tex2D(_MainTex, distortedUV) * _Tint;

                // 3. 위에서 아래로 흐르는 림(Highlight) 계산
                // frac을 사용하여 0~1 사이를 반복하게 함
                float scanPos = frac(i.uv.y * _RimInterval - _Time.y * _RimSpeed);
                
                // 림의 두께만큼 마스크 생성 (smoothstep으로 부드러운 경계)
                float rimMask = smoothstep(0.5 - _RimWidth, 0.5, scanPos) * (1.0 - smoothstep(0.5, 0.5 + _RimWidth, scanPos));
                
                // 4. 왜곡된 노이즈를 림에도 섞어서 일직선이 아닌 꿀렁이는 선으로 만듦
                rimMask *= (1.0 + noise.r); 

                // 5. 최종 합성 (기본 컬러 + 흐르는 림)
                // 메인 텍스처의 알파값이 있는 곳에만 림이 나타나도록 처리
                col.rgb += _RimColor.rgb * rimMask * _RimIntensity * col.a;

                return col;
            }
            ENDCG
        }
    }
}