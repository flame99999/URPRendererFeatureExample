using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlitchRenderPassFeature : ScriptableRendererFeature
{
    GlitchRenderPass _ScriptablePass;

    public override void Create()
    {
        _ScriptablePass = new GlitchRenderPass();
        _ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var dest = RenderTargetHandle.CameraTarget;
        _ScriptablePass.Setup(renderer.cameraColorTarget, dest);
        renderer.EnqueuePass(_ScriptablePass);
    }
}

public class GlitchRenderPass : ScriptableRenderPass
{
    static readonly string _profilerTag = "Glitch Effects";
    static readonly int _BlockSizeId = Shader.PropertyToID("_BlockSize");
    static readonly int _SpeedId = Shader.PropertyToID("_Speed");
    static readonly int _Params = Shader.PropertyToID("_Params");
    private RenderTargetHandle Destination { get; set; }
    readonly Material _glitchMat;
    GlitchVolumeEffect _glitchVolumeEffect;
    RenderTargetIdentifier _currentTarget;
    RenderTargetHandle _temporaryColorTexture;

    public GlitchRenderPass()
    {
        var shader = Shader.Find("Custom/Glitch");
        _glitchMat = CoreUtils.CreateEngineMaterial(shader);
        _temporaryColorTexture.Init("temporaryColorTexture");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_glitchMat == null) return;
        if (!renderingData.cameraData.postProcessEnabled) return;
        //Find GlitchVolumeEffect to get the data set
        var stack = VolumeManager.instance.stack;
        _glitchVolumeEffect = stack.GetComponent<GlitchVolumeEffect>();
        if (_glitchVolumeEffect == null) return;
        if (!_glitchVolumeEffect.IsActive()) return;

        var cmd = CommandBufferPool.Get(_profilerTag);
        Render(cmd, ref renderingData);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    private void Render(CommandBuffer cmd, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.isSceneViewCamera) return;
        var source = _currentTarget;

        _glitchMat.SetFloat(_BlockSizeId, _glitchVolumeEffect.blockSize.value);
        _glitchMat.SetFloat(_SpeedId, _glitchVolumeEffect.speed.value);
        var scanLineThreshold = Mathf.Clamp01(1.0f - _glitchVolumeEffect.scanLineJitter.value * 1.2f);
        var scanLineDisplacement = 0.002f + Mathf.Pow(_glitchVolumeEffect.scanLineJitter.value, 3) * 0.05f;
        var colorDrift = new Vector2(_glitchVolumeEffect.colorDrift.value * 0.04f, Time.time * 606.11f);

        _glitchMat.SetVector(_Params, new Vector4(scanLineDisplacement, scanLineThreshold, colorDrift.x, colorDrift.y));

        var opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
        opaqueDesc.depthBufferBits = 0;
        if (Destination == RenderTargetHandle.CameraTarget)
        {
            cmd.GetTemporaryRT(_temporaryColorTexture.id, opaqueDesc, FilterMode.Bilinear);
            Blit(cmd, source, _temporaryColorTexture.Identifier(), _glitchMat, 0);
            Blit(cmd, _temporaryColorTexture.Identifier(), source);
        }
        else
        {
            Blit(cmd, source, Destination.Identifier(), _glitchMat, 0);
        }
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        if (Destination == RenderTargetHandle.CameraTarget)
            cmd.ReleaseTemporaryRT(_temporaryColorTexture.id);
    }

    public void Setup(in RenderTargetIdentifier currentTarget, RenderTargetHandle dest)
    {
        Destination = dest;
        _currentTarget = currentTarget;
    }
}