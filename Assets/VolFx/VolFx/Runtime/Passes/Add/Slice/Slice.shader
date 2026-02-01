//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Slice"
{
    SubShader
    {
        name "Crush"
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
                float2 uv = i.uv;
                
                // slice calculations
                float2 dir   = float2(cos(_Settings.w), sin(_Settings.w));
                float offset = (step(sin(dot(i.uv - .5, dir) * _Settings.y/* + _Settings.z*/), 0) - .5f) * _Settings.x;
                
                half4 sample  = tex2D(_MainTex, saturate(i.uv + float2(-sin(_Settings.w), cos(_Settings.w)) * offset));

                return sample;
            }
            ENDHLSL
        }
    }
}