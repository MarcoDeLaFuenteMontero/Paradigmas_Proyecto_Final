Shader "Custom/TextoCartel"
{
    Properties { _MainTex ("Font", 2D) = "white" {} }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            V vert(A v)
            {
                V o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz);
                o.uv=v.uv; o.color=v.color; return o;
            }
            half4 frag(V i):SV_Target
            {
                half4 c=i.color;
                c.a*=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a;
                return c;
            }
            ENDHLSL
        }
    }
}
