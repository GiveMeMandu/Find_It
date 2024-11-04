using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using SO;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityWeld;
using UnityWeld.Binding;


namespace UnityWeld
{
    [Binding]
    public class DailyRewardView : ViewModel
    {
        private DailyRewardGroupView dailyRewardGroupView;
        private void OnEnable() {
            dailyRewardGroupView = GetComponentInParent<DailyRewardGroupView>();

        }
        [LabelText("보상 설정")] [SerializeField]
        private DailyRewardSO dailyRewardSO;
        [LabelText("날짜 설정")] [SerializeField]
        private int DayValue;
        [SerializeField] private Button rewardBtn;
        private void Start() {
            Day = string.Format("Day {0}", DayValue);
            Refresh();
        }
        public void Refresh()
        {
            Reward = Global.RewardManager.CalculateRewardByType(dailyRewardSO);
            var status = Global.RewardManager.GetDailyRewardStatus(DayValue - 1);
            CanReward = status.Item1;
            if(status.Item2 == DailyRewardStatus.UNCLAIMED_UNAVAILABLE) rewardBtn.gameObject.SetActive(false);
            else rewardBtn.gameObject.SetActive(true);
        }
        [Binding]
        public void DoClaimReward()
        {
            dailyRewardGroupView.ClaimRewardAll(DayValue - 1);
            ClaimReward();
        }
        public void ClaimReward()
        {
            if(Global.RewardManager.ClaimReward(DayValue - 1, dailyRewardSO)) Refresh();
        }

        private string _day;

        [Binding]
        public string Day
        {
            get => _day;
            set
            {
                _day = value;
                OnPropertyChanged(nameof(Day));
            }
        }

        private bool _canReward;

        [Binding]
        public bool CanReward
        {
            get => _canReward;
            set
            {
                _canReward = value;
                OnPropertyChanged(nameof(CanReward));
            }
        }
        private string _reward;

        [Binding]
        public string Reward
        {
            get => _reward;
            set
            {
                _reward = value;
                OnPropertyChanged(nameof(Reward));
            }
        }
    }
}