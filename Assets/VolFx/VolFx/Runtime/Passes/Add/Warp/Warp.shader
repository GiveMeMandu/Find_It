//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Warp"
{
    SubShader
    {
        LOD 0

        ZTest Always
        Cull Off
        ZWrite Off
        ZClip false

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityShaderVariables.cginc"
            
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

            uniform sampler2D _MainTex;
            uniform sampler2D _NoiseTex;
            uniform sampler2D _GradTex;
            uniform half4 _MainTex_ST;

            uniform float4 _WarpDataA; // x: RadialScale, y: Tiling, z: Animation, w: Power
            uniform float4 _WarpDataB; // x: Remap, y: MaskScale, z: MaskHardness, w: MaskPower
            uniform float4 _WarpDataC; // x: 
            uniform float4 _Color;

            #define _RadialScale   _WarpDataA.x
            #define _Tiling        _WarpDataA.y
            #define _Animation     _WarpDataA.z
            #define _Power         _WarpDataA.w

            #define _Remap         _WarpDataB.x
            #define _MaskScale     _WarpDataB.y
            #define _MaskHardness  _WarpDataB.z
            #define _MaskPower     _WarpDataB.w
            
            #define _DistortionX   _WarpDataC.x
            #define _DistortionY   _WarpDataC.y
            #define _Distortion    _WarpDataC.z

            frag_in vert(vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }
            
            fixed luma(fixed3 rgb)
            {
                return dot(rgb.rgb, fixed3(0.299, 0.587, 0.114));
            }
            
            half4 frag(frag_in i) : SV_Target
            {
                float2 uv = i.uv.xy;

                float2 centeredUV = uv - 0.5;
                float dist = pow(length(centeredUV), .7);
                float2 distortedUV = centeredUV * (1.0 + _Distortion * dist);
                distortedUV += 0.5;

                distortedUV = saturate(distortedUV);

                float2 uvMainTex = distortedUV * _MainTex_ST.xy + _MainTex_ST.zw;
                half4 sceneColor = tex2D(_MainTex, uvMainTex);

                // Polar coordinates with radial scale and tiling
                float2 polarUV;
                polarUV.x = length(centeredUV) * _RadialScale * 2.0;
                polarUV.y = atan2(centeredUV.x, centeredUV.y) / 6.28318548 * _Tiling;

                // Animate noise input
                float2 noiseInput = frac(polarUV / 100 - float2(_Animation * _Time.y, 0) / 77);
                float noise = tex2D(_NoiseTex, noiseInput);

                noise = noise * 0.5 + 0.5;

                // Speed lines calculation with power and remap
                float threshold = _Remap;
                float speedLines = saturate((pow(noise, _Power) - threshold) / (1.0 - threshold));

                // Mask calculations
                float2 maskUV = uv * 2.0 - 1.0;
                float hardnessLerp = lerp(0.0, _MaskScale, _MaskHardness);
                float mask = pow(1.0 - saturate((length(maskUV) - _MaskScale) / (hardnessLerp - _MaskScale - 0.001)), _MaskPower);

                float maskedSpeedLines = speedLines * mask;

                float4 colorRGB = tex2D(_GradTex, luma(maskedSpeedLines)) + _Color.rgba;

                //tex2D(_GradTex, luma(maskedSpeedLines))
                
                half4 finalColor = lerp(sceneColor, half4(maskedSpeedLines * (colorRGB.rgb), 0.0), maskedSpeedLines * colorRGB.a);

                return half4(finalColor.rgb, sceneColor.a);
            }
            
            ENDCG
        }
    }
}
