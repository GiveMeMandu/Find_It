using System;
using UnityEngine.Rendering;

namespace VolFx
{
    [Serializable]
    public class DitherGradientParameter : VolumeParameter<DitherGradientValue>
    {
        public DitherGradientParameter(DitherGradientValue value, bool overrideState) : base(value, overrideState) { }

        public override void Interp(DitherGradientValue from, DitherGradientValue to, float t)
        {
            m_Value.Blend(from, to, t);
        }

        public override void SetValue(VolumeParameter parameter)
        {
            m_Value.SetValue(((VolumeParameter<DitherGradientValue>)parameter).value);
        }
    }
}