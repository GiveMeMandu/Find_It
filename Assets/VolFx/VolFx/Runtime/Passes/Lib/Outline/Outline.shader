Shader "Hidden/Vol/Outline"
{
    SubShader
    {
        name "Outline"

        Tags { "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_local _LUMA _ALPHA _CHROMA _DEPTH
            #pragma multi_compile_local _SHARP _
            #pragma multi_compile_local _ADAPTIVE _
            #pragma multi_compile_local _SCREEN _

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            sampler2D _GradientTex;

            float4 _Data; // x - thickness, y - sensitive, z - depth space
            float4 _Fill;

            #define _Sensitive _Data.y
            #define _Thickness _Data.x
            #define _Depth _Data.z

            struct vert_in
            {
                float4 pos : POSITION;
                float2 uv  : TEXCOORD0;
            };

            struct frag_in
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float luma(float3 rgb)
            {
                return dot(rgb, float3(.299, .587, .114));
            }

            float luma(float4 rgba)
            {
                return dot(rgba.rgb, float3(.299, .587, .114)) + rgba.a;
            }

            float chroma(float3 rgb)
            {
                return max(rgb.r, max(rgb.g, rgb.b));
            }

            float chroma(float4 rgba)
            {
                return max(rgba.r, max(rgba.g, rgba.b)) + rgba.a;
            }

            float SampleDepth(float2 uv)
            {
#ifdef _LUMA
                return luma(tex2D(_MainTex, uv).rgba);
#endif
#ifdef _ALPHA
                return tex2D(_MainTex, uv).a;
#endif
#ifdef _CHROMA
                return chroma(tex2D(_MainTex, uv).rgba);
#endif
#ifdef _DEPTH
                return tex2D(_CameraDepthTexture, uv).r;
#endif
            }

            float sobel(float2 uv)
            {
                float2 delta = float2(_Thickness, _Thickness);
                float hr = 0;
                float vt = 0;

                hr += SampleDepth(uv + float2(-1, -1) * delta) *  1.0;
                hr += SampleDepth(uv + float2( 1, -1) * delta) * -1.0;
                hr += SampleDepth(uv + float2(-1,  0) * delta) *  2.0;
                hr += SampleDepth(uv + float2( 1,  0) * delta) * -2.0;
                hr += SampleDepth(uv + float2(-1,  1) * delta) *  1.0;
                hr += SampleDepth(uv + float2( 1,  1) * delta) * -1.0;

                vt += SampleDepth(uv + float2(-1, -1) * delta) *  1.0;
                vt += SampleDepth(uv + float2( 0, -1) * delta) *  2.0;
                vt += SampleDepth(uv + float2( 1, -1) * delta) *  1.0;
                vt += SampleDepth(uv + float2(-1,  1) * delta) * -1.0;
                vt += SampleDepth(uv + float2( 0,  1) * delta) * -2.0;
                vt += SampleDepth(uv + float2( 1,  1) * delta) * -1.0;

                return sqrt(hr * hr + vt * vt);
            }

            float sobel(float2 uv, float mul)
            {
                float2 delta = float2(_Thickness, _Thickness) * mul;
                float hr = 0;
                float vt = 0;

                hr += SampleDepth(uv + float2(-1, -1) * delta) *  1.0;
                hr += SampleDepth(uv + float2( 1, -1) * delta) * -1.0;
                hr += SampleDepth(uv + float2(-1,  0) * delta) *  2.0;
                hr += SampleDepth(uv + float2( 1,  0) * delta) * -2.0;
                hr += SampleDepth(uv + float2(-1,  1) * delta) *  1.0;
                hr += SampleDepth(uv + float2( 1,  1) * delta) * -1.0;

                vt += SampleDepth(uv + float2(-1, -1) * delta) *  1.0;
                vt += SampleDepth(uv + float2( 0, -1) * delta) *  2.0;
                vt += SampleDepth(uv + float2( 1, -1) * delta) *  1.0;
                vt += SampleDepth(uv + float2(-1,  1) * delta) * -1.0;
                vt += SampleDepth(uv + float2( 0,  1) * delta) * -2.0;
                vt += SampleDepth(uv + float2( 1,  1) * delta) * -1.0;

                return sqrt(hr * hr + vt * vt);
            }

            frag_in vert(vert_in input)
            {
                frag_in output;
                output.vertex = input.pos;
                output.uv     = input.uv;
                return output;
            }

            half4 frag(frag_in input) : SV_Target
            {
                half4 col = tex2D(_MainTex, input.uv);
                float s = 0;

#ifdef _ADAPTIVE
    #ifdef _LUMA
                s = pow(1 - saturate(sobel(input.uv, luma(col))), _Sensitive);
    #elif  _ALPHA
                s = pow(1 - saturate(sobel(input.uv, col.a)), _Sensitive);
    #elif  _CHROMA
                s = pow(1 - saturate(sobel(input.uv, chroma(col))), _Sensitive);
    #elif  _DEPTH
                s = pow(1 - saturate(sobel(input.uv, SampleDepth(input.uv) * _Depth)), _Sensitive);
    #endif
#else
                s = pow(1 - saturate(sobel(input.uv)), _Sensitive);
#endif

#ifdef _SCREEN
                half4 outline = tex2D(_GradientTex, input.uv.yx);
#else
                half4 outline = tex2D(_GradientTex, float2(1.03 - s, 0));
#endif

#ifdef _SHARP
    #ifdef _DEPTH
                half l = outline.a * (1 - floor(s + .03));
    #else
                half l = outline.a * (1 - round(s + .03));
    #endif
#else
                half l = outline.a * (1 - s);
#endif

                col.rgb = lerp(lerp(col.rgb, _Fill.rgb, _Fill.a), outline.rgb, l);
                col.a = saturate(col.a + l);

                return col;
            }

            ENDHLSL
        }
    }
}
