Shader "Custom/FlickerUIShader"
{
    Properties
    {
        _IdleColor ("Idle Color", Color) = (1,1,1,1)
        _FlickerColor ("Flicker Color", Color) = (0,0,0,1)
        _FlickerHz ("Flicker Hz", Float) = 15
        _FlickerDuration ("Flicker Duration", Float) = 2
        _FlickerPhase ("Flicker Phase", Float) = 1.1
        _FlickerIntensity ("Flicker Intensity", Range(0,1)) = 1
        _FlickerStartTime ("Flicker Start Time", Float) = 0
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
            float _FlickerHz;
            float _FlickerDuration;
            float _FlickerPhase;
            float _FlickerIntensity;
            float _FlickerStartTime;

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

            // --- Helpers ---
            float hash11(float n)
            {
                return frac(sin(n) * 43758.5453);
            }

            float mytrunc(float x, float l)
            {
                return floor(x * l) / l;
            }

            float flickerSmooth(float t)
            {
                if (t <= 0.0) return 0.0; // no flicker before start

                float timeLoop = fmod(t, _FlickerDuration);

                const int NUM_SAMPLES = 5;
                float invSamples = 1.0 / NUM_SAMPLES;
                float dt = 1.0 / _FlickerHz;
                float filterWidth = 2.0 * dt;

                float stepSize = filterWidth * invSamples;
                float sum = 0.0;
                float st = timeLoop - 0.25 * filterWidth;

                for (int i = 0; i < NUM_SAMPLES; i++)
                {
                    float ft = mytrunc(st + _FlickerPhase, _FlickerHz);
                    sum += hash11(ft);
                    st += stepSize;
                }

                return sum * invSamples;
}


            // --- Fragment ---
            fixed4 frag(v2f i) : SV_Target
            {
                float t = _Time.y - _FlickerStartTime;
                if (t <= 0.0 || t >= _FlickerDuration) return float4(_IdleColor.rgb, _IdleColor.a);
                if (t <= 0.0) return float4(_IdleColor.rgb, _IdleColor.a);
                float f = flickerSmooth(t);
                f = saturate(f * _FlickerIntensity); // clamp 0..1
                f = f > 0.5 ? 1.0 : 0.0;             // sharp on/off for SSVEP
                float3 color = lerp(_IdleColor.rgb, _FlickerColor.rgb, f);


                //float3 color = lerp(_IdleColor.rgb, _FlickerColor.rgb, f * _FlickerIntensity);

                return float4(color, _IdleColor.a);
            }

            ENDCG
        }
    }
}