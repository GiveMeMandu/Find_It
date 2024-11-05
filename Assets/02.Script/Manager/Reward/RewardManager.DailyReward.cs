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
        private DailyRewardData store = Global.UserDataManager.userStorage.dailyRewardData;

        private void OnEnable()
        {
            CheckFirstInit();
            if(store.needToRestOnNextDay) ResetReward();
            Global.DailyCheckManager.OnInternetTimeChecked += OnInternetTimeChecked;
            Global.DailyCheckManager.OnDayChanged += OnDayChanged;
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
            // 마지막 로그인 날짜와 현재 날짜가 다르면 누적 출석 일수를 증가시킴
            int DayDiff = store.lastRwardUTCTime.Day - currentUTCTime.Day;
            DayDiff = Mathf.Abs(DayDiff);
            if (DayDiff > 0)
            {
                Debug.Log("성공적으로 일일보상 갱신");
                store.curCountDay = (store.curCountDay + DayDiff) % 7; // 7일 주기로 초기화
                SaveStore();
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

        public bool ClaimReward(int day, SO.DailyRewardSO dailyRewardSO) //* 인자로 보상 주기
        {
            if (day <= store.curCountDay && !store.claimedDays.Contains(day))
            {
                Global.UIManager.OpenPage<UI.Page.LoadingPopUpPage>();

                GiveRewardByType(dailyRewardSO);

                // 인자로 온 보상 지급한 뒤 
                store.claimedDays.Add(day); // 보상을 수령한 날로 기록
                store.lastRwardUTCTime = Global.DailyCheckManager.GetClientTime();  // 보상 받은 날짜 기록
                if(day >= 7 && store.curCountDay > 7) {
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