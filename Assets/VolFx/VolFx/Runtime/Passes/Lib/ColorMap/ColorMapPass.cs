using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//  ColorMap © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/Vol/ColorMap")]
    public class ColorMapPass : VolFx.Pass
    {
        private static readonly int s_Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int s_Gradient  = Shader.PropertyToID("_GradientTex");
        private static readonly int s_Weights   = Shader.PropertyToID("_Weights");
        private static readonly int s_Mask      = Shader.PropertyToID("_Mask");
        private static readonly int s_LutTex    = Shader.PropertyToID("_lutTex");
        private static readonly int s_PalTex    = Shader.PropertyToID("_palTex");
		
		public override string ShaderName => string.Empty;
        
        private Texture2D _texOver;
        
        private Dictionary<Texture2D, Texture2D> _paletteCache = new Dictionary<Texture2D, Texture2D>();
        private Texture2D                        _palTex;

        private bool  _usePalettePrev;
        private float _motion;

        // =======================================================================
        public static class LutGenerator
        {
            private static Texture2D _lut16;
            private static Texture2D _lut32;
            private static Texture2D _lut64;

            // =======================================================================
            [Serializable]
            public enum LutSize
            {
                x16,
                x32,
                x64
            }

            [Serializable]
            public enum Gamma
            {
                rec601,
                rec709,
                rec2100,
                average,
            }
            
            // =======================================================================
            public static Texture2D Generate(Texture2D _palette, LutSize lutSize = LutSize.x16, Gamma gamma = Gamma.rec601)
            {
                var clean  = _getLut(lutSize);
                var lut    = clean.GetPixels();
                var colors = _palette.GetPixels();
                
                var _lutPalette = new Texture2D(clean.width, clean.height, TextureFormat.ARGB32, false);

                // grade colors from lut to palette by rgb 
                var palette = lut.Select(lutColor => colors.Select(gradeColor => (grade: compare(lutColor, gradeColor), color: gradeColor)).OrderBy(n => n.grade).First())
                                .Select(n => n.color)
                                .ToArray();
                
                _lutPalette.SetPixels(palette);
                _lutPalette.filterMode = FilterMode.Point;
                _lutPalette.wrapMode   = TextureWrapMode.Clamp;
                _lutPalette.Apply();
                
                return _lutPalette;

                // -----------------------------------------------------------------------
                float compare(Color a, Color b)
                {
                    // compare colors by grayscale distance
                    var weight = gamma switch
                    {
                        Gamma.rec601  => new Vector3(0.299f, 0.587f, 0.114f),
                        Gamma.rec709  => new Vector3(0.2126f, 0.7152f, 0.0722f),
                        Gamma.rec2100 => new Vector3(0.2627f, 0.6780f, 0.0593f),
                        Gamma.average => new Vector3(0.33333f, 0.33333f, 0.33333f),
                        _             => throw new ArgumentOutOfRangeException()
                    };

                    // var c = a.ToVector3().Mul(weight) - b.ToVector3().Mul(weight);
                    var c = new Vector3(a.r * weight.x, a.g * weight.y, a.b * weight.z) - new Vector3(b.r * weight.x, b.g * weight.y, b.b * weight.z);
                    
                    return c.magnitude;
                }
            }

            // =======================================================================
            internal static int _getLutSize(LutSize lutSize)
            {
                return lutSize switch
                {
                    LutSize.x16 => 16,
                    LutSize.x32 => 32,
                    LutSize.x64 => 64,
                    _           => throw new ArgumentOutOfRangeException()
                };
            }
            
            internal static Texture2D _getLut(LutSize lutSize)
            {
                var size = _getLutSize(lutSize);
                var _lut = lutSize switch
                {
                    LutSize.x16 => _lut16,
                    LutSize.x32 => _lut32,
                    LutSize.x64 => _lut64,
                    _           => throw new ArgumentOutOfRangeException(nameof(lutSize), lutSize, null)
                };
                
                if (_lut != null && _lut.height == size)
                     return _lut;
                
                _lut            = new Texture2D(size * size, size, TextureFormat.RGBA32, 0, false);
                _lut.filterMode = FilterMode.Point;

                for (var y = 0; y < size; y++)
                for (var x = 0; x < size * size; x++)
                    _lut.SetPixel(x, y, _lutAt(x, y));
                
                _lut.Apply();
                return _lut;

                // -----------------------------------------------------------------------
                Color _lutAt(int x, int y)
                {
                    return new Color((x % size) / (size - 1f), y / (size - 1f), Mathf.FloorToInt(x / (float)size) * (1f / (size - 1f)), 1f);
                }
            }
        }
        
        // =======================================================================
        public override void Init()
        {
            _paletteCache.Clear();
            _usePalettePrev = false;
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<ColorMapVol>();

            if (settings.IsActive() == false)
            {
                _motion = 0f;
                return false;
            }
            
            
            var palette = settings.m_Palette.value as Texture2D;
            Texture2D paletteLut = null;
            if (palette != null && _paletteCache.TryGetValue(palette, out paletteLut) == false)
            {
                paletteLut = LutGenerator.Generate(palette);
                _paletteCache.Add(palette, paletteLut);
            }
            settings.m_Impact.value.GetTexture(ref _palTex);
            
            var usePalette = palette != null && settings.m_Impact.overrideState;
            
            if (usePalette != _usePalettePrev)
                _validateMaterial();
            
            if (usePalette)
            {
                mat.SetTexture(s_LutTex, paletteLut);
                mat.SetTexture(s_PalTex, _palTex);
            }
            
            if (_texOver == null)
            {
                _texOver = new Texture2D(GradientValue.k_Width, 1, TextureFormat.RGBA32, false);
                _texOver.wrapMode = TextureWrapMode.Repeat;
            }
            
            var grad = settings.m_Gradient.value;
            _texOver.filterMode = grad._grad.mode == GradientMode.Fixed ? FilterMode.Point : FilterMode.Bilinear;
            _texOver.wrapMode   = TextureWrapMode.Clamp;
            _texOver.SetPixels(grad._pixels);
            _texOver.Apply();
            
            _motion += settings.m_Motion.value * Time.deltaTime;

            mat.SetTexture(s_Gradient, _texOver);
            
            mat.SetFloat(s_Intensity, settings.m_Weight.value);
            
            var stretch = settings.m_Stretch.value >= 0f ? Mathf.Lerp(1, 7, Mathf.Pow(settings.m_Stretch.value, 3)) : Mathf.Lerp(1, 0, Mathf.Abs(settings.m_Stretch.value));
            var mask = new Vector4(settings.m_Mask.value.x, settings.m_Mask.value.y, settings.m_Offset.value + _motion, stretch);
            if (mask.x == mask.y)
                mask.x += 0.01f;

            mat.SetVector(s_Mask, mask);

            return true;

            // -----------------------------------------------------------------------
            void _validateMaterial()
            {
                mat.DisableKeyword("USE_PALETTE");
                
                if (usePalette)
                    mat.EnableKeyword("USE_PALETTE");
                
                _usePalettePrev = usePalette;
            }
        }
        
    }
}