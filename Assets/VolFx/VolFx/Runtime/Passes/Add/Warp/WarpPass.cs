using UnityEngine;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Warp")]
    public class WarpPass : VolFx.Pass
    {
		private static readonly int s_Color        = Shader.PropertyToID("_Color");
		private static readonly int s_NoiseTex     = Shader.PropertyToID("_NoiseTex");
		private static readonly int s_WarpDataA    = Shader.PropertyToID("_WarpDataA");
		private static readonly int s_WarpDataB    = Shader.PropertyToID("_WarpDataB");
		private static readonly int s_WarpDataC    = Shader.PropertyToID("_WarpDataC");
		private static readonly int s_GradTex      = Shader.PropertyToID("_GradTex");

		private static  Texture2D _noiseTex;
		
		public override string    ShaderName => string.Empty;

		private Vector2 _count     = new Vector2(0, 300f);
		public  float   _maskPower = 2f;

		private Texture2D _gradTex;
		
		protected override bool Invert => true;
		
        // =======================================================================
		public override void Init()
		{
			if(_noiseTex == null)
				_noiseTex = NoiseTexGenerator.PerlinTex(2048, 256);
		}

		public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<WarpVol>();

            if (settings.IsActive() == false)				
				return false;

			var tiling = _count.x + _count.y * settings._count.value;
			
			var warpDataA = new Vector4(
				settings._size.value,        // _RadialScale
				tiling,                      // _Tiling
				settings._speed.value,       // _Animation
				settings._density.value      // _Power
			);
			
			var warpDataB = new Vector4(
				1f - settings._intensity.value,  // _Remap
				settings._depth.value,           // _MaskScale
				settings._hardness.value,        // _MaskHardness
				_maskPower                       // _MaskPower
			);

			var warpDataC = new Vector4(
				0,
				0,
				-settings._distort.value,
				0
			);
			
			settings._color.value.GetTexture(ref _gradTex);
		
			mat.SetColor(s_Color, settings._emission.value);
			mat.SetVector(s_WarpDataA, warpDataA);
			mat.SetVector(s_WarpDataB, warpDataB);
			mat.SetVector(s_WarpDataC, warpDataC);
			
			mat.SetTexture(s_NoiseTex, _noiseTex);
			mat.SetTexture(s_GradTex, _gradTex);

			return true;
        }
		
		public static class NoiseTexGenerator
		{
		    public static Texture2D SimplexTex(int size = 256, float radialScale = 2.0f, float tiling = 1.0f)
		    {
		        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
		        {
		            wrapMode = TextureWrapMode.Repeat,
		            filterMode = FilterMode.Bilinear
		        };

		        var pixels = new Color[size * size];

		        for (var y = 0; y < size; y++)
		        for (var x = 0; x < size; x++)
		        {
		            var uv = new Vector2((float)x / size, (float)y / size);
		            var centered = uv - new Vector2(0.5f, 0.5f);

		            var polarX = centered.magnitude * radialScale * 2.0f;
		            var polarY = Mathf.Atan2(centered.x, centered.y) / (2 * Mathf.PI) * tiling;

		            var sample = new Vector2(polarX, polarY);
		            var n = Snoise(sample);
		            var v = (n + 1f) * 0.5f;

		            pixels[y * size + x] = new Color(v, 0f, 0f, 1f);
		        }

		        tex.SetPixels(pixels);
		        tex.Apply();
		        return tex;
		    }

			public static Texture2D PerlinTex(int size, float scale)
			{
			    var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
			    tex.wrapMode = TextureWrapMode.Repeat;

			    for (var y = 0; y < size; y++)
			    {
			        for (var x = 0; x < size; x++)
			        {
			            var u = (float)x / size * scale;
			            var v = (float)y / size * scale;
			            var n = Mathf.PerlinNoise(u, v);
			            tex.SetPixel(x, y, new Color(n, 0, 0, 1)); // only R used
			        }
			    }

			    tex.Apply();
			    return tex;
			}

			
		    static float Snoise(Vector2 v)
		    {
		        var C = new Vector4(0.211324865f, 0.366025403f, -0.577350269f, 0.0243902439f);
		        var i = Floor(v + Vector2.Dot(v, new Vector2(C.y, C.y)) * Vector2.one);
		        var x0 = v - i + Vector2.Dot(i, new Vector2(C.x, C.x)) * Vector2.one;

		        var i1 = (x0.x > x0.y) ? new Vector2(1, 0) : new Vector2(0, 1);
		        var x12 = new Vector4(
		            x0.x - i1.x + C.x,
		            x0.y - i1.y + C.x,
		            x0.x - 1.0f + 2.0f * C.x,
		            x0.y - 1.0f + 2.0f * C.x
		        );

		        var ixy = Mod289(new Vector3(i.x, i.y, i.y));
		        var p = Permute(Permute(new Vector3(ixy.y, ixy.y + i1.y, ixy.y + 1.0f)) + new Vector3(ixy.x, ixy.x + i1.x, ixy.x + 1.0f));

		        var m = new Vector3(
		            0.5f - Vector2.Dot(x0, x0),
		            0.5f - (x12.x * x12.x + x12.y * x12.y),
		            0.5f - (x12.z * x12.z + x12.w * x12.w)
		        );
		        m = Vector3.Max(Vector3.zero, m);
		        m.x = m.x * m.x;
		        m.y = m.y * m.y;
		        m.z = m.z * m.z;

		        var x = 2.0f * Frac(p * C.w) - Vector3.one;
		        var h = Abs(x) - new Vector3(0.5f, 0.5f, 0.5f);
		        var ox = Floor(x + new Vector3(0.5f, 0.5f, 0.5f));
		        var a0 = x - ox;

		        var g = new Vector3(
		            a0.x * x0.x + h.x * x0.y,
		            a0.y * x12.x + h.y * x12.y,
		            a0.z * x12.z + h.z * x12.w
		        );

		        return 130.0f * Vector3.Dot(m, g);
		    }

		    static Vector2 Floor(Vector2 v) => new(Mathf.Floor(v.x), Mathf.Floor(v.y));
		    static Vector3 Floor(Vector3 v) => new(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
		    static Vector3 Frac(Vector3 v) => v - Floor(v);
		    static Vector3 Abs(Vector3 v)
			{
				v.x = Mathf.Abs(v.x);
				v.y = Mathf.Abs(v.y);
				v.z = Mathf.Abs(v.z);
				
				return v;
			}

			static Vector3 Mod289(Vector3 x) => x - Floor(x / 289.0f) * 289.0f;
		    static Vector3 Permute(Vector3 x)
			{
				x.x = (x.x * 34.0f + 1.0f) * x.x;
				x.y = (x.y * 34.0f + 1.0f) * x.y;
				x.z = (x.z * 34.0f + 1.0f) * x.z;
				
				return Mod289(x);
			}
		}
	}
}