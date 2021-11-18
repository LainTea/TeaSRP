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
        //LightMapShadowMask();

        this.shadowSettings = shadowSettings;
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }

    void LightMapShadowMask()
    {
#if UNITY_EDITOR
        SupportedRenderingFeatures.active = new SupportedRenderingFeatures()
        {
            mixedLightingModes = SupportedRenderingFeatures.LightmapMixedBakeModes.IndirectOnly| SupportedRenderingFeatures.LightmapMixedBakeModes.Subtractive,
        };

        SetupDrawMode();
#endif
    }

    static HashSet<UnityEditor.SceneView> sceneViewHaveValidateFunction = new HashSet<UnityEditor.SceneView>();

    static bool RejectDrawMode(UnityEditor.SceneView.CameraMode cameraMode)
    {
        if (cameraMode.drawMode == UnityEditor.DrawCameraMode.TexturedWire ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.ShadowCascades ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.RenderPaths ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.AlphaChannel ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.Overdraw ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.Mipmaps ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.SpriteMask ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.DeferredDiffuse ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.DeferredSpecular ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.DeferredSmoothness ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.DeferredNormal ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.ValidateAlbedo ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.ValidateMetalSpecular ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.ShadowMasks ||
            cameraMode.drawMode == UnityEditor.DrawCameraMode.LightOverlap
        )
            return false;

        return true;
    }

    static void UpdateSceneViewStates()
    {
        foreach (UnityEditor.SceneView sceneView in UnityEditor.SceneView.sceneViews)
        {
            if (sceneViewHaveValidateFunction.Contains(sceneView))
                continue;


            sceneView.onValidateCameraMode += RejectDrawMode;
            sceneViewHaveValidateFunction.Add(sceneView);
        }
    }

    public static void SetupDrawMode()
    {
        UnityEditor.EditorApplication.update -= UpdateSceneViewStates;
        UnityEditor.EditorApplication.update += UpdateSceneViewStates;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach(Camera camera in cameras)
        {
            renderer.Render(context, camera, useDynamicBatching, useGPUInstancing, shadowSettings);
        }
    }
}
