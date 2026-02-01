using System;
using System.Collections.Generic;
using UnityEngine;

//  ScreenFx © NullTale - https://twitter.com/NullTale/
namespace ScreenFx
{
    public static class Utils
    {
        private static Sprite s_SpriteClearInstance;
        public static Sprite s_SpriteClear
        {
            get
            {
                if (s_SpriteClearInstance == null)
                {
                    var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false, false);
                    tex.SetPixel(0, 0, Color.clear);
                    tex.Apply();
                    
                    s_SpriteClearInstance = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
                }
                
                return s_SpriteClearInstance;
            }
        }

        // =======================================================================
        public static Vector2 ToNormal(this float rad)
        {
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
        
        public static float Round(this float f)
        {
            return Mathf.Round(f);
        }
        
        public static float Clamp01(this float f)
        {
            return Mathf.Clamp01(f);
        }
        
        public static float OneMinus(this float f)
        {
            return 1f - f;
        }
        
        public static float Remap(this float f, float min, float max)
        {
            return min + (max - min) * f;
        }
        
        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }
        
        public static Vector2 To2DXY(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }
        
        public static Vector3 To3DXZ(this Vector2 vector)
        {
            return vector.To3DXZ(0);
        }
        
        public static Vector3 To3DXZ(this Vector2 vector, float y)
        {
            return new Vector3(vector.x, y, vector.y);
        }

        public static Vector3 To3DXY(this Vector2 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }
        
        public static Vector2 ToVector2XY(this float value)
        {
            return new Vector2(value, value);
        }
        
        public static Color MulA(this Color color, float a)
        {
            return new Color(color.r, color.g, color.b, color.a * a);
        }
        
        public static Rect GetRect(this Texture2D texture)
        {
            return new Rect(0, 0, texture.width, texture.height);
        }
        
        public static int RoundToInt(this float f)
        {
            return Mathf.RoundToInt(f);
        }
        
        public static TKey MaxOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, TSource noOptionsValue = default)
        {
            var result = source.MaxOrDefault(selector, Comparer<TKey>.Default, noOptionsValue);
            if (Equals(result, default))
                return default;
            
            return selector(result);
        }

        public static TSource MaxOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, TSource fallback = default)
        {
            using (var sourceIterator = source.GetEnumerator())
            {
                if (sourceIterator.MoveNext() == false)
                    return fallback;

                var max = sourceIterator.Current;
                var maxKey = selector(max);
	
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);

                    if (comparer.Compare(candidateProjected, maxKey) > 0)
                    {
                        max = candidate;
                        maxKey = candidateProjected;
                    }
                }
                return max;
            }
        }
    }
}