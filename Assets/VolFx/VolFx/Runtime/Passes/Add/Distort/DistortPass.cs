using UnityEngine;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Distort")]
    public class DistortPass : VolFx.Pass
    {
        private static readonly int   s_Settings = Shader.PropertyToID("_Settings");
        private static readonly int   s_Weight = Shader.PropertyToID("_Weight");
        
		public override string ShaderName => string.Empty;
        
        private                 float _offset;
        public float            _motionMul = 1f;

        // =======================================================================
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<DistortVol>();

            if (settings.IsActive() == false)
                return false;

            var sharpness = settings.m_Value.value * .3f;
            var tiling    = settings.m_Tiling.value * 200f;
            var angle     = (settings.m_Angle.value + 90f) * Mathf.Deg2Rad;
            _offset += settings.m_Motion.value * settings.m_Tiling.value * Time.deltaTime * 70f * _motionMul;
            
            mat.SetVector(s_Settings, new Vector4(sharpness, tiling, _offset, angle));
            mat.SetFloat(s_Weight, Mathf.Max(settings.m_Weight.value, settings.m_WeightOverride.value));
            
            return true;
        }
    }
}