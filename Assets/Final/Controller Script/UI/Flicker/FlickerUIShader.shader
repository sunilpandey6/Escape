Shader "Custom/FlickerUIShader"
{
    Properties
    {
        _IdleColor ("Idle Color", Color) = (1,1,1,1)
        _FlickerColor ("Flicker Color", Color) = (0,0,0,1)
        _FlickerState ("Flicker State (0/1)", Float) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _IdleColor;
            float4 _FlickerColor;
            float _FlickerState;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            // --- Vertex ---
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            // --- Fragment ---
            fixed4 frag(v2f i) : SV_Target
            {
                // Clamp to ensure clean 0 or 1
                float f = saturate(_FlickerState);

                // Hard switch (square wave)
                float3 color = lerp(_IdleColor.rgb, _FlickerColor.rgb, f > 0.5 ? 1.0 : 0.0);

                return float4(color, _IdleColor.a);
            }

            ENDCG
        }
    }
}