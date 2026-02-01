using UnityEngine;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Posterize")]
    public class PosterizePass : VolFx.Pass
    {
        private static readonly int s_Count = Shader.PropertyToID("_Count");
		
		public override string ShaderName => string.Empty;

        // =======================================================================
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<PosterizeVol>();

            if (settings.IsActive() == false)
                return false;

            mat.SetFloat(s_Count, settings.m_Count.value);
            return true;
        }
    }
}