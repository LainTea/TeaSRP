#ifndef TEA_UNLIT_PASS_INCLUDED
#define TEA_UNLIT_PASS_INCLUDED

    #include "../ShaderLibrary/TeaCommon.hlsl"

    //CBUFFER_START(UnityPerMaterial)
    //    float4 _BaseColor;
    //CBUFFER_END

    struct Attributes{
        float3 positionOS : POSITION;
        float2 baseUV : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings{
        float4 positionCS : SV_POSITION;
        float2 baseUV : VAR_BASE_UV;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    Varyings TeaUnlitPassVertex(Attributes input){
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input,output);
        float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
        output.positionCS = TransformWorldToHClip(positionWS);
        output.baseUV = TransformBaseUV(input.baseUV);
        return output;
    }

    float4 TeaUnlitPassFragment(Varyings input) : SV_TARGET{
        UNITY_SETUP_INSTANCE_ID(input);
        float4 base = GetBase(input.baseUV);
        #if defined(_CLIPPING)
            clip(base.a - GetCutoff(input.baseUV));
        #endif
        return base;
    }

#endif
