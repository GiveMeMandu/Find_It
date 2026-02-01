//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Comics"
{    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        Cull Off
        ZWrite Off
        ZTest Always
        ZClip false
            
        Pass    // 0
        {
            Name "Comics"
            
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_local OUTLINE_BLACK OUTLINE_WHITE OUTLINE_DOUBLE OUTLINE_CUSTOM
            #pragma multi_compile_local _ SAMPLE_DEPTH

            sampler2D _MainTex;
	        sampler2D _DitherTex;
            
	        sampler2D _ShadesTex;
	        sampler2D _ColorsTex;
            
	        sampler2D _MeasureTex;
	        sampler2D _OutlineTex;
            
            sampler2D _CameraDepthTexture;
            
            uniform float4 _DataA;
            uniform float4 _DataB;
            uniform float4 _DataC;
            
            uniform float4 _OutlineColor;

            #define _DitherScale  _DataA.xy
            #define _DitherOffset _DataA.zw

            #define _Thickness    _DataB.x
            #define _DitherPower  _DataB.y
            #define _Sharpness    _DataB.z
            #define _DitherWeight _DataB.w

            #define _ColorWeight  _DataC.x
            #define _PaperWeight  _DataC.y
            #define _ShadeWeight  _DataC.z
            #define _FancyWeight  _DataC.w
            
            
            struct vert_in
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct frag_in
            {
                float2 uv : TEXCOORD0;
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
#ifdef SAMPLE_DEPTH
                return tex2D(_CameraDepthTexture, uv).r;
#endif
                
                float4 col = tex2D(_MainTex, uv);
                return dot(col.rgb, float3(0.299, 0.587, 0.114)) - (1 - col.a);
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
            
            float luma(float3 col)
            {
                return dot(col.rgb, float3(0.299, 0.587, 0.114));
            }
            
            float bright(float3 col)
            {
                return max(col.r, max(col.g, col.b));
            }
            
            float cont(float3 col)
            {
                return saturate(col - .5) * 2;
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
                
                //result.rgb = result.rgb + abs(hue * .3);
                result.rgb = lerp(luma(initial.rgb), result.rgb, hue * .3 + 1);
                result.rgb = applyHue(result.rgb, hue);
                
                return result;
            }
            
            inline float3 poster(float3 col, float2 uv)
            {
                float gray = luma(col);
                half4 color = tex2D(_ColorsTex, float2(gray, 0));
                float3 shade = lerp(col, color, _ColorWeight * color.a);
                float4 shadeNext = tex2D(_ShadesTex, float2(gray, 0));

                float measure = pow(shadeNext.a, _DitherPower);
                measure *= tex2D(_MeasureTex, float2(gray, 0));
                
                half4 dither = step(tex2D(_DitherTex, uv * _DitherScale + _DitherOffset), measure);
                shade = lerp(shade, lerp(shade, shadeNext, dither), step(measure - .5, .5) * _DitherWeight);

                shade.rgb = lerp(shade, applyHsb(shade, _FancyWeight), measure * _ShadeWeight * _FancyWeight);
                
                return shade.rgb;
            }
                        
            half4 frag(frag_in i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);

                half4 colSample = (col
                    + tex2D(_MainTex, i.uv + float2(_Thickness, 0))
                    + tex2D(_MainTex, i.uv - float2(_Thickness, 0))
                    + tex2D(_MainTex, i.uv + float2(0, _Thickness))
                    + tex2D(_MainTex, i.uv - float2(0, _Thickness))) / 5;
                
                // half4 colSample = max(max(max(max(col,
                //     tex2D(_MainTex, i.uv + float2(_Thickness, 0))),
                //     tex2D(_MainTex, i.uv - float2(_Thickness, 0))),
                //     tex2D(_MainTex, i.uv + float2(0, _Thickness))),
                //     tex2D(_MainTex, i.uv - float2(0, _Thickness)));
                
                float lum = luma(colSample);
                
                half4 comics = half4(poster(col, i.uv), col.a);
                
                float outlineWeight = tex2D(_OutlineTex, float2(lum, 0));
                float outline = step(_Sharpness, sobel(i.uv, outlineWeight)) * step(.1, colSample.a);

#ifdef OUTLINE_BLACK
                comics = lerp(comics, half4(0, 0, .02, 1), outline);
#endif
#ifdef OUTLINE_WHITE
                comics = lerp(comics, half4(1, 1, .98, 1), outline);
#endif
#ifdef OUTLINE_DOUBLE
                half4 outlineColor = lerp(half4(1, 1, 1, 1), half4(0, 0, .02, 1), step(lum, .3)) * outline;
                comics             = lerp(comics, outlineColor, outline);
#endif
#ifdef OUTLINE_CUSTOM
                comics = lerp(comics, _OutlineColor, outline);
#endif
                
                return comics;
            }
            
            ENDHLSL
        }
    }
}