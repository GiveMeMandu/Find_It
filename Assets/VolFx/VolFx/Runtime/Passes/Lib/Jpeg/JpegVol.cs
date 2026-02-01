using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  Jpeg © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Jpeg")]
    public sealed class JpegVol : VolumeComponent, IPostProcessComponent
    {
        [Header("===== Jpeg =====")]
        //[Header("===== ⊹₊ ˚‧︵‿₊୨")]
        //[Header("⊹₊ ˚‧︵‿₊୨ ")]
        public ClampedFloatParameter _intensity        = new ClampedFloatParameter(0f, -5f, 5f);
        public ClampedFloatParameter _blockSize        = new ClampedFloatParameter(7f, 0.1f, 200f);
        public ClampedFloatParameter _quantization     = new ClampedFloatParameter(15f, 2f, 32f);
        
        
        [Header("===== Distortions =====")]
        //[InspectorName("Scale")]
        public ClampedFloatParameter _distortionScale  = new ClampedFloatParameter(0f, 0f, 7f);
        
        [Header("YCbCr")]
        public ClampedFloatParameter _applyToY        = new ClampedFloatParameter(0f, 0f, 20f);
        public ClampedFloatParameter _applyToChroma   = new ClampedFloatParameter(0f, 0f, 20f);
        public ClampedFloatParameter _applyToGlitch   = new ClampedFloatParameter(0f, 0f, 20f);
        
        [Header("Animation")]
        public NoInterpClampedFloatParameter _fps  = new NoInterpClampedFloatParameter(0f, 0f, 60f);
        public ClampedFloatParameter  _fpsBreak    = new ClampedFloatParameter(0f, 0f, 1f);
        public NoInterpCurveParameter _fpsLag      = new NoInterpCurveParameter(AnimationCurve.Linear(0, 1, 1, 1));
        public TextureParameter       _fpsBlending = new TextureParameter(null, false);
        public ClampedFloatParameter  _quantSpread = new ClampedFloatParameter(0f, 0f, 3f);
        
        [Header("===== Additional =====")]
        [Header("Scanlines")]
        public ClampedFloatParameter         _scanlineDrift   = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedFloatParameter         _scanlineRes     = new ClampedFloatParameter(720f, 120f, 720f);
        public NoInterpClampedFloatParameter _scanlinesFps = new NoInterpClampedFloatParameter(120f, 1f, 120f);
        
        [Header("Channel Shift")]
        [InspectorName("Pow")]
        public ClampedFloatParameter _channelShiftPow    = new ClampedFloatParameter(0f, -10f, 10f);
        [InspectorName("Shift X")]
        public ClampedFloatParameter _channelShiftX      = new ClampedFloatParameter(0f, -1f, 1f);
        [InspectorName("Shift Y")]
        public ClampedFloatParameter _channelShiftY      = new ClampedFloatParameter(0f, -1f, 1f);
        [InspectorName("Spread")]
        public ClampedFloatParameter _channelShiftSpread = new ClampedFloatParameter(0f, 0f, 1f);
        
        [Header("Noise")]
        public ClampedFloatParameter _noise         = new ClampedFloatParameter(0f, -1.37f, 1.75f);
        public BoolParameter         _noiseBilinear = new BoolParameter(false);
        [Header("Custom Distortion Texture")]
        public TextureParameter      _distortionTex = new TextureParameter(null, false);
        
        // =======================================================================
        public bool IsActive() => active && (_intensity != 0f || _scanlineDrift.value > 0f || _channelShiftPow.value > 0f || _fpsLag.overrideState);

        public bool IsTileCompatible() => true;
    }
}