using System;
using UnityEngine;

namespace VolFx
{
    [Serializable]
    public class DitherGradientValue
    {
        public const int k_Width = 32;
        
        public Gradient _grad;
        public Color[]  _colorPixels;
        public Color[]  _ditherPixels;

        internal bool _build;
        
        public static GradientValue White
        {
            get
            {
                var grad = new Gradient();
                grad.SetKeys(new []{new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f)}, new GradientAlphaKey[]{new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0f)});
                
                return new GradientValue(grad);
            }
        }
        
        // =======================================================================
        public void Build(GradientMode _mode)
        {
            _build = true;
            
            _grad.mode    = _mode;
            _colorPixels  = new Color[k_Width];
            _ditherPixels = new Color[k_Width];
            
            var ditherGrad = new Gradient();
            ditherGrad.mode = _grad.mode;

            var srcKeys = _grad.colorKeys;
            if (srcKeys.Length < 2)
            {
                var col = srcKeys.Length == 1 ? srcKeys[0].color : Color.black;
                var fallback = new[]
                {
                    new GradientColorKey(col, 0f),
                    new GradientColorKey(col, 1f)
                };
                ditherGrad.SetKeys(fallback, _grad.alphaKeys);
            }
            else
            {
                var keys = new GradientColorKey[srcKeys.Length - 1];

                for (var i = 1; i < srcKeys.Length; i++)
                {
                    var col  = srcKeys[i - 1].color;
                    var time = srcKeys[i].time;
                    keys[i - 1] = new GradientColorKey(col, time);
                }

                ditherGrad.SetKeys(keys, _grad.alphaKeys);
            }

            for (var x = 0; x < k_Width; x++)
            {
                _colorPixels[x]  = _grad.Evaluate(x / (float)(k_Width - 1));
                _ditherPixels[x] = _ditherColor(x / (float)(k_Width - 1));
            }

            // -----------------------------------------------------------------------
            Color _ditherColor(float t)
            {
                var col  = ditherGrad.Evaluate(t);
                var keys = _grad.colorKeys;

                col.a = 1f - _localMeasure();
                return col;
                
                // -----------------------------------------------------------------------
                float _localMeasure()
                {
                    if (t <= keys[0].time)
                    {
                        var range = keys[0].time;
                        if (range <= 0f) return 0f;
                        return Mathf.Clamp01(t / range);
                    }

                    for (var i = 1; i < keys.Length; i++)
                    {
                        var prev = keys[i - 1];
                        var next = keys[i];

                        if (t <= next.time)
                        {
                            var range = next.time - prev.time;
                            if (range <= 0f) return 0f;
                            return Mathf.Clamp01((t - prev.time) / range);
                        }
                    }

                    return 1f;
                }
            }
        }
        
        internal void SetValue(DitherGradientValue val)
        {
            if (val._build == false)
                val.Build(val._grad.mode);
            
            _grad = val._grad;
        
            val._colorPixels.CopyTo(_colorPixels, 0);
            val._ditherPixels.CopyTo(_ditherPixels, 0);
        }
        
        public void Blend(DitherGradientValue a, DitherGradientValue b, float t)
        {
            _build = true;
            
            if (b._ditherPixels == null || b._ditherPixels.Length == 0)
                b.Build(GradientMode.Fixed);
            
            for (var x = 0; x < k_Width; x++)
            {
                _colorPixels[x]  = Color.LerpUnclamped(a._colorPixels[x], b._colorPixels[x], t);
                _ditherPixels[x] = Color.LerpUnclamped(a._ditherPixels[x], b._ditherPixels[x], t);
            }
            
            _grad.mode = t < .5f ? a._grad.mode : b._grad.mode;
        }
        
        public Texture2D GetColorsTex(ref Texture2D tex)
        {
            if (tex == null)
            {
                tex            = new Texture2D(GradientValue.k_Width, 1, TextureFormat.RGBA32, false);
                tex.wrapMode   = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Point;
            }
            
            tex.SetPixels(_colorPixels);
            tex.Apply();
            
            return tex;
        }
        
        public Texture2D GetDitherTex(ref Texture2D tex)
        {
            if (tex == null)
            {
                tex            = new Texture2D(GradientValue.k_Width, 1, TextureFormat.RGBA32, false);
                tex.wrapMode   = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Point;
            }
            
            tex.SetPixels(_ditherPixels);
            tex.Apply();
            
            return tex;
        }

        public DitherGradientValue(Gradient grad)
        {
            _grad = grad;
            Build(GradientMode.Fixed);
        }
    }
}