using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine.UI;
using UnityWeld.Binding;


namespace UI
{
    public class UIPartHelper_Button : UIPartHelper
    {
#if UNITY_EDITOR
        [Button("바인딩 - 버튼")]
        public void AddTextBinder()
        {
            var newComponent = gameObject.AddComponent<EventBinding>();
            newComponent.ViewEventName = $"{nameof(UnityEngine)}.{nameof(UnityEngine.UI)}.{nameof(Button)}.{nameof(Button.onClick)}";
        }
#endif
    }
}