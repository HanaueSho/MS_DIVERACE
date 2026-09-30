using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class ToonRendererFeatureTestScript : ScriptableRendererFeature
{
    [Header("Toon Settings")]
    public Material toonMaterial;

    [Header("Layer")]
    public LayerMask toonLayer;

    ToonRenderPass toonRenderPass;  //ToonRenderPassのインスタンスを保持する変数


    public override void Create()
    {
        toonRenderPass = new ToonRenderPass();

        toonRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (toonMaterial == null)
            return;

        toonRenderPass.Setup(toonMaterial, toonLayer);  // ToonRenderPassにマテリアルとレイヤーを設定
        renderer.EnqueuePass(toonRenderPass);           // ToonRenderPassをレンダラーに追加
    }

    protected override void Dispose(bool disposing)
    {
        toonRenderPass = null;
    }


    class ToonRenderPass : ScriptableRenderPass
    {
        Material toonMaterial;
        LayerMask toonLayer;


        // Setupメソッドでマテリアルとレイヤーを設定
        public void Setup(Material material, LayerMask layer)
        {
            toonMaterial = material;
            toonLayer = layer;
        }


        // PassDataクラスを定義
        class PassData
        {
            public RendererListHandle rendererListHandle;
        }


        // RenderGraphを使ってレンダリングするためのメソッドをオーバーライド
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (toonMaterial == null)
                return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer)
                return;

            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
            RenderQueueRange renderQueueRange = RenderQueueRange.opaque;

            FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, toonLayer);
            ShaderTagId shaderTagId = new ShaderTagId("UniversalForward");

            DrawingSettings drawingSettings =
                RenderingUtils.CreateDrawingSettings(
                    shaderTagId,
                    renderingData,
                    cameraData,
                    lightData,
                    sortFlags
                );


            // 指定したマテリアルを使って再描画
            drawingSettings.overrideMaterial = toonMaterial;
            RendererListParams rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filterSettings);

            // RenderGraphにレンダーパスを追加
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Toon Objects", out var passData))
            {
                passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParams);

                builder.UseRendererList(passData.rendererListHandle);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => 
                        { context.cmd.DrawRendererList(data.rendererListHandle); }
                    );
            }
        }
    }
}