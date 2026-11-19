

Shader "Custom/RoomPerspectiveProjection"
{
    Properties
    {
        _ProjectionTex ("Projection Texture", 2D) = "white" {}
        _Opacity ("Opacity", Range(0,1)) = 1
        _RoomProjectionVisibility ("Room Visibility", Float) = 1

        _OcclusionTex ("Occlusion Positions", 2D) = "black" {}
        _OcclusionEnabled ("Occlusion Enabled", Float) = 0
        _OcclusionTolerance ("Occlusion Tolerance", Float) = 0.05
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
            Offset -1, -1
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_ProjectionTex);
            SAMPLER(sampler_ProjectionTex);

            TEXTURE2D(_OcclusionTex);
            SAMPLER(sampler_OcclusionTex);
            float4 _OcclusionTex_TexelSize;

            float4x4 _ProjectionVP;
            float _Opacity;
            float _RoomProjectionVisibility;

            float _OcclusionEnabled;
            float _OcclusionTolerance;

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

                // Check projection occlusion
                if (_OcclusionEnabled > 0.5)
                {
                    float4 firstHit = SAMPLE_TEXTURE2D(
                        _OcclusionTex,
                        sampler_OcclusionTex,
                        uv
                    );

                    float distanceToFirstHit = firstHit.a > 0.5
                        ? distance(input.positionWS, firstHit.xyz) : 1e10;

                    // En una arista, el texel central puede pertenecer a la
                    // superficie vecina. Buscar continuidad a un texel de distancia.
                    if (distanceToFirstHit > _OcclusionTolerance)
                    {
                        [unroll] for (int y = -1; y <= 1; y++)
                        {
                            [unroll] for (int x = -1; x <= 1; x++)
                            {
                                float4 neighbor = SAMPLE_TEXTURE2D(_OcclusionTex,
                                    sampler_OcclusionTex,
                                    uv + float2(x, y) * _OcclusionTex_TexelSize.xy);
                                if (neighbor.a > 0.5)
                                    distanceToFirstHit = min(distanceToFirstHit,
                                        distance(input.positionWS, neighbor.xyz));
                            }
                        }
                    }

                    if (distanceToFirstHit > _OcclusionTolerance)
                        discard;
                }

                // El bake usa UV de la matriz GPU; la imagen de la camara
                // necesita la orientacion de textura del backend grafico.
                float2 imageUV = uv;
                #if UNITY_UV_STARTS_AT_TOP
                    imageUV.y = 1.0 - imageUV.y;
                #endif

                half4 color = SAMPLE_TEXTURE2D(
                    _ProjectionTex,
                    sampler_ProjectionTex,
                    imageUV
                );

                color.a *=
                    _Opacity * _RoomProjectionVisibility;

                if (color.a <= 0.001)
                    discard;

                return color;
            }

            ENDHLSL
        }
    }
}

