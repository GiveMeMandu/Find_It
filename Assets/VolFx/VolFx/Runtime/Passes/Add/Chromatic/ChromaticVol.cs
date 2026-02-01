//  VolFx © NullTale - https://x.com/NullTale

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Chromatic")]
    public sealed class ChromaticVol : VolumeComponent, IPostProcessComponent
    {
        // Strength of the effect blending. 0 = disabled, 1 = full effect.
        public ClampedFloatParameter _Weight = new ClampedFloatParameter(1f, 0f, 1f);
        [Tooltip("Chromatic aberration intensity. Positive values push channels outward, negative inward.")]
        public ClampedFloatParameter _Intensity = new ClampedFloatParameter(0f, -1f, 1f);
        [Tooltip("Radial distortion strength. Alters intensity based on distance from screen center.")]
        public ClampedFloatParameter _Radial = new ClampedFloatParameter(0f, -1f, 1f);
        [Tooltip("Alpha blending multiplier for the chromatic effect.")]
        public ClampedFloatParameter _Alpha = new ClampedFloatParameter(0f, -1f, 1f);
        [Tooltip("Rotates chromatic shift directions. Useful for artistic distortion.")]
        public ClampedFloatParameter _Angle = new ClampedFloatParameter(0f, -1f, 1f);
        [Tooltip("Controls RGB split strength. Acts as a global multiplier for color displacement.")]
        public NoInterpClampedFloatParameter _Split = new NoInterpClampedFloatParameter(0f, -1f, 1f);
        [Tooltip("Saturation multiplier applied after shift. 1 = original saturation, 0 = grayscale.")]
        public ClampedFloatParameter _Sat = new ClampedFloatParameter(1f, 0f, 1f);
        [Tooltip("Convert result to monochrome after applying chromatic shift.")]
        public BoolParameter _Mono = new BoolParameter(false, false);

        // =======================================================================
        public bool IsActive() => active && (_Intensity.value != 0f || _Radial.value != 0f && _Weight.value > 0f);

        public bool IsTileCompatible() => true;
    }
}
