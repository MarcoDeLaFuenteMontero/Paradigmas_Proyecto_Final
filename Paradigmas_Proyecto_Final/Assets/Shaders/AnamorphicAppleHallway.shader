Shader "Custom/AnamorphicAppleHallway"
{
    Properties
    {
        _ViewOrigin ("Punto de vista", Vector) = (-21.5,7.73,10.5,1)
        _PlaneX ("Plano de pared aparente", Float) = -27.85
        _FloorY ("Altura del suelo", Float) = 6
        _Burgundy ("Salmon", Color) = (.64,.39,.33,1)
        _Cream ("Crema", Color) = (.76,.70,.48,1)
        _Wood ("Madera", Color) = (.8,.65,.4,1)
        _SurfaceSmoothness ("Suavidad", Range(0,1)) = .5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+100" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            ZWrite Off
            ZTest LEqual
            Offset -2,-2
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            float4 _ViewOrigin;
            float _PlaneX, _FloorY;
            half4 _Burgundy, _Cream, _Wood;
            half _SurfaceSmoothness;
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float3 normalWS:TEXCOORD1; };
            V vert(A input) { V o; o.world=TransformObjectToWorld(input.positionOS.xyz); o.normalWS=TransformObjectToWorldNormal(input.normalOS); o.positionCS=TransformWorldToHClip(o.world); return o; }
            half4 frag(V input):SV_Target
            {
                // Same ray/virtual-wall construction as AnamorphicHallway.
                // Only these two connected spaces receive this separate material.
                float3 eye=_WorldSpaceCameraPos;
                bool room=eye.x>=-28.2 && eye.x<=-15.8 && eye.z>=5.8 && eye.z<=16.2;
                bool hall=eye.x>=-36.2 && eye.x<=-27.8 && eye.z>=6.3 && eye.z<=9.3;
                if((!room && !hall) || eye.y<5.8 || eye.y>15.8) discard;
                float3 ray=input.world-_ViewOrigin.xyz;
                if(abs(ray.x)<.00001) discard;
                float t=(_PlaneX-_ViewOrigin.x)/ray.x;
                if(t<=0) discard;
                float3 q=_ViewOrigin.xyz+ray*t;
                float h=q.y-_FloorY;
                // The centered false door is real scenery in front of this wall.
                // Leave its silhouette free of the offset painted trim.
                if(abs(q.z-11)<1.04 && h>=0 && h<3.06) discard;
                if(h<1.19)
                {
                    // Real wall sections keep their actual projecting trim.
                    if(input.world.x>_PlaneX-.05 && abs(input.normalWS.x)>.5) discard;
                    InputData lighting=(InputData)0;
                    lighting.positionWS=float3(_PlaneX,q.y,q.z);
                    lighting.normalWS=half3(1,0,0);
                    lighting.viewDirectionWS=normalize(_WorldSpaceCameraPos-lighting.positionWS);
                    lighting.bakedGI=SampleSH(lighting.normalWS);
                    lighting.shadowMask=half4(1,1,1,1);
                    lighting.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(input.positionCS);
                    SurfaceData surface=(SurfaceData)0;
                    surface.albedo=(h<.18 || h>=1.11)?_Wood.rgb:_Cream.rgb;
                    surface.alpha=1;
                    surface.occlusion=1;
                    surface.smoothness=_SurfaceSmoothness;
                    surface.normalTS=half3(0,0,1);
                    return UniversalFragmentPBR(lighting,surface);
                }
                return _Burgundy;
            }
            ENDHLSL
        }
    }
}
