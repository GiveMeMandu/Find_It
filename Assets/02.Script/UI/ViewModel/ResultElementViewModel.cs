using UnityEngine;
using UnityEngine.UI;
using UnityWeld.Binding;

namespace UI
{
    /// <summary>
    /// GameEndViewModel의 GroupView에서 사용되는 범용 결과 아이템 ViewModel입니다.
    /// 스티커(컬렉션), 코인, 기타 보상 등 다양한 결과 아이템을 표시할 수 있습니다.
    /// </summary>
    [Binding]
    public class ResultElementViewModel : UnityWeld.ViewModel
    {
        private Sprite _icon;
        [Binding]
        public Sprite Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        private string _displayName;
        [Binding]
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        private int _count;
        [Binding]
        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(CountText));
                OnPropertyChanged(nameof(ShowCount));
            }
        }

        /// <summary>
        /// 수량 표시 텍스트 (예: "x3")
        /// </summary>
        [Binding]
        public string CountText => Count > 1 ? $"x{Count}" : "";

        /// <summary>
        /// 수량이 2 이상일 때만 표시
        /// </summary>
        [Binding]
        public bool ShowCount => Count > 1;

        /// <summary>
        /// 범용 초기화 메서드. 아이콘, 이름, 수량을 직접 지정합니다.
        /// </summary>
        public void Init(Sprite icon, string displayName, int count = 1)
        {
            Icon = icon;
            DisplayName = displayName;
            Count = count;
        }
    }
}
