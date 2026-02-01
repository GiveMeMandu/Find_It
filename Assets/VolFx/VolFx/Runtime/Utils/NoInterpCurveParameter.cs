using System;
using UnityEngine;
using UnityEngine.Rendering;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [Serializable]
    public class NoInterpCurveParameter : VolumeParameter<AnimationCurveRange>
    {
        public NoInterpCurveParameter(AnimationCurve value, bool overrideState = false)
            : base(new AnimationCurveRange() { _curve = value }, overrideState) { }
    }
    
    [Serializable]
    public class AnimationCurveRange
    {
        public AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve Value => _curve;
        
        public float Evaluate(float time) => _curve.Evaluate(time);
        
        public static implicit operator AnimationCurve(AnimationCurveRange curve) => curve._curve;
    }
}