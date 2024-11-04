using Manager;
using System;
using System.Collections.Generic;
using UnityWeld;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class DailyRewardPage : PageViewModel
    {
        private int _score;

        [Binding]
        public int Score // 이점수는 페이지안의 텍스트가 참조하고 있음 그래서 얘가 바뀔 때마다 텍스트 숫자도 바뀜
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged(nameof(Score));
            }
        }
    }
}