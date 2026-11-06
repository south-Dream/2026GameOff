Shader "PlaneShift/CloudMist"
{
    Properties
    {
        _Color ("Cloud Color", Color) = (0.86, 0.91, 0.88, 1)
        _Opacity ("Opacity", Range(0, 1)) = 0.45
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            float _Opacity;
            struct v2f { float4 vertex : SV_POSITION; float3 normal : TEXCOORD0; float3 view : TEXCOORD1; };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.view = UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float edge = saturate(dot(normalize(i.normal), normalize(i.view)));
                float alpha = pow(edge, 1.5) * _Opacity;
                float light = 0.94 + saturate(normalize(i.normal).y) * 0.06;
                return fixed4(_Color.rgb * light, alpha);
            }
            ENDCG
        }
    }
}
