Shader "Custom/ViewFacing_Stretched" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;

            v2f vert (appdata v) {
                v2f o;

                // 1. 모델 좌표계의 원점(파티클 중심)을 월드로 변환
                float3 centerWorld = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
                
                // 2. 카메라의 Right, Up 벡터 가져오기 (항상 화면을 바라보게 함)
                float3 camRight = UNITY_MATRIX_V[0].xyz;
                float3 camUp = UNITY_MATRIX_V[1].xyz;
                
                // 3. 유니티 엔진이 Stretched 연산을 위해 v.vertex에 넣어둔 '늘어난 비율'을 활용
                // Stretched 모드일 때 v.vertex.y는 이미 속도에 따라 늘어나 있습니다.
                float3 worldPos = centerWorld 
                                + camRight * v.vertex.x 
                                + camUp * v.vertex.y;

                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return tex2D(_MainTex, i.uv) * i.color;
            }
            ENDCG
        }
    }
}