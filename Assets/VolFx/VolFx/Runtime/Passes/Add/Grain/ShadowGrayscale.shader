//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/ShadowGrayscale"
{
    Properties
    {
        [HideInInspector] [NoScaleOffset] [MainTexture] _MainTex("Base (RGB)", 2D) = "white" {}
        _Sensitivity("Sensitivity", Float) = 256
    }

    SubShader
    {
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
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            float  _Sensitivity;

            Texture2D    _MainTex;
            SamplerState _point_clamp_sampler;

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
                // Pixel colour
                half4 source = _MainTex.Sample(_point_clamp_sampler, i.uv);

                half gray = dot(source.rgb, half3(0.3, 0.59, 0.11));
                half impact = pow(1 - gray, _Sensitivity);

                //return lerp(source, half4(1, 0, 0, 0), impact);
                return lerp(source, gray, impact);
            }
            ENDHLSL
        }
    }
}