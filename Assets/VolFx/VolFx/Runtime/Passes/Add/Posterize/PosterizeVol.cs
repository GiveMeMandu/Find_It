using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Posterize")]
    public sealed class PosterizeVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Number of brightness levels from 1 to 64")]
        public ClampedIntParameter m_Count = new ClampedIntParameter(64, 1, 64);

        // =======================================================================
        public bool IsActive() => active && m_Count.overrideState;

        public bool IsTileCompatible() => false;
    }
}