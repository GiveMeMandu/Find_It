using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  ColorMap © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Color Map")]
    public sealed class ColorMapVol : VolumeComponent, IPostProcessComponent
    {
        public static GradientValue Default 
        {
            get
            {
                var grad = new Gradient();
                grad.SetKeys(new []{new GradientColorKey(Color.black, 0f), new GradientColorKey(Color.white, 1f)}, new GradientAlphaKey[]{new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0f, 0f)});
                
                return new GradientValue(grad);
            }
        }

        public ClampedFloatParameter m_Weight    = new ClampedFloatParameter(0, 0, 1f);
        [Header("Gradient")]
        public GradientParameter     m_Gradient  = new GradientParameter(Default, false);
        public ClampedFloatParameter m_Offset    = new ClampedFloatParameter(0, 0, 1f);
        public ClampedFloatParameter m_Motion    = new ClampedFloatParameter(0, -1f, 1f);
        public ClampedFloatParameter m_Stretch   = new ClampedFloatParameter(0, -1, 1f);
        public FloatRangeParameter   m_Mask      = new FloatRangeParameter(new Vector2(0, 1), 0, 1);
        
        [Header("Palette")]
        public Texture2DParameter    m_Palette = new Texture2DParameter(null, false);
        public CurveParameter        m_Impact  = new CurveParameter(new CurveValue(AnimationCurve.Linear(0, 0, 1, 0)), false);

        // =======================================================================
        public bool IsActive() => active && m_Weight.value > 0;

        public bool IsTileCompatible() => false;
    }
}