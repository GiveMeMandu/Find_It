using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InteractiveWind2D
{
    [CustomEditor(typeof(ShaderInstancer))]
    public class ShaderInstancerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;

            EditorGUILayout.LabelField("Some shaders require a <b>unique material instance</b> per object.", style);
            EditorGUILayout.LabelField("Changing or referencing the material at runtime automatically fixes the issue.", style);
            EditorGUILayout.LabelField("This component will <b>instantiate</b> any material it is <b>attached</b> to.", style);
            EditorGUILayout.LabelField(" ", style);
            EditorGUILayout.LabelField("<b>Where is this needed:</b>", style);
            EditorGUILayout.LabelField("- Shaders with <b>Object</b> shader space.", style);
            EditorGUILayout.LabelField("- <b>Wind Shaders</b> if no other script is attached to the <b>Sprite Renderer</b>.", style);
        }
    }
}