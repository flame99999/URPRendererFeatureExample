using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class PipelineSetter : MonoBehaviour
{
    public UniversalRenderPipelineAsset pipelineAsset;

    private void OnEnable()
    {

        if (pipelineAsset)
        {
            GraphicsSettings.renderPipelineAsset = pipelineAsset;
        }
    }
}


