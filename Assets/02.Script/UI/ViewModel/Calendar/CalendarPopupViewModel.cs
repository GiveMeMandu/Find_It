using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class CalendarPopupViewModel : ViewModel
    {

        [Sirenix.OdinInspector.LabelText("활성화할 날짜 정보 팝업")]
        [SerializeField] private GameObject InfoPanel;
        //* 날짜 정보 팝업

        [Binding]
        public void ShowDayInfo()
        {
            InfoPanel.SetActive(true);
        }

        [Binding]
        public void HideDayInfo()
        {
            InfoPanel.SetActive(false);
        }

        private string _dayName;

        [Binding]
        public string DayName
        {
            get => _dayName;
            set
            {
                _dayName = value;
                OnPropertyChanged(nameof(DayName));
            }
        }
        private string _dayInfo;

        [Binding]
        public string DayInfo
        {
            get => _dayInfo;
            set
            {
                _dayInfo = value;
                OnPropertyChanged(nameof(DayInfo));
            }
        }
    }
}