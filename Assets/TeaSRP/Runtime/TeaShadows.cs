﻿using UnityEngine;
using UnityEngine.Rendering;

public class TeaShadows
{
    const string bufferName = "Shadows";

    CommandBuffer buffer = new CommandBuffer { name = bufferName };

    ScriptableRenderContext context;

    CullingResults cullingResults;

    TeaShadowSettings settings;

    const int maxShadowedDirectionalLightCount = 4;

    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    ShadowedDirectionalLight[] ShadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    int ShadowedDirectionalLightCount;

    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    static int dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");

    static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount];

    public Vector2 ReserveDirectionalShadows(Light light,int visibleLightIndex)
    {
        if(ShadowedDirectionalLightCount < maxShadowedDirectionalLightCount &&
            light.shadows != LightShadows.None && light.shadowStrength > 0f &&
            cullingResults.GetShadowCasterBounds(visibleLightIndex,out Bounds b))
        {
            ShadowedDirectionalLights[ShadowedDirectionalLightCount] = new ShadowedDirectionalLight { visibleLightIndex = visibleLightIndex };
            return new Vector2(light.shadowStrength, ShadowedDirectionalLightCount++);
        }

        return Vector2.zero;
    }

    public void Render()
    {
        if(ShadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            buffer.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)settings.directional.atlasSize;
        buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true, false, Color.clear);
        buffer.BeginSample(bufferName);
        ExcuteBuffer();

        int split = ShadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = atlasSize / split;

        for(int i = 0;i < ShadowedDirectionalLightCount;i++)
        {
            RenderDirectionalShadows(i, split, tileSize);
        }

        buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
        buffer.EndSample(bufferName);
        ExcuteBuffer();
    }

    Vector2 SetTileViewport(int index,int split,float tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        return offset;
    }

    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m,Vector2 offset,int split)
    {
        if(SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }

        float scale = 1f / split;

        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);

        return m;
    }

    void RenderDirectionalShadows(int index,int split,int tileSize)
    {
        ShadowedDirectionalLight light = ShadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f,
                                            out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);
        shadowSettings.splitData = splitData;
        dirShadowMatrices[index] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, 
                                        SetTileViewport(index, split, tileSize), split);
        dirShadowMatrices[index] = projectionMatrix * viewMatrix;
        buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
        ExcuteBuffer();
        context.DrawShadows(ref shadowSettings);
    }

    public void Setup(ScriptableRenderContext context,CullingResults cullingResults,TeaShadowSettings settings)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;
        ShadowedDirectionalLightCount = 0;
    }

    void ExcuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(dirShadowAtlasId);
        ExcuteBuffer();
    }
}
