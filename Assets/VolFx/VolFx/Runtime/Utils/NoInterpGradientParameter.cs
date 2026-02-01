using System;
using UnityEngine.Rendering;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable]
    public class NoInterpGradientParameter : VolumeParameter<GradientValue> 
    {
        public NoInterpGradientParameter(GradientValue value, bool overrideState) : base(value, overrideState) { }

        public override void Interp(GradientValue from, GradientValue to, float t)
        {
            m_Value.Blend(from, to, t > 0f ? 1f : 0f);
        }
        
        public override void SetValue(VolumeParameter parameter)
        {
            m_Value.SetValue(((VolumeParameter<GradientValue>)parameter).value);
        }
    }
}