
Shader "Custom/PerspectiveProjection"
{
    Properties
    {
        _ProjectionTex ("Projection Texture", 2D) = "white" {}
        _Opacity ("Opacity", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "PerspectiveProjection"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_ProjectionTex);
            SAMPLER(sampler_ProjectionTex);

            float4x4 _ProjectionVP;
            float _Opacity;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionWS =
                    TransformObjectToWorld(input.positionOS.xyz);

                output.positionCS =
                    TransformWorldToHClip(output.positionWS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float4 projectedPosition = mul(
                    _ProjectionVP,
                    float4(input.positionWS, 1.0)
                );

                if (projectedPosition.w <= 0.0)
                    discard;

                float2 uv =
                    projectedPosition.xy / projectedPosition.w;

                uv = uv * 0.5 + 0.5;

                if (uv.x < 0.0 || uv.x > 1.0 ||
                    uv.y < 0.0 || uv.y > 1.0)
                    discard;

                half4 color = SAMPLE_TEXTURE2D(
                    _ProjectionTex,
                    sampler_ProjectionTex,
                    uv
                );

                color.a *= _Opacity;

                if (color.a <= 0.001)
                    discard;

                return color;
            }

            ENDHLSL
        }
    }
}
