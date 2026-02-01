using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  AsciiFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Ascii")]
    public sealed class AsciiVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Ascii resolution")]
        public ClampedFloatParameter m_Scale = new ClampedFloatParameter(1, 0, 1);
        
        [Header("Colors")]
        [Tooltip("Color of an ascii signs")]
        public ColorParameter        m_Ascii = new ColorParameter(new Color(1, 1, 1, 0), false);
        [Tooltip("Gradient evaluation curve(maps luminance to sign value)")]
        public CurveParameter        m_Mapping = new CurveParameter(new CurveValue(AnimationCurve.Linear(0, 0, 1, 1)), false);
        [Tooltip("Image color, by default it used as a tint but can be used as a solid overlay")]
        public ColorParameter        m_Image = new ColorParameter(Color.white, false);
        [Tooltip("Solid color (interpolation between tint and solid overlay)")]
        public ClampedFloatParameter m_Solid = new ClampedFloatParameter(0, 0, 1);

        [Header("Ascii")]
        [Tooltip("Signs gradient, can be multidimentional but the cell size must match")]
        public Texture2DParameter          m_Gradient = new Texture2DParameter(null, false);
        [Tooltip("Depth of multi dimensional gradient, can be used for common one for glitches")]
        public NoInterpClampedIntParameter m_Depth    = new NoInterpClampedIntParameter(1, 1, 7);
        [Tooltip("Fps of gradient animation (for animation used screen noise texture with a random offset)")]
        public ClampedIntParameter         m_Fps      = new ClampedIntParameter(0, 0, 120);
        [Tooltip("Screen noise texture scale, (for animation of multi dimensional gradient")]
        public ClampedFloatParameter       m_Noise    = new ClampedFloatParameter(1, 0, 3);
        
        [Header("Palette")]
        [Tooltip("Palette for color replacement")]
        public Texture2DParameter          m_Palette  = new Texture2DParameter(null, false);
        [Tooltip("Impact of the palette to the final image")]
        public ClampedFloatParameter       m_Impact   = new ClampedFloatParameter(0, 0, 1);
        
        // =======================================================================
        public bool IsActive() => active && (m_Scale.value < 1f || m_Impact.value > 0f);

        public bool IsTileCompatible() => false;
    }
}