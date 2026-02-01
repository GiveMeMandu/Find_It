using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  Pixelation © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Pixelation")]
    public sealed class PixelationVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("General Image scale")]
        public ClampedFloatParameter         m_Scale     = new ClampedFloatParameter(1, 0, 1f);
        [Tooltip("Gap between pixels")]
        public NoInterpClampedFloatParameter m_Grid      = new NoInterpClampedFloatParameter(1f, 0, 1f);
        [Tooltip("Pixel roundness")]
        public ClampedFloatParameter         m_Roundness = new ClampedFloatParameter(0f, 0f, 1f);
        [Tooltip("Grid color")]
        public NoInterpColorParameter        m_Color     = new NoInterpColorParameter(Color.clear);
        [Tooltip("Palette texture (just set of colors in image format)")]
        public Texture2DParameter            m_Palette  = new Texture2DParameter(null, false);
        
        [Tooltip("Palette impact")]
        public ClampedFloatParameter         m_Impact   = new ClampedFloatParameter(0, 0, 1);
        
        [Tooltip("Pixelate without texture sampling, keep colors, but parts of the image may disappear")]
        public BoolParameter                 m_Crisp    = new BoolParameter(false, false);
        
        // =======================================================================
        public bool IsActive() => active && (m_Scale.value < 1 || (m_Palette.value != null && m_Impact.value > 0f));

        public bool IsTileCompatible() => false;
    }
}