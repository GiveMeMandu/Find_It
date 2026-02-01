using System;
using System.IO;
using System.Linq;
using UnityEngine;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Dissolve")]
    public class DissolvePass : VolFx.Pass
    {
        private static readonly int       s_DissolveTex = Shader.PropertyToID("_DissolveTex");
        private static readonly int       s_DissolveMad = Shader.PropertyToID("_DissolveMad");
        private static readonly int       s_Dissolve   = Shader.PropertyToID("_Dissolve");
        private static readonly int       s_ColorTex   = Shader.PropertyToID("_ColorTex");
        private static readonly int       s_Mask = Shader.PropertyToID("_Mask");
        private static readonly int       s_OverlayTex = Shader.PropertyToID("_OverlayTex");
        private static readonly int       s_OverlayMad = Shader.PropertyToID("_OverlayMad");
		
		public override string ShaderName => string.Empty;
        
        public  Optional<RangeFloat> _tweenMove = new Optional<RangeFloat>(new RangeFloat(new Vector2(0, 7f), 1), true);
        public  Optional<RangeFloat> _tweenLerp = new Optional<RangeFloat>(new RangeFloat(new Vector2(0, 7f), .3f), true);
        public  float                _scaleDefault = 0f;
        public  Vector3              _velocityDefault;
        public  Texture2D            _textureDefault;
        public  bool                 _shadeDefault;
        
        private bool                 _persistant;
        private Texture              _tex;
        private float                _time;
        private float                _weight;
        private float                _scale;
        private float                _angle;
        private bool                 _shade;
        private bool                 _over;
        private Vector3              _velocity;
        private Color                _mask;
        private Texture2D            _color;
        
        private                 Texture2D _perlin;
        private                 Texture2D _motif;
        private                 Texture2D _splatter;
        private                 Texture2D _vessel;
        private                 Texture2D _french;

        // =======================================================================
        public enum DisolveTex
        {
            Perlin,
            Motif,
            Splatter,
            Vessel,
            French,
            
            Custom = 128
        }

        // =======================================================================
        public override void Init()
        {
            _time = 0f;
            _shade = false;
            _over  = false;
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<DissolveVol>();
            var isTweenMode = false; //_tweenMove.Enabled || _tweenLerp.Enabled;
            var tweenFade = false;

            if (settings.IsActive() == false)
            {
                if (isTweenMode && _weight > 0f)
                {
                    tweenFade = true;
                }
                else
                {
                    return false;
                }
            }
            
            _time += Time.unscaledDeltaTime;
            
            if (tweenFade == false)
                _scale = settings.m_Scale.overrideState ? settings.m_Scale.value : _scaleDefault;
            
            if (tweenFade == false)
                _tex = settings.m_Disolve.overrideState ? settings.m_Disolve.value switch
                {
                    DisolveTex.Perlin   => _perlin,
                    DisolveTex.Motif    => _motif,
                    DisolveTex.Splatter => _splatter,
                    DisolveTex.Vessel   => _vessel,
                    DisolveTex.French   => _french,
                    
                    DisolveTex.Custom   => settings.m_Custom.value,
                    _                   => throw new ArgumentOutOfRangeException()
                } : _textureDefault;
            
            if (_tex == null)
                _tex = Texture2D.grayTexture;
            
            if (tweenFade == false)
                _velocity = settings.m_Velocity.overrideState ? settings.m_Velocity.value : _velocityDefault; 
            
            var aspect = Screen.width / (float)Screen.height * ((float)_tex.height / _tex.width);
            var disolveMad = new Vector4(.03f * aspect, .03f, 0, 0) + new Vector4(aspect, 1f, 0, 0) * _scale 
                                                                    + new Vector4(0, 0, _velocity.x * _time, _velocity.y * _time) * .1f;
            
            var weightGoal = settings.m_Weight.value;
            
            if (_tweenMove.Enabled)
                _weight = Mathf.MoveTowards(_weight, weightGoal, _tweenMove.Value.Value * Time.deltaTime);
            if (_tweenLerp.Enabled)
                _weight = Mathf.Lerp(_weight, weightGoal, _tweenLerp.Value.Value * Time.deltaTime);
            
            if (isTweenMode == false)
                _weight = settings.m_Weight.value;
            
            if (tweenFade == false)
                if (settings.m_Color.overrideState)
                    settings.m_Color.value.GetTexture(ref _color);

            _angle += (_velocity.z * Time.unscaledDeltaTime) * .1f;
            if (tweenFade == false && _velocity.z == 0f)
                _angle = settings.m_Angle.value * Mathf.PI;
            
            var value = new Vector4(_weight - 0.01f, _angle, 0, 0);
            if (tweenFade == false)
            {
                var shade = settings.m_Shade.overrideState ? settings.m_Shade.value :_shadeDefault;
                if (_shade != shade)
                {
                    if (shade)
                        mat.EnableKeyword("_SHADE");
                    else
                        mat.DisableKeyword("_SHADE");
                    
                    _shade = shade;
                }
            }
            
            if (tweenFade == false)
                _mask = settings.m_Mask.value;
            
            var over = settings.m_Overlay.value;
            
            var hasOver = over != null;
            if (hasOver != _over)
            {
                if (hasOver)
                    mat.EnableKeyword("_OVER");
                else
                    mat.DisableKeyword("_OVER");
                
                _over = hasOver;
            }
            
            mat.SetTexture(s_DissolveTex, _tex);
            mat.SetVector(s_DissolveMad, disolveMad);
            mat.SetVector(s_Dissolve, value);
            mat.SetTexture(s_ColorTex, _color);
            
            if (hasOver)
            {
                mat.SetTexture(s_OverlayTex, over);
                mat.SetVector(s_OverlayMad, _getScaleOffset(over != null ? new Vector2(over.width, over.height) : Vector2.zero, new Vector2(Screen.width, Screen.height)));
            }
            
            return true;
        }

        private Vector4 _getScaleOffset(Vector2 textureSize, Vector2 screenSize)
        {
            var aspectTex    = textureSize.x / textureSize.y;
            var aspectScreen = screenSize.x / screenSize.y;

            var scale  = Vector2.one;
            var offset = Vector2.zero;

            if (aspectTex > aspectScreen)
            {
                scale.x = aspectScreen / aspectTex;
                offset.x = (1.0f - scale.x) * 0.5f;
            }
            else if (aspectTex < aspectScreen)
            {
                scale.y = aspectTex / aspectScreen;
                offset.y = (1.0f - scale.y) * 0.5f;
            }

            return new Vector4(scale.x, scale.y, offset.x, offset.y);
        }


        protected override bool _editorValidate => _perlin == null || _motif == null || _splatter == null || _vessel == null || _french == null;
        protected override void _editorSetup(string folder, string asset)
        {
#if UNITY_EDITOR
			var sep = Path.DirectorySeparatorChar;
			
            _perlin = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}{sep}Data{sep}Perlin.png");
            _motif = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}{sep}Data{sep}Motif.png");
            _splatter = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}{sep}Data{sep}Splatter.png");
            _vessel = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}{sep}Data{sep}Vessel.png");
            _french = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}{sep}Data{sep}French.png");
            
            if (_textureDefault == null)
                _textureDefault = _motif;
#endif
        }
    }
}