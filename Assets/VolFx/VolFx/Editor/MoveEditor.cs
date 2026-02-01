using UnityEditor;
using UnityEngine;

namespace VolFx
{
    [CustomEditor(typeof(Move))]
    public class MoveEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var move = (Move)target;
            
            EditorGUILayout.Space();
            
            // disable button outside play mode
            GUI.enabled = Application.isPlaying;

            // draw button with tooltip
            var buttonStyle = new GUIContent("Play");
            if (GUILayout.Button(buttonStyle))
                move.Play();

            // restore GUI state
            GUI.enabled = true;
        }
    }
}