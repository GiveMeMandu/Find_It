using UnityEngine;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Sharpen")]
    public class SharpenPass : VolFx.Pass
    {
        private static readonly int s_Center    = Shader.PropertyToID("_Center");
        private static readonly int s_Side      = Shader.PropertyToID("_Side");
        private static readonly int s_Color     = Shader.PropertyToID("_Color");
        private static readonly int s_Data      = Shader.PropertyToID("_Data");
        private static readonly int s_Thickness = Shader.PropertyToID("_Thickness");
        private static readonly int s_Value     = Shader.PropertyToID("_ValueTex");
		
		public override string ShaderName => string.Empty;

        public                  AnimationCurve _lerp  = AnimationCurve.Linear(0, 0, 1, 1);
        public                  Vector2        _range = new Vector2(0, 3f);
        private                 bool           _isBox;
        private                 Texture2D      _value;

        // =======================================================================
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<SharpenVol>();

            if (settings.IsActive() == false)
                return false;
            
            var steps  = mat.IsKeywordEnabled("BOX") ? 8f : 4f;
            var impact = _range.x + _range.y * _lerp.Evaluate(settings.m_Impact.value);
            mat.SetVector(s_Data, new Vector4(0f, 1f + impact * steps, -impact));
            
            var aspect  = Screen.width / (float)Screen.height;
            var thickness = settings.m_Thikness.overrideState 
                ? new Vector4(settings.m_Thikness.value * aspect * 0.003f, settings.m_Thikness.value * 0.003f) 
                : new Vector4(1f / (float)(Screen.width), 1f / (float)(Screen.height));
            
            thickness.z = settings.m_OffsetX.value * .01f;
            thickness.w = settings.m_OffsetY.value * .01f;
            mat.SetVector(s_Thickness, thickness);
            
            mat.SetTexture(s_Value, settings.m_Value.value.GetTexture(ref _value));
            mat.SetColor(s_Color, settings.m_Tint.value);
            
            return true;
        }
    }
}