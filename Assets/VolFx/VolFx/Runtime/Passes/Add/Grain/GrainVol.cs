using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Grain")]
    public sealed class GrainVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Base grain strength Controls noise visibility")]
        public ClampedFloatParameter m_Grain = new ClampedFloatParameter(0f, 0f, 3f);
        [Tooltip("Response curve used to apply grain based on luminance")]
        public CurveParameter m_Response = new CurveParameter(
            new CurveValue(new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(1, 0, 0, 0))),
            false);

        [Tooltip("Brightness adjustment after grain applied")]
        public ClampedFloatParameter m_Brightness = new ClampedFloatParameter(0, -1, 1);
        [Tooltip("Hue shift in radians applied after grain")]
        public ClampedFloatParameter m_Hue = new ClampedFloatParameter(0, -1, 1);
        [Tooltip("Saturation boost applied after grain.")]
        public ClampedFloatParameter m_Saturation = new ClampedFloatParameter(0, -1, 1);
        [Tooltip("Grain texture to use")]
        
        public GrainTexParameter m_GainTex = new GrainTexParameter(GrainPass.GrainTex.Grain_Thin_A, false);
        [Tooltip("Alpha fade of grain effect")]
        public ClampedFloatParameter m_Alpha = new ClampedFloatParameter(1, 0, 1);

        //[Tooltip("Contrast adjustment (not used).")]
        //public ClampedFloatParameter m_Contrast = new ClampedFloatParameter(0, -1, 1);
        [Tooltip("Final color tint applied after processing")]
        public ColorParameter m_Color = new ColorParameter(new Color(1, 1, 1, 0));
        [Tooltip("Grain animation framerate. 0 = static")]
        public ClampedIntParameter m_Fps = new ClampedIntParameter(60, 0, 60);
        [Tooltip("Grain texture UV scaling. Negative values zoom in")]
        public ClampedFloatParameter m_Scale = new ClampedFloatParameter(0, -1, 1);
        [Tooltip("Custom grain texture (overrides preset if assigned)")]
        public Texture2DParameter m_Texture = new Texture2DParameter(null);

        // =======================================================================
        [Serializable]
        public class GrainTexParameter : VolumeParameter<GrainPass.GrainTex>
        {
            public GrainTexParameter(GrainPass.GrainTex value, bool overrideState) : base(value, overrideState) { }
        }

        // =======================================================================
        public bool IsActive() => m_Grain.value > 0f;

        public bool IsTileCompatible() => true;
    }
}