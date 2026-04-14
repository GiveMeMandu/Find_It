using System;
using TMPro;
using UnityEngine;
using I2.Loc;
using UI;

namespace OptionPageNamespace
{
    public class OptionGamepadBinding : MonoBehaviour
    {
        public Localize labelText;
        public TextMeshProUGUI keyText;
        public UnSelectableButton bindingButton;

        private Action _onClick;

        public void Init(string label, string shortDesc, Action onClick)
        {
            // I2.Loc을 사용하여 로컬라이즈된 텍스트로 설정
            if (labelText != null)
            {
                labelText.SetTerm(label);
            }

            _onClick = onClick;

            UpdateKeyText(shortDesc);

            // 버튼 클릭 이벤트 리스너 등록
            if (bindingButton != null)
            {
                bindingButton.onClick.RemoveListener(OnBindingClick);
                bindingButton.onClick.AddListener(OnBindingClick);
            }
        }

        public void UpdateKeyText(string text)
        {
            if (keyText != null)
            {
                // 텍스트 그대로 출력 (스프라이트 태그 포함)
                keyText.text = text;
            }
        }

        private void OnBindingClick()
        {
            _onClick?.Invoke();
        }

        private void OnDestroy()
        {
            // 이벤트 리스너 제거
            if (bindingButton != null)
            {
                bindingButton.onClick.RemoveListener(OnBindingClick);
            }
        }
    }
}
