using DeskCat.FindIt.Scripts.Core.Main.System;
using UnityEditor;
using UnityEngine;
using NaughtyAttributes.Editor;

namespace DeskCat.FindIt.Scripts.Editor
{
    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerEditor : NaughtyInspector
    {
        private LevelManager LevelManagerTarget;
        
        protected override void OnEnable()
        {
            base.OnEnable();
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