using System;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;
using Cysharp.Threading.Tasks;
using Data;

namespace UI.Page
{
    [Binding]
    public class MapSelectPage : PageViewModel
    {
        [Binding]
        public override void ClosePage()
        {
            base.ClosePage();
            Global.UIManager.GetPages<MainMenuPage>()[0].ShowMapButton = true;
        }

        [Binding]
        public void OnClickOptionButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            Global.UIManager.OpenPage<OptionPage>();
        }
        [Binding]
        public void OnClickAdButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            // Global.GoogleMobileAdsManager.ShowRewardedAd();
        }
    }
}