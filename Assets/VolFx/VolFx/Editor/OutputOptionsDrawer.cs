using System;
using UnityEditor;
using UnityEngine;

namespace VolFx.Editor
{
    [CustomPropertyDrawer(typeof(VolFx.OutputOptions))]
    public class OutputOptionsDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var mode = (VolFx.OutputOptions.Output)property.FindPropertyRelative(nameof(VolFx._output._output)).intValue;
            return mode switch
            {
                VolFx.OutputOptions.Output.____      => 1,
                VolFx.OutputOptions.Output.Camera    => 1,
                VolFx.OutputOptions.Output.GlobalTex => 2,
                VolFx.OutputOptions.Output.RenderTex => 2,
                VolFx.OutputOptions.Output.Sprite    => 3,
                _                                    => throw new ArgumentOutOfRangeException()
            } * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var mode = property.FindPropertyRelative(nameof(VolFx._output._output));
            var tex  = property.FindPropertyRelative(nameof(VolFx._output._outputTex));
            var rt   = property.FindPropertyRelative(nameof(VolFx._output._renderTex));
            var so   = property.FindPropertyRelative(nameof(VolFx._output._sortingOrder));
            var cd   = property.FindPropertyRelative(nameof(VolFx._output._camDistance));

            var line = 0;
            EditorGUI.PropertyField(_fieldRect(line ++), mode, label, true);
            EditorGUI.indentLevel ++;
            
            switch ((VolFx.OutputOptions.Output)mode.intValue)
            {
                case VolFx.OutputOptions.Output.____:
                    break;
                case VolFx.OutputOptions.Output.Camera:
                    break;
                case VolFx.OutputOptions.Output.GlobalTex:
                    EditorGUI.PropertyField(_fieldRect(line ++), tex, true);
                    break;
                case VolFx.OutputOptions.Output.RenderTex:
                    EditorGUI.PropertyField(_fieldRect(line ++), rt, true);
                    break;
                case VolFx.OutputOptions.Output.Sprite:
                    EditorGUI.PropertyField(_fieldRect(line ++), so, true);
                    EditorGUI.PropertyField(_fieldRect(line ++), cd, true);
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