using UnityEngine;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Slice")]
    public class SlicePass : VolFx.Pass
    {
        private static readonly int   s_Settings = Shader.PropertyToID("_Settings");
		
		public override string ShaderName => string.Empty;
        
        // =======================================================================
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<SliceVol>();

            if (settings.IsActive() == false)
                return false;

            var sharpness = settings.m_Value.value * 0.1f;
            var tiling    = settings.m_Tiling.value;
            var angle     = settings.m_Angle.value * Mathf.Deg2Rad;
            
            mat.SetVector(s_Settings, new Vector4(sharpness, tiling, 0, angle));
            
            return true;
        }
    }
}