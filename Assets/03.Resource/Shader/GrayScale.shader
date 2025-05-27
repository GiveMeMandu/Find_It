Shader "Custom/GrayScale"
{
    Properties
    {
        _BaseMap("BaseMap", 2D) = "white"{}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // 텍스처 오브젝트와 샘플러를 부른다.
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // 프로퍼티와 같은 이름을 써주면 연결된다.
            // SRP Batcher가 Compatible 하게 해준다.
            CBUFFER_START(UnityPerMaterial)
            // 프로퍼티 텍스쳐 이름과 그 뒤에 _ST를 붙이면 된다. _ST는 S, T 좌표계를 의미하는 이름이다.
            // float4인 이유는 타일링과 옵셋 총 4개의 숫자
            float4 _BaseMap_ST;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // #define TRANSFORM_TEX(tex, name) ((tex.xy) * name##_ST.xy + name##_ST.zw)
                // IN.uv에 ST 연산을 해준 뒤에 OUT.uv로 전달한다. 
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            // 프래그먼트 셰이더 함수 입력에 Varyings를 추가한다. 
            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                // Luminance 수치를 이용한 GrayScale
                color = (color.r * 0.3 + color.g * 0.59 + color.b * 0.11);
                
                // dot product를 이용한 방법
                // color = dot(color, float3(0.3, 0.59, 0.11));

                return color;
            }
            ENDHLSL
        }
    }
}