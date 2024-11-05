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
        private int _hiddenRabbitCount;

        [Binding]
        public int HiddenRabbitCount
        {
            get => _hiddenRabbitCount;
            set
            {
                _hiddenRabbitCount = value;
                OnPropertyChanged(nameof(HiddenRabbitCount));
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