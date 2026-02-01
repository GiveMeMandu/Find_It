using UnityEngine;

namespace VolFx
{
    public class GrayscalePass : VolFx.Pass
    {
        // shader name for auto created material in Validate function
        public override string ShaderName => "Hidden/VolFx/Grayscale";

        // shader assignment before draw call
        public override bool Validate(Material mat)
        {
            // use stack from feature settings
            var settings = Stack.GetComponent<GrayscaleVol>();
            
            // return false if we don't want to execute pass
            if (settings.IsActive() == false)
                return false;
            
            // setup material before drawing
            mat.SetFloat("_Weight", settings.m_Weight.value);
            return true;
        }
    }
}