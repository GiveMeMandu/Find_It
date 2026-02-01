using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  OutlineFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/Vol/Outline")]
    public class OutlinePass : VolFx.Pass
    {
        private static readonly int s_Data        = Shader.PropertyToID("_Data");
        private static readonly int s_GradientTex = Shader.PropertyToID("_GradientTex");
        private static readonly int s_Fill        = Shader.PropertyToID("_Fill");
		
		public override string ShaderName => string.Empty;
        
        [Tooltip("Depth adaptation parameter for depth mode in with adaptation behaviour")]
        public  float                   _depthSpace   = 100;
        [Space]
        [Tooltip("Default adaptation")]
        public bool                     _adaptiveDefault;
        [Tooltip("Default sharp")]
        public bool                     _sharpDefault;
        [Tooltip("Default mode to filter by value if volume parameter is not set")]
        public  Mode                    _modeDefault  = Mode.Luma;
        [Tooltip("Default remap")]
        public Remap                    _remapDefault;
        
        private Mode      _mode     = Mode.Luma;
        private Mode      _modePrev = Mode.Alpha;
        private bool      _adaptive;
        private bool      _adaptivePrev;
        private bool      _sharp;
        private bool      _sharpPrev;
        private Remap     _remap;
        private Remap     _remapPrev;
        private Texture2D _tex;
        private Texture2D _texFill;


        // =======================================================================
        public enum Mode
        {
            // grayscale
            [Tooltip("Use Grayscale contrast to detect outline")]
            Luma = 0,
            // brightness
            [Tooltip("Use Brightness contrast to detect outline")]
            Chroma = 1,
            // alpha, default render camera format hos no alpha channel used only in VolFx
#if !VOL_FX
            [InspectorName(null)]
#endif
            [Tooltip("Use Alpha contrast to detect outline (alpha channel presented only with LayerMask, Pool or GlobalTexture drawing)")]
            Alpha = 2,
            // from depth buffer [depth texture must be enabled in renderer asset settings!]
            [Tooltip("Use Depth to outline")]
            Depth = 3
        }
        
        public enum Remap
        {
            [Tooltip("Apply gradient over the screen")]
            Screen,
            [Tooltip("Colorize line relative to a contrast detection value")]
            Value
        }
        
        // =======================================================================
        public override void Init()
        {
            if (_material == null)
                return;
            
            _updateMode(_material);
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<OutlineVol>();

            if (settings.IsActive() == false)
                return false;
            
            var updaMat = false;
            _mode = settings.m_Mode.overrideState ? settings.m_Mode.value : _modeDefault;
            if (_modePrev != _mode)
                updaMat = true;
            
            _adaptive = settings.m_Adaptive.overrideState ? settings.m_Adaptive.value : _adaptiveDefault;
            if (_adaptivePrev != _adaptive)
                updaMat = true;
            
            _sharp = settings.m_Sharp.overrideState ? settings.m_Sharp.value : _sharpDefault; 
            if (_sharpPrev != _sharp)
                updaMat = true;
            
            _remap = settings.m_Remap.overrideState ? settings.m_Remap.value : _remapDefault; 
            if (_remapPrev != _remap)
                updaMat = true;
            
            if (updaMat)
                _updateMode(mat);
            
            var thickMul = settings.m_Mode == Mode.Depth ? 3f : 1;
            var sensMul = settings.m_Mode == Mode.Depth ? 3f : 1;
            var depthSpace = _depthSpace;
            if (settings.m_Sensitive.value == 0)
                thickMul = .0f;

#if UNITY_WEBGL && !UNITY_EDITOR
            // depth texture adaptive sampling is not supported for web gl make it look more close to the original
            if (settings.m_Mode == Mode.Depth && _adaptive)
                depthSpace = (_depthSpace / 1000f) * 2f;
#endif

            mat.SetVector(s_Data, new Vector4(settings.m_Thickness.value.Remap(0, .005f) * thickMul, (settings.m_Sensitive.value / 5f).Remap(0, 50f) * sensMul, depthSpace));
            
            if (_tex == null)
            {
                _tex = new Texture2D(GradientValue.k_Width, 1, TextureFormat.RGBA32, false);
                _tex.wrapMode = TextureWrapMode.Clamp;
            }
            
            var grad = settings.m_Color.value;
            _tex.filterMode = grad._grad.mode == GradientMode.Fixed ? FilterMode.Point : FilterMode.Bilinear;
            _tex.SetPixels(grad._pixels);
            _tex.Apply();
            mat.SetTexture(s_GradientTex, _tex);
            
            /*if (_texFill == null)
            {
                _texFill = new Texture2D(GradientValue.k_Width, 1, TextureFormat.RGBA32, false);
                _texFill.wrapMode = TextureWrapMode.Clamp;
            }
            var gradFill = settings.m_Fill.value;
            _texFill.filterMode = gradFill._grad.mode == GradientMode.Fixed ? FilterMode.Point : FilterMode.Bilinear;
            _texFill.SetPixels(gradFill._pixels);
            _texFill.Apply();*/
            mat.SetColor(s_Fill, settings.m_Fill.value);
            
            return true;
        }

        private void _updateMode(Material mat)
        {
#if UNITY_EDITOR
            if (_modePrev != _mode 
                && _mode == Mode.Depth
                && GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp && urp.supportsCameraDepthTexture == false)
            {
                Debug.LogWarning("ScreenOutline work in depth mode, but the depth texture is disabled in UrpAsset settings");
            }
#endif
            
            // mode upd    
            _modePrev = _mode;
                
            mat.DisableKeyword("_LUMA");
            mat.DisableKeyword("_ALPHA");
            mat.DisableKeyword("_CHROMA");
            mat.DisableKeyword("_DEPTH");
            
            mat.EnableKeyword(_mode switch
            {
                Mode.Luma   => "_LUMA",
                Mode.Alpha  => "_ALPHA",
                Mode.Chroma => "_CHROMA",
                Mode.Depth  => "_DEPTH",
                _           => throw new ArgumentOutOfRangeException()
            });
            
            // adaptive upd
            _adaptivePrev = _adaptive;
            
            mat.DisableKeyword("_ADAPTIVE");
            
            if (_adaptive)
                mat.EnableKeyword("_ADAPTIVE");
            
            // sharp upd
            _sharpPrev = _sharp;
            
            mat.DisableKeyword("_SHARP");
            
            if (_sharp)
                mat.EnableKeyword("_SHARP");
            
            // sharp upd
            _remapPrev = _remap;
            
            mat.DisableKeyword("_SCREEN");
            
            if (_remap == Remap.Screen)
                mat.EnableKeyword("_SCREEN");
        }
    }
}