using UnityEngine;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Scanlines")]
    public class ScanlinesPass : VolFx.Pass
    {
        private static readonly int s_Scanlines = Shader.PropertyToID("_Scanlines");
        private static readonly int s_Screen    = Shader.PropertyToID("_Screen");
        private static readonly int s_Color     = Shader.PropertyToID("_Color");
		
		public override string ShaderName => string.Empty;

        [CurveRange]
        [Tooltip("Move animation curve")]
        public AnimationCurve   _move = new AnimationCurve(new Keyframe(0, .5f, 0, 0), new Keyframe(1f, 1f, 0, 0)) { postWrapMode = WrapMode.PingPong };
        [HideInInspector]
        public float            _movePeriod = 5f;
        
        [CurveRange]
        [Tooltip("Flicker animation curve")]
        [InspectorName("Flicker")]
        public AnimationCurve   _flickerCurve = new AnimationCurve(new Keyframe(0, 0, 1.1597f, 1.1597f, 0f, .2274f), new Keyframe(1f, 1f, 1.5078f, 1.5078f, .2807f, 0f)) { postWrapMode = WrapMode.PingPong };
        [HideInInspector]
        public  float           _flickerPeriod = 1.2f;
        [HideInInspector]
        public  Vector2         _gradRange = new Vector2(.07f, 3f);
        [HideInInspector]
        public  Vector2         _flickerMove = new Vector2(.2f, 3f);
        [CurveRange]
        [Tooltip("Flip animation curve")]
        [InspectorName("Flip")]
        public  AnimationCurve  _flipCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [HideInInspector]
        public  float           _flipRelease = .1f;
        
        public  Optional<float> _forceCount = new Optional<float>(570f, false);

        private float _flickerTime;
        private float _flip;
        private float _offset;

        // =======================================================================
        public override void Init()
        {
            _offset = 0;
        }

        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<ScanlinesVol>();

            if (settings.IsActive() == false)
                return false;
            
            // intensity & move
            var intensity = settings.m_Intensity.value;
            _offset += _move.Evaluate(Time.time / _movePeriod) * settings.m_Speed.value;
            
            while (_offset > 1f)
                _offset -= 1f;
            
            // flip
            var flipSpeed = settings.m_Flip.value;
            var hasFlip = flipSpeed > 0;
            
            if (hasFlip || _flip != 0)
                _flip += (hasFlip ? flipSpeed : (_flip > .5f ? 1f / _flipRelease : -1f / _flipRelease)) * Time.deltaTime;
            
            if (hasFlip == false && (_flip > 1f || _flip < 0f))
                _flip = 0f;
            
            _flip %= 1f;
            
            // flicker
            var flicker   =  Mathf.Lerp(1f,_flickerCurve.Evaluate(Time.time / _flickerPeriod), settings.m_Animation.value) * settings.m_Flicker.value;
            _flickerTime += Time.deltaTime * Mathf.Lerp(_flickerMove.x, _flickerMove.y, settings.m_GradSpeed.value);
            
            var move = _flickerCurve.Evaluate((_flickerTime % _flickerPeriod) / _flickerPeriod);
            var grad = 1 - Mathf.Lerp(_gradRange.x, _gradRange.y, settings.m_Grad.value);
            
            // flicker color
            var col = settings.m_GradColor.value;
            col.r *= col.a;
            col.g *= col.a;
            col.b *= col.a;
            
            // params
            mat.SetVector(s_Scanlines, new Vector4(_forceCount.Enabled ? _forceCount.Value : settings.m_Count.value, intensity, settings.m_Color.value, _offset));
            mat.SetVector(s_Screen, new Vector4(flicker, _flipCurve.Evaluate(_flip), grad, move));
            mat.SetColor(s_Color, col);
            
            return true;
        }
    }
}