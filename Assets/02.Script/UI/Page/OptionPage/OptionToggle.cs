using System;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;

namespace OptionPageNamespace
{
    public class OptionToggle : MonoBehaviour
    {
        public Toggle toggle;
        public Localize labelText;

        private Action<bool> _onValueChanged;

        public void Init(string label, bool initialValue, Action<bool> onValueChanged)
        {
            // I2.Loc을 사용하여 로컬라이즈된 텍스트로 설정
            labelText.Term = label;
            _onValueChanged = onValueChanged;

            // 초기값 설정
            toggle.SetIsOnWithoutNotify(initialValue);

            // 토글 값 변경 이벤트 리스너 등록
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool value)
        {
            // 콜백 호출
            _onValueChanged?.Invoke(value);
        }

        private void OnDestroy()
        {
            // 이벤트 리스너 제거
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }
}
