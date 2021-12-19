
namespace UnityEngine.Rendering.Universal
{
    [System.Serializable, VolumeComponentMenu("Custom/Glitch")]
    public sealed class GlitchVolumeEffect : VolumeComponent, IPostProcessComponent
    {
        
        public BoolParameter enableEffect = new BoolParameter(true);

        [Range(0f,1f)]
        public FloatParameter scanLineJitter = new FloatParameter(0.15f);

        [Range(0f, 1f)]
        public FloatParameter colorDrift = new FloatParameter(0f);

        [Range(0f, 100f)]
        public FloatParameter speed = new FloatParameter(0f);

        [Range(0f, 100f)]
        public FloatParameter blockSize = new FloatParameter(6f);
        public bool IsActive() => enableEffect==true;
        public bool IsTileCompatible() => false;
    }
}
