using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VolFx.Tools;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Mask")]
    public class MaskPass : VolFx.Pass
    {
        private static readonly int s_MaskTex   = Shader.PropertyToID("_MaskTex");
        private static readonly int s_SourceTex = Shader.PropertyToID("_SourceTex");
        private static readonly int s_Weight    = Shader.PropertyToID("_Weight");
		
		public override string ShaderName => string.Empty;
        
        [Tooltip("Default Mask blending mode")]
        public  Mode   _mode;
        [Tooltip("Control mask via volume settings")]
        public bool    _useVolumeSettings = true;
        [Tooltip("Always execute mask pass (in can be used for multiply mask instances controlled via scriptable object)")]
        public bool    _persistent;
        [Range(0, 1)]
        [Tooltip("Mask default weight")]
        public float   _weight = 1;
        [Tooltip("Mask texture source if not set in volume (to create a pool add VolFx Pool Render feature to the renderer asset)")]
        public  Pool   _mask;
        [SerializeField] [Tooltip("Show pass as a ScriptableObject to have an ability to control it via Script and Unity Events")]
        public bool    _showInInspector;
        private bool _blending;
        private Pool _maskCur;
        private Mode _modeCur;
        private bool _inverseCur;
        
        private bool   _inverse;

        public  RenderTarget _result;

        public Pool Mask
        {
            get => _mask;
            set => _mask = value;
        }
        
        public float Weight
        {
            get => _weight;
            set => _weight = value;
        }
        
        protected override int MatPass => _modeCur switch
        {
            Mode.Alpha => 0 + (_blending ? 1 : 0),
            Mode.Grayscale  => 2 + (_blending ? 1 : 0),
            _          => throw new ArgumentOutOfRangeException()
        };


        // =======================================================================
        public enum Mode
        {
            Alpha,
            Grayscale
        }
        
        // =======================================================================
        public override void Init()
        {
            _result = new RenderTarget().Allocate($"{_owner.name}_mask");
            _inverseCur = false;
        }

        public override void Init(VolFx.InitApi initApi)
        {
            initApi.Allocate(_result, Screen.width, Screen.height, GraphicsFormat.R8G8B8A8_UNorm);
        }

        public override void Invoke(RTHandle source, RTHandle dest, VolFx.CallApi callApi)
        {
            if (callApi.CamType != CameraType.Game)
                return;
            
            // use blending blit call to avoid draw in to the source texture
            //_blending = Source == dest;
            _blending = false;
            var matPass = _modeCur switch
            {
                Mode.Alpha => 0 + (_blending ? 1 : 0),
                Mode.Grayscale  => 2 + (_blending ? 1 : 0),
                _          => throw new ArgumentOutOfRangeException()
            };
            
            callApi.Mat.SetTexture(s_SourceTex, callApi.CamColor);
            callApi.Mat.SetTexture(s_MaskTex, _maskCur.Texture);
            callApi.Blit(source, _result, _material, matPass);
            callApi.Blit(_result, dest, _owner._blit);
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<MaskVol>();
            if (_persistent == false && (_useVolumeSettings && settings.IsActive() == false) || (_persistent == false && _useVolumeSettings == false))
                return false;

            _maskCur = settings.m_Mask.value;
            if (_maskCur == null)
                _maskCur = _mask;
            
            var weight = settings.m_Weight.value;
            if (_useVolumeSettings == false)
                weight = _weight;
            
            if (_maskCur == null)
                return false;
            
            _modeCur = settings.m_Mode.overrideState ? settings.m_Mode.value : _mode;
            
            var invers = settings.m_Inverse.overrideState ? settings.m_Inverse.value : _inverse;
            if (_inverseCur != invers)
            {
                _inverseCur = invers;
                mat.DisableKeyword("INVERSE");
                
                if (invers)
                    mat.EnableKeyword("INVERSE");
            }
            
            mat.SetFloat(s_Weight, weight);
            
            return true;
        }
        
        public virtual void OnValidate()
        {
#if UNITY_EDITOR
            if ((_showInInspector && hideFlags == HideFlags.None) ||
                (_showInInspector == false && hideFlags == (HideFlags.HideInInspector | HideFlags.HideInHierarchy)))
                return;
            
            if (_showInInspector && hideFlags != HideFlags.None)
                hideFlags = HideFlags.None;
            else
                hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            
            _updateAsset();
            
            // =======================================================================
            async void _updateAsset()
            {
                await Task.Yield();
                UnityEditor.AssetDatabase.SaveAssets();
            }
#endif
        }
    }
}