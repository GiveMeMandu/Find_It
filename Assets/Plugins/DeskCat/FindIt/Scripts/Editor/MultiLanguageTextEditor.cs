using DeskCat.FindIt.Scripts.Core.Main;
using DeskCat.FindIt.Scripts.Core.Main.Utility.Tracker;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEditor;
using UnityEngine;

namespace DeskCat.FindIt.Scripts.Editor
{
    [CustomEditor(typeof(MultiLanguageTextTracker))]
    public class MultiLanguageTextEditor : UnityEditor.Editor
    {
        private MultiLanguageTextTracker HiddenObjTarget;

        private void OnEnable()
        {
            HiddenObjTarget = (MultiLanguageTextTracker) target;
        }

        public override void OnInspectorGUI()
        {
            
            if (GUILayout.Button("Auto Generate Key List"))
            {
                HiddenObjTarget.LanguageValue = GlobalSetting.GetDefaultLanguageKey<MultiLanguageTextModel>();
            }
            
            base.OnInspectorGUI();

        }
    }
}
