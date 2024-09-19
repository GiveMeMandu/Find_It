using DeskCat.FindIt.Scripts.Core.Main;
using DeskCat.FindIt.Scripts.Core.Main.System;
using DeskCat.FindIt.Scripts.Core.Main.Utility.Tracker;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEditor;
using UnityEngine;

namespace DeskCat.FindIt.Scripts.Editor
{
    [CustomEditor(typeof(DialogPanel))]
    public class DialogPanelEditor : UnityEditor.Editor
    {
        private DialogPanel DialogPanelTarget;

        private void OnEnable()
        {
            DialogPanelTarget = (DialogPanel) target;
        }

        public override void OnInspectorGUI()
        {
            
            if (GUILayout.Button("Auto Generate Dialog Key List"))
            {
                DialogPanelTarget.DialogContent = GlobalSetting.GetDefaultLanguageKey<MultiDialogTextListModel>();
            }
            
            base.OnInspectorGUI();

        }
    }
}
