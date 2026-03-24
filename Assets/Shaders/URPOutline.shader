Shader "Custom/URP/URPOutline"
{
    Properties
    {
        _OutlineColor   ("Outline / Glow Color",    Color)  = (1, 0.88, 0.21, 1)   // FFE135 default
        _OutlineThickness("Outline Thickness",      Range(0.0, 10.0)) = 1.5
        _GlowIntensity  ("Glow Intensity",          Range(1.0, 8.0))  = 2.0
        _DepthThreshold ("Depth Threshold",         Range(0.0001, 0.05)) = 0.005
        _NormalThreshold("Normal Threshold",        Range(0.0, 1.0))  = 0.4
    }

    SubShader
    {
        // ── Render after opaque geometry, before transparent ──
        Tags
        {
            "RenderType"      = "Opaque"
            "RenderPipeline"  = "UniversalPipeline"
            "Queue"           = "Transparent+1"
        }

        // ── Single pass, no depth write ──
        ZWrite Off
        ZTest Always
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "URPOutlineSobel"

            HLSLPROGRAM

            // ── VR: Single Pass Instanced ──
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
            #pragma multi_compile_instancing
            #pragma instancing_options renderingLayer

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            // ── Texel size supplied by URP ──
            float4 _CameraDepthTexture_TexelSize;

            // ── Properties ──
            CBUFFER_START(UnityPerMaterial)
                half4   _OutlineColor;
                float   _OutlineThickness;
                float   _GlowIntensity;
                float   _DepthThreshold;
                float   _NormalThreshold;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO          // VR stereo eye index
            };

            // ─────────────────────────────────────────────────────────────
            //  Vertex
            // ─────────────────────────────────────────────────────────────
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = IN.uv;
                return OUT;
            }

            // ─────────────────────────────────────────────────────────────
            //  Sobel kernel helpers
            //  Samples the depth texture at 8 neighbours and returns the
            //  Sobel edge magnitude (0 = flat, 1 = sharp edge)
            // ─────────────────────────────────────────────────────────────
            float SampleDepth(float2 uv)
            {
                // LinearEyeDepth converts the raw depth to view-space metres
                return LinearEyeDepth(
                    SampleSceneDepth(uv),
                    _ZBufferParams);
            }

            float SobelDepth(float2 uv, float2 texelSize)
            {
                float2 offset = texelSize * _OutlineThickness;

                // 3×3 neighbourhood
                float d00 = SampleDepth(uv + float2(-offset.x,  offset.y));
                float d10 = SampleDepth(uv + float2( 0.0,        offset.y));
                float d20 = SampleDepth(uv + float2( offset.x,   offset.y));
                float d01 = SampleDepth(uv + float2(-offset.x,  0.0      ));
                float d21 = SampleDepth(uv + float2( offset.x,  0.0      ));
                float d02 = SampleDepth(uv + float2(-offset.x, -offset.y ));
                float d12 = SampleDepth(uv + float2( 0.0,       -offset.y ));
                float d22 = SampleDepth(uv + float2( offset.x,  -offset.y ));

                // Sobel X and Y kernels
                float gx = -d00 - 2.0*d01 - d02 + d20 + 2.0*d21 + d22;
                float gy = -d00 - 2.0*d10 - d20 + d02 + 2.0*d12 + d22;

                return sqrt(gx*gx + gy*gy);
            }

            // ─────────────────────────────────────────────────────────────
            //  Normal Sobel — catches edges the depth filter misses on
            //  coplanar surfaces (e.g. flat UI panels at the same depth)
            // ─────────────────────────────────────────────────────────────
            float SobelNormals(float2 uv, float2 texelSize)
            {
                float2 offset = texelSize * _OutlineThickness;

                float3 n00 = SampleSceneNormals(uv + float2(-offset.x,  offset.y));
                float3 n20 = SampleSceneNormals(uv + float2( offset.x,  offset.y));
                float3 n02 = SampleSceneNormals(uv + float2(-offset.x, -offset.y));
                float3 n22 = SampleSceneNormals(uv + float2( offset.x, -offset.y));

                float3 gx = -n00 + n20 - n02 + n22;
                float3 gy = -n00 - n20 + n02 + n22;

                return sqrt(dot(gx, gx) + dot(gy, gy));
            }

            // ─────────────────────────────────────────────────────────────
            //  Fragment
            // ─────────────────────────────────────────────────────────────
            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);   // VR eye index

                float2 uv        = IN.uv;
                float2 texelSize = _CameraDepthTexture_TexelSize.xy;

                // ── Sobel edge detection (depth + normals combined) ──
                float edgeDepth   = SobelDepth(uv, texelSize);
                float edgeNormal  = SobelNormals(uv, texelSize);

                float depthEdge   = step(_DepthThreshold,  edgeDepth);
                float normalEdge  = step(_NormalThreshold, edgeNormal);

                float edge        = saturate(depthEdge + normalEdge);

                // ── Glow: boost color intensity ──
                half4 glowColor   = _OutlineColor;
                glowColor.rgb    *= _GlowIntensity;

                // ── Output: outline color where edge detected, transparent elsewhere ──
                return half4(glowColor.rgb, edge * _OutlineColor.a);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
