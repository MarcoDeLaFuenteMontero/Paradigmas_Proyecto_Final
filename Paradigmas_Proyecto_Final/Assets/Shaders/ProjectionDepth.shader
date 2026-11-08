
Shader "Custom/ProjectionDepth"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ProjectionDepth"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            float4 frag(Varyings input) : SV_Target
            {
                // Convertir posicion global a espacio de camara
                float3 positionVS =
                    TransformWorldToView(input.positionWS);

                // Profundidad real en unidades de Unity
                float eyeDepth = -positionVS.z;

                // R = profundidad
                // A = 1 indica superficie valida
                return float4(eyeDepth, 0.0, 0.0, 1.0);
            }

            ENDHLSL
        }
    }
}

