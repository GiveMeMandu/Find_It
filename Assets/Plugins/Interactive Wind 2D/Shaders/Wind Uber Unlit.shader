// Made with Amplify Shader Editor v1.9.8.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Interactive Wind 2D/Wind Uber Unlit"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[Toggle(_ENABLEWIND_ON)] _EnableWind("Enable Wind", Float) = 1
		_WindRotation("Wind: Rotation", Float) = 0
		_WindMaxRotation("Wind: Max Rotation", Float) = 2
		_WindRotationWindFactor("Wind: Rotation Wind Factor", Float) = 1
		_WindSquishFactor("Wind: Squish Factor", Float) = 0.3
		_WindSquishWindFactor("Wind: Squish Wind Factor", Range( 0 , 1)) = 0
		[Toggle(_WINDLOCALWIND_ON)] _WindLocalWind("Wind: Local Wind", Float) = 0
		_WindNoiseScale("Wind: Noise Scale", Float) = 0.1
		_WindMinIntensity("Wind: Min Intensity", Float) = -0.5
		_WindMaxIntensity("Wind: Max Intensity", Float) = 0.5
		[Toggle(_WINDHIGHQUALITYNOISE_ON)] _WindHighQualityNoise("Wind: High Quality Noise", Float) = 0
		[Toggle(_WINDISPARALLAX_ON)] _WindIsParallax("Wind: Is Parallax", Float) = 0
		_WindXPosition("Wind: X Position", Float) = 0
		_WindFlip("Wind: Flip", Float) = 0
		[Toggle(_ENABLEUVDISTORT_ON)] _EnableUVDistort("Enable UV Distort", Float) = 1
		[KeywordEnum(UV,World)] _ShaderSpace("Shader Space", Float) = 0
		_UVDistortFade("UV Distort: Fade", Range( 0 , 1)) = 1
		[NoScaleOffset]_UVDistortShaderMask("UV Distort: Shader Mask", 2D) = "white" {}
		_UVDistortFrom("UV Distort: From", Vector) = (-0.02,-0.02,0,0)
		_UVDistortTo("UV Distort: To", Vector) = (0.02,0.02,0,0)
		_UVDistortSpeed("UV Distort: Speed", Vector) = (2,2,0,0)
		_UVDistortNoiseScale("UV Distort: Noise Scale", Vector) = (0.1,0.1,0,0)
		_UVDistortNoiseTexture("UV Distort: Noise Texture", 2D) = "white" {}
		[Toggle(_ENABLEUVSCALE_ON)] _EnableUVScale("Enable UV Scale", Float) = 0
		_UVScaleScale("UV Scale: Scale", Vector) = (1,1,0,0)
		_UVScalePivot("UV Scale: Pivot", Vector) = (0.5,0.5,0,0)
		[Toggle(_ENABLEBRIGHTNESS_ON)] _EnableBrightness("Enable Brightness", Float) = 0
		_Brightness("Brightness", Float) = 1
		[Toggle(_ENABLECONTRAST_ON)] _EnableContrast("Enable Contrast", Float) = 0
		_Contrast("Contrast", Float) = 1
		[Toggle(_ENABLESATURATION_ON)] _EnableSaturation("Enable Saturation", Float) = 0
		_Saturation("Saturation", Float) = 1
		[Toggle(_ENABLEHUE_ON)] _EnableHue("Enable Hue", Float) = 0
		_Hue("Hue", Range( -1 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		
		Pass
		{
		CGPROGRAM
			#define ASE_VERSION 19801

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_FRAG_COLOR
			#pragma shader_feature_local _ENABLEHUE_ON
			#pragma shader_feature_local _ENABLESATURATION_ON
			#pragma shader_feature_local _ENABLECONTRAST_ON
			#pragma shader_feature_local _ENABLEBRIGHTNESS_ON
			#pragma shader_feature_local _ENABLEUVSCALE_ON
			#pragma shader_feature_local _ENABLEUVDISTORT_ON
			#pragma shader_feature_local _ENABLEWIND_ON
			#pragma shader_feature_local _WINDLOCALWIND_ON
			#pragma shader_feature_local _WINDHIGHQUALITYNOISE_ON
			#pragma shader_feature_local _WINDISPARALLAX_ON
			#pragma shader_feature_local _SHADERSPACE_UV _SHADERSPACE_WORLD


			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord1 : TEXCOORD1;
			};

			uniform fixed4 _Color;
			uniform float _EnableExternalAlpha;
			uniform sampler2D _MainTex;
			uniform sampler2D _AlphaTex;
			uniform float _WindRotationWindFactor;
			uniform float WindMinIntensity;
			uniform float _WindMinIntensity;
			uniform float WindMaxIntensity;
			uniform float _WindMaxIntensity;
			uniform float _WindXPosition;
			uniform float WindNoiseScale;
			uniform float _WindNoiseScale;
			uniform float WindTime;
			uniform float _WindRotation;
			uniform float _WindMaxRotation;
			uniform float _WindFlip;
			uniform float _WindSquishFactor;
			uniform float _WindSquishWindFactor;
			uniform float2 _UVDistortFrom;
			uniform float2 _UVDistortTo;
			uniform sampler2D _UVDistortNoiseTexture;
			float4 _MainTex_TexelSize;
			uniform float2 _UVDistortSpeed;
			uniform float2 _UVDistortNoiseScale;
			uniform float _UVDistortFade;
			uniform sampler2D _UVDistortShaderMask;
			uniform float4 _UVDistortShaderMask_ST;
			uniform float2 _UVScalePivot;
			uniform float2 _UVScaleScale;
			uniform float _Brightness;
			uniform float _Contrast;
			uniform float _Saturation;
			uniform float _Hue;
			float FastNoise101_g356( float x )
			{
				float i = floor(x);
				float f = frac(x);
				float s = sign(frac(x/2.0)-0.5);
				    
				float k = 0.5+0.5*sin(i);
				return s*f*(f-1.0)*((16.0*k-4.0)*f*(f-1.0)-1.0);
			}
			
			float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float snoise( float2 v )
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D289( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			
			float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
			{
				original -= center;
				float C = cos( angle );
				float S = sin( angle );
				float t = 1 - C;
				float m00 = t * u.x * u.x + C;
				float m01 = t * u.x * u.y - S * u.z;
				float m02 = t * u.x * u.z + S * u.y;
				float m10 = t * u.x * u.y + S * u.z;
				float m11 = t * u.y * u.y + C;
				float m12 = t * u.y * u.z - S * u.x;
				float m20 = t * u.x * u.z - S * u.y;
				float m21 = t * u.y * u.z + S * u.x;
				float m22 = t * u.z * u.z + C;
				float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
				return mul( finalMatrix, original ) + center;
			}
			
			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}


			v2f vert( appdata_t IN  )
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( IN.vertex ).xyz, 1 ) ).xyz;
				OUT.ase_texcoord1.xyz = ase_positionWS;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				OUT.ase_texcoord1.w = 0;

				IN.vertex.xyz +=  float3(0,0,0) ;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				fixed4 alpha = tex2D (_AlphaTex, uv);
				color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			fixed4 frag(v2f IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#ifdef _WINDLOCALWIND_ON
				float staticSwitch117_g356 = _WindMinIntensity;
				#else
				float staticSwitch117_g356 = WindMinIntensity;
				#endif
				#ifdef _WINDLOCALWIND_ON
				float staticSwitch118_g356 = _WindMaxIntensity;
				#else
				float staticSwitch118_g356 = WindMaxIntensity;
				#endif
				float4 transform62_g356 = mul(unity_WorldToObject,float4( 0,0,0,1 ));
				#ifdef _WINDISPARALLAX_ON
				float staticSwitch111_g356 = _WindXPosition;
				#else
				float staticSwitch111_g356 = transform62_g356.x;
				#endif
				#ifdef _WINDLOCALWIND_ON
				float staticSwitch113_g356 = _WindNoiseScale;
				#else
				float staticSwitch113_g356 = WindNoiseScale;
				#endif
				float temp_output_50_0_g356 = ( ( staticSwitch111_g356 * staticSwitch113_g356 ) + WindTime );
				float x101_g356 = temp_output_50_0_g356;
				float localFastNoise101_g356 = FastNoise101_g356( x101_g356 );
				float2 temp_cast_0 = (temp_output_50_0_g356).xx;
				float simplePerlin2D121_g356 = snoise( temp_cast_0*0.5 );
				simplePerlin2D121_g356 = simplePerlin2D121_g356*0.5 + 0.5;
				#ifdef _WINDHIGHQUALITYNOISE_ON
				float staticSwitch123_g356 = simplePerlin2D121_g356;
				#else
				float staticSwitch123_g356 = ( localFastNoise101_g356 + 0.5 );
				#endif
				float lerpResult86_g356 = lerp( staticSwitch117_g356 , staticSwitch118_g356 , staticSwitch123_g356);
				float clampResult29_g356 = clamp( ( ( _WindRotationWindFactor * lerpResult86_g356 ) + _WindRotation ) , -_WindMaxRotation , _WindMaxRotation );
				float2 temp_output_1_0_g356 = IN.texcoord.xy;
				float temp_output_39_0_g356 = ( temp_output_1_0_g356.y + _WindFlip );
				float3 appendResult43_g356 = (float3(0.5 , -_WindFlip , 0.0));
				float2 appendResult27_g356 = (float2(0.0 , ( _WindSquishFactor * min( ( ( _WindSquishWindFactor * abs( lerpResult86_g356 ) ) + abs( _WindRotation ) ) , _WindMaxRotation ) * temp_output_39_0_g356 )));
				float3 rotatedValue19_g356 = RotateAroundAxis( appendResult43_g356, float3( ( appendResult27_g356 + temp_output_1_0_g356 ) ,  0.0 ), float3( 0,0,1 ), ( clampResult29_g356 * temp_output_39_0_g356 ) );
				#ifdef _ENABLEWIND_ON
				float2 staticSwitch95 = (rotatedValue19_g356).xy;
				#else
				float2 staticSwitch95 = IN.texcoord.xy;
				#endif
				float2 texCoord2_g357 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float3 ase_positionWS = IN.ase_texcoord1.xyz;
				#if defined( _SHADERSPACE_UV )
				float2 staticSwitch1_g357 = ( texCoord2_g357 / ( 100.0 * (_MainTex_TexelSize).xy ) );
				#elif defined( _SHADERSPACE_WORLD )
				float2 staticSwitch1_g357 = (ase_positionWS).xy;
				#else
				float2 staticSwitch1_g357 = ( texCoord2_g357 / ( 100.0 * (_MainTex_TexelSize).xy ) );
				#endif
				float2 lerpResult21_g358 = lerp( _UVDistortFrom , _UVDistortTo , tex2D( _UVDistortNoiseTexture, ( ( staticSwitch1_g357 + ( _UVDistortSpeed * _Time.y ) ) * _UVDistortNoiseScale ) ).r);
				float2 appendResult2_g360 = (float2(_MainTex_TexelSize.z , _MainTex_TexelSize.w));
				float2 uv_UVDistortShaderMask = IN.texcoord.xy * _UVDistortShaderMask_ST.xy + _UVDistortShaderMask_ST.zw;
				float4 tex2DNode3_g361 = tex2D( _UVDistortShaderMask, uv_UVDistortShaderMask );
				#ifdef _ENABLEUVDISTORT_ON
				float2 staticSwitch97 = ( staticSwitch95 + ( lerpResult21_g358 * ( 100.0 / appendResult2_g360 ) * ( _UVDistortFade * ( tex2DNode3_g361.r * tex2DNode3_g361.a ) ) ) );
				#else
				float2 staticSwitch97 = staticSwitch95;
				#endif
				#ifdef _ENABLEUVSCALE_ON
				float2 staticSwitch96 = ( ( ( staticSwitch97 - _UVScalePivot ) / _UVScaleScale ) + _UVScalePivot );
				#else
				float2 staticSwitch96 = staticSwitch97;
				#endif
				float4 tex2DNode17 = tex2D( _MainTex, staticSwitch96 );
				float4 temp_output_2_0_g363 = tex2DNode17;
				float4 appendResult6_g363 = (float4(( (temp_output_2_0_g363).rgb * _Brightness ) , temp_output_2_0_g363.a));
				#ifdef _ENABLEBRIGHTNESS_ON
				float4 staticSwitch101 = appendResult6_g363;
				#else
				float4 staticSwitch101 = tex2DNode17;
				#endif
				float4 temp_output_1_0_g364 = staticSwitch101;
				float3 saferPower5_g364 = abs( (temp_output_1_0_g364).rgb );
				float3 temp_cast_3 = (_Contrast).xxx;
				float4 appendResult4_g364 = (float4(pow( saferPower5_g364 , temp_cast_3 ) , temp_output_1_0_g364.a));
				#ifdef _ENABLECONTRAST_ON
				float4 staticSwitch103 = appendResult4_g364;
				#else
				float4 staticSwitch103 = staticSwitch101;
				#endif
				float4 temp_output_1_0_g365 = staticSwitch103;
				float4 break2_g366 = temp_output_1_0_g365;
				float3 temp_cast_6 = (( ( break2_g366.x + break2_g366.y + break2_g366.z ) / 3.0 )).xxx;
				float3 lerpResult5_g365 = lerp( temp_cast_6 , (temp_output_1_0_g365).rgb , _Saturation);
				float4 appendResult8_g365 = (float4(lerpResult5_g365 , temp_output_1_0_g365.a));
				#ifdef _ENABLESATURATION_ON
				float4 staticSwitch107 = appendResult8_g365;
				#else
				float4 staticSwitch107 = staticSwitch103;
				#endif
				float4 temp_output_2_0_g367 = staticSwitch107;
				float3 hsvTorgb1_g367 = RGBToHSV( temp_output_2_0_g367.rgb );
				float3 hsvTorgb3_g367 = HSVToRGB( float3(( hsvTorgb1_g367.x + _Hue ),hsvTorgb1_g367.y,hsvTorgb1_g367.z) );
				float4 appendResult8_g367 = (float4(hsvTorgb3_g367 , temp_output_2_0_g367.a));
				#ifdef _ENABLEHUE_ON
				float4 staticSwitch108 = appendResult8_g367;
				#else
				float4 staticSwitch108 = staticSwitch107;
				#endif
				
				fixed4 c = ( staticSwitch108 * IN.color );
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
	CustomEditor "InteractiveWind2D.InteractiveWindShaderGUI"
	
	Fallback Off
}
/*ASEBEGIN
Version=19801
Node;AmplifyShaderEditor.TexCoordVertexDataNode;19;-3109.2,-25.78311;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;120;-2871.628,78.63365;Inherit;False;_Wind;1;;356;ec83a27ec2e35b8448baba8208c7d3fd;0;1;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;44;-3035.372,-510.5242;Inherit;False;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;95;-2646.999,-32.56262;Inherit;False;Property;_EnableWind;Enable Wind;0;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;112;-2787.651,-190.6187;Inherit;False;ShaderSpace;16;;357;be729ef05db9c224caec82a3516038dc;0;1;3;SAMPLER2D;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;111;-2318.146,-183.8598;Inherit;False;_UVDistort;18;;358;d6b8c102b9317a0418c08eb00598bec7;0;5;27;FLOAT;0;False;28;FLOAT2;0,0;False;1;FLOAT2;0,0;False;26;SAMPLER2D;;False;3;SAMPLER2D;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;97;-1939.292,-14.77417;Inherit;False;Property;_EnableUVDistort;Enable UV Distort;15;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;94;-1569.877,68.20679;Inherit;False;_UVScale;27;;362;9e0460161ab290d45839710d925e18c3;0;1;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;96;-1270.48,-46.32493;Inherit;False;Property;_EnableUVScale;Enable UV Scale;26;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;17;-845.595,-161.8598;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.FunctionNode;102;-389.3303,-39.81989;Inherit;False;_Brightness;31;;363;168f0b77e62f7fb42a256db752700c2b;0;1;2;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;101;-93.03845,-148.7292;Inherit;False;Property;_EnableBrightness;Enable Brightness;30;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;104;238.9317,-27.199;Inherit;False;_Contrast;34;;364;4615bb5f6468d1e4b879b863721c1960;0;1;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;103;530.9072,-146.7212;Inherit;False;Property;_EnableContrast;Enable Contrast;33;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;106;848.8085,-23.39538;Inherit;False;_Saturation;37;;365;d00cab46f082e684d9395d44e5a9f33a;0;1;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;107;1114.848,-136.7793;Inherit;False;Property;_EnableSaturation;Enable Saturation;36;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;105;1535.63,41.54014;Inherit;False;_Hue;40;;367;0bae267a40388e6498a395bd27428bc6;0;1;2;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;108;1899.791,-104.8561;Inherit;False;Property;_EnableHue;Enable Hue;39;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;15;2212.265,-93.82904;Inherit;False;TintVertex;-1;;368;b0b94dd27c0f3da49a89feecae766dcc;0;1;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;14;2572.595,-82.1935;Float;False;True;-1;2;InteractiveWind2D.InteractiveWindShaderGUI;0;10;Interactive Wind 2D/Wind Uber Unlit;0f8ba0101102bb14ebf021ddadce9b49;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;False;True;3;1;False;;10;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;120;1;19;0
WireConnection;95;1;19;0
WireConnection;95;0;120;0
WireConnection;112;3;44;0
WireConnection;111;28;112;0
WireConnection;111;1;95;0
WireConnection;111;3;44;0
WireConnection;97;1;95;0
WireConnection;97;0;111;0
WireConnection;94;1;97;0
WireConnection;96;1;97;0
WireConnection;96;0;94;0
WireConnection;17;0;44;0
WireConnection;17;1;96;0
WireConnection;102;2;17;0
WireConnection;101;1;17;0
WireConnection;101;0;102;0
WireConnection;104;1;101;0
WireConnection;103;1;101;0
WireConnection;103;0;104;0
WireConnection;106;1;103;0
WireConnection;107;1;103;0
WireConnection;107;0;106;0
WireConnection;105;2;107;0
WireConnection;108;1;107;0
WireConnection;108;0;105;0
WireConnection;15;1;108;0
WireConnection;14;0;15;0
ASEEND*/
//CHKSM=D34F603749D13C72442E7477FC1D2B3444B084CE