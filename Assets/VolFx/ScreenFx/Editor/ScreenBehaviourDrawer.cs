using UnityEditor;
using UnityEngine;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx.Editor
{
    [CustomPropertyDrawer(typeof(ScreenBehaviour))]
    public class ScreenBehaviourDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var lineHeight = EditorGUIUtility.singleLineHeight + 2f;
            var lines = 2; // _color, _sprite

            var spriteProp = property.FindPropertyRelative(nameof(ScreenBehaviour._sprite));
            var spriteType = (ScreenBehaviour.SpriteSource)spriteProp.enumValueIndex;

            if (spriteType == ScreenBehaviour.SpriteSource.Image)
                lines += 2; // _image + _scale
            else if (spriteType == ScreenBehaviour.SpriteSource.GradHor ||
                     spriteType == ScreenBehaviour.SpriteSource.GradVert ||
                     spriteType == ScreenBehaviour.SpriteSource.GradCircle)
                lines += 1; // _gradient
            else if (spriteType == ScreenBehaviour.SpriteSource.ScreenShot)
                lines += 1; // _scale для ScreenShot

            return lineHeight * lines;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var lineHeight = EditorGUIUtility.singleLineHeight + 2f;
            var rect       = new Rect(position.x, position.y, position.width, lineHeight);

            // _color
            var colorProp = property.FindPropertyRelative(nameof(ScreenBehaviour._color));
            EditorGUI.PropertyField(rect, colorProp);
            rect.y += lineHeight;

            // _sprite
            var spriteProp = property.FindPropertyRelative(nameof(ScreenBehaviour._sprite));
            EditorGUI.PropertyField(rect, spriteProp, new GUIContent("View"));
            rect.y += lineHeight;

            var spriteType = (ScreenBehaviour.SpriteSource)spriteProp.enumValueIndex;

            if (spriteType == ScreenBehaviour.SpriteSource.Image)
            {
                var imageProp = property.FindPropertyRelative(nameof(ScreenBehaviour._image));
                EditorGUI.PropertyField(rect, imageProp);
                rect.y += lineHeight;

                var scaleProp = property.FindPropertyRelative(nameof(ScreenBehaviour._scale));
                EditorGUI.PropertyField(rect, scaleProp);
                rect.y += lineHeight;
            }
            else if (spriteType == ScreenBehaviour.SpriteSource.GradHor ||
                     spriteType == ScreenBehaviour.SpriteSource.GradVert ||
                     spriteType == ScreenBehaviour.SpriteSource.GradCircle)
            {
                var gradProp = property.FindPropertyRelative(nameof(ScreenBehaviour._gradient));
                EditorGUI.PropertyField(rect, gradProp);
                rect.y += lineHeight;
            }
            else if (spriteType == ScreenBehaviour.SpriteSource.ScreenShot)
            {
                var scaleProp = property.FindPropertyRelative(nameof(ScreenBehaviour._scale));
                EditorGUI.PropertyField(rect, scaleProp);
                rect.y += lineHeight;
            }

            EditorGUI.EndProperty();
        }
    }
}
