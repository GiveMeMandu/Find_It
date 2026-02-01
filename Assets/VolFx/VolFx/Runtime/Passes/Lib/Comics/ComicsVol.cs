using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Comics")]
    public sealed class ComicsVol : VolumeComponent, IPostProcessComponent
    {
        public static DitherGradientValue Default 
        {
            get
            {
                var grad = new Gradient();
                grad.SetKeys(new []{new GradientColorKey(Color.black, .3f), new GradientColorKey(Color.gray, .7f), new GradientColorKey(Color.white, 1f)}, new GradientAlphaKey[]{new GradientAlphaKey(1f, 0f)});
                grad.mode = GradientMode.Fixed;
                
                return new DitherGradientValue(grad);
            }
        }
        
        [Header("Colors")]
        [InspectorName("Weight")]
        public ClampedFloatParameter   m_ColorWeight = new ClampedFloatParameter(0f, 0f, 1f);
        public DitherGradientParameter m_Colors      = new DitherGradientParameter(Default, false);

        [Header("Dithering")]
        public CurveParameter    m_Measure = new CurveParameter(new CurveValue(AnimationCurve.Linear(0, 0, 1, 0)), false);
        [InspectorName("Weight")]
        public NoInterpClampedFloatParameter m_DitherWeight = new NoInterpClampedFloatParameter(1f,-1f, 1f);
        [InspectorName("Power")]
        public ClampedFloatParameter m_DitherPower  = new ClampedFloatParameter(.26f, 0f, 1f);
        [InspectorName("Scale")]
        public NoInterpClampedFloatParameter m_DitherScale  = new NoInterpClampedFloatParameter(5f, 0f, 14f);
        [InspectorName("Speed")]
        public NoInterpClampedFloatParameter m_PatternSpeed = new NoInterpClampedFloatParameter(0f, 0f, 3f);
        
        [Header("Line")]
        [InspectorName("Measure")]
        public CurveParameter         m_OutlineWeight      = new CurveParameter(new CurveValue(AnimationCurve.Linear(0, 0, 1, 0)), false);
        public ClampedFloatParameter  m_Thickness = new ClampedFloatParameter(.5f, 0f, 1f);
        public ClampedFloatParameter  m_Sharpness = new ClampedFloatParameter(.5f, 0f, 1f);
        public OutlineModeParameter   m_Mode      = new OutlineModeParameter(ComicsPass.OutlineMode.Black, false);
        [InspectorName("Color")]
        public NoInterpColorParameter m_OulineColor = new NoInterpColorParameter(Color.magenta, false);

        [Header("Advanced")]
        [InspectorName("Fancy")]
        [Tooltip("Hue shifting in dither transitions \nNOTE: dither can be disabled and fancy effect can be used without it")]
        public ClampedFloatParameter m_FancyWeight = new ClampedFloatParameter(0f, 0f, 1f);
        public TextureParameter m_DitherTex = new TextureParameter(null);
        [InspectorName("Shade")]
        [HideInInspector]
        public ClampedFloatParameter m_ShadeWeight = new ClampedFloatParameter(1f, 0f, 1f);
        
        [Tooltip("Use depth(z) buffer for outline calculation " +
                 "\nNOTE: Depth texture must be enabled in Urp asset settings" +
                 "\nNOTE: Useful for 3D only scenes to avoid inner outline cluttering")]
        public BoolParameter    m_Depth = new BoolParameter(false, false);

        // =======================================================================
        [Serializable]
        public class OutlineModeParameter : VolumeParameter<ComicsPass.OutlineMode>
        {
            public OutlineModeParameter(ComicsPass.OutlineMode value, bool overrideState) : base(value, overrideState) { }
        }
        
        // =======================================================================
        public bool IsActive() => active && (m_ColorWeight.value > 0 
                                             || m_Thickness.value > .5f
                                             || m_Thickness.value < .5f
                                             || m_FancyWeight.value > 0 
                                             || m_ShadeWeight.value < 1f);

        public bool IsTileCompatible() => false;
    }
}