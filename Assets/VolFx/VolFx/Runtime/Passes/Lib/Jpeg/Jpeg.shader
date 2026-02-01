//  Jpeg © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Jpeg"
{
    SubShader
    {
        name "Jpeg"
        Tags { "RenderPipeline" = "UniversalPipeline" }
        LOD 0

        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off

        Pass // 0
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _DistortionTex;
            sampler2D _ShotTex;
            sampler2D _LockTex;
            sampler2D _FpsTex;

            float _Weight;

            #pragma multi_compile_local _ ENABLE_CHANNEL_SHIFT

            float2  _BlockSize;
            float4  _ChannelShift;
            #define _DistortionOffset _ChannelShift.zw

            float4  _DistortionData; // scale, mask, quantization, glitch intensity
            #define _DistortionScale _DistortionData.x
            #define _Noise           _DistortionData.y
            #define _Quantization    _DistortionData.z
            #define _Glitch          _DistortionData.w

            float4  _FxData; // intensity, applyY, applyChroma, applyGlitch
            #define _Intensity           _FxData.x
            #define _ApplyDistortY       _FxData.y
            #define _ApplyDistortChroma  _FxData.z
            #define _ApplyDistortGlitch  _FxData.w

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o; o.vertex = v.vertex; o.uv = v.uv;
                return o;
            }

            // ===== RGB ↔ YCbCr Conversion =====
            float3 RGBtoYCbCr(float3 col)
            {
                float3x3 mat = float3x3(
                    0.299,     0.587,     0.114,
                   -0.168736, -0.331264,  0.5,
                    0.5,     -0.418688, -0.081312
                );
                return mul(mat, col);
            }

            float3 YCbCrtoRGB(float3 col)
            {
                float3x3 mat = float3x3(
                    1.0,  0.0,      1.402,
                    1.0, -0.34414, -0.71414,
                    1.0,  1.772,    0.0
                );
                return mul(mat, col);
            }
                
            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;

                float4 d = tex2D(_DistortionTex, uv * _DistortionScale + _DistortionOffset + 0.5) - float4(.5, .5, 0, 0);
                float2 dir = d.rg * 2.0 * 0.02;
                float strength = d.b;

                float2 offsetY      = dir * strength * _ApplyDistortY;
                float2 offsetChroma = dir * strength * _ApplyDistortChroma;

                float2 uvY      = uv + offsetY;
                float2 uvChroma = uv + offsetChroma;

                float2 blockShift = (frac(d.rg * 7.0) - 0.5) * _BlockSize * 1.5 * strength * _ApplyDistortChroma;
                float2 blockUV    = floor((uvChroma + blockShift) / _BlockSize) * _BlockSize;

                uvY      += 0.5;
                uvChroma += 0.5;
                blockUV  += 0.5;

                float4 rgbaY = tex2D(_MainTex, uvY);
                float4 rgbaC = tex2D(_MainTex, blockUV);
                float alphaY = (rgbaY.a + rgbaC.a) * .5;

                float3 yccY = RGBtoYCbCr(rgbaY.rgb);
                float3 yccC = RGBtoYCbCr(rgbaC.rgb);

                yccC.yz += (d.wz - .5) * _Noise;
                yccC.yz = floor(yccC.yz * _Quantization) / _Quantization;

                float3 yccMix = (yccY.xyz + yccC.xyz) * .5;
                float3 col = YCbCrtoRGB(yccMix);

                float4 glitchCol;
                {
                    float2 uvG = uv + dir * strength * _ApplyDistortGlitch + 0.5;

                #if ENABLE_CHANNEL_SHIFT
                    float4 r = tex2D(_MainTex, uvG + _ChannelShift);
                    float4 g = tex2D(_MainTex, uvG);
                    float4 b = tex2D(_MainTex, uvG - _ChannelShift);

                    glitchCol = lerp(float4(r.r, g.g, b.b, dot(float3(r.a, g.a, b.a), float3(.333, .334, .333))), g, _Glitch);
                #else
                    glitchCol = tex2D(_MainTex, uvG);
                #endif
                }

                float mixAmt = _Intensity;
                float4 result = lerp(float4(col, alphaY), glitchCol, mixAmt);
                result.a = (alphaY + glitchCol.a) * .5;

                return result;
            }

            ENDHLSL
        }

        Pass // 1: Scanline-Based Blending
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _ShotTex;
            sampler2D _LockTex;
            sampler2D _FpsTex;
            sampler2D _NoiseTex;

            float _Weight;
            
            #pragma multi_compile_local _ ENABLE_NOISE_BLENDING

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o; o.vertex = v.vertex; o.uv = v.uv;
                return o;
            }

            half luma(half3 rgb)
            {
                return dot(rgb, half3(.299, .585, .114));
            }
            
            float4 _ScanlinesDrift;

            float4 frag(v2f i) : SV_Target
            {
                float scanLine = floor(i.uv.y * _ScanlinesDrift.y);
                float mask = step(1, fmod(scanLine, 2.0));

                float4 main = tex2D(_ShotTex, i.uv);
                float4 lock = tex2D(_LockTex, i.uv);
                float4 fps  = tex2D(_FpsTex, i.uv);

#if ENABLE_NOISE_BLENDING
                float4 noise = tex2D(_NoiseTex, i.uv);
                main = lerp(fps, main, step(1 - _Weight, noise.r));
                
                //main = lerp(fps, main, noise.r * _Weight);
                //main = lerp(fps, main, smoothstep(1.0 - _Weight - 0.1, 1.0 - _Weight + 0.1, noise.r));
#else
                main = lerp(fps, main, _Weight);
#endif

                float4 result = lerp(main, lock, mask * (1 - luma(main.rgb)) * _ScanlinesDrift.x);
                return result;
            }

            ENDHLSL
        }
        
        Pass // 2: Blit
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float4 main = tex2D(_MainTex, i.uv);
                return main;
            }

            ENDHLSL
        }
    }
}
