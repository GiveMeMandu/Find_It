using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Distort")]
    public sealed class DistortVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Effect blend factor")]
        public ClampedFloatParameter m_Weight = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Effect blend factor (fixed override not affected by volume weight - can used for volume animation)")]
        [InspectorName("Override")]
        public NoInterpClampedFloatParameter m_WeightOverride = new NoInterpClampedFloatParameter(0, 0, 1);
        [Tooltip("Distortion strength")]
        public ClampedFloatParameter m_Value = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Tiling factor for the distortion wave")]
        public ClampedFloatParameter m_Tiling = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Direction of wave distortion in degrees")]
        public NoInterpClampedFloatParameter m_Angle = new NoInterpClampedFloatParameter(0, -180, 180);
        [Tooltip("Wave animation phase offset")]
        public ClampedFloatParameter m_Motion = new ClampedFloatParameter(0, 0, 1);
        

        // =======================================================================
        public bool IsActive() => active && m_Value.value > 0;

        public bool IsTileCompatible() => false;
    }
}
