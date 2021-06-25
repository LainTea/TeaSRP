#ifndef TEA_UNITY_INPUT_INCLUDED
#define TEA_UNITY_INPUT_INCLUDED

CBUFFER_START(UnityPerDraw)
float4x4 unity_ObjectToWorld;
float4x4 unity_WorldToObject;
real4 unity_WorldTransformParams;
float4 unity_LODFade;
CBUFFER_END

float3 _WorldSpaceCameraPos;
float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;

#endif
