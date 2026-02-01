using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    public class BlitPass : VolFx.Pass
    {
        public override string ShaderName => string.Empty;
        
        [SerializeField] [Tooltip("Show pass as a ScriptableObject to have an ability to control it via Script and Unity Events")]
        internal  bool       _showInInspector;
        [Tooltip("Invert draw matrix")] [HideInInspector]
        public bool          _invert;

        public Material      _mat;
        public Optional<int> _pass;

        public Material Mat
        {
            set => _mat = value;
            get => _mat;
        }
        
        public int Pass
        {
            set => _pass.Value = value;
            get => _pass.Value;
        }
        
        protected override bool Invert => _invert;

        // =======================================================================
        public override bool Validate(Material mat)
        {
            _material = _mat;
            return _mat != null;
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

        public override void Invoke(RTHandle source, RTHandle dest, VolFx.CallApi callApi)
        {
            callApi.Blit(source, dest, _mat, _pass.GetValueOrDefault(0));
        }
    }
}
