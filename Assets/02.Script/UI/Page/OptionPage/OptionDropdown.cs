using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;
using UnityEngine.Events;

namespace OptionPageNamespace
{
	public class OptionDropdown : MonoBehaviour
	{
		public TMP_Dropdown dropdown;
		public Localize labelText;
		public UnityEvent _onValueChangedUnityEvent;
		[Header("Localization")]
		[Tooltip("폰트를 변경할 I2 Term Key를 입력하세요")]
		[TermsPopup]
		public string fontTerm = "Gumi Romance SDF";
		
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
			
			// 1. 인스펙터에 지정한 Term Key를 이용해 폰트 에셋을 가져옵니다.
			TMP_FontAsset targetFont = null;
			if (!string.IsNullOrEmpty(fontTerm))
			{
				targetFont = LocalizationManager.GetTranslatedObjectByTermName<TMP_FontAsset>(fontTerm);
			}

			if (options != null)
			{
				// 2. 드롭다운 본체의 폰트 설정 (선택된 항목 표시용)
				if (targetFont != null && dropdown.captionText != null)
				{
					dropdown.captionText.font = targetFont;
				}

				// 3. 드롭다운 리스트 아이템들의 폰트 설정 (중요: 리스트가 펼쳐졌을 때의 폰트)
				if (targetFont != null && dropdown.itemText != null)
				{
					dropdown.itemText.font = targetFont;
				}

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
			_onValueChangedUnityEvent?.Invoke();
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
