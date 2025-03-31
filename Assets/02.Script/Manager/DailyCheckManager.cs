using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Manager
{
    public class DailyCheckManager : AutoTaskControl
    {
        public EventHandler<DateTime> OnDayChanged;
        public EventHandler<TimeSpan> OnTimeLeftChanged;
        public EventHandler<DateTime> OnInternetTimeChecked;
        private bool hasDayChangedToday = false;  // 오늘 이벤트가 발생했는지 추적
        // 구독자가 현재 상태를 즉시 받을 수 있도록 하는 메서드
        public void SubscribeToDayChanged(EventHandler<DateTime> handler)
        {
            OnDayChanged += handler;
            
            // 이미 오늘 이벤트가 발생했다면 즉시 알림
            if (hasDayChangedToday)
            {
                handler?.Invoke(this, m_LastLogOut_time);
            }
        }

        private DateTime lastUTCStore;
        private DateTime lastOfflineRewardStore;
        private DateTime m_LastLogOut_time;  // 일일 체크용
        private DateTime m_LastOfflineRewardTime;  // 오프라인 보상용
        private DateTime resetTime;
        private DateTime clientTime;

        private TimeZoneInfo clientTimeZone;
        private void Awake()
        {
            clientTimeZone = GetClientTimeZone();
            lastUTCStore = Global.UserDataManager.userStorage.lastUTCTime;
            m_LastLogOut_time = lastUTCStore;
            lastOfflineRewardStore = Global.UserDataManager.userStorage.lastOfflineRewardTime;
            m_LastOfflineRewardTime = lastOfflineRewardStore;
            CheckFirstInit();
            GetClientTimeZone();
            UpdateTimeCounter().Forget();
        }

        private void CheckFirstInit()
        {
            if(resetTime == DateTime.MinValue) CheckDayChangeLeftAsync();
            //* 처음 접속인지 체크
            if(m_LastLogOut_time == DateTime.MinValue)
            {
                DayChange();
                m_LastLogOut_time = clientTime;
                lastUTCStore = m_LastLogOut_time;
                SaveStore();
            }
        }

        private async UniTaskVoid UpdateTimeCounter()
        {
            while (true)
            {
                clientTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, clientTimeZone);
                var diff = resetTime - clientTime;

                if (clientTime.Day != m_LastLogOut_time.Day || diff.TotalSeconds <= 0)
                {
                    hasDayChangedToday = false;  // 새로운 날이 시작될 때 리셋
                    DayChange();
                }
                OnTimeLeftChanged?.Invoke(this, diff);
                // 남은 시간이 2분 이하일 경우, 짧은 딜레이 설정
                // TimeSpan delayDuration = diff.TotalSeconds <= 120 ? TimeSpan.FromSeconds(1) : TimeSpan.FromSeconds(60);
                TimeSpan delayDuration =  TimeSpan.FromSeconds(1);
                await UniTask.Delay(delayDuration, true, PlayerLoopTiming.Update, destroyCancellation.Token);
            }
        }
        private void CheckDayChangeLeftAsync()
        {
            clientTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, clientTimeZone);

            resetTime = new DateTime(clientTime.Year, clientTime.Month, clientTime.Day, 0, 0, 0);
            if (clientTime >= resetTime)
            {
                resetTime = resetTime.AddDays(1);
            }
        }
        public TimeZoneInfo GetClientTimeZone()
        {
            return clientTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id);
        }
        public DateTime GetClientTime() {
            if(clientTimeZone == null) {
                clientTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id);
            }
            if(clientTime == DateTime.MinValue) {
                clientTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, clientTimeZone);
            }
            return clientTime;
        }
        public TimeSpan GetTimeSpanFromNow(DateTime lastDateTime) => DateTime.UtcNow - lastDateTime;
        public TimeSpan GetTimeSpanDiff(DateTime a, DateTime b) => a - b;

        private void DayChange()
        {
            if (m_LastLogOut_time.Day != clientTime.Day)
            {
                m_LastLogOut_time = clientTime;
                lastUTCStore = m_LastLogOut_time;
                SaveStore();
                Debug.Log("성공적으로 날짜 바뀜");
                hasDayChangedToday = true;
                OnDayChanged?.Invoke(this, m_LastLogOut_time);
            }
        }
        private void SaveStore()
        {
            Global.UserDataManager.userStorage.lastUTCTime = lastUTCStore;
            Global.UserDataManager.userStorage.lastOfflineRewardTime = lastOfflineRewardStore;
            Global.UserDataManager.Save();
        }

        public void SetLastLogoutTime(DateTime time)
        {
            m_LastLogOut_time = time;
            lastUTCStore = m_LastLogOut_time;
            m_LastOfflineRewardTime = time;
            lastOfflineRewardStore = m_LastOfflineRewardTime;
            SaveStore();
        }

        public DateTime GetLastOfflineRewardTime()
        {
            // MinValue인 경우 현재 시간을 반환 (초기 접속시에는 보상이 없음)
            if (m_LastOfflineRewardTime == DateTime.MinValue)
            {
                return DateTime.UtcNow;
            }
            return m_LastOfflineRewardTime;
        }

        private void OnApplicationQuit()
        {
            SaveLastLogoutTime();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveLastLogoutTime();
            }
        }

        private void SaveLastLogoutTime()
        {
            DateTime currentUtcTime = DateTime.UtcNow;
            SetLastLogoutTime(currentUtcTime);
        }
    }
}