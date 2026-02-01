//  VhsFx © NullTale - https://x.com/NullTale
Shader "Hidden/Vol/Vhs" 
{
	SubShader 
	{
		Pass 
		{
			name "Vhs"
			
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
					
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_local _LINE_DISTORTION_ON _

			sampler2D _MainTex;
			sampler2D _VhsTex;
			sampler2D _ShadesTex;
            sampler2D _NoiseTex;
			
			float4 _InputA;	// _yScanline, _xScanline, _Intensity, _Rocking
			#define _yScanline _InputA.x
			#define _xScanline _InputA.y
			#define _Intensity _InputA.z
			#define _Rocking   _InputA.w
			
			float4 _InputB;	// _Tape, Pow, _Flickering, _Bleed
			#define _Tape        _InputB.x
			#define _Pow         _InputB.y
			#define _Flickering  _InputB.z
			#define _Bleed       _InputB.w
			
            float4 _Noise; // intensity, noise, scale, shades
            float4 _NoiseOffset;
			
			fixed4 _Glitch;
			
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }
			
			float rand(float3 co)
			{
			     return frac(sin(dot(co.xyz, float3(12.9898,78.233,45.5432) )) * 43758.5453);
			}
 
			fixed4 frag (v2f i) : COLOR
			{
				const fixed4 main = tex2D(_MainTex, i.uv);
				fixed4 vhs    = tex2D(_VhsTex, i.uv) * _Tape;
				
				fixed4 shades = tex2D(_ShadesTex, i.uv) * _Noise.w;
				vhs.rgb  += shades.rgb;

				float dx = abs(distance(i.uv.y, _xScanline));
				float dy = 1 - abs(distance(i.uv.y, _yScanline));

				// squeeze
				i.uv.x += (dy - .5) * _Rocking + (rand(float3(dy, dy, dy)) - .1) * _Pow;

				// line distortion
#ifdef _LINE_DISTORTION_ON
                i.uv.y = lerp(i.uv.y, _xScanline, step(dx, 0.01));
#endif
				i.uv = frac(i.uv);
				
				const fixed4 c = tex2D(_MainTex, i.uv);
				vhs.a = c.a;
				
				// flickering
				vhs.rgb += c.rgb - (rand(float3(i.uv.x, i.uv.y, _xScanline)) * _xScanline / 5) * _Flickering * _Glitch.rgb * _Glitch.a;
				
				// glow
				fixed3 bleed = tex2D(_MainTex, i.uv + float2(0.01, 0) * _Bleed).rgb;
				bleed += tex2D(_MainTex, i.uv + float2(0.02, 0) * _Bleed).rgb;
				bleed += tex2D(_MainTex, i.uv + float2(0.01, -0.01) * _Bleed).rgb;
				bleed += tex2D(_MainTex, i.uv + float2(0.02, -0.02) * _Bleed).rgb;
				bleed /= 6;

				vhs.rgb += fixed3(bleed.rgb * _xScanline * _Glitch.rgb * _Glitch.a) * step(.1, dot(bleed, fixed3(1, 1, 1)));


				// noise
                float4 noise = tex2D(_NoiseTex, i.uv * _Noise.z + _NoiseOffset.xy);
                float  alpha = tex2D(_NoiseTex, i.uv * _Noise.z + _NoiseOffset.zw).a;
                float  s = (alpha * (1 - _Noise.x) + _Noise.x * 2) * step(alpha, _Noise.y);
                
                vhs.rgb   = lerp(vhs.rgb, noise.rgb, s);
				
                // Output to screen
				return lerp(main, vhs, _Intensity);
			}
			ENDCG
		}
	}
Fallback off
}