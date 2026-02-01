using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Sharpen")]
    public sealed class SharpenVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Strength of the sharpening effect")]
        public ClampedFloatParameter         m_Impact   = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Thickness of sharpening edges")]
        public NoInterpClampedFloatParameter m_Thikness = new NoInterpClampedFloatParameter(0, 0, 1);
        [Tooltip("Curve controlling sharpening intensity over input")]
        public CurveParameter                m_Value    = new CurveParameter(new CurveValue(AnimationCurve.Linear(0, 1, 1, 1)), false);
        [Tooltip("Color tint applied to sharpening")]
        public ColorParameter                m_Tint     = new ColorParameter(Color.white);
        [Tooltip("Horizontal offset for sharpening sample")]
        public ClampedFloatParameter         m_OffsetX  = new ClampedFloatParameter(0, -1, 1);
        [Tooltip("Vertical offset for sharpening sample")]
        public ClampedFloatParameter         m_OffsetY  = new ClampedFloatParameter(0, -1, 1);
        
        // =======================================================================
        // Can be used to skip rendering if false
        public bool IsActive() => active && m_Impact.value > 0;

        public bool IsTileCompatible() => false;
    }
}
