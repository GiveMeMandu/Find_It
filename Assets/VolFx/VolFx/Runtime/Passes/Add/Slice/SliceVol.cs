using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Slice")]
    public sealed class SliceVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Strength of slice offset")]
        public ClampedFloatParameter m_Value = new ClampedFloatParameter(0, 0, 3);
        [Tooltip("Number of slices per unit")]
        public NoInterpClampedFloatParameter m_Tiling = new NoInterpClampedFloatParameter(500, 0, 700);
        [Tooltip("Slice angle in degrees")]
        public NoInterpClampedFloatParameter m_Angle = new NoInterpClampedFloatParameter(0, -180, 180);

        // =======================================================================
        public bool IsActive() => active && m_Value.value > 0;

        public bool IsTileCompatible() => false;
    }
}
