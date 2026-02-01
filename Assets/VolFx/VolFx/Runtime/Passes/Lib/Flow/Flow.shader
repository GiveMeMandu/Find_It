//  FlowFx © NullTale - https://x.com/NullTale
Shader "Hidden/Vol/Flow"
{
    HLSLINCLUDE
    
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
    
    ENDHLSL

	SubShader 
	{
        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off
		
        Pass	// 0
		{
			name "Flow"
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
	        
            #pragma multi_compile_local _MOTION _
	        	        
	        sampler2D    _MainTex;
	        sampler2D    _FlowTex;
	        sampler2D    _MotionTex;
	        
	        float4		 _Weight;
	        float4		 _Tiling;
	        float4		 _Tint;
	        float4		 _Data;
	        
            #define _offset _Tiling.xy
            #define _scale  _Tiling.z
            #define _rot    _Tiling.w
	        
	        float2 rotate(float2 vec, float angle)
	        {
		        float c = cos(angle);
		        float s = sin(angle);
	        	
	        	return float2(dot(vec, float2(c, -s)), dot(vec, float2(s, c)));
	        }

	        float2 sampleFlow(float2 uv)
            {
                float flow  = tex2D(_MotionTex, uv).r; // grayscale (0-1)
                float angle = flow * 6.2831853; // map to 0-2PI
                float2 dir  = float2(cos(angle), sin(angle));
                return dir * _Data.y * flow;
            }
	        
            float luma(float3 rgb)
            {
                return dot(rgb, float3(.299, .587, .114));
            }
	        
	        half4 frag(frag_in i) : SV_Target 
	        {
	            half4 col  = tex2D(_MainTex, i.uv);
	            half4 flow = tex2D(_FlowTex, frac(rotate(i.uv - float2(.5, .5), _Tiling.w) * _scale + float2(.5, .5) + _offset
#ifdef _MOTION
	            	+ sampleFlow(i.uv + _Data.w)
#endif
	            	));
	        	
                float dist = distance(i.uv, float2(0.5, 0.5));
                float vig  = 1 - smoothstep(_Data.x, _Data.x - 0.5, dist);
	        	float adaptive = _Data.z * luma(col.rgb);

	        	_Weight.xy = lerp(float2(1, 0), _Weight, saturate(vig - adaptive));
	        	flow.rgb = lerp(flow.rgb, flow.rgb * _Tint.rgb, _Tint.a);
	        	
				return saturate(col * _Weight.x + flow * _Weight.y);
	        }
			
			ENDHLSL
		}
		
        Pass	// 1
		{
			name "Blit"
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
	        	        
	        sampler2D    _MainTex;
	        	        
	        half4 frag(frag_in i) : SV_Target 
	        {
	            return tex2D(_MainTex, i.uv);
	        }
			
			ENDHLSL
		}
		
        Pass	// 2
		{
			name "Print"

			Blend SrcAlpha OneMinusSrcAlpha
			
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
			
	        sampler2D    _MainTex;
	        float4		 _Weight;
	        
            half luma(half3 rgb)
            {
                return dot(rgb, half3(.299, .585, .114));
            }
	        	      
	        	        
	        half4 frag(frag_in i) : SV_Target 
	        {
	            half4 result =  tex2D(_MainTex, i.uv);

	        	result.a *= _Weight.z * lerp(1, luma(result.rgb) * 2, _Weight.w);
	        	return result;
	        }
			
			ENDHLSL
		}
		
        Pass	// 3
		{
			name "Filter"

			Blend SrcAlpha OneMinusSrcAlpha
			
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
			
	        sampler2D    _MainTex;
	        sampler2D    _ValueTex;
	        
            half luma(half3 rgb)
            {
                return dot(rgb, half3(.299, .585, .114));
            }
	        	        
	        half4 frag(frag_in i) : SV_Target 
	        {
	            half4 result =  tex2D(_MainTex, i.uv);
	        	result *= tex2D(_ValueTex, i.uv).r;

	        	return result;
	        }
			
			ENDHLSL
		}
	}
}