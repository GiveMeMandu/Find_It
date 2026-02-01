using UnityEngine;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Invert")]
    public class InvertPass : VolFx.Pass
    {
        private static readonly int s_Weight   = Shader.PropertyToID("_Weight");
        private static readonly int s_ValueTex = Shader.PropertyToID("_ValueTex");
		
		public override string ShaderName => string.Empty;
        
        private Texture2D _adaptiveTex;

        // =======================================================================
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<InvertVol>();

            if (settings.IsActive() == false)
                return false;
            
            mat.SetVector(s_Weight, new Vector4(settings.m_Weight.value, 0f));
            mat.SetTexture(s_ValueTex, settings.m_Value.value.GetTexture(ref _adaptiveTex));
            
            return true;
        }
    }
}