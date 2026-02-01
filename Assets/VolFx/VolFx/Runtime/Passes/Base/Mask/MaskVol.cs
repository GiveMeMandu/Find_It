using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VolFx.Tools;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Mask")]
    public sealed class MaskVol : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Mask weight")]
        public ClampedFloatParameter m_Weight = new ClampedFloatParameter(1, 0, 1);
        [Tooltip("Application mode")]
        public ModeParameter         m_Mode = new ModeParameter(MaskPass.Mode.Alpha, false);
        [Tooltip("Inverse mask")]
        public BoolParameter         m_Inverse = new BoolParameter(false, false);
        [Tooltip("Mask texture source (to create a pool add VolFxPool RenderFeature to the renderer asset)")]
        public BufferParameter       m_Mask   = new BufferParameter();

        // =======================================================================
        [Serializable]
        public class BufferParameter : VolumeParameter<Pool> { }
        
        [Serializable]
        public class ModeParameter : VolumeParameter<MaskPass.Mode>
        {
            public ModeParameter(MaskPass.Mode value, bool overrideState) : base(value, overrideState) { }
        }
        
        // =======================================================================
        // Can be used to skip rendering if false
        public bool IsActive() => active && (m_Weight.overrideState || m_Mask.overrideState);

        public bool IsTileCompatible() => false;
    }
}