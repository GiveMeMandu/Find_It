using System;
using System.Linq;
using UnityEngine;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Comics")]
    public class ComicsPass : VolFx.Pass
    {
        private static readonly int s_DataA        = Shader.PropertyToID("_DataA");
        private static readonly int s_DataB        = Shader.PropertyToID("_DataB");
        private static readonly int s_DataC        = Shader.PropertyToID("_DataC");
        private static readonly int s_DitherTex    = Shader.PropertyToID("_DitherTex");
        private static readonly int s_PaperTex     = Shader.PropertyToID("_PaperTex");
        private static readonly int s_ShadesTex    = Shader.PropertyToID("_ShadesTex");
        private static readonly int s_ColorsTex    = Shader.PropertyToID("_ColorsTex");
        private static readonly int s_MeasureTex   = Shader.PropertyToID("_MeasureTex");
        private static readonly int s_OutlineTex   = Shader.PropertyToID("_OutlineTex");
        private static readonly int s_OutlineColor = Shader.PropertyToID("_OutlineColor");
        
        public override string ShaderName => string.Empty;
        
        private Vector2 _ditherOffset;
        
        [Tooltip("Default dither texture")]
        public  Texture2D _ditherTex;
        
        private Texture2D   _valueTex;
        private Texture2D   _shadesTex;
        private Texture2D   _colorsTex;
        private Texture2D   _measure;
        private Texture2D   _gradient;
        private Texture2D   _outline;
        private OutlineMode _mode;
        private bool        _sampleDepth;
        
        // =======================================================================
        public enum OutlineMode
        {
            Black,
            White,
            Double,
            Custom
        }

        // =======================================================================
        public override void Init()
        {
            _ditherOffset = Vector2.zero;
            _mode = OutlineMode.Black;
            _sampleDepth = false;
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<ComicsVol>();

            if (settings.IsActive() == false)
                return false;
            
            var aspect = Screen.width / (float)Screen.height;
            
            var time   = Time.time;
            var speed = settings.m_PatternSpeed.value;
            _ditherOffset = new Vector2(time * speed, time * speed);
            
            var dataA = new Vector4(
                settings.m_DitherScale.value * aspect,
                settings.m_DitherScale.value,
                _ditherOffset.x * aspect,
                _ditherOffset.y
            );

            var dataB = new Vector4(
                settings.m_Thickness.value * 0.01f,
                Mathf.Lerp(7f, .7f, settings.m_DitherPower.value),
                Mathf.Lerp(2f, .3f, settings.m_Sharpness.value),
                settings.m_DitherWeight.value
            );

            var dataC = new Vector4(
                settings.m_ColorWeight.value,
                0,
                Mathf.LerpUnclamped(0, Mathf.PI * 2f, settings.m_FancyWeight.value),
                Mathf.Lerp(0, .375f, settings.m_ShadeWeight.value)
            );
            
            if (_mode != settings.m_Mode.value)
            {
                mat.DisableKeyword("OUTLINE_BLACK");
                mat.DisableKeyword("OUTLINE_WHITE");
                mat.DisableKeyword("OUTLINE_DOUBLE");
                mat.DisableKeyword("OUTLINE_CUSTOM");
                
                switch (settings.m_Mode.value)
                {
                    case OutlineMode.Black:
                        mat.EnableKeyword("OUTLINE_BLACK");
                        break;
                    case OutlineMode.White:
                        mat.EnableKeyword("OUTLINE_WHITE");
                        break;
                    case OutlineMode.Double:
                        mat.EnableKeyword("OUTLINE_DOUBLE");
                        break;
                    case OutlineMode.Custom:
                        mat.EnableKeyword("OUTLINE_CUSTOM");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _mode = settings.m_Mode.value;
            }
            
            if (_sampleDepth != settings.m_Depth.value)
            {
                _sampleDepth = settings.m_Depth.value;
                if (_sampleDepth)
                    mat.EnableKeyword("SAMPLE_DEPTH");
                else
                    mat.DisableKeyword("SAMPLE_DEPTH");
            }
            
            settings.m_Colors.value.GetColorsTex(ref _colorsTex);
            settings.m_Colors.value.GetDitherTex(ref _shadesTex);
            
            mat.SetVector(s_DataA, dataA);
            mat.SetVector(s_DataB, dataB);
            mat.SetVector(s_DataC, dataC);
            
            mat.SetColor(s_OutlineColor, settings.m_OulineColor.value);
            
            mat.SetTexture(s_DitherTex, (settings.m_DitherTex.overrideState && settings.m_DitherTex.value != null) ? settings.m_DitherTex.value : _ditherTex);
            
            mat.SetTexture(s_ShadesTex, _shadesTex);
            mat.SetTexture(s_ColorsTex, _colorsTex);
            
            mat.SetTexture(s_MeasureTex, settings.m_Measure.value.GetTexture(ref _measure));
            mat.SetTexture(s_OutlineTex, settings.m_OutlineWeight.value.GetTexture(ref _outline));
            
            return true;
        }

        protected override bool _editorValidate => _ditherTex == null;
        protected override void _editorSetup(string folder, string asset)
        {
#if UNITY_EDITOR
            _ditherTex = UnityEditor.AssetDatabase.FindAssets("t:texture", new string[] { $"{folder}\\Data" })
                                    .Select(n => UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(n)))
                                    .Where(n => n != null)
                                    .FirstOrDefault(n => n.name == "Dither_Dots");
#endif
        }
    }
}