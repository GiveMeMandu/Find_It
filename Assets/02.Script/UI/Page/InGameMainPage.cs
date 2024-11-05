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
        public void OnClickOptionButton()
        {
            Global.UIManager.OpenPage<OptionPage>();
        }
        [Binding]
        public void OnClickAdButton()
        {
            // Global.GoogleMobileAdsManager.ShowRewardedAd();
        }
        
    }
}