//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Scanlines"
{
    SubShader
    {
        name "Scanlines"
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
            
            Texture2D    _MainTex;
            SamplerState _point_clamp_sampler;
            
            float4       _Scanlines;
            float4       _Screen;
            float4       _Color;

            #define _Count     _Scanlines.x
            #define _Intensity _Scanlines.y
            #define _Colorize  _Scanlines.z
            #define _Offset    _Scanlines.w
            
            #define _Flicker   _Screen.x
            #define _Flip      _Screen.y
            #define _Grad      _Screen.z
            #define _Move      _Screen.w
            
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
                    
            half4 frag (frag_in i) : SV_Target
            {
                float grad = saturate(1 - frac(i.uv.y + _Move) - _Grad) * (1 / (1 - _Grad)) * _Flicker;
                
                float2 uv  = float2(i.uv.x, (i.uv.y + _Flip) % 1);
                float4 col = _MainTex.Sample(_point_clamp_sampler, uv);

                i.uv.y          += _Scanlines.w / _Count * 6;
                float2 sl        = float2(sin(i.uv.y * _Count), cos(i.uv.y * _Count));
	            float3 scanlines = lerp(float3(sl.y + sl.x, sl.y + sl.x, sl.y + sl.x), float3(sl.x, sl.y, sl.x), _Colorize);

                col.rgb += col.rgb * scanlines * _Intensity;
                col.rgb += col.rgb * grad + grad * _Color.rgb;

                // Output to screen
                return col;
            }
            ENDHLSL
        }
    }
}