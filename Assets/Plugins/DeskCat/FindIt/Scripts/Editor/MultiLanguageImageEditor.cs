using DeskCat.FindIt.Scripts.Core.Main;
using DeskCat.FindIt.Scripts.Core.Main.Utility.Tracker;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEditor;
using UnityEngine;

namespace DeskCat.FindIt.Scripts.Editor
{
    [CustomEditor(typeof(MultiLanguageImageTracker))]
    public class MultiLanguageImageEditor : UnityEditor.Editor
    {
        private MultiLanguageImageTracker HiddenObjTarget;

        private void OnEnable()
        {
            HiddenObjTarget = (MultiLanguageImageTracker) target;
        }

        public override void OnInspectorGUI()
        {
            
            if (GUILayout.Button("Auto Generate Key List"))
            {
                HiddenObjTarget.LanguageValue = GlobalSetting.GetDefaultLanguageKey<MultiLanguageImageModel>();
            }
            
            base.OnInspectorGUI();

        }
    }
}
