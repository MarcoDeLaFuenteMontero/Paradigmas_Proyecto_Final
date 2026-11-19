Shader "Custom/AnamorphicHallway"
{
    Properties
    {
        _ViewOrigin ("Punto de vista", Vector) = (30,1.7,9.75,1)
        _PlaneX ("Plano de pared aparente", Float) = 24.2
        _RestrictOpening ("Solo dentro del hueco aparente", Float) = 0
        _Burgundy ("Granate", Color) = (0.55,0.25,0.22,1)
        _Cream ("Crema", Color) = (0.76,0.70,0.48,1)
        _ShowStripes ("Mostrar lineas", Float) = 0
        _ShowPictures ("Mostrar cuadros", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+100" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            ZWrite Off
            ZTest LEqual
            Offset -2, -2
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float4 _ViewOrigin;
            float _PlaneX;
            float _RestrictOpening;
            half4 _Burgundy;
            half4 _Cream;
            float _ShowStripes;
            float _ShowPictures;
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; };
            V vert(A input)
            {
                V o; o.world=TransformObjectToWorld(input.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.world); return o;
            }
            half4 frag(V input):SV_Target
            {
                // Volumen completo del dormitorio mas el corredor de salida.
                // El acabado real reaparece al entrar en la biblioteca.
                float3 eye = _WorldSpaceCameraPos;
                bool inRoom = eye.x >= 23.8 && eye.x <= 36.2
                    && eye.z >= -0.2 && eye.z <= 10.2;
                bool inHall = eye.x >= 19.8 && eye.x <= 24.2
                    && eye.z >= 7.3 && eye.z <= 10.2;
                bool inEntry = eye.x >= 27.8 && eye.x <= 32.2
                    && eye.z >= 9.8 && eye.z <= 16.2;
                if ((!inRoom && !inHall && !inEntry) || eye.y < -0.25 || eye.y > 5.7) discard;
                float3 ray=input.world-_ViewOrigin.xyz;
                if(abs(ray.x)<0.00001) discard;
                float t=(_PlaneX-_ViewOrigin.x)/ray.x;
                if(t<=0) discard;
                float3 q=_ViewOrigin.xyz+ray*t;
                // Pintar todo el fondo mientras se observa desde la zona valida
                // evita ventanas verdes al moverse dentro del dormitorio.
                half4 color=_Burgundy;
                if(q.y<0.18) color=half4(0.56,0.41,0.23,1);
                else if(q.y<1.145) color=_Cream;
                else if(q.y<1.235) color=half4(0.90,0.84,0.66,1);
                else if (_ShowStripes > 0.5)
                {
                    float phase=frac((q.z+0.16)/0.32);
                    float edge=min(phase,1-phase)*0.32;
                    float aa=max(fwidth(q.z),0.001);
                    color=lerp(_Cream,_Burgundy,smoothstep(0.008-aa,0.008+aa,edge));
                }
                // Dos cuadros: uno en la pared real y otro repartido en el pasillo.
                float centre=q.z<6.2?3.5:8.75;
                float2 picture=abs(float2(q.z-centre,q.y-2.05));
                if(_ShowPictures > 0.5 && picture.x<0.48 && picture.y<0.62)
                {
                    color=half4(0.40,0.29,0.15,1);
                    if(picture.x<0.41 && picture.y<0.55)
                    {
                        color=q.z<6.2?half4(0.30,0.42,0.28,1):half4(0.26,0.42,0.47,1);
                        if(abs(q.z-centre+0.08)<0.16 && abs(q.y-2.01)<0.25) color=_Cream;
                    }
                }
                return color;
            }
            ENDHLSL
        }
    }
}
