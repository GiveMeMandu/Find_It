using System;
using UnityEngine;
using I2.Loc;
using UI;

namespace OptionPageNamespace
{
    public class OptionButton : MonoBehaviour
    {
        public Localize labelText;
        public UnSelectableButton button;

        private Action _onClick;

        public void Init(string label, Action onClick)
        {
            // I2.Loc을 사용하여 로컬라이즈된 텍스트로 설정
            if (labelText != null)
            {
                labelText.Term = label;
            }

            _onClick = onClick;

            // 버튼 클릭 이벤트 리스너 등록
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
                button.onClick.AddListener(OnButtonClick);
            }
        }

        private void OnButtonClick()
        {
            _onClick?.Invoke();
        }

        private void OnDestroy()
        {
            // 이벤트 리스너 제거
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }
        }
    }
}
