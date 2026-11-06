Shader "PlaneShift/SkyBackdrop"
{
    Properties
    {
        _TopColor ("Top", Color) = (0.25, 0.43, 0.49, 1)
        _MiddleColor ("Middle", Color) = (0.60, 0.74, 0.75, 1)
        _BottomColor ("Bottom", Color) = (0.84, 0.87, 0.81, 1)
        _GlowColor ("Light", Color) = (0.98, 0.86, 0.68, 1)
        _Glow ("Light Strength", Range(0, 1)) = 0.35
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _TopColor, _MiddleColor, _BottomColor, _GlowColor;
            float _Glow;
            struct v2f { float4 vertex : SV_POSITION; float4 screen : TEXCOORD0; };
            v2f vert(float4 vertex : POSITION)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(vertex);
                o.screen = ComputeScreenPos(o.vertex);
                return o;
            }
            float haze(float2 p)
            {
                return sin(p.x * 2.3 + sin(p.y * 1.7)) * sin(p.y * 2.1 + p.x * 0.6);
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.screen.xy / i.screen.w;
                float y = smoothstep(0, 1, uv.y);
                float3 color = lerp(_BottomColor.rgb, _MiddleColor.rgb, smoothstep(0, 0.6, y));
                color = lerp(color, _TopColor.rgb, smoothstep(0.3, 1, y));
                float2 light = (uv - float2(0.78, 0.82)) * float2(1.3, 1);
                float glow = exp(-dot(light, light) * 5) * _Glow;
                color = lerp(color, _GlowColor.rgb, glow);
                float mist = haze(uv * float2(5, 4)) * 0.018;
                return fixed4(color + mist, 1);
            }
            ENDCG
        }
    }
}
