//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Posterize"
{
	SubShader 
	{
		Tags { "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline" }
		LOD 200
		
        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off
		
		Pass
		{
			name "Posterize"
			
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            
			#pragma vertex vert
			#pragma fragment frag
            
            Texture2D    _MainTex;
            SamplerState _point_repeat_sampler;
            
            float _Count;
            
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
                        
            frag_in vert(vert_in input)
            {
                frag_in output;
                output.vertex = input.pos;
                output.uv     = input.uv;
                
                return output;
            }
            
            half4 frag (frag_in input) : SV_Target 
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, _point_repeat_sampler, input.uv);

                col = pow(abs(col), 0.4545);
                float3 c = RgbToHsv(col.xyz);
                // c.x = round(c.x * _Count) / _Count;
                // c.y = round(c.y * _Count) / _Count;
                c.z = round(c.z * _Count) / _Count;
                col = float4(HsvToRgb(c), col.a);
                col = pow(abs(col), 2.2);

            	return col;
            }
			
			ENDHLSL
		}
	}
}
