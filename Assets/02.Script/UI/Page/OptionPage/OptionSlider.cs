using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using I2.Loc;

namespace OptionPageNamespace
{
    public class OptionSlider : MonoBehaviour
    {
        public Slider slider;
        public Localize labelText;
        public TextMeshProUGUI valueText;

        private Action<int> _onValueChanged;

        public void Init(string label, int initialValue, Action<int> onValueChanged)
        {
            // I2.Loc을 사용하여 로컬라이즈된 텍스트로 설정
            labelText.Term = label;
            _onValueChanged = onValueChanged;

            // 슬라이더 범위 설정 (0-1)
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;

            // 초기값 설정 (0-100을 0-1로 변환)
            slider.value = initialValue / 100f;

            // 값 표시 업데이트
            UpdateValueText(initialValue);

            // 슬라이더 값 변경 이벤트 리스너 등록
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            // 0-1 범위를 0-100 정수로 변환
            int intValue = Mathf.RoundToInt(value * 100f);
            
            // 값 표시 업데이트
            UpdateValueText(intValue);

            // 콜백 호출
            _onValueChanged?.Invoke(intValue);
        }

        private void UpdateValueText(int value)
        {
            valueText.text = value.ToString();
        }

        private void OnDestroy()
        {
            // 이벤트 리스너 제거
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }
}
