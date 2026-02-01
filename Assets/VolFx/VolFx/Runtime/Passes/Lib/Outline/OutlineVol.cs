using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

//  OutlineFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Outline")]
    public sealed class OutlineVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Outline strength")]
        public ClampedFloatParameter m_Sensitive = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Outline thickness")]
        public ClampedFloatParameter m_Thickness = new ClampedFloatParameter(0.15f, 0, 1);
        [Tooltip("Outline color defined by gradient")][FormerlySerializedAs("m_Outline")]
        public GradientParameter     m_Color   = new GradientParameter(OutlineDefault, false);
        [Tooltip("Solid Color overlay, can be used for stylization")]
        public ColorParameter        m_Fill      = new ColorParameter(Color.clear, false);
        
        [Header("Advanced")]
        [Tooltip("Outline mode detection")]
        public ModeParameter         m_Mode      = new ModeParameter(OutlinePass.Mode.Luma, false);
        [Tooltip("Clamped Sharp outline without soft edges")]
        public BoolParameter         m_Sharp     = new BoolParameter(false, false);
        [Tooltip("Adaptive application with dynamic thickness based on contrast source (luma, chroma, depth)")]
        public BoolParameter         m_Adaptive  = new BoolParameter(false, false);
        [Tooltip("Gradient colorization method")]
        public RemapParameter        m_Remap     = new RemapParameter(OutlinePass.Remap.Screen, false);

        public static GradientValue OutlineDefault
        {
            get
            {
                var grad = new Gradient();
                grad.SetKeys(new []{new GradientColorKey(Color.black, 0f), new GradientColorKey(Color.black, 1f)}, new GradientAlphaKey[]{new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0f, 0f)});
                
                return new GradientValue(grad);
            }
        }
        
        [Serializable]
        public class ModeParameter : VolumeParameter<OutlinePass.Mode>
        {
            public ModeParameter(OutlinePass.Mode value, bool overrideState) : base(value, overrideState) { }
        } 
        
        [Serializable]
        public class RemapParameter : VolumeParameter<OutlinePass.Remap>
        {
            public RemapParameter(OutlinePass.Remap value, bool overrideState) : base(value, overrideState) { }
        } 
        
        // =======================================================================
        public bool IsActive() => active && ((m_Thickness.value > 0f && m_Sensitive.value > 0f) || m_Fill.overrideState);

        public bool IsTileCompatible() => false;
    }
}