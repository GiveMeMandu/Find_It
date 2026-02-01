using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Bloom")]
    public class BloomPass : VolFx.Pass
    {
        private static readonly int s_ValueTex  = Shader.PropertyToID("_ValueTex");
        private static readonly int s_ColorTex  = Shader.PropertyToID("_ColorTex");
        private static readonly int s_Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int s_BloomTex  = Shader.PropertyToID("_BloomTex");
        private static readonly int s_DownTex   = Shader.PropertyToID("_DownTex");
        private static readonly int s_Blend     = Shader.PropertyToID("_Blend");
		
		public override string ShaderName => string.Empty;
        
        [Tooltip("Bloom filter")]
        public ValueMode         _mode = ValueMode.Brightness;
        [Range(3, 14)] [Tooltip("Default number of bloom samples")]
        public  int              _samples = 7;
        [CurveRange(0, 0, 1, 1)]
        [Tooltip("Max Scattering curve (bloom samples blending)")]
        public AnimationCurve    _scatterMax = AnimationCurve.Linear(0, 0, 1, 0);
        [CurveRange(0, 0, 1, 1)]
        [Tooltip("Zero Scattering (bloom samples blending)")]
        public AnimationCurve    _scatterMin = AnimationCurve.Linear(0, 1, 1, 0);
        
        [CurveRange(0, 0, 1, 1)] [Tooltip("Intensity flickering")]
        public  AnimationCurve   _flicker = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(.5f, .77f), new Keyframe(1, 1) }) { postWrapMode = WrapMode.Loop };
        public float             _flickerPeriod = 7f;
        [Tooltip("Use hdr texture")]
        public bool              _hdr = true;
        [Tooltip("Debug, draw bloom only")]
        public bool              _bloomOnly;
        
        private float            _time;
        private float            _intensity;
        private Texture2D        _valueTex;
        private Texture2D        _colorTex;
        
        private RenderTarget[]   _mipDown;
        private RenderTarget[]   _mipUp;
        private float            _scatterLerp;
        private ProfilingSampler _sampler;

        public int Samples
        {
            get => _samples;
            set 
            { 
                _samples = value;
                Init();
            }
        }

        // =======================================================================
        public enum ValueMode
        {
            Luma,
            Brightness
        }

        // =======================================================================
        public override void Init()
        {
            _validateSamples();
            _validateMaterial();
            
            _sampler = new ProfilingSampler("Bloom");
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<BloomVol>();

            if (settings.IsActive() == false)
                return false;
            
            _time += Time.deltaTime;
            
            mat.SetTexture(s_ValueTex, settings.m_Threshold.value.GetTexture(ref _valueTex));
            mat.SetTexture(s_ColorTex, settings.m_Color.value.GetTexture(ref _colorTex));
            _intensity = settings.m_Intencity.value * Mathf.Lerp(1, _flicker.Evaluate(_time / _flickerPeriod), settings.m_Flicker.value);
            _scatterLerp = settings.m_Scatter.value;
            
            return true;
        }

        private void OnValidate()
        {
            if (Application.isPlaying == false)
                _validateSamples();

            _validateMaterial();
        }

        private void _validateMaterial()
        {
            if (_material != null)
            {
                _material.DisableKeyword("_BRIGHTNESS");
                _material.DisableKeyword("_LUMA");
                _material.DisableKeyword("_BLOOM_ONLY");
                _material.DisableKeyword("_HDR");

                _material.EnableKeyword(_mode switch
                {
                    ValueMode.Luma       => "_LUMA",
                    ValueMode.Brightness => "_BRIGHTNESS",
                    _                    => throw new ArgumentOutOfRangeException()
                });

                if (_bloomOnly)
                    _material.EnableKeyword("_BLOOM_ONLY");
                
                if (_hdr)
                    _material.EnableKeyword("_HDR");
            }
        }
        
        private void _validateSamples()
        {
            if (_mipDown == null || _mipDown.Length != _samples)
            {
                _mipDown = new RenderTarget[_samples];
                _mipUp   = new RenderTarget[_samples - 1];
                
                for (var n = 0; n < _samples; n++)
                    _mipDown[n] = new RenderTarget().Allocate($"bloom_{name}_down_{n}");
                
                for (var n = 0; n < _samples - 1; n++)
                    _mipUp[n]   = new RenderTarget().Allocate($"bloom_{name}_up_{n}");
            }
        }

        public override void Init(VolFx.InitApi initApi)
        {
            _validateSamples();
            var width  = initApi.Width;
            var height = initApi.Height;
            var format = _hdr ? GraphicsFormat.B10G11R11_UFloatPack32 : GraphicsFormat.R8G8B8A8_UNorm;
            
            for (var n = 0; n < _samples - 1; n++)
            {
                width  = Mathf.Max(1, width >> 1);
                height = Mathf.Max(1, height >> 1);
                
                initApi.Allocate(_mipDown[n], width, height, format, TextureWrapMode.Clamp, FilterMode.Bilinear);
                initApi.Allocate(_mipUp[n], width, height, format, TextureWrapMode.Clamp, FilterMode.Bilinear);
            }
            
            width  = Mathf.Max(1, width >> 1);
            height = Mathf.Max(1, height >> 1);
            initApi.Allocate(_mipDown[_samples - 1], width, height, format, TextureWrapMode.Clamp, FilterMode.Bilinear);
        }

        public override void Invoke(RTHandle source, RTHandle dest, VolFx.CallApi callApi)
        {
            callApi.BeginSample(_sampler);
            // blit filter
            callApi.Blit(source, _mipDown[0], _material);
                
            // blit down
            for (var n = 1; n < _samples; n++)
                callApi.Blit(_mipDown[n - 1], _mipDown[n], _material, 1);
            
            // draw first
            var blend = Mathf.Lerp(_scatterMin.Evaluate(0f), _scatterMax.Evaluate(0f), _scatterLerp);
            
            callApi.Mat.SetFloat(s_Blend, blend);
            callApi.Mat.SetTexture(s_DownTex, _mipDown[_samples - 2].Handle);
            callApi.Blit(_mipDown[_samples - 1].Handle, _mipUp[_samples - 2].Handle, _material, 2);
            
            // blit up
            for (var n = _samples - 3; n >= 0; n--)
            {
                var t = n / (float)(_samples - 2);
                blend = Mathf.Lerp(_scatterMin.Evaluate(t), _scatterMax.Evaluate(t), _scatterLerp);
                
                callApi.Mat.SetFloat(s_Blend, blend);
                callApi.Mat.SetTexture(s_DownTex, _mipDown[n].Handle);
                callApi.Blit(_mipUp[n + 1].Handle, _mipUp[n].Handle, _material, 2);
            }
            
            // combine call
            _material.SetFloat(s_Intensity, _intensity);
            callApi.Mat.SetTexture(s_BloomTex, _mipUp[0].Handle);
            callApi.Blit(source, dest, _material, 3);
            
            callApi.EndSample(_sampler);
        }

        public override void Cleanup(CommandBuffer cmd)
        {
            foreach (var rt in _mipDown)
                rt.Release(cmd);
            
            foreach (var rt in _mipUp)
                rt.Release(cmd);
        }
    }
}