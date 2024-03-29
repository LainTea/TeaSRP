﻿#ifndef TEA_COMMON_INCLUDED
#define TEA_COMMON_INCLUDED

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
    #include "TeaUnityInput.hlsl"

    #define UNITY_MATRIX_M unity_ObjectToWorld
    #define UNITY_MATRIX_I_M unity_WorldToObject
    #define UNITY_MATRIX_V unity_MatrixV
    #define UNITY_MATRIX_VP unity_MatrixVP
    #define UNITY_MATRIX_P glstate_matrix_projection

    #if defined(_SHADOW_MASK_ALWAYS) || defined(_SHADOW_MASK_DISTANCE)
        #define SHADOW_SHADOWMASK
    #endif

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

    float Square(float v)
    {
        return v * v;
    }

    float DistanceSquared(float3 pA,float3 pB)
    {
        return dot(pA - pB,pA - pB);
    }

#endif
