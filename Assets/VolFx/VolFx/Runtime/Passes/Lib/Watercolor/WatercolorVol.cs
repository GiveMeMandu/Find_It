using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Watercolor")]
    public sealed class WatercolorVol : VolumeComponent, IPostProcessComponent
    {
        [Header("୨ WaterColor ˚‧︵‿୨")]
        [Tooltip("Flow strength")]
        public ClampedFloatParameter m_Strength = new ClampedFloatParameter(0, 0f, 1f);
        
        [Tooltip("Contour sensitivity")]
        public ClampedFloatParameter m_Contour = new ClampedFloatParameter(0, 0, 1);

        [Tooltip("Color mixing, more watercolor approach or solid painting look")]
        [InspectorName("Color Mix")]
        public ClampedFloatParameter m_Saturation = new ClampedFloatParameter(0f, -1, 1);
        
        public ClampedFloatParameter m_ColorNotes = new ClampedFloatParameter(0f, -1f, 1f);
        
        [Tooltip("Splatter texture scale")]
        public NoInterpClampedFloatParameter m_Density = new NoInterpClampedFloatParameter(1f, .007f, 12f);

        [Tooltip("Effect strength mapped by image luminance, can be used to make more clear accent on shadows or lights")]
        public CurveParameter m_Focus = new CurveParameter(new CurveValue(AnimationCurve.Linear(0, 1, 1, 1)), false);
        
        [Header("⊹₊ Animation ₊‧︵‿˚‧")]
        public ClampedIntParameter   m_Fps = new ClampedIntParameter(3, 0, 14);
        [Tooltip("Motion animation of watercolor flow")]
        public ClampedFloatParameter m_Motion = new ClampedFloatParameter(0, -1, 1);
        [Tooltip("Use smooth interpolation")]
        public BoolParameter         m_Smooth = new BoolParameter(true, false);
        [InspectorName("Contour")]
        [Tooltip("Contour deviation range (updated each frame applied randomly on Contour parameter)")]
        public ClampedFloatParameter m_ContourDeviation = new ClampedFloatParameter(0, 0, 1);
        [InspectorName("Strength")]
        [Tooltip("Strength deviation range (updated each frame applied randomly on Strength parameter)")]
        public ClampedFloatParameter m_StrengthDeviation = new ClampedFloatParameter(0, 0, 1);
        [InspectorName("Color Notes")]
        public ClampedFloatParameter m_ColorNotesDeviation = new ClampedFloatParameter(0, 0, 1);
        
        [Header("˚‧ Advanced ‧ ˚ ₊")]
        [Tooltip("Flow texture (directional noise)")]
        public TextureParameter m_Splatters = new TextureParameter(null);
        [Tooltip("Paper mask texture")]
        public TextureParameter m_Paper = new TextureParameter(null);
        
        [Header("˚ Tech ⊹₊")]
        [Tooltip("Contour sensitivity")]
        [InspectorName("Contour sensitivity")]
        public ClampedFloatParameter m_ContourThickness = new ClampedFloatParameter(.5f, 0f, 1f);
        
        [Tooltip("Paint blending strength")]
        public ClampedFloatParameter m_Blending = new ClampedFloatParameter(.1f, 0f, 1f);
        
        [Tooltip("Frames interpolation speed(for smooth mode)")]
        public ClampedFloatParameter m_FramesLerp = new ClampedFloatParameter(3f, 0f, 7f);
        

        // =======================================================================
        public bool IsActive() => active && (m_Strength.value > 0 || m_Contour.value > 0);

        public bool IsTileCompatible() => false;
    }
}