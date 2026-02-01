using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VhsFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Vhs")]
    public sealed class VhsVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Total blending of full applied effect")]
        public ClampedFloatParameter _weight     = new ClampedFloatParameter(0f, 0f, 1f);
        [Tooltip("Tape noises impact")]
        public ClampedFloatParameter _tape       = new ClampedFloatParameter(0f, 0f, 2f);
        [Tooltip("Tape noises impact")]
        public ClampedFloatParameter _shades     = new ClampedFloatParameter(0f, 0f, 3f);

        [Header("Distort")]
        [Tooltip("Frame distortions")]
        public ClampedFloatParameter _rocking   = new ClampedFloatParameter(0f, 0f, 0.1f);
        [Tooltip("Tape squeeze distortions")]
        public ClampedFloatParameter _squeeze   = new ClampedFloatParameter(0f, 0f, 1f);
        
        [Header("Noise")]
        [Tooltip("White noise density")]
        public ClampedFloatParameter _density    = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("White noise intensity")]
        public ClampedFloatParameter _intensity  = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("White noise scale")]
        public ClampedFloatParameter _scale      = new ClampedFloatParameter(1, .3f, 3f);
        [Tooltip("Grain flickering")]
        public ClampedFloatParameter _flickering = new ClampedFloatParameter(0f, -1f, 1f);
        [Tooltip("Line distortion")]
        public BoolParameter         _lines = new BoolParameter(true, false);
        
        [Header("Glow")]
        [Tooltip("Crt glow color")]
        public ColorParameter        _color      = new ColorParameter(new Color(1f, 0f, 0f, 1));
        [Tooltip("Crt glow offset")]
        public ClampedFloatParameter _bleed      = new ClampedFloatParameter(.7f, 0f, 3f);
        
        //[HideInInspector]
        [Header("Anim")]
        [Tooltip("Speed of flow animations")]
        public ClampedFloatParameter _flow  = new ClampedFloatParameter(1f, 0, 24f);
        [Tooltip("Speed of pulsating animations")]
        public ClampedFloatParameter _pulsation  = new ClampedFloatParameter(1f, 0f, 14f);
        
        // =======================================================================
        [Serializable]
        public class ModeParameter : VolumeParameter<VhsPass.Mode>
        {
            public ModeParameter(VhsPass.Mode value, bool overrideState) : base(value, overrideState) { }
        } 
        // =======================================================================
        public bool IsActive() => active && _weight.value > 0f;

        public bool IsTileCompatible() => true;
    }
}