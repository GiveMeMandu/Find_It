using UnityEditor;
using UnityEngine;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx.Editor
{
    [CustomPropertyDrawer(typeof(DitherGradientValue), true)]
    public class DitherGradientValueDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return UnityEditor.EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var grad = property.FindPropertyRelative("_grad");
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, grad, label);
            if (EditorGUI.EndChangeCheck())
            {
                // rebuild pixels
                var colorPix  = property.FindPropertyRelative("_colorPixels");
                var ditherPix = property.FindPropertyRelative("_ditherPixels");
                var val       = _getGradient(grad);
                
                // hardlock to fixed
                if (val.mode != GradientMode.Fixed)
                {
                    val.mode = GradientMode.Fixed;
                    
#if UNITY_2022_1_OR_NEWER
                    grad.gradientValue = val;
#else
                    var pi = typeof(SerializedProperty).GetProperty("gradientValue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    pi?.SetValue(grad, locked, null);
#endif
                }
                
                // build dither gradient
                var ditherGrad = new Gradient();
                ditherGrad.mode = val.mode;

                var srcKeys = val.colorKeys;
                if (srcKeys.Length < 2)
                {
                    var col = srcKeys.Length == 1 ? srcKeys[0].color : Color.black;
                    var fallback = new[]
                    {
                        new GradientColorKey(col, 0f),
                        new GradientColorKey(col, 1f)
                    };
                    ditherGrad.SetKeys(fallback, val.alphaKeys);
                }
                else
                {
                    var keys = new GradientColorKey[srcKeys.Length - 1];

                    for (var i = 1; i < srcKeys.Length; i++)
                    {
                        var col  = srcKeys[i - 1].color;
                        var time = srcKeys[i].time;
                        keys[i - 1] = new GradientColorKey(col, time);
                    }

                    ditherGrad.SetKeys(keys, val.alphaKeys);
                }
                
                // evaluate pixels
                for (var n = 0; n < DitherGradientValue.k_Width; n++)
                {
                    colorPix.GetArrayElementAtIndex(n).colorValue  = val.Evaluate(n / (float)(DitherGradientValue.k_Width - 1));
                    ditherPix.GetArrayElementAtIndex(n).colorValue = _ditherColor(n / (float)(DitherGradientValue.k_Width - 1));
                }

                // -----------------------------------------------------------------------
                Color _ditherColor(float t)
                {
                    var col  = ditherGrad.Evaluate(t);
                    var keys = val.colorKeys;

                    col.a = 1f - _localMeasure();
                    return col;
                    
                    // -----------------------------------------------------------------------
                    float _localMeasure()
                    {
                        if (t <= keys[0].time)
                        {
                            var range = keys[0].time;
                            if (range <= 0f) return 0f;
                            return Mathf.Clamp01(t / range);
                        }

                        for (var i = 1; i < keys.Length; i++)
                        {
                            var prev = keys[i - 1];
                            var next = keys[i];

                            if (t <= next.time)
                            {
                                var range = next.time - prev.time;
                                if (range <= 0f) return 0f;
                                return Mathf.Clamp01((t - prev.time) / range);
                            }
                        }

                        return 1f;
                    }
                }
            }

            // =======================================================================
            Gradient _getGradient(SerializedProperty gradientProperty)
            {
#if UNITY_2022_1_OR_NEWER
                return grad.gradientValue;
#else
                System.Reflection.PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty("gradientValue",
                                                                                                     System.Reflection.BindingFlags.Public |
                                                                                                     System.Reflection.BindingFlags.NonPublic |
                                                                                                     System.Reflection.BindingFlags.Instance);
                
                return propertyInfo.GetValue(gradientProperty, null) as Gradient;
#endif
            }
        }
    }
}