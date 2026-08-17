Shader "UnstableExperiment/VisionFog"
{
    Properties
    {
        _Color ("Dark Color", Color) = (0, 0, 0, 1)
        _InnerRadius ("Inner Radius", Float) = 1.6
        _OuterRadius ("Outer Radius", Float) = 3.2
        _FalloffPower ("Falloff Power", Float) = 2.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+500"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float2 _PlayerWorldPos;
            float _InnerRadius;
            float _OuterRadius;
            float _FalloffPower;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = distance(i.worldPos.xy, _PlayerWorldPos);
                float t = smoothstep(_InnerRadius, _OuterRadius, dist);
                t = pow(saturate(t), _FalloffPower);
                return fixed4(_Color.rgb, _Color.a * t);
            }
            ENDCG
        }
    }
}
