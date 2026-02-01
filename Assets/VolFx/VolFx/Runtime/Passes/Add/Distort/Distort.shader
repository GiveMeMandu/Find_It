//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Distort"
{
    Properties
    {
        _Settings("Settigns", Vector) = (0,0,0,0)
        _Weight("Settigns", Float) = 0
    }

    SubShader
    {
        name "Distort"
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
            float4       _Settings;  // x - sharpness, y - tiling, x - offset, w - angle
            float        _Weight;
            
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
                // Sharpen calculations
                half4 initial = tex2D(_MainTex, i.uv);
                
                float2 dir   = float2(cos(_Settings.w), sin(_Settings.w));
                float offset = sin(dot(i.uv - .5, dir) * _Settings.y + _Settings.z) * _Settings.x;

                half4 sample  = tex2D(_MainTex, frac(i.uv + float2(-sin(_Settings.w), cos(_Settings.w)) * offset)); // repeat sampling
                //half4 sample  = tex2D(_MainTex, abs(step(.5f, frac(i.uv / 2)) - frac(i.uv))); // mirror sampling

                // Output to screen
                return lerp(initial, sample, _Weight);
                return sample;
            }
            
            ENDHLSL
        }
    }
}