using System.Collections.Generic;
using Manager;
// using I2.Loc;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class OptionPage : PageViewModel
    {
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

        // protected override void Awake()
        // {
        //     base.Awake();
        //     CurLanguage = LocalizationManager.CurrentLanguage;
        // }
        // [Binding]
        // public void OnClickChangeLanguage()
        // {
        //     var allLanguage = LocalizationManager.GetAllLanguages();
        //     for (int i = 0; i < allLanguage.Count; i++)
        //     {
        //         if (LocalizationManager.CurrentLanguage == allLanguage[i])
        //         {
        //             int nextLang = (i + 1) >= allLanguage.Count ? 0 : i + 1;

        //             LocalizationManager.CurrentLanguage = allLanguage[nextLang];
        //             break; 
        //         }
        //     }
        //     CurLanguage = LocalizationManager.CurrentLanguage;
        // }

        // [Binding]
        // public void OnClickMuteBGM()
        // {
        //     IsMuteBGM = Global.SoundManager.MuteBGM();
        // }
        // [Binding]
        // public void OnClickStage1()
        // {
        //     Global.UserDataManager.bunnyStorage.curScene = BunnyCafe.Data.SceneName.InGame1;
        //     Global.UserDataManager.Save();
        //     LoadingSceneManager.LoadScene(SceneNum.InGame1);
        // }
        // [Binding]
        // public void OnClickStage2()
        // {
        //     Global.UserDataManager.bunnyStorage.curScene = BunnyCafe.Data.SceneName.InGame3;
        //     Global.UserDataManager.Save();
        //     LoadingSceneManager.LoadScene(SceneNum.InGame3);
        // }
        [Binding]
        public void OnClickMuteSFX()
        {
            IsMuteSFX = Global.SoundManager.MuteSFX();
        }
    }
}