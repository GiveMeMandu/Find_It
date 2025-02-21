using System;
using System.Collections.Generic;
using UnityEngine;

namespace Manager
{
    public class DailyRewardData
    {
        public DateTime lastRwardUTCTime; //* 마지막 보상 받은 시간
        public int curCountDay;
        public bool needToRestOnNextDay = false;
        public HashSet<int> claimedDays = new HashSet<int>();  // 이미 수령한 날짜들
    }
    public enum DailyRewardStatus
    {
        UNCLAIMED_AVAILABLE,
        CLAIMED,
        UNCLAIMED_UNAVAILABLE
    }

    public partial class RewardManager : MonoBehaviour
    {
        private const int MAX_DAILY_REWARD_DAYS = 6;
        
        private DailyRewardData store;
        void Awake()
        {
            store = Global.UserDataManager.userStorage.dailyRewardData;
        }

        private void OnEnable()
        {
            CheckFirstInit();
            Global.DailyCheckManager.OnInternetTimeChecked += OnInternetTimeChecked;
            Global.DailyCheckManager.SubscribeToDayChanged(OnDayChanged);
        }

        private void CheckFirstInit()
        {
            if(store.lastRwardUTCTime == DateTime.MinValue)
            {
                store.lastRwardUTCTime = Global.DailyCheckManager.GetClientTime();
                SaveStore();
            }
        }

        private void OnInternetTimeChecked(object sender, DateTime e)
        {
            UpdateLoginDate(e);
        }

        private void OnDayChanged(object sender, DateTime e)
        {
            UpdateLoginDate(e);
        }

        public void UpdateLoginDate(DateTime currentUTCTime)
        {
            // 날짜 차이 계산을 더 정확하게 수정
            int dayDiff = (currentUTCTime.Date - store.lastRwardUTCTime.Date).Days;
            Debug.Log($"dayDiff: {dayDiff}");
            
            if (dayDiff > 0)
            {
                if(store.needToRestOnNextDay) 
                {
                    ResetReward();
                    return;
                }

                store.curCountDay += dayDiff;
                SaveStore();
                Debug.Log($"일일보상 갱신: {dayDiff}일 증가");
            }
        }
        public (bool, DailyRewardStatus) GetDailyRewardStatus(int day)
        {
            // 입력된 날이 이미 보상을 수령한 날인지 확인
            if (store.claimedDays.Contains(day))
            {
                return (false, DailyRewardStatus.CLAIMED); // 이미 수령한 날
            }

            // 현재 출석 일수보다 작은 날짜는 수령 가능
            if (day <= store.curCountDay)
            {
                return (true, DailyRewardStatus.UNCLAIMED_AVAILABLE); // 보상 가능
            }

            return (false, DailyRewardStatus.UNCLAIMED_UNAVAILABLE); // 아직 수령 불가
        }

    public bool ClaimReward(int day, SO.RewardSO dailyLoginRewardSO)
        {
            if (day <= store.curCountDay && !store.claimedDays.Contains(day))
            {
                Global.UIManager.OpenPage<UI.Page.LoadingPopUpPage>();

                GiveRewardByType(dailyLoginRewardSO);

                store.claimedDays.Add(day);
                store.lastRwardUTCTime = Global.DailyCheckManager.GetClientTime();
                
                if(store.curCountDay >= MAX_DAILY_REWARD_DAYS) 
                {
                    store.needToRestOnNextDay = true;
                }
                
                SaveStore();
                Global.UIManager.ClosePage();
                return true;
            }
            return false;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Debug/Daily Login/Reset")]
#endif
        public void ResetReward()
        {
            store = new DailyRewardData();
            store.curCountDay = 0;
            store.claimedDays.Clear();
            SaveStore();
        }
        private void SaveStore()
        {
            Global.UserDataManager.userStorage.dailyRewardData = store;
            Global.UserDataManager.Save();
        }
    }
}