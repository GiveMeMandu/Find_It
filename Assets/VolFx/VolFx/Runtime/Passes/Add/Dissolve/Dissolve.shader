//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Dissolve"
{
    SubShader
    {
        name "Dissolve"
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
            
            sampler2D    _MainTex;
            sampler2D    _DissolveTex;
            sampler2D    _ColorTex;
            sampler2D    _OverlayTex;
            float4       _OverlayMad;
            float4       _DissolveMad;
            float4       _Dissolve;
            
            #pragma multi_compile_local _SHADE _
            #pragma multi_compile_local _OVER _
            
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
                return dot(rgb, half3(.299, .587, .114));
            }
            
	        float2 rotate(float2 vec, float angle)
	        {
		        float c = cos(angle);
		        float s = sin(angle);
	        	
	        	return float2(dot(vec, float2(c, -s)), dot(vec, float2(s, c)));
	        }
            
            half4 frag (frag_in i) : SV_Target
            {
                half4 sample  = tex2D(_MainTex, i.uv);
                half4 disolve = tex2D(_DissolveTex, rotate(mad(i.uv - float2(.5, .5), _DissolveMad.xy, _DissolveMad.zw), _Dissolve.y) + float2(.5, .5));

#ifndef _SHADE
                float val = _Dissolve.x;
#else
                float val = lerp(saturate((1 - luma(sample))) * 2 * _Dissolve.x, _Dissolve.x, pow(_Dissolve.x, 3));
                //float val = lerp((1 - luma(sample.rgb)) * _Dissolve.x, _Dissolve.x, _Dissolve.x);
#endif
                
                half4 color = tex2D(_ColorTex, float2(val, .5));
                color.a *= sample.a;
                
#ifdef _OVER
                half4 overlay = tex2D(_OverlayTex, mad(i.uv, _OverlayMad.xy, _OverlayMad.zw));
                color *= overlay;
#endif
                
                return lerp(sample, color, step(luma(disolve), val));
            }
            ENDHLSL
        }
    }
}