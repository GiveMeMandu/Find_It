using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Scanlines")]
    public sealed class ScanlinesVol : VolumeComponent, IPostProcessComponent
    {
        [Header("Scanlines")]
        [Tooltip("Intensity of scanline effect")]
        public ClampedFloatParameter m_Intensity = new ClampedFloatParameter(0, 0, 1.2f);
        [Tooltip("Scanline density along vertical axis")]
        public ClampedFloatParameter m_Count = new ClampedFloatParameter(570, 100, 1000);
        [Tooltip("Vertical scrolling speed of scanlines")]
        public ClampedFloatParameter m_Speed = new ClampedFloatParameter(0, -1, 1);
        [Tooltip("Adds color separation between lines")]
        public ClampedFloatParameter m_Color = new ClampedFloatParameter(1, 0, 1);

        [Header("Screen")]
        [Tooltip("Strength of glowing flicker band")]
        public ClampedFloatParameter m_Flicker = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Vertical height of the flicker band")]
        public ClampedFloatParameter m_Grad = new ClampedFloatParameter(.33f, 0, 1);
        [Tooltip("Enables animation of the flicker")]
        public ClampedFloatParameter m_Animation = new ClampedFloatParameter(1, 0, 1);
        [InspectorName("Speed")]
        [Tooltip("Speed of flicker movement")]
        public ClampedFloatParameter m_GradSpeed = new ClampedFloatParameter(.2f, 0, 1);
        [Tooltip("Vertical shift of the screen image")]
        public ClampedFloatParameter m_Flip = new ClampedFloatParameter(0, 0, 10);
        [InspectorName("Color")]
        [Tooltip("Tint of the flicker glow")]
        public ColorParameter m_GradColor = new ColorParameter(new Color(1, 1, 1, .07f));

        // =======================================================================
        // Can be used to skip rendering if false
        public bool IsActive() => active && (m_Intensity.value > 0 || m_Flip.value > 0 || m_Flicker.value > 0);

        public bool IsTileCompatible() => false;
    }
}
