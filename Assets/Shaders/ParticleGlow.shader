// Soft additive dot for sigil particles.
// Needs no texture: each particle is drawn as a round glow that fades to
// nothing at its edge, tinted by the particle's own color.
Shader "SigilCaster/ParticleGlow"
{
    Properties
    {
        _Intensity ("Intensity", Float) = 2
        _Softness ("Edge Softness", Range(0.5, 6)) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ParticleGlow"

            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half _Intensity;
                half _Softness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // 1 at the middle of the particle, 0 at its edge
                half fromCenter = saturate(1 - length(input.uv - 0.5) * 2);
                half glow = pow(fromCenter, _Softness);

                return half4(input.color.rgb * _Intensity, glow * input.color.a);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
