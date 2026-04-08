Shader "Custom/DwellUIShader"
{
    Properties
    {
        _IdleColor ("Idle Color", Color) = (1,1,1,1)
        _MidColor ("Mid Color", Color) = (1,0.5,0,1)
        _ActiveColor ("Active Color", Color) = (0,1,0,1)
        _Progress ("Progress", Range(0,1)) = 0
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

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 _IdleColor;
            float4 _MidColor;
            float4 _ActiveColor;
            float _Progress;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 color;

                if (_Progress < 0.5)
                {
                    float t = _Progress / 0.5;
                    color = lerp(_IdleColor.rgb, _MidColor.rgb, t);
                }
                else
                {
                    float t = (_Progress - 0.5) / 0.5;
                    color = lerp(_MidColor.rgb, _ActiveColor.rgb, t);
                }

                return float4(color, 1);
            }
            ENDCG
        }
    }
}