//  ColorMap © NullTale - https://x.com/NullTale
Shader "Hidden/Vol/ColorMap"
{    
    SubShader
    {
        name "Color Map"
        
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 0

        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off

        Pass
        {
            HLSLPROGRAM
            
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_local USE_PALETTE _
            
            float  _Intensity;
            float4 _Mask;

            sampler2D    _MainTex;
            sampler2D    _GradientTex;
            
            sampler2D    _hueTex;
            sampler2D    _satTex;
            sampler2D    _valTex;
            
            sampler2D    _lutTex;
            sampler2D    _palTex;

            struct vert_in
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct frag_in
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            frag_in vert(vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }
            
            half3 GetLinearToSRGB(half3 c)
            {
#if _USE_FAST_SRGB_LINEAR_CONVERSION
                return FastLinearToSRGB(c);
#else
                return LinearToSRGB(c);
#endif
            }

            half3 GetSRGBToLinear(half3 c)
            {
#if _USE_FAST_SRGB_LINEAR_CONVERSION
                return FastSRGBToLinear(c);
#else
                return SRGBToLinear(c);
#endif
            }
                        
            #define LUT_SIZE 16.
            #define LUT_SIZE_MINUS (16. - 1.)
            
            float3 lut_sample(in half3 col, const sampler2D tex)
            {
#if !defined(UNITY_COLORSPACE_GAMMA)
                float3 uvw = GetLinearToSRGB(col);
#else
                float3 uvw = col;
#endif
                float2 uv;
                
                // get replacement color from the lut set
                uv.y = uvw.y * (LUT_SIZE_MINUS / LUT_SIZE) + .5 * (1. / LUT_SIZE);
                uv.x = uvw.x * (LUT_SIZE_MINUS / (LUT_SIZE * LUT_SIZE)) + .5 * (1. / (LUT_SIZE * LUT_SIZE)) + floor(uvw.z * LUT_SIZE) / LUT_SIZE;    

                float3 lutColor = tex2D(tex, uv).rgb;
                
//#if !defined(UNITY_COLORSPACE_GAMMA)
//                lutColor = GetSRGBToLinear(lutColor.xyz);
//#endif

                return lutColor;
            }
            
            half luma(half3 rgb)
            {
                return dot(rgb, half3(.299, .587, .114));
            }

            half4 frag (frag_in i) : SV_Target
            {
                half4 initial  = tex2D(_MainTex, i.uv);
                
                half val   = luma(initial.rgb);
                half4 grad = tex2D(_GradientTex, frac(float2(val + _Mask.z, 0)) * _Mask.w);

                //return  half4(lut_sample(initial.rgb, _lutTex), 1);
#ifdef USE_PALETTE
                initial.rgb = lerp(initial.rgb, lerp(initial.rgb, lut_sample(initial.rgb, _lutTex), tex2D(_palTex, val)), _Intensity);
#endif
                half mask   = step(_Mask.x, val) * step(val, _Mask.y);

                return half4(lerp(initial.rgb, grad.rgb, _Intensity * grad.a * mask), initial.a);
            }
            ENDHLSL
        }
    }
}