using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityWeld.Binding;

namespace UI
{
    public class UIPartHelper_Text : UIPartHelper
    {
        [Button("바인딩 - Text")]
        public void AddTextBinder()
        {
            string targetPropertyName = $"{nameof(TMPro)}.{nameof(TextMeshProUGUI)}.{nameof(TextMeshProUGUI.text)}";
            var newComponent = gameObject.AddComponent<OneWayPropertyBinding>();
            newComponent.ViewPropertyName = targetPropertyName;
        }

        [Button("바인딩 - Text Color")]
        public void AddTextColorBinder()
        {
            string targetPropertyName = $"{nameof(TMPro)}.{nameof(TextMeshProUGUI)}.{nameof(TextMeshProUGUI.color)}";
            var newComponent = gameObject.AddComponent<OneWayPropertyBinding>();
            newComponent.ViewPropertyName = targetPropertyName;
        }
    }
}