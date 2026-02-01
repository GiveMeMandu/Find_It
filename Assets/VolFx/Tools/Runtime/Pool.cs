#if UNITY_6000_0_OR_NEWER
#define UNITY_RENDER_GRAPH
#else
#define UNITY_LEGACY
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx.Tools
{
    public class Pool : ScriptableObject
    {
        [Tooltip("When draw renderers")]
        public RenderPassEvent               _event = RenderPassEvent.AfterRenderingOpaques;
        [Tooltip("Renderers collected by render mask, also renderers can be collected via InLayer script")]
        public Optional<LayerMask>           _mask  = new Optional<LayerMask>(true);
        [Tooltip("Clear color, if not set, cleaning will not be performed")]
        public Optional<Color>               _clear = new Optional<Color>(new Color(1, 1, 1, 0), true);
        [Tooltip("Global texture name, that can be accessed via shader")]
        public string                        _globalTex = "_PoolTex";
        [Tooltip("Depth source texture")]
        public DepthStencil                  _depth = DepthStencil.None;
        [Tooltip("Output format")] [HideInInspector]
        public Optional<RenderTextureFormat> _format = new Optional<RenderTextureFormat>(RenderTextureFormat.ARGB32, true);
        
        internal RenderTexture               _poolTex;
        
        internal RTHandle _tex;
        internal bool _isLegacy;
        
        internal RTHandle GetHandle()
        {
            return _tex;
        }
        
        internal RTHandle GetHandle(int width, int height)
        {
            var rtd = new RenderTextureDescriptor(width, height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.None, 0);
#if UNITY_RENDER_GRAPH
            RenderingUtils.ReAllocateHandleIfNeeded(ref _tex, rtd, FilterMode.Point, TextureWrapMode.Repeat, name: GlobalTexName);
#elif UNITY_LEGACY
            RenderingUtils.ReAllocateIfNeeded(ref _tex, rtd, FilterMode.Point, TextureWrapMode.Repeat, name: GlobalTexName);
#endif
            
            return _tex;
        }

        public Color Background
        {
            get => _clear.Value;
            set => _clear.Value = value;
        }
        
        public Texture Texture
        {
            get
            {
                if (_isLegacy)
                {
                    var tx = Shader.GetGlobalTexture(GlobalTexName);
                    if (tx == null || tx.width == 4)
                        tx = GetHandle(Screen.width, Screen.height);
                    
                    return tx;
                }
                
                return _tex;
            }
        }

        public string  GlobalTexName => _globalTex;

        // =======================================================================
        public enum DepthStencil
        {
            None,
            Camera,
            Clean,
            Copy,
        }
        
        // =======================================================================
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;
            
            var lrf =UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(this)).OfType<VolFxPool>().FirstOrDefault(n => n._list.Contains(this));
            if (lrf != null)
                lrf.Create();
#endif
        }
    }
}