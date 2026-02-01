//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Watercolor"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Watercolor"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _MotionTex;
            sampler2D _PaperTex;
            sampler2D _FocusTex;

            float4 _DataA; // x - flowStrength, y - flowOffset, z - flowScale, w - blending
            float4 _DataB; // x - thickness, y - sensitive, z - depth, w - saturation

            #define _FlowStrength _DataA.x
            #define _FlowOffset   _DataA.y
            #define _FlowScale    _DataA.z
            #define _Blending     _DataA.w

            #define _Thickness    _DataB.x
            #define _Sensitive    _DataB.y
            #define _Depth        _DataB.z
            #define _Saturation   _DataB.w

            struct vert_in
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct frag_in
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            frag_in vert(vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }

            float sampleLuma(float2 uv)
            {
                float3 col = tex2D(_MainTex, uv).rgb;
                return dot(col, float3(0.299, 0.587, 0.114));
            }
            
            half luma(half3 rgb)
            {
                return dot(rgb, half3(.299, .587, .114));
            }

            float sobel(float2 uv, float mul)
            {
                float2 d = float2(_Thickness, _Thickness) * mul;
                float hr = 0, vt = 0;

                hr += sampleLuma(uv + float2(-1, -1) * d) *  1.0;
                hr += sampleLuma(uv + float2( 1, -1) * d) * -1.0;
                hr += sampleLuma(uv + float2(-1,  0) * d) *  2.0;
                hr += sampleLuma(uv + float2( 1,  0) * d) * -2.0;
                hr += sampleLuma(uv + float2(-1,  1) * d) *  1.0;
                hr += sampleLuma(uv + float2( 1,  1) * d) * -1.0;

                vt += sampleLuma(uv + float2(-1, -1) * d) *  1.0;
                vt += sampleLuma(uv + float2( 0, -1) * d) *  2.0;
                vt += sampleLuma(uv + float2( 1, -1) * d) *  1.0;
                vt += sampleLuma(uv + float2(-1,  1) * d) * -1.0;
                vt += sampleLuma(uv + float2( 0,  1) * d) * -2.0;
                vt += sampleLuma(uv + float2( 1,  1) * d) * -1.0;

                return sqrt(hr * hr + vt * vt);
            }

            float2 sampleFlow(float2 uv)
            {
                float flow = saturate(tex2D(_MotionTex, uv * _FlowScale + _FlowOffset).r);
                float angle = flow * 6.2831853;
                float2 dir = float2(cos(angle), sin(angle));
                return dir * flow * _FlowStrength;
            }
            
            inline half3 applyHue(half3 col, half angle)
            {
                half3 k = float3(0.57735, 0.57735, 0.57735);
                half cosAngle = cos(angle);
                
                return col * cosAngle + cross(k, col) * sin(angle) + k * dot(k, col) * (1 - cosAngle);
            }

            inline half3 applyHsb(half3 initial, float hue)
            {
                half3 result = initial;
                
                result.rgb = result.rgb + hue * .01;
                result.rgb = lerp(luma(initial.rgb), result.rgb, hue * .3 + 1);
                result.rgb = applyHue(result.rgb, hue);
                
                return result;
            }

            half4 frag(frag_in i) : SV_Target
            {
                float2 uv = i.uv;
                float2 totalOffset = float2(0, 0);
                float4 src = tex2D(_MainTex, uv);
                float4 paper = tex2D(_PaperTex, uv + _FlowOffset);
                float4 result = src;
                float  weight = tex2D(_FocusTex, float2(luma(src.rgb), 0));

                [unroll(16)]
                for (int s = 0; s < 16; s++)
                {
                    float2 offset = sampleFlow(uv + totalOffset);
                    totalOffset += offset * weight;

                    float4 samp = tex2D(_MainTex, uv + totalOffset);
                    result = lerp(lerp(result, samp, _Blending), result * lerp(1.0, samp, _Blending), _Saturation);
                }

                float flowLength = length(totalOffset);
                float motion = saturate(1 - flowLength * 7);
                float sobelMask = motion * tex2D(_MotionTex, uv * _FlowScale * 0.1 + _FlowOffset).r;
                float outline = pow(saturate(sobel(uv, sobelMask)), _Sensitive);

                result.rgb = applyHsb(result.rgb, saturate((_Depth * motion - 0.5f) * 3 + .5) * weight -.5);
                
                return saturate((result - float4(outline, outline, outline, 0)) * lerp(paper, 1, motion));
            }

            ENDHLSL
        }
    }
}
