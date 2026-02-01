using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  GlitchFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Glitch")]
    public sealed class GlitchVol : VolumeComponent, IPostProcessComponent
    {
        public static GradientValue Wave 
        {
            get
            {
                var grad = new Gradient();
                grad.SetKeys(new []{new GradientColorKey(new Color(0.773f, 0.693f, 1f), 0f), new GradientColorKey(new Color(0f, 0.98f, 1f), 1f)}, new GradientAlphaKey[]{new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0f)});
                
                return new GradientValue(grad);
            }
        }
        
        public static GradientValue Warm 
        {
            get
            {
                var grad = new Gradient();
                grad.SetKeys(new []{new GradientColorKey(new Color(0.37f, 0.29f, 0.25f), 0f), new GradientColorKey(new Color(1f, .94f, .5f), 1f)}, new GradientAlphaKey[]{new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0f, 0f)});
                
                return new GradientValue(grad);
            }
        }
        
        public static GradientValue Clear 
        {
            get
            {
                var grad = new Gradient();
                grad.SetKeys(new []{new GradientColorKey(new Color(.0f, .0f, .0f), 0f), new GradientColorKey(new Color(.0f, .0f, .0f), 1f)}, new GradientAlphaKey[]{new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0f, 0f)});
                
                return new GradientValue(grad);
            }
        }
        
        public ClampedFloatParameter _weight     = new ClampedFloatParameter(0f, 0f, 1f);
        [Header("Noise")]
        public ClampedFloatParameter _power      = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter _scale      = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter _dispersion = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter _period     = new ClampedFloatParameter(0f, 0f, 1f);
        public GradientParameter     _color      = new GradientParameter(Wave, false);
        public ClampedFloatParameter _chaotic    = new ClampedFloatParameter(0f, 0f, 1f);
        [InspectorName("No Lock")]
        public BoolParameter         _noLockNoise  = new BoolParameter(false);
        
        [Header("Mosaic")]
        public ClampedFloatParameter _density = new ClampedFloatParameter(0f, 0f, 1f);
        [InspectorName("Period")]
        public ClampedFloatParameter _lock    = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter _sharpen = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter _crush   = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter _grid    = new ClampedFloatParameter(0f, 0f, 1f);
        public GradientParameter     _bleed   = new GradientParameter(Warm, false);
        public ClampedFloatParameter _screen  = new ClampedFloatParameter(0f, 0f, 1f);
        public BoolParameter         _noLock  = new BoolParameter(false);
        
        // =======================================================================
        public bool IsActive() => active && _weight.value > 0f;

        public bool IsTileCompatible() => true;
    }
}