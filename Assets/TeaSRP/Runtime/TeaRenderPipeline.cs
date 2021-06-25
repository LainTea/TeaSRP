using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TeaRenderPipeline : RenderPipeline
{
    TeaCameraRender renderer = new TeaCameraRender();

    bool useDynamicBatching, useGPUInstancing;

    TeaShadowSettings shadowSettings;

    public TeaRenderPipeline(bool useDynamicBatching, bool useGPUInstancing,bool useSRPBatcher,TeaShadowSettings shadowSettings)
    {
        this.shadowSettings = shadowSettings;
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach(Camera camera in cameras)
        {
            renderer.Render(context, camera, useDynamicBatching, useGPUInstancing, shadowSettings);
        }
    }
}
