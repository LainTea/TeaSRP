﻿#ifndef TEA_LIT_PASS_INCLUDED
#define TEA_LIT_PASS_INCLUDED

    #include "../ShaderLibrary/TeaCommon.hlsl"
    #include "../ShaderLibrary/TeaSurface.hlsl"
    #include "../ShaderLibrary/TeaShadows.hlsl"
    #include "../ShaderLibrary/TeaLight.hlsl"
    #include "../ShaderLibrary/TeaBRDF.hlsl"
    #include "../ShaderLibrary/TeaLighting.hlsl"

    //CBUFFER_START(UnityPerMaterial)
    //    float4 _BaseColor;
    //CBUFFER_END

    TEXTURE2D(_BaseMap);
    SAMPLER(sampler_BaseMap);

    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
        UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
        UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor) 
        UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
        UNITY_DEFINE_INSTANCED_PROP(float,_Metallic)
        UNITY_DEFINE_INSTANCED_PROP(float,_Smoothness)
    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

    struct Attributes{
        float3 positionOS : POSITION;
        float2 baseUV : TEXCOORD0;
        float3 normalOS : NORMAL;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings{
        float4 positionCS : SV_POSITION;
        float3 positionWS : VAR_POSITION;
        float2 baseUV : VAR_BASE_UV;
        float3 normalWS : VAR_NORMAL;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    Varyings TeaLitPassVertex(Attributes input){
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input,output);
        output.positionWS = TransformObjectToWorld(input.positionOS);
        output.positionCS = TransformWorldToHClip(output.positionWS);

        #if UNITY_REVERSED_Z
            output.positionCS.z = min(output.positionCS.z,output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
        #else
            output.positionCS.z = max(output.positionCS.z,output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
        #endif

        output.normalWS = TransformObjectToWorldNormal(input.normalOS);
        float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
        output.baseUV = input.baseUV * baseST.xy + baseST.zw;
        return output;
    }

    float4 TeaLitPassFragment(Varyings input) : SV_TARGET{
        UNITY_SETUP_INSTANCE_ID(input);
        float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.baseUV);
        float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
        float4 base = baseMap * baseColor;

        #if defined (_CLIPPING)
        clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff));
        #endif
        
        Surface surface;
        surface.position = input.positionWS;
        surface.normal = normalize(input.normalWS);
        surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
        surface.depth = -TransformWorldToView(input.positionWS).z;
        surface.color = base.rgb;
        surface.alpha = base.a;    
        surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Metallic);
        surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Smoothness);
        surface.dither = InterleavedGradientNoise(input.positionCS.xy,0);

        #if defined(_PREMULTIPLY_ALPHA)
            BRDF brdf = GetBRDF(surface,true);
        #else
            BRDF brdf = GetBRDF(surface);
        #endif
        float3 color = GetLighting(surface,brdf);

        return float4(color,surface.alpha);
    }

#endif
