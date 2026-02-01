using UnityEditor;
using UnityEngine;

namespace VolFx.Editor
{
    [CustomPropertyDrawer(typeof(AnimationCurveRange), true)]
    public class AnimationCurveRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.CurveField(position, property.FindPropertyRelative(nameof(AnimationCurveRange._curve)), Color.green, new Rect(0, 0, 1, 1), label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}