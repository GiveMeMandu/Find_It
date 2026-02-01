//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Sharpen"
{
    SubShader
    {
        name "Sharpen"
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 0

        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off
        
        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
            
			#pragma multi_compile_local __ BOX

            sampler2D       _MainTex;
            sampler2D       _ValueTex;
            float4          _Thickness;         // 1 / width, 1 / height, offset x, offset y
            
            half4           _Data;
            
            #define _Center _Data.y
            #define _Side   _Data.z
            
            half4           _Color;

            // half4x4        _kernel; 
            
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

            frag_in vert (vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }
            
            half luma(half3 rgb)
            {
                return dot(rgb, half3(.299, .585, .114));
            }

            half4 frag (frag_in i) : SV_Target
            {
                // Sharpen calculations
                half4 initial = tex2D(_MainTex, i.uv);
                half4 result = 0;

                half l = luma(initial.rgb);
                _Thickness.xy *= tex2D(_ValueTex, float2(l, .5));
                
                i.uv += float2(_Thickness.z * l, _Thickness.w * l);
                
                result += tex2D(_MainTex, i.uv + float2( _Thickness.x, 0));
                result += tex2D(_MainTex, i.uv + float2(-_Thickness.x, 0));
                result += tex2D(_MainTex, i.uv + float2(0, _Thickness.y));
                result += tex2D(_MainTex, i.uv + float2(0,-_Thickness.y));
#ifdef BOX
                result += tex2D(_MainTex, i.uv + float2( _Thickness.x, _Thickness.y));
                result += tex2D(_MainTex, i.uv + float2(-_Thickness.x, _Thickness.y));
                result += tex2D(_MainTex, i.uv + float2( _Thickness.x,-_Thickness.y));
                result += tex2D(_MainTex, i.uv + float2(-_Thickness.x,-_Thickness.y));
#endif

                result *= _Side;
                result += initial * _Center;

                // Output to screen
                return initial + (result - initial) * _Color;
            }
            ENDHLSL
        }
    }
}