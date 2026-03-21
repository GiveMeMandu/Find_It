using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;
using UI;

namespace OptionPageNamespace
{
	public class OptionStepper : MonoBehaviour
	{
		public UnSelectableButton decrementButton;
		public UnSelectableButton incrementButton;
		public Localize labelText;
		public TextMeshProUGUI valueText;

		private Action<int> _onValueChanged;
		private int _minValue;
		private int _maxValue;
		private int _step;
		private int _currentValue;

		public void Init(string label, int initialValue, int minValue, int maxValue, int step, Action<int> onValueChanged)
		{
			// I2.Loc을 사용하여 로컬라이즈된 텍스트로 설정
			labelText.Term = label;

			_minValue = minValue;
			_maxValue = maxValue;
			_step = Mathf.Max(1, step);
			_onValueChanged = onValueChanged;

			decrementButton.onClick.RemoveListener(OnClickDecrement);
			incrementButton.onClick.RemoveListener(OnClickIncrement);

			SetValue(initialValue, notify: false);

			// 버튼 클릭 이벤트 리스너 등록
			decrementButton.onClick.AddListener(OnClickDecrement);
			incrementButton.onClick.AddListener(OnClickIncrement);
		}

		private void OnClickDecrement()
		{
			SetValue(_currentValue - _step, notify: true);
		}

		private void OnClickIncrement()
		{
			SetValue(_currentValue + _step, notify: true);
		}

		private void SetValue(int value, bool notify)
		{
			if (_minValue > _maxValue)
			{
				int temp = _minValue;
				_minValue = _maxValue;
				_maxValue = temp;
			}

			int clampedValue = Mathf.Clamp(value, _minValue, _maxValue);
			_currentValue = clampedValue;

			UpdateValueText(_currentValue);
			UpdateButtonInteractable();

			if (notify)
				_onValueChanged?.Invoke(_currentValue);
		}

		private void UpdateButtonInteractable()
		{
			if (decrementButton != null)
				decrementButton.enabled = _currentValue > _minValue;
			if (incrementButton != null)
				incrementButton.enabled = _currentValue < _maxValue;
		}
    
		private void UpdateValueText(int value)
		{
			if (valueText == null)
				return;

			valueText.text = value.ToString();
		}

		private void OnDestroy()
		{
			// 이벤트 리스너 제거
			decrementButton.onClick.RemoveListener(OnClickDecrement);
			incrementButton.onClick.RemoveListener(OnClickIncrement);
		}
	}
}
