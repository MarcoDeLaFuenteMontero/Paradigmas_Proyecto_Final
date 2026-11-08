
Shader "Custom/PerspectiveProjection"
{
    Properties
    {
        _ProjectionTex ("Projection Texture", 2D) = "white" {}
        _Opacity ("Opacity", Range(0,1)) = 1
        _DepthBias ("Depth Bias", Range(0, 0.1)) = 0.02
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

            // Textura del objeto de referencia
            TEXTURE2D(_ProjectionTex);
            SAMPLER(sampler_ProjectionTex);

            // Textura de profundidad
            TEXTURE2D(_ProjectionDepthTex);
            SAMPLER(sampler_ProjectionDepthTex);

            // Matrices de la camara de proyeccion
            float4x4 _ProjectionVP;
            float4x4 _ProjectionView;

            // Parametros de profundidad
            float _ProjectionFar;
            float _DepthBias;

            // Control de visibilidad
            float _Opacity;
            float _PerspectiveProjectionVisibility;

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

                // Posicion del vertice en coordenadas globales
                output.positionWS =
                    TransformObjectToWorld(input.positionOS.xyz);

                // Posicion para renderizar en pantalla
                output.positionCS =
                    TransformWorldToHClip(output.positionWS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Proyectar el fragmento desde ProjectionCamera
                float4 projectedPosition = mul(
                    _ProjectionVP,
                    float4(input.positionWS, 1.0)
                );

                // Descartar puntos detras de la camara
                if (projectedPosition.w <= 0.0)
                {
                    discard;
                }

                // 2. Convertir a coordenadas UV
                float2 uv =
                    projectedPosition.xy / projectedPosition.w;

                uv = uv * 0.5 + 0.5;

                // Descartar puntos fuera de la proyeccion
                if (uv.x < 0.0 || uv.x > 1.0 ||
                    uv.y < 0.0 || uv.y > 1.0)
                {
                    discard;
                }

                // 3. Obtener el color del objeto proyectado
                half4 color = SAMPLE_TEXTURE2D(
                    _ProjectionTex,
                    sampler_ProjectionTex,
                    uv
                );

                // Aplicar opacidad y estado de materializacion
                color.a *= _Opacity *
                           _PerspectiveProjectionVisibility;

                // Descartar fondo transparente
                if (color.a <= 0.001)
                {
                    discard;
                }

                // 4. Calcular la profundidad real del fragmento
                float4 viewPosition = mul(
                    _ProjectionView,
                    float4(input.positionWS, 1.0)
                );

                float surfaceDepth = -viewPosition.z;

                // 5. Leer la textura de profundidad
                float4 depthSample = SAMPLE_TEXTURE2D(
                    _ProjectionDepthTex,
                    sampler_ProjectionDepthTex,
                    uv
                );

                // Descartar si no existe una superficie valida
                if (depthSample.a < 0.5)
                {
                    discard;
                }

                // Profundidad real almacenada en unidades Unity
                float nearestDepth = depthSample.r;

                // 6. Comparar profundidades
                float depthDifference =
                    surfaceDepth - nearestDepth;

                // Si el fragmento esta detras de otra superficie,
                // no debe recibir la proyeccion
                if (depthDifference > _DepthBias)
                {
                    discard;
                }

                // 7. Dibujar el color proyectado
                return color;
            }

            ENDHLSL
        }
    }
}
