// Soft additive glow for the wand orb.
// Brightest where the surface faces the camera and fades to nothing at the
// silhouette, so a plain sphere reads as a ball of light with no hard edge.
Shader "SigilCaster/OrbGlow"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.4, 0.75, 1, 1)
        _Intensity ("Intensity", Float) = 1.5
        _Falloff ("Edge Falloff", Range(0.1, 8)) = 2
        _WhiteCore ("White Core", Range(0, 1)) = 0.5
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
            Name "OrbGlow"

            Blend SrcAlpha One
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                half _Falloff;
                half _WhiteCore;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half facing = saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS)));
                half glow = pow(facing, _Falloff);

                // push the middle of the orb toward white so it looks hot
                half3 color = lerp(_Color.rgb, half3(1, 1, 1), glow * glow * _WhiteCore);
                return half4(color * _Intensity, glow * _Color.a);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
