using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;

namespace OptionPageNamespace
{
	public class OptionDropdown : MonoBehaviour
	{
		public TMP_Dropdown dropdown;
		public Localize labelText;

		private Action<int> _onValueChanged;
		private List<string> _optionKeys;
		private bool _isLocalizedOptions;

		public void Init(string label, IReadOnlyList<string> options, int initialIndex, Action<int> onValueChanged, bool isLocalizedOptions = false)
		{
			// I2.Loc을 사용하여 로컬라이즈된 텍스트로 설정
			labelText.Term = label;
			_onValueChanged = onValueChanged;
			_isLocalizedOptions = isLocalizedOptions;

			if (_isLocalizedOptions)
			{
				_optionKeys = new List<string>(options);
				LocalizationManager.OnLocalizeEvent -= OnLocalize;
				LocalizationManager.OnLocalizeEvent += OnLocalize;
			}
			else
			{
				_optionKeys = null;
				LocalizationManager.OnLocalizeEvent -= OnLocalize;
			}

			dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);

			RefreshOptions(options);

			int clampedIndex = 0;
			if (dropdown.options != null && dropdown.options.Count > 0)
				clampedIndex = Mathf.Clamp(initialIndex, 0, dropdown.options.Count - 1);

			// 값 설정 시 이벤트 발생 방지
			dropdown.SetValueWithoutNotify(clampedIndex);
			dropdown.RefreshShownValue();

			// 드롭다운 값 변경 이벤트 리스너 등록
			dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
		}

		private void RefreshOptions(IReadOnlyList<string> options)
		{
			dropdown.ClearOptions();
			if (options != null)
			{
				List<string> optionList = new List<string>(options.Count);
				for (int i = 0; i < options.Count; i++)
				{
					string text = options[i];
					if (_isLocalizedOptions)
					{
						string translation = LocalizationManager.GetTranslation(text);
						if (!string.IsNullOrEmpty(translation))
							text = translation;
					}
					optionList.Add(text);
				}
				dropdown.AddOptions(optionList);
			}
		}

		private void OnLocalize()
		{
			if (_isLocalizedOptions && _optionKeys != null)
			{
				int currentIndex = dropdown.value;
				RefreshOptions(_optionKeys);
				dropdown.SetValueWithoutNotify(currentIndex);
				dropdown.RefreshShownValue();
			}
		}

		private void OnDropdownValueChanged(int index)
		{
			_onValueChanged?.Invoke(index);
		}

		private void OnDestroy()
		{
			// 이벤트 리스너 제거
			if (dropdown != null)
				dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);

			LocalizationManager.OnLocalizeEvent -= OnLocalize;
		}
	}
}
