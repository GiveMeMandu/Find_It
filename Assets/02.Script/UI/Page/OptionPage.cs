using System.Collections.Generic;
using Data;
using I2.Loc;
using Manager;
using OutGame;

// using I2.Loc;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class OptionPage : PageViewModel
    {

        private string _settingTitle;

        [Binding]
        public string SettingTitle
        {
            get => _settingTitle;
            set
            {
                _settingTitle = value;
                OnPropertyChanged(nameof(SettingTitle));
            }
        }
        private bool _isMainMenuButtonActive;

        [Binding]
        public bool IsMainMenuButtonActive
        {
            get => _isMainMenuButtonActive;
            set
            {
                _isMainMenuButtonActive = value;
                OnPropertyChanged(nameof(IsMainMenuButtonActive));
            }
        }
        private bool _isRetryButtonActive;

        [Binding]
        public bool IsRetryButtonActive
        {
            get => _isRetryButtonActive;
            set
            {
                _isRetryButtonActive = value;
                OnPropertyChanged(nameof(IsRetryButtonActive));
            }
        }
        private string _curLanguage;

        [Binding]
        public string CurLanguage
        {
            get => _curLanguage;
            set
            {
                _curLanguage = value;
                OnPropertyChanged(nameof(CurLanguage));
            }
        }
        private bool _isMuteSFX;

        [Binding]
        public bool IsMuteSFX
        {
            get => _isMuteSFX;
            set
            {
                _isMuteSFX = value;
                OnPropertyChanged(nameof(IsMuteSFX));
            }
        }
        private bool _isMuteBGM;

        [Binding]
        public bool IsMuteBGM
        {
            get => _isMuteBGM;
            set
            {
                _isMuteBGM = value;
                OnPropertyChanged(nameof(IsMuteBGM));
            }
        }

        protected override void Awake()
        {
            base.Awake();
            CurLanguage = LocalizationManager.CurrentLanguage;
            if(Global.CurrentScene is OutGame.MainMenu)
            {
                IsMainMenuButtonActive = false;
                IsRetryButtonActive = false;
            }
            else
            {
                IsMainMenuButtonActive = true;
                IsRetryButtonActive = true;
            }
            if( Global.CurrentScene is InGameSceneBase)
                SettingTitle = "PAUSE";
            else
                SettingTitle = "SETTING";
        }
        [Binding]
        public void OnClickPrevLanguage()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            var allLanguage = LocalizationManager.GetAllLanguages();
            for (int i = 0; i < allLanguage.Count; i++)
            {
                if (LocalizationManager.CurrentLanguage == allLanguage[i])
                {
                    int prevLang = (i - 1) < 0 ? allLanguage.Count - 1 : i - 1;
                    LocalizationManager.CurrentLanguage = allLanguage[prevLang];
                    break;
                }
            }
            CurLanguage = LocalizationManager.CurrentLanguage;
        }

        [Binding]
        public void OnClickNextLanguage()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            var allLanguage = LocalizationManager.GetAllLanguages();
            for (int i = 0; i < allLanguage.Count; i++)
            {
                if (LocalizationManager.CurrentLanguage == allLanguage[i])
                {
                    int nextLang = (i + 1) >= allLanguage.Count ? 0 : i + 1;
                    LocalizationManager.CurrentLanguage = allLanguage[nextLang];
                    break;
                }
            }
            CurLanguage = LocalizationManager.CurrentLanguage;
        }
        [Binding]
        public void OnClickChangeLanguage()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            var allLanguage = LocalizationManager.GetAllLanguages();
            for (int i = 0; i < allLanguage.Count; i++)
            {
                if (LocalizationManager.CurrentLanguage == allLanguage[i])
                {
                    int nextLang = (i + 1) >= allLanguage.Count ? 0 : i + 1;

                    LocalizationManager.CurrentLanguage = allLanguage[nextLang];
                    break; 
                }
            }
            CurLanguage = LocalizationManager.CurrentLanguage;
        }
        [Binding]
        public void OnClickBackToMainMenu()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            LoadingSceneManager.LoadScene(SceneNum.START);
        }
        [Binding]
        public void OnClickRetry()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            LoadingSceneManager.LoadScene((int)Global.CurrentScene.SceneName);
        }
        [Binding]
        public void OnClickMuteBGM()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            IsMuteBGM = Global.SoundManager.MuteBGM();
        }
        [Binding]
        public void OnClickMuteSFX()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            IsMuteSFX = Global.SoundManager.MuteSFX();
        }

        
        [Binding]
        public void OnClickExitButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            UnityEngine.Application.Quit();
        }
    }
}