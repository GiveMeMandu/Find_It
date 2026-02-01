using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

//  GlitchFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/Vol/Glitch")]
    public class GlitchPass : VolFx.Pass
    {
		private static readonly int   s_Grid       = Shader.PropertyToID("_Grid");
		private static readonly int   s_GridMad    = Shader.PropertyToID("_GridMad");
		private static readonly int   s_Sobel      = Shader.PropertyToID("_Sobel");
		private static readonly int   s_SettingsEx = Shader.PropertyToID("_Settings_Ex");
		private static readonly int   s_Settings   = Shader.PropertyToID("_Settings");
		private static readonly int   s_ClipTex    = Shader.PropertyToID("_ClipTex");
		private static readonly int   s_BleedTex   = Shader.PropertyToID("_BleedTex");
		private static readonly int   s_ColorTex   = Shader.PropertyToID("_ColorTex");
		private static readonly int   s_NoiseMad   = Shader.PropertyToID("_NoiseMad");
		private static readonly int   s_NoiseTex   = Shader.PropertyToID("_NoiseTex");
		private static readonly int   s_LockTex    = Shader.PropertyToID("_LockTex");
		private static readonly int   s_ShotTex    = Shader.PropertyToID("_ShotTex");
		
		public override string ShaderName => string.Empty;

		[Tooltip("Reference crush resolution")]
		[HideInInspector]
		public  float _crushPix = 720f;
		
		[Tooltip("Screen clip speed multiplier")] 
		[HideInInspector]
		public  float _clipSpeed = 15f;
		
		//[Header("Advanced interpolations settings")]
		[CurveRange]
		[HideInInspector]
		public  AnimationCurve _chaoticPeriod = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, .4698f, 0f, 0f, 0f, .3333f),
			new Keyframe(1f, 1f, 1f, 1f, .3333f, 0f)
		});
		
		[CurveRange] 
		[HideInInspector]
		public  AnimationCurve _chaoticImpact = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f, 1f, 1f, 0f, .3333f),
			new Keyframe(1f, 1f, 1f, 1f, .3333f, 0f)
		});
		
		[CurveRange] 
		[HideInInspector]
		public AnimationCurve _periodMax = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f, 1f, 1f, 0f, .3333f),
			new Keyframe(1f, 1f, 1f, 1f, 1f, 1f)
		});
			
		[CurveRange] 
		[HideInInspector]
		public AnimationCurve _periodMin = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(.0f, .0f, -0.0542f, -0.0542f, 0f, .3333f)
		});

		[CurveRange] 
		[HideInInspector]
		public AnimationCurve _lockMax = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, .0481f, .9518f, .9518f, 0f, .3333f),
			new Keyframe(1f, 1f, .9518f, .9518f, .3333f, 0f)
		});
			
		[CurveRange] 
		[HideInInspector]
		public AnimationCurve _noiseScale = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, .018f, .9819f, .9819f, 0f, .3333f),
			new Keyframe(1f, 1f, .9819f, .9819f, .3333f, 0f)
		});

		[CurveRange] 
		[HideInInspector]
		public AnimationCurve _crushLerp = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 1f,-5.7671f,-5.7671f, 0f, .1525f),
			new Keyframe(1f, .1397f, -0.254803f, -0.2548f, .4444f, 0f)
		});
		
		[CurveRange] 
		[HideInInspector]
		public AnimationCurve _gridScale = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0.0241f, 0.1746f, 0.17f, 0f, .33f),
			new Keyframe(1f, 0.1987f, 0.1746f, 0.1746f, .33f, 0f)
		});
		
		[Tooltip("Crush period correction (x - pow, y - mul)")]
		[HideInInspector]
		public Vector2 _crushCorrection = new Vector2(4f, 2f);
		[Tooltip("Lock period correction (x - pow, y - mul)")]
		[HideInInspector]
		public Vector2 _lockCorrection = new Vector2(2f, 1f);
		
		private Texture2D    _noiseTex;
		private Texture2D    _colorTex;
		private Texture2D    _bleedTex;
		
		private RenderTarget _shotTex;
		private RenderTarget _lockTex;
		[HideInInspector]
        public  Texture2D[] _clipTex;
		
		private Vector4 _noiseMad;
		private Vector4 _gridMad;
		
		private float _clipTime;
		private float _shotTime;
		private bool  _takeShot;

		private float   _lockTime;
		private bool    _takeLock;
		
		private                 float _chaoticValue;
		private                 float _chaoticTime;
		private                 float _chaoticInterval;
		private                 float _bleed;
        private int              _warmup;

		protected override bool Invert => true;

        // =======================================================================
		public override void Init()
		{
			_shotTime = 0f;
			_clipTime = 0f;
			_lockTime = 0f;
			
			_shotTex = new RenderTarget().Allocate($"{name}_shot_rt");
			_lockTex = new RenderTarget().Allocate($"{name}_lock_rt");
			
			_initNoise();
			_warmup = 0;
			
			// -----------------------------------------------------------------------
			void _initNoise()
			{
				_noiseTex = new Texture2D(128, 128, TextureFormat.ARGB32, false);

				_noiseTex.filterMode = FilterMode.Point;
				_noiseTex.wrapMode   = TextureWrapMode.Repeat;
				var pixels = new Color[_noiseTex.width * _noiseTex.height];
				var c      = Color.clear;

				// noise tex (step, dir, offset, color)
				var check = 0f;
				for (var y = 0; y < _noiseTex.height; y++)
				{
					var dir    = Random.value > .5f ? 1f : 0f;
					var offset = Random.value;
					var color  = Random.value;
					for (var x = 0; x < _noiseTex.width; x++)
					{
						if (Random.value < .735f)
						{
							check = Random.value;
							color = Random.value;
						}

						c = new Color(check, dir, offset, color);

						pixels[y * _noiseTex.width + x] = c;
					}
				}

				_noiseTex.SetPixels(pixels);
				_noiseTex.Apply();
			}
		}

		public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<GlitchVol>();

            var isActive = settings.IsActive();
			if (isActive == false)
                return false;
			
			var delta  = Time.unscaledDeltaTime;
            var aspect = Screen.width / (float)Screen.height;
				
			mat.SetTexture(s_NoiseTex, _noiseTex);
			
			if (_shotTime <= Time.unscaledTime)
			{
				var periodVal = settings._period.value;
				var period    = Mathf.Pow(Random.Range(_periodMin.Evaluate(periodVal), _periodMax.Evaluate(periodVal)), _crushCorrection.x) * _crushCorrection.y;
				
				var noiseScale = _noiseScale.Evaluate(settings._scale.value) * 2f;
				_noiseMad = new Vector4(noiseScale * aspect, noiseScale, Random.value, Random.value);
				_shotTime = Time.unscaledTime + period;
				_takeShot = true;
			}
			
			if (_chaoticTime < 0f || Random.value <= 0.05f)
			{
				var value = settings._chaotic.value;
				
				_chaoticInterval = Random.Range(0f, _chaoticPeriod.Evaluate(value));
				_chaoticTime     = _chaoticInterval;
				_chaoticValue    = Random.Range(0f, _chaoticImpact.Evaluate(value));
			}
			
			_bleed -= Time.unscaledDeltaTime * 0.1f;
			if (_bleed <= 0 || Random.value < 0.05)
				_bleed = Random.value;
			
			_chaoticTime -= Time.unscaledDeltaTime;
			
			mat.SetVector(s_NoiseMad, _noiseMad);

			mat.SetTexture(s_ColorTex, settings._color.value.GetTexture(ref _colorTex));
			mat.SetTexture(s_BleedTex, settings._bleed.value.GetTexture(ref _bleedTex));
			
			_clipTime += _clipSpeed * delta;
			var clipFrame = Mathf.FloorToInt(_clipTime) % _clipTex.Length;
			mat.SetTexture(s_ClipTex,  _clipTex[clipFrame]);
			
			// x - power, y - sharpen, z - screen, w - lock
			mat.SetVector(s_Settings, new Vector4(Mathf.Lerp( -0.03f, 1f, settings._power.value) + _chaoticValue, settings._sharpen.value, settings._screen.value, settings._lock.value));
			// x - dispersion, y - weight, z - grid density, w - bleed
			mat.SetVector(s_SettingsEx, new Vector4(settings._dispersion.value, settings._weight.value, Mathf.Lerp(-0.01f, 1f, settings._density.value), _bleed));
			
            var sobel = settings._sharpen.value;
			mat.SetVector(s_Sobel, new Vector4(sobel * aspect * 0.003f, sobel * 0.003f, 4f, -1f));
			
			if (_lockTime <= Time.unscaledTime || Random.value < .05)
			{
				var lockVal    = settings._lock.value;
				var lockPeriod = Mathf.Pow(Random.Range(_lockMax.Evaluate(lockVal), _lockMax.Evaluate(lockVal)), _lockCorrection.x) * _lockCorrection.y;
				
				var geedScale = _gridScale.Evaluate(settings._grid.value);
				_gridMad = new Vector4(geedScale * aspect, geedScale, Random.value, Random.value);
				_lockTime = Time.unscaledTime + lockPeriod;
				_takeLock = true;
			}
			
			if (settings._noLockNoise.value)
			{
				_takeShot = true;
			}
			
			if (settings._noLock.value)
			{
				_takeLock = true;
			}
			
			mat.SetVector(s_GridMad, _gridMad);
			
            var pix = new Vector4(aspect * _crushPix, _crushPix) * _crushLerp.Evaluate(settings._crush.value);
			if (settings._crush.value <= 0f)
				pix = new Vector4(10000, 10000);
			
			mat.SetVector(s_Grid, pix);
			
			return true;
        }

        public override void Init(VolFx.InitApi initApi)
        {
            initApi.Allocate(_lockTex, Screen.width, Screen.height, GraphicsFormat.R8G8B8A8_UNorm);
            initApi.Allocate(_shotTex, Screen.width, Screen.height, GraphicsFormat.R8G8B8A8_UNorm);
		}
		
		public override void Invoke(RTHandle source, RTHandle dest, VolFx.CallApi callApi)
		{
            // warmup textures allocation to avoid bиg on a new unity versions
            _warmup ++;
            if (_warmup < 2)
			{
				callApi.Blit(source, dest);
				return;
			}
			
			if (callApi.CamType == CameraType.Game)
			{ 
/*				var desc = callData._camDesc;
				desc.colorFormat        = RenderTextureFormat.ARGB32;
				desc.depthStencilFormat = GraphicsFormat.None;
				desc.autoGenerateMips   = false;
				desc.useMipMap          = false;
				desc.depthBufferBits    = 0;
				desc.msaaSamples        = 1;
*/
				//_shotTex.Get(cmd, in desc);
				if (_takeShot)
				{
					callApi.Blit(source, _shotTex.Handle, _material, 1);
					//Utils.Blit(cmd, source, _shotTex.Handle, _material, 1);
					_takeShot = false;
				}
				//cmd.SetGlobalTexture(s_ShotTex, _shotTex.Handle.nameID);
				callApi.Mat.SetTexture(s_ShotTex, _shotTex.Handle);

				//_lockTex.Get(cmd, in desc);
				if (_takeLock)
				{
					callApi.Blit(source, _lockTex.Handle, _material, 1);
					//Utils.Blit(cmd, source, _lockTex.Handle, _material, 1);
					_takeLock = false;
				}
				//cmd.SetGlobalTexture(s_LockTex, _lockTex.Handle.nameID);
				callApi.Mat.SetTexture(s_LockTex, _lockTex.Handle);
			}
			
			base.Invoke(source, dest, callApi);
		}
		/*public override void Invoke(CommandBuffer cmd, RTHandle source, RTHandle dest, VolFx.PassExecution.CallData callData)
		{
			if (callData._cam.cameraType == CameraType.Game)
			{ 
				var desc = callData._camDesc;
				desc.colorFormat        = RenderTextureFormat.ARGB32;
				desc.depthStencilFormat = GraphicsFormat.None;
				desc.autoGenerateMips   = false;
				desc.useMipMap          = false;
				desc.depthBufferBits    = 0;
				desc.msaaSamples        = 1;

				_shotTex.Get(cmd, in desc);
				if (_takeShot)
				{
					Utils.Blit(cmd, source, _shotTex.Handle, _material, 1);
					_takeShot = false;
				}
				cmd.SetGlobalTexture(s_ShotTex, _shotTex.Handle.nameID);

				_lockTex.Get(cmd, in desc);
				if (_takeLock)
				{
					Utils.Blit(cmd, source, _lockTex.Handle, _material, 1);
					_takeLock = false;
				}
				cmd.SetGlobalTexture(s_LockTex, _lockTex.Handle.nameID);
			}
			
			base.Invoke(cmd, source, dest, callData);
		}*/

		// =======================================================================
        protected override bool _editorValidate => _clipTex == null || _clipTex.Length == 0 || (Application.isPlaying == false && _clipTex.Any(n => n == null));
        protected override void _editorSetup(string folder, string asset)
        {
#if UNITY_EDITOR
			var sep = Path.DirectorySeparatorChar;
			
			_clipTex = UnityEditor.AssetDatabase.FindAssets("t:texture", new string[] {$"{folder}{sep}Screen"})
							   .Select(n => UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(n)))
							   .Where(n => n != null)
							   .ToArray();
#endif
        }
    }
}