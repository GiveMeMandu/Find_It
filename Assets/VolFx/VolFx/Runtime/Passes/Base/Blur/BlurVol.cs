using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Blur")]
    public sealed class BlurVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Base blur strength. Higher value increases overall blur")]
        public ClampedFloatParameter m_Radius = new ClampedFloatParameter(0, 0, 1);

        [Tooltip("Adds radial blur from center")]
        public ClampedFloatParameter m_Radial = new ClampedFloatParameter(0, 0, 1);

        [Tooltip("Number of blur samples")]
        public NoInterpClampedIntParameter m_Samples = new NoInterpClampedIntParameter(9, 3, 18);

        [Tooltip("Aspect ratio control. Negative = horizontal bias, Positive = vertical bias.")]
        public ClampedFloatParameter m_Aspect = new ClampedFloatParameter(0, -1, 1);

        [Tooltip("Rotation of blur direction in degrees. Affects orientation of the blur.")]
        public NoInterpClampedFloatParameter m_Angle = new NoInterpClampedFloatParameter(0, -360f, 360f);

        [Tooltip("Controls adaptive blur strength based on image luminance.")]
        public CurveParameter m_Adaptive = new CurveParameter(new CurveValue(new AnimationCurve(new []{ new Keyframe(0, 1), new Keyframe(1, 1) })), false);

        // =======================================================================
        // Can be used to skip rendering if false
        public bool IsActive() => active && (m_Radius.value > 0 || m_Radial.value > 0);

        public bool IsTileCompatible() => false;
    }
}