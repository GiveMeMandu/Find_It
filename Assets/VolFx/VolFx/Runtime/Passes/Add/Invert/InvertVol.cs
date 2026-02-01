using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Invert")]
    public sealed class InvertVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Intensity of the invert effect (0 = no effect, 1 = full invert)")]
        public ClampedFloatParameter m_Weight = new ClampedFloatParameter(0, 0, 1);
        
        [Tooltip("Curve that defines how the invert effect is applied based on brightness (luma).\n" +
         "X-axis: pixel brightness (0 = black, 1 = white)\n" +
         "Y-axis: inversion amount (0 = no effect, 0.5 = full invert, 1 = over-invert)\n\n" +
         "Use this to adaptively control which brightness ranges are affected.")]
        public CurveParameter        m_Value  = new CurveParameter(new CurveValue(new AnimationCurve(
                                                                                      new Keyframe[]{new Keyframe(0, .5f), new Keyframe(1, .5f)})), false);
        
        // =======================================================================
        // Can be used to skip rendering if false
        public bool IsActive() => active && m_Weight.value > 0;

        public bool IsTileCompatible() => false;
    }
}