Shader "Custom/CoinAdd"
{
    Properties
    {
        _RotationAngle("Rotation Angle", Range(0, 1)) = 0.3
        _Speed("Speed", Range(0, 50)) = 3
        _Frequancy("Frequancy", Range(0, 50)) = 3
        _LineThickness("Line Thickness", Range(0, 10)) = 9.8
        _EmissionColor("Emission Color", Color) = (1, 1, 0.5, 1)
        _EmissionIntensity("Emission Intensity", Range(0, 10)) = 3
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline" 
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 objectCenterWS : TEXCOORD1;
                float fogCoord : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float _RotationAngle;
                float _Speed;
                float _Frequancy;
                float _LineThickness;
                half4 _EmissionColor;
                float _EmissionIntensity;
            CBUFFER_END

            // Rotate around axis function (simplified for mobile)
            float3 RotateAroundAxis(float3 center, float3 position, float3 axis, float angle)
            {
                float3 delta = position - center;
                float cosAngle = cos(angle);
                float sinAngle = sin(angle);
                
                float3 rotated;
                rotated.x = delta.x * cosAngle - delta.y * sinAngle;
                rotated.y = delta.x * sinAngle + delta.y * cosAngle;
                rotated.z = delta.z;
                
                return rotated + center;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                // Get object center position in world space
                float3 objectCenterWS = TransformObjectToWorld(float3(0, 0, 0));
                
                // Get vertex position in world space
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                
                // Calculate rotation angle based on time and frequency
                float time = _Time.y * _Speed;
                float rotationValue = positionWS.x * _Frequancy + time;
                float sinValue = sin(rotationValue);
                
                // Apply rotation around Z axis
                float3 rotationAxis = float3(0, 0, 1);
                float rotationAngle = _RotationAngle * sinValue;
                float3 rotatedPosition = RotateAroundAxis(objectCenterWS, positionWS, rotationAxis, rotationAngle);
                
                OUT.positionWS = rotatedPosition;
                OUT.objectCenterWS = objectCenterWS;
                OUT.positionHCS = TransformWorldToHClip(rotatedPosition);
                OUT.fogCoord = ComputeFogFactor(OUT.positionHCS.z);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Get X component for line calculation
                float xComponent = IN.positionWS.x - IN.objectCenterWS.x;
                
                // Calculate animated value
                float time = _Time.y * _Speed;
                float animValue = xComponent * _Frequancy + time;
                float sinResult = sin(animValue);
                
                // Remap sin from [-1, 1] to [0, 1] for better visibility
                sinResult = sinResult * 0.5 + 0.5;
                
                // Apply line thickness effect
                float lineValue = _LineThickness * 0.1;
                lineValue = min(lineValue, 0.999999);
                
                // Smoothstep for line effect - using proper range
                float finalValue = smoothstep(lineValue, 1.0, sinResult);
                
                // Apply emission color with intensity
                half4 color = _EmissionColor * finalValue * _EmissionIntensity;
                
                // Apply fog
                color.rgb = MixFog(color.rgb, IN.fogCoord);
                
                return color;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
