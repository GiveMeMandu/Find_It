using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class CalendarViewModel : BaseViewModel
    {
        private string _yearNum;

        [Binding]
        public string YearNum
        {
            get => _yearNum;
            set
            {
                _yearNum = value;
                OnPropertyChanged(nameof(YearNum));
            }
        }
        private string _monthNum;

        [Binding]
        public string MonthNum
        {
            get => _monthNum;
            set
            {
                _monthNum = value;
                OnPropertyChanged(nameof(MonthNum));
            }
        }
        private Vector2 _spacing;

        [Binding]
        public Vector2 Spacing
        {
            get => _spacing;
            set
            {
                _spacing = value;
                OnPropertyChanged(nameof(Spacing));
            }
        }
    }
}