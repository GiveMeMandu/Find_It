using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  FlowFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Flow")]
    public sealed class FlowVol : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter m_Fade     = new ClampedFloatParameter(0, 0, 1, false);
        
        [Tooltip("Weight additional impact")]
        public ClampedFloatParameter m_Strain   = new ClampedFloatParameter(0, 0, 1, false);
        [Tooltip("Samples per render, if value is high value can be achieved immediate effect")]
        public ClampedIntParameter   m_Samples  = new ClampedIntParameter(1, 1, 14, false);
        [Tooltip("Angle offset")]
        public FloatParameter        m_Angle    = new FloatParameter(0, false);
        [Tooltip("Move offset")]
        public Vector3Parameter      m_Flow     = new Vector3Parameter(new Vector3(0, 0, 0), false);
        public ColorParameter        m_Tint     = new ColorParameter(new Color(1, 1, 1, 0), false);
        [Header("Advanced")]
        [Tooltip("Print source over the effect (valuable for selective application with VolFx)")]
        public ClampedFloatParameter m_Print    = new ClampedFloatParameter(0f, 0f, 1f, false);
        [Tooltip("Use adaptive Print to make impact only in high value zones")]
        public ClampedFloatParameter m_Adaptive = new ClampedFloatParameter(0f, 0f, 1f, false);
        [Tooltip("Value of focus effect")]
        public ClampedFloatParameter m_Focus    = new ClampedFloatParameter(0f, 0f, 1f, false);
        [Tooltip("Frame rate update - if not set used default from render feature")]
        public ClampedFloatParameter m_Fps      = new ClampedFloatParameter(60f, 0f, 120f, false);
        [Header("Motion")]
        [Tooltip("Grayscale texture for screen space motion vectors")]
        [InspectorName("Texture")]
        public Texture2DParameter    m_MotionTex   = new Texture2DParameter(null);
        [Tooltip("Offset power applied from motion texture")]
        [InspectorName("Power")]
        public ClampedFloatParameter m_MotionPower = new ClampedFloatParameter(0f, 0f, .3f, false);
        [Tooltip("Animation of motion texture (offset over time)")]
        [InspectorName("Move")]
        public ClampedFloatParameter m_MotionMove = new ClampedFloatParameter(0f, -.3f, .3f, false);
        
        // =======================================================================
        public bool IsActive() => active && (m_Fade.value > 0f || m_Strain.value > 0f || m_Flow.value != Vector3.zero);
        public bool IsTileCompatible() => false;
    }
}