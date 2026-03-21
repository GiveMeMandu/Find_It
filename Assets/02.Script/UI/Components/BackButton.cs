using Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Components
{
    [RequireComponent(typeof(UnSelectableButton))]
    public class BackButton : MonoBehaviour
    {

        protected virtual void Awake()
        {
            var button = GetComponent<UnSelectableButton>();
            button.onClick.AddListener(OnBackButtonClicked);
        }
        protected virtual void OnBackButtonClicked()
        {
            Global.UIManager.EscapePressedThisFrame = true;
        }
    }
}