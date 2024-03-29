﻿#ifndef TEA_LIT_PASS_INCLUDED
#define TEA_LIT_PASS_INCLUDED

    #include "../ShaderLibrary/TeaSurface.hlsl"
    #include "../ShaderLibrary/TeaShadows.hlsl"
    #include "../ShaderLibrary/TeaLight.hlsl"
    #include "../ShaderLibrary/TeaBRDF.hlsl"
    #include "../ShaderLibrary/GI.hlsl"
    #include "../ShaderLibrary/TeaLighting.hlsl"

    //CBUFFER_START(UnityPerMaterial)
    //    float4 _BaseColor;
    //CBUFFER_END

    struct Attributes{
        float3 positionOS : POSITION;
        float2 baseUV : TEXCOORD0;
        float3 normalOS : NORMAL;
        GI_ATTRIBUTE_DATA
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings{
        float4 positionCS : SV_POSITION;
        float3 positionWS : VAR_POSITION;
        float2 baseUV : VAR_BASE_UV;
        float3 normalWS : VAR_NORMAL;
        GI_VARYINGS_DATA
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    Varyings TeaLitPassVertex(Attributes input){
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input,output);
        TRANSFER_GI_DATA(input,output);

        output.positionWS = TransformObjectToWorld(input.positionOS);
        output.positionCS = TransformWorldToHClip(output.positionWS);

        #if UNITY_REVERSED_Z
            output.positionCS.z = min(output.positionCS.z,output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
        #else
            output.positionCS.z = max(output.positionCS.z,output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
        #endif

        output.normalWS = TransformObjectToWorldNormal(input.normalOS);
        output.baseUV = TransformBaseUV(input.baseUV);
        return output;
    }

    float4 TeaLitPassFragment(Varyings input) : SV_TARGET{
        UNITY_SETUP_INSTANCE_ID(input);
        float4 base = GetBase(input.baseUV);

        #if defined (_CLIPPING)
        clip(base.a - GetCutoff(input.baseUV));
        #endif
        
        Surface surface;
        surface.position = input.positionWS;
        surface.normal = normalize(input.normalWS);
        surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
        surface.depth = -TransformWorldToView(input.positionWS).z;
        surface.color = base.rgb;
        surface.alpha = base.a;    
        surface.metallic = GetMetallic(input.baseUV);
        surface.smoothness = GetSmoothness(input.baseUV);
        surface.dither = InterleavedGradientNoise(input.positionCS.xy,0);

        #if defined(_PREMULTIPLY_ALPHA)
            BRDF brdf = GetBRDF(surface,true);
        #else
            BRDF brdf = GetBRDF(surface);
        #endif

        GI gi = GetGI(GI_FRAGMENT_DATA(input),surface);
        float3 color = GetLighting(surface,brdf,gi);
        color += GetEmission(input.baseUV);

        return float4(color,surface.alpha);
    }

#endif
