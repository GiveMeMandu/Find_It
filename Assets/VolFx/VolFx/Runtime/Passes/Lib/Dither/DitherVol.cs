using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  Dither © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Dither")]
    public sealed class DitherVol : VolumeComponent, IPostProcessComponent
    {
        [InspectorName("Weight")]
        [Tooltip("Full effects impact")]
        public ClampedFloatParameter m_Impact = new ClampedFloatParameter(0, 0, 1);
        
        [Header("Main")]
        [Tooltip("Power of pattern distribution")]
        public ClampedFloatParameter m_Power = new ClampedFloatParameter(0, 0, 1);
        
        [Tooltip("Image scale for dithering")]
        public ClampedFloatParameter m_Scale    = new ClampedFloatParameter(1, 0, 1);
        
        [Header("Settings")]
        [Tooltip("Pixelate image for dithering")]
        public BoolParameter         m_Pixelate = new BoolParameter(true, false);
        [Tooltip("Fps of dither animation")]
        public ClampedIntParameter   m_Fps     = new ClampedIntParameter(0, 0, 120);
        [Tooltip("Dither palette")]
        public Texture2DParameter    m_Palette = new Texture2DParameter(null, false);
        [Tooltip("Dither pattern")]
        public Texture2DParameter    m_Pattern = new Texture2DParameter(null, false);
        [Tooltip("Dithering method")]
        public NoiseModeParameter    m_Mode    = new NoiseModeParameter(DitherPass.Mode.Dither, false);
        
        [Header("Noise")]
        [InspectorName("Scale")]
        [Tooltip("Scale of a custom noise texture")]
        public NoInterpClampedFloatParameter m_NoiseScale = new NoInterpClampedFloatParameter(1, 0, 7);
        [Tooltip("Custom noise texture")]
        public Texture2DParameter    m_Noise              = new Texture2DParameter(null, false);

        // =======================================================================
        [Serializable]
        public class NoiseModeParameter : VolumeParameter<DitherPass.Mode>
        {
            public NoiseModeParameter(DitherPass.Mode value, bool overrideState) : base(value, overrideState) { }
        }
        
        // =======================================================================
        public bool IsActive() => active && (m_Scale.value < 1f || m_Power.value > 0f || m_Impact.value > 0f);

        public bool IsTileCompatible() => false;
    }
}