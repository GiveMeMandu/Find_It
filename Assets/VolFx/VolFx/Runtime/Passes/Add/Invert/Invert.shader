//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Invert"
{
    SubShader
    {
        name "Invert"
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
            
            sampler2D       _MainTex;
            sampler2D       _ValueTex;
            half4           _Weight;
            
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
		        return dot(rgb.rgb, half3(0.299, 0.587, 0.114));
		    }

            half4 frag (frag_in i) : SV_Target
            {
                half4 initial = tex2D(_MainTex, i.uv);
                half  power   = tex2D(_ValueTex, luma(initial.rgb));
                half4 invert  = half4(half3(2, 2, 2) * power - initial.rgb, initial.a);

                return lerp(initial, invert, _Weight.r);
            }
            ENDHLSL
        }
    }
}