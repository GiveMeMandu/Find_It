using System;
using UnityEngine;
using I2.Loc;

namespace UI
{
    public class TabButton : MonoBehaviour
    {
        [Header("Components")]
        public UnSelectableButton button;
        public GameObject selectedHighlight;

        private Localize[] _labelTexts;
        private Action _onClick;

        void Awake()
        {
            _labelTexts = GetComponentsInChildren<Localize>();
        }

        public void Init(string labelTerm, Action onClick)
        {
            if (_labelTexts != null)
            {
                foreach (var labelText in _labelTexts)
                {
                    labelText.Term = labelTerm;
                }
            }

            _onClick = onClick;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => _onClick?.Invoke());
            }
        }

        public void SetSelected(bool selected)
        {
            if (selectedHighlight != null)
                selectedHighlight.SetActive(selected);

            if (button != null)
                button.enabled = !selected;
        }
    }
}
