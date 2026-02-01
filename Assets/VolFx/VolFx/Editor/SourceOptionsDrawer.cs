using System;
using UnityEditor;
using UnityEngine;

//  VolFx © NullTale - https://x.com/NullTale
namespace VolFx.Editor
{
    [CustomPropertyDrawer(typeof(VolFx.SourceOptions))]
    public class SourceOptionsDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var mode = (VolFx.SourceOptions.Source)property.FindPropertyRelative(nameof(VolFx._source._source)).intValue;
            return mode switch
            {
                VolFx.SourceOptions.Source.Camera    => 1,
                VolFx.SourceOptions.Source.Custom => 2,
                //VolFx.SourceOptions.Source.RenderTex => 2,
                VolFx.SourceOptions.Source.LayerMask => 1, // + ((VolFx.SourceOptions.MaskOutput)property.FindPropertyRelative(nameof(VolFx._source._output)).intValue == VolFx.SourceOptions.MaskOutput.Texture ? 2 : 0),
                VolFx.SourceOptions.Source.Pool      => 2,
                _                                    => throw new ArgumentOutOfRangeException()
            } * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var mode  = property.FindPropertyRelative(nameof(VolFx._source._source));
            var tex   = property.FindPropertyRelative(nameof(VolFx._source._globalTex));
            var rt    = property.FindPropertyRelative(nameof(VolFx._source._renderTex));
            var buf   = property.FindPropertyRelative(nameof(VolFx._source._pool));

            var line = 0;
            EditorGUI.PropertyField(_fieldRect(line ++), mode, label, true);
            EditorGUI.indentLevel ++;
            
            switch ((VolFx.SourceOptions.Source)mode.intValue)
            {
                case VolFx.SourceOptions.Source.Camera:
                    break;
                case VolFx.SourceOptions.Source.Custom:
                    EditorGUI.PropertyField(_fieldRect(line ++), tex, true);
                    break;
                // case VolFx.SourceOptions.Source.RenderTex:
                //     EditorGUI.PropertyField(_fieldRect(line ++), rt, true);
                //     break;
                case VolFx.SourceOptions.Source.LayerMask:
                    break;
                case VolFx.SourceOptions.Source.Pool:
                    EditorGUI.PropertyField(_fieldRect(line ++), buf, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            EditorGUI.indentLevel --;
            
            // -----------------------------------------------------------------------
            Rect _fieldRect(int line)
            {
                return new Rect(position.x, position.y + line * EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            }
        }
    }
}