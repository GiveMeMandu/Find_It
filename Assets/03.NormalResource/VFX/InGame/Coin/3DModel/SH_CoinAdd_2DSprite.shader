Shader "SH_CoinAdd_2DSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _RotationAngle ("Rotation Angle", Range(0, 1)) = 0.3
        _Speed ("Speed", Range(0, 50)) = 1
        [Toggle(_RANDOMIZE_SPEED)] _RandomizeSpeed ("Randomize Speed", Float) = 0
        _SpeedRandomMin ("Speed Random Min", Range(0, 50)) = 0.5
        _SpeedRandomMax ("Speed Random Max", Range(0, 50)) = 3
        _Frequancy ("Frequancy", Range(0, 50)) = 5
        _LineThickness ("Line Thickness", Range(0, 100)) = 0
        _LineColor ("Line Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0, 5)) = 1

        // Sprite renderer support
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)

        // Stencil (for masking)
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "UniversalMaterialType" = "Unlit"
            "DisableBatching" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature_local _RANDOMIZE_SPEED

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float  randomSeed : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _RendererColor;
                half4 _LineColor;
                float _RotationAngle;
                float _Speed;
                float _SpeedRandomMin;
                float _SpeedRandomMax;
                float _Frequancy;
                float _LineThickness;
                float _Intensity;
            CBUFFER_END

            // 오브젝트 월드 위치 기반 해시 (0~1 반환)
            float Hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            // UV를 중심 기준으로 회전하는 함수
            float2 RotateUV(float2 uv, float2 center, float angle)
            {
                float cosA = cos(angle);
                float sinA = sin(angle);
                float2 offset = uv - center;
                float2 rotated;
                rotated.x = offset.x * cosA - offset.y * sinA;
                rotated.y = offset.x * sinA + offset.y * cosA;
                return rotated + center;
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color * _RendererColor;

                // 오브젝트 월드 위치로 인스턴스별 고유 시드 생성
                float3 objWorldPos = GetObjectToWorldMatrix()._m03_m13_m23;
                output.randomSeed = Hash(objWorldPos.xy + objWorldPos.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // 스프라이트 텍스처 샘플링
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                texColor *= input.color;

                // UV를 중심(0.5, 0.5) 기준으로 회전
                float angle = _RotationAngle * TWO_PI; // 0~1 -> 0~2PI
                float2 rotatedUV = RotateUV(input.uv, float2(0.5, 0.5), angle);

                // 속도 결정 (랜덤화 토글에 따라)
                float speed = _Speed;
                #if defined(_RANDOMIZE_SPEED)
                    speed = lerp(_SpeedRandomMin, _SpeedRandomMax, input.randomSeed);
                #endif

                // 시간 기반 애니메이션 (랜덤 시드로 시간 오프셋도 추가)
                float timeOffset = 0;
                #if defined(_RANDOMIZE_SPEED)
                    timeOffset = input.randomSeed * TWO_PI * 10.0;
                #endif
                float animTime = _TimeParameters.x * speed + timeOffset;

                // 대각선 스캔라인 계산 (원본 셰이더의 핵심 로직 재현)
                float lineValue = sin((rotatedUV.x * _Frequancy) + animTime);
                float thickness = min(_LineThickness * 0.1, 0.999999);
                float scanLine = smoothstep(thickness, 1.0, lineValue);

                // 스캔라인을 스프라이트 위에 Additive로 합성
                half3 addColor = _LineColor.rgb * scanLine * _Intensity;
                half3 finalColor = texColor.rgb + addColor * texColor.a;

                // premultiplied alpha
                half finalAlpha = texColor.a;
                finalColor *= finalAlpha;

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }

        // UniversalForward 패스 (Universal2D가 아닌 렌더러에서도 동작하도록)
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature_local _RANDOMIZE_SPEED

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float  randomSeed : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _RendererColor;
                half4 _LineColor;
                float _RotationAngle;
                float _Speed;
                float _SpeedRandomMin;
                float _SpeedRandomMax;
                float _Frequancy;
                float _LineThickness;
                float _Intensity;
            CBUFFER_END

            float Hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float2 RotateUV(float2 uv, float2 center, float angle)
            {
                float cosA = cos(angle);
                float sinA = sin(angle);
                float2 offset = uv - center;
                float2 rotated;
                rotated.x = offset.x * cosA - offset.y * sinA;
                rotated.y = offset.x * sinA + offset.y * cosA;
                return rotated + center;
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color * _RendererColor;

                float3 objWorldPos = GetObjectToWorldMatrix()._m03_m13_m23;
                output.randomSeed = Hash(objWorldPos.xy + objWorldPos.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                texColor *= input.color;

                float angle = _RotationAngle * TWO_PI;
                float2 rotatedUV = RotateUV(input.uv, float2(0.5, 0.5), angle);

                float speed = _Speed;
                #if defined(_RANDOMIZE_SPEED)
                    speed = lerp(_SpeedRandomMin, _SpeedRandomMax, input.randomSeed);
                #endif

                float timeOffset = 0;
                #if defined(_RANDOMIZE_SPEED)
                    timeOffset = input.randomSeed * TWO_PI * 10.0;
                #endif
                float animTime = _TimeParameters.x * speed + timeOffset;

                float lineValue = sin((rotatedUV.x * _Frequancy) + animTime);
                float thickness = min(_LineThickness * 0.1, 0.999999);
                float scanLine = smoothstep(thickness, 1.0, lineValue);

                half3 addColor = _LineColor.rgb * scanLine * _Intensity;
                half3 finalColor = texColor.rgb + addColor * texColor.a;

                half finalAlpha = texColor.a;
                finalColor *= finalAlpha;

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
