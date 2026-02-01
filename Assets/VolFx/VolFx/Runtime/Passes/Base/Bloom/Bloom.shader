//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Bloom"
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
    
    half luma(half3 rgb)
    {
        return dot(rgb.rgb, half3(0.299, 0.587, 0.114));
    }
    
    half bright(half3 rgb)
    {
        return max(max(rgb.r, rgb.g), rgb.b);
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
			name "Filter"
			
	        HLSLPROGRAM
	        
            #pragma multi_compile_local _LUMA _BRIGHTNESS _
	        
			#pragma vertex vert
			#pragma fragment frag
	        
	        sampler2D    _MainTex;
	        sampler2D	 _ValueTex;
	        
	        half4 frag(frag_in i) : SV_Target 
	        {
	            half4 col  = tex2D(_MainTex, i.uv);
	        	half  val = 0;
#ifdef _LUMA
	        	val  = luma(col.rgb);
#endif
#ifdef _BRIGHTNESS
	        	val  = bright(col.rgb);
#endif
	        	// evaluate threshold
	        	val = tex2D(_ValueTex, half2(val, 0)).r;
	        	
	        	// get color replacement
	            return col * val;
	        }
			
			ENDHLSL
		}
		
		Pass	// 1
		{
			name "Down Sample"
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
	        
	        sampler2D    _MainTex;
	        float4		 _MainTex_TexelSize;
	        
	        float4 frag(frag_in i) : SV_Target 
	        {
				half4 offset = _MainTex_TexelSize.xyxy * float4(-1, -1, +1, +1);

			    half4 s;
				s  = tex2D(_MainTex, i.uv + offset.xy);
				s += tex2D(_MainTex, i.uv + offset.zy);
				s += tex2D(_MainTex, i.uv + offset.xw);
			    s += tex2D(_MainTex, i.uv + offset.zw);

	        	//return  tex2D(_MainTex, i.uv);
			    return s * (1.0 / 4.0);
	        }
			
			ENDHLSL
		}
		
		Pass	// 2
		{
			name "Up Sample"
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
	        
	        sampler2D    _MainTex;
	        sampler2D    _DownTex;
	        
	        float		 _Blend;
	        float4		 _DownTex_TexelSize;
	        float4		 _MainTex_TexelSize;
	        
	        float4 frag(frag_in i) : SV_Target 
	        {
				half4 offset = _MainTex_TexelSize.xyxy * half4(-1, -1, +1, +1);
			    half4 s;
			    s  = tex2D(_MainTex, i.uv + offset.xy);
			    s += tex2D(_MainTex, i.uv + offset.zy);
			    s += tex2D(_MainTex, i.uv + offset.xw);
			    s += tex2D(_MainTex, i.uv + offset.zw);

			    s = s * (1.0 / 4);
	        	
	            half4 down = tex2D(_DownTex, i.uv);
	        	// return tex2D(_MainTex, i.uv);
	            return lerp(s, down, _Blend);
	            //return lerp(tex2D(_MainTex, i.uv), down, _Blend);
	        	
	        	//s  = tex2D(_MainTex, i.uv);
	        	return tex2D(_MainTex, i.uv);
	        }
			
			ENDHLSL
		}
		
		Pass	// 3
		{
			name "Combine"
			
	        HLSLPROGRAM
	        
	        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	        #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
	        
			#pragma vertex vert
			#pragma fragment frag
	        
            #pragma multi_compile_local _BLOOM_ONLY _
            #pragma multi_compile_local _HDR _
	        
	        sampler2D    _MainTex;
	        sampler2D    _BloomTex;
	        sampler2D	 _ColorTex;
	        
	        float		 _Intensity;
	        float4		 _BloomTex_TexelSize;
	        
	        half4 frag(frag_in i) : SV_Target 
	        {
	            half4 col  = tex2D(_MainTex, i.uv);
	            half4 bloom = tex2D(_BloomTex, i.uv)  * _Intensity;
	        	
	        	half l = luma(bloom.rgb);
	        	half4 tint = tex2D(_ColorTex, half2(l, 0));
	        	bloom.rgb = lerp(bloom.rgb, tint.rgb* l, tint.a).rgb ;
#if (_HDR)
	        	 #if _GAMMA_20 && !UNITY_COLORSPACE_GAMMA
	             {
	                 bloom.rgb = LinearToGamma20(bloom).rgb;
	             }
	             #elif UNITY_COLORSPACE_GAMMA || _LINEAR_TO_SRGB_CONVERSION
	             {
	             	bloom.rgb = GetLinearToSRGB(bloom).rgb;
	             }
	             #endif
#endif
	        	
	        	bloom.a   = bright(bloom.rgb);
	        	//bloom.rgb *=bloom.a;
#ifdef _BLOOM_ONLY
	        	return bloom;
	        	
#endif
				return col + bloom;
				return half4(col.rgb + bloom.rgb, col.a);
	        }
			
			ENDHLSL
		}
	}
}