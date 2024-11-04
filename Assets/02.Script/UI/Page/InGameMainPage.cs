using System;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class InGameMainPage : PageViewModel
    {
        private string _EPS;

        [Binding]
        public string EPS
        {
            get => _EPS;
            set
            {
                _EPS = value;
                OnPropertyChanged(nameof(EPS));
            }
        }
        [Binding]
        public void GetEPS()
        {
            EPS = Global.GoldManager.GetGoldEPS();
        }
        [Binding]
        public void OnClickQuestButton()
        {
            Global.UIManager.OpenPage<QuestPage>();
        }
        
        [Binding]
        public void OnClickCalendarButton()
        {
            Global.UIManager.OpenPage<CalendarPage>();
        }
        
        [Binding]
        public void OnClickOptionButton()
        {
            Global.UIManager.OpenPage<OptionPage>();
        }
        [Binding]
        public void OnClickAdButton()
        {
            // Global.GoogleMobileAdsManager.ShowRewardedAd();
        }
        
        [Binding]
        public void OnClickReturnToMainPageButton()
        {
            int maxCount = 10;
            while(!(Global.UIManager.GetCurrentPage() is InGameMainPage) && maxCount > 0)
            {
                ClosePage();
                maxCount--;
            }
        }
        
        [Binding]
        public void OnClickDailyRewardPageButton()
        {
            Global.UIManager.OpenPage<DailyRewardPage>();
        }
        [Binding]
        public void OnClickExitButton()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}