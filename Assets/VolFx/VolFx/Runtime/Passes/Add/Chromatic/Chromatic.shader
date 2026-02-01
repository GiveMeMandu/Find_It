//  VolFx © NullTale - https://x.com/NullTale
Shader "Hidden/VolFx/Chromatic"
{
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        ZClip false
            
        Pass
        {
            Name "Chromatic"
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag


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

            uniform sampler2D _MainTex;            
            fixed _Intensity;
            fixed4 _Data;
            #define  _Center _Data.x
            #define  _Weight _Data.y
            #define  _Radial _Data.z
            #define  _Alpha  _Data.w
            
            float2 _R;
            float2 _G;
            float2 _B;
            float4 _Rw;
            float4 _Gw;
            float4 _Bw;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }

            half4 sample(float mov, float2 uv)
            {
                float2 uvR = saturate(uv + _R * mov);
                float2 uvG = saturate(uv + _G * mov);
                float2 uvB = saturate(uv + _B * mov);
                
                half4 colR = tex2D(_MainTex, uvR) * _Rw;
                half4 colG = tex2D(_MainTex, uvG) * _Gw;
                half4 colB = tex2D(_MainTex, uvB) * _Bw;
                
                return half4(colR.rgb + colG.rgb + colB.rgb, saturate((colR.a + colG.a + colB.a) * _Alpha));
            }

            fixed4 frag(v2f i) : COLOR
            {
                float mov = _Intensity + pow(distance(float2(.5f, .5f), i.uv), 3) * _Radial;
                fixed4 result = sample(mov, i.uv);

                return lerp(tex2D(_MainTex, i.uv), result, _Weight);
            }
            ENDCG
        }
    }
}