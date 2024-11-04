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

        private DateTime store = Global.UserDataManager.userStorage.lastUTCTime;

        private DateTime m_LastLogOut_time;
        private DateTime resetTime;
        private DateTime clientTime;
        private TimeZoneInfo clientTimeZone;
        private void Start()
        {
            m_LastLogOut_time = store;
            
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
                store = m_LastLogOut_time;
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
            clientTimeZone = GetClientTimeZone();
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
        public DateTime GetClientTime() => clientTime;
        public TimeSpan GetTimeSpanFromNow(DateTime lastDateTime) => DateTime.UtcNow - lastDateTime;
        public TimeSpan GetTimeSpanDiff(DateTime a, DateTime b) => a - b;

        private void DayChange()
        {
            if (m_LastLogOut_time.Day != clientTime.Day)
            {
                m_LastLogOut_time = clientTime;
                store = m_LastLogOut_time;
                SaveStore();
                Debug.Log("성공적으로 날짜 바뀜");
                OnDayChanged?.Invoke(this, m_LastLogOut_time);
            }
        }
        private void SaveStore()
        {
            Global.UserDataManager.userStorage.lastUTCTime = store;
            Global.UserDataManager.Save();
        }
    }
}