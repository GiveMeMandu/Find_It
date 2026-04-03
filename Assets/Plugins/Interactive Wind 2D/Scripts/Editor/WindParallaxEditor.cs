using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InteractiveWind2D
{
    [CustomEditor(typeof(WindParallax))]
    public class WindParallaxEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;

            EditorGUILayout.LabelField("This component fixes <b>parallax</b> issues for the <b>Wind</b> shader.", style);
            EditorGUILayout.LabelField("Attach this to your wind <b>Sprite Renderers</b> and enable <b>Is Parallax</b>.", style);
        }
    }
}