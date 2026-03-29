using System;
using UnityEngine;
using I2.Loc;
using UnityEngine.Events;

namespace UI
{
    public class TabButton : MonoBehaviour
    {
        [Header("Components")]
        public UnSelectableButton button;
        public GameObject selectedHighlight;
        public bool isSetSiblingIndex;

        private Localize[] _labelTexts;
        private Action _onClick;
        public UnityEvent OnSelected;
        public UnityEvent OnDeselected;
        public UnityEvent OnDeselectedLeft;  // 선택된 탭 기준 왼쪽에 깔렸을 때 호출
        public UnityEvent OnDeselectedRight; // 선택된 탭 기준 오른쪽에 깔렸을 때 호출

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
                button.onClick.AddListener(() => {
                    _onClick?.Invoke();
                });
            }
        }

        public void SetState(bool selected, bool isLeftOfSelected)
        {
            if (selectedHighlight != null)
                selectedHighlight.SetActive(selected);

            if (button != null)
                button.enabled = !selected;

            if (selected)
                OnSelected?.Invoke();
            else
            {
                OnDeselected?.Invoke();
                if (isLeftOfSelected)
                    OnDeselectedLeft?.Invoke();
                else
                    OnDeselectedRight?.Invoke();
            }
        }
    }
}
