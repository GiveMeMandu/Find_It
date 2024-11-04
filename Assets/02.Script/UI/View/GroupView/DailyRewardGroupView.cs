using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using Sirenix.OdinInspector;
using UnityWeld.Binding;


namespace UnityWeld
{
    [Binding]
    public class DailyRewardGroupView : GroupView
    {
        private string _timeLeft;

        [Binding]
        public string TimeLeft
        {
            get => _timeLeft;
            set
            {
                _timeLeft = value;
                OnPropertyChanged(nameof(TimeLeft));
            }
        }
        [LabelText("출석 보상들")] public List<DailyRewardView> dailyRewardViews;
        private void OnEnable()
        {
            Global.DailyCheckManager.OnDayChanged += OnDayChanged;
            Global.DailyCheckManager.OnTimeLeftChanged += OnTimeLeftChanged;
        }

        private void OnDisable()
        {
            Global.DailyCheckManager.OnDayChanged -= OnDayChanged;
            Global.DailyCheckManager.OnTimeLeftChanged -= OnTimeLeftChanged;
        }
        public void ClaimRewardAll(int day)
        {
            for (int i = 0; i < day; i++)
            {
                dailyRewardViews[i].ClaimReward();
            }
        }
        private void OnTimeLeftChanged(object sender, TimeSpan e)
        {
            // Format TimeLeft as "D.HH:MM:SS" without fractional seconds
            TimeLeft = string.Format("{0:D2}:{1:D2}:{2:D2}", e.Hours, e.Minutes, e.Seconds);
        }

        private void OnDayChanged(object sender, DateTime e) => Refresh();

        private void Refresh()
        {
            foreach(var d in dailyRewardViews) {
                d.Refresh();
            }
        }
    }
}