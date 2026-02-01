//  GlitchFx © NullTale - https://x.com/NullTale
Shader "Hidden/Vol/Glitch" 
{
	SubShader 
	{
		Pass 
		{
			name "Glitch"
			
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
					
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			sampler2D _MainTex;
			sampler2D _NoiseTex;	// step, dir, offset, color
			sampler2D _ColorTex;
			sampler2D _ShotTex;
			sampler2D _LockTex;
			sampler2D _ClipTex;
			sampler2D _BleedTex;
			
			float4  _NoiseMad;
			float4  _GridMad;
			float4  _Settings;	    // x - power, y - sharpen, z - crush, w - lock
			float4  _Settings_Ex;	// x - dispersion, y - weight, z - density, w - bleed
			
			#define _Power   _Settings.x
			#define _Sharpen _Settings.y
			#define _Screen  _Settings.z
			#define _Lock    _Settings.w
			
			#define _Dispersion _Settings_Ex.x
			#define _Weight     _Settings_Ex.y
			#define _Density    _Settings_Ex.z
			#define _Bleed      _Settings_Ex.w

			float4  _Sobel;
			float2  _Grid;
			
			//--------------------------------------------------------------
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
			
			float2 _snap(float2 uv)
            {
	            return float2(round(uv.x * _Grid.x) / _Grid.x, round(uv.y * _Grid.y) / _Grid.y);
            }
            
			float2 _snap_cor(float2 uv, float add)
            {
                float2 grid = _Grid.xy - 100 * add;
	            return float2(round(uv.x * grid.x) / grid.x, round(uv.y * grid.y) / grid.y);
            }
			
			fixed4 _sobel(sampler2D tex, float2 uv)
            {
            	fixed4 color =  tex2D(tex, uv) * _Sobel.z;
                color += tex2D(tex, uv + float2( _Sobel.x, 0)) * _Sobel.w;
                color += tex2D(tex, uv + float2(-_Sobel.x, 0)) * _Sobel.w;
                color += tex2D(tex, uv + float2(0, _Sobel.y))  * _Sobel.w;
                color += tex2D(tex, uv + float2(0,-_Sobel.y))  * _Sobel.w;

            	return color;
            }
			
			fixed4 _bleed(sampler2D tex, float2 uv)
            {
				fixed4 bleed = tex2D(_MainTex, uv + float2(0.01, 0));
				bleed += tex2D(_MainTex, uv + float2(0.02, 0));
				bleed += tex2D(_MainTex, uv + float2(0.01, -0.01));
				bleed += tex2D(_MainTex, uv + float2(0.02, -0.02));
				bleed /= 6; 
				fixed4 c = tex2D(_BleedTex, float2(_Bleed, 0));
				bleed.rgb *= c.rgb;

            	return bleed * c.a;
            }
 
			fixed4 frag(v2f i) : COLOR
			{
				fixed4 initial = tex2D(_MainTex, i.uv);

				// crush
				fixed4 noise = tex2D(_NoiseTex, mad(i.uv, _NoiseMad.xy, _NoiseMad.zw));
				
				fixed mask = step(noise.r, _Power);
				fixed dir = (noise.g - .5) * _Dispersion;

				fixed4 filter = tex2D(_ColorTex, float2(noise.a, 0));
				filter.rgb += initial * filter.a;
				//filter.rgb += filter.a * .735;
				
				fixed4 shot = tex2D(_ShotTex, i.uv + float2(noise.b * dir, 0));
				shot.rgb *= filter.rgb;
				
				// mosaic
				fixed4 grid = tex2D(_NoiseTex, mad(i.uv, _GridMad.xy, _GridMad.zw));
				fixed  cell = step(grid.a, _Density);
				
				fixed4 sob    = _sobel(_MainTex, i.uv);
				fixed4 pix    = tex2D(_LockTex, _snap_cor(i.uv, grid.a));
				fixed4 bleed  = abs(_bleed(_MainTex, i.uv));
				fixed4 screen = fixed4(tex2D(_ClipTex, i.uv).rgb * _Screen, 0);

				// combine
				fixed4 result = lerp(initial + sob, pix + screen, cell);
				result = lerp(result, shot, mask) + bleed;
				
				return lerp(initial, result, _Weight);
			}
			ENDCG
		}
		
		Pass 
		{
			name "Blit"
			
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
					
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			sampler2D _MainTex;
			
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
			 
			fixed4 frag (v2f i) : COLOR
			{
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
Fallback off
}