using DeskCat.FindIt.Scripts.Core.Main.System;
using UnityEditor;
using UnityEngine;

namespace DeskCat.FindIt.Scripts.Editor
{
    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerEditor : UnityEditor.Editor
    {
        private LevelManager LevelManagerTarget;
        private void OnEnable()
        {
            LevelManagerTarget = (LevelManager)target;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Hide/Show Canvas On Editor"))
            {
                LevelManagerTarget.Canvas.SetActive(!LevelManagerTarget.Canvas.activeSelf);
            }
            
            base.OnInspectorGUI();
        }

    }
}