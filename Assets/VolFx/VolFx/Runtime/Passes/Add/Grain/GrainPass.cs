using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Grain")]
    public class GrainPass : VolFx.Pass
    {
        private static readonly int s_Intensity   = Shader.PropertyToID("_Intensity");
        private static readonly int s_ResponseTex = Shader.PropertyToID("_ResponseTex");
        private static readonly int s_Tiling      = Shader.PropertyToID("_Tiling");
        private static readonly int s_GrainTex    = Shader.PropertyToID("_GrainTex");
        private static readonly int s_Grain       = Shader.PropertyToID("_Grain");
        private static readonly int s_Tint        = Shader.PropertyToID("_Tint");
        private static readonly int s_Adj         = Shader.PropertyToID("_Adj");
		
		public override string ShaderName => string.Empty;

        [Tooltip("Default grain tex if override is not set")]
        public                  GrainTex        _grain = GrainTex.Grain_Medium_A;
        [HideInInspector]
        public                  Texture2D[]     _grainTex;
        public                  float           _grainClamp = .5f;
        private                 float           _lastStep;
        private                 Texture2D       _impactTex;
        private                 Vector4         _tiling;

        // =======================================================================
        public enum GrainTex
        {
            Grain_Large_A,
            Grain_Large_B,
            Grain_Medium_A,
            Grain_Medium_B,
            Grain_Medium_C,
            Grain_Medium_D,
            Grain_Medium_E,
            Grain_Medium_F,
            Grain_Thin_A,
            Grain_Thin_B,
            
            Hatching_Cross,
            Hatching_Dots
        }
        
        // =======================================================================
        public override void Init()
        {
            _tiling = new Vector4(Screen.width / (float)_grainTex[0].width, Screen.height / (float)_grainTex[0].height, 0f, 0f);
            _lastStep = 0f;
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<GrainVol>();

            if (settings.IsActive() == false)
                return false;

            var isStatic = false;
            var fps = settings.m_Fps.value;
            if (fps == 0)
            {
                isStatic = true;
            }
            else
            if (fps == settings.m_Fps.max)
            {
                isStatic = false;
            }
            else
            if (fps < settings.m_Fps.max)
            {
                var timeStep = Time.unscaledTime - _lastStep;
                if (timeStep < (1f / fps))
                    isStatic = true;
            }
            
            var grainTex = settings.m_Texture.overrideState && settings.m_Texture.value != null ? settings.m_Texture.value : _grainTex[(int)(settings.m_GainTex.overrideState ? settings.m_GainTex.value : _grain)];

            if (isStatic == false)
            {
                var scale = settings.m_Scale.value;
                if (scale > 0f)
                    scale = scale * 3f + 1f;
                else
                    scale += 1f;
                
                _tiling   = new Vector4((Screen.width / (float)grainTex.width) * scale, (Screen.height / (float)grainTex.height) * scale, Random.value, Random.value);
                _lastStep = Time.unscaledTime;
            }

            mat.SetTexture(s_ResponseTex, settings.m_Response.value.GetTexture(ref _impactTex));
            mat.SetTexture(s_GrainTex, grainTex);
            mat.SetVector(s_Grain, new Vector4(_grainClamp, (1f / (1f - _grainClamp)) * settings.m_Grain.value * 3f));
            mat.SetVector(s_Tiling, _tiling);
            mat.SetVector(s_Adj, new Vector4(settings.m_Hue.value * Mathf.PI * .5f, 1f + settings.m_Saturation.value * (settings.m_Saturation.value > 0f ? 7f : 1f), settings.m_Brightness.value, 1f - settings.m_Alpha.value));
            mat.SetVector(s_Tint, settings.m_Color.value);
            
            return true;
        }

        protected override bool _editorValidate => _grainTex == null || _grainTex.Length < 12 || _grainTex.Any(n => n == null);

        protected override void _editorSetup(string folder, string asset)
        {
#if UNITY_EDITOR
            _grainTex = Enum
                        .GetNames(typeof(GrainTex))
                        .Select(n => UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}\\Grain\\{n}.png"))
                        .ToArray();
            
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}