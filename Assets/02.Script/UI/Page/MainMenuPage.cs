using Data;
using Manager;
using UI.Effect;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class MainMenuPage : PageViewModel
    {
        private string _inGameTimer;
        [Binding]
        public string InGameTimer
        {
            get => _inGameTimer;
            set
            {
                _inGameTimer = value;
                OnPropertyChanged(nameof(InGameTimer));
            }
        }
        private bool _canPlay;

        [Binding]
        public bool CanPlay
        {
            get => _canPlay;
            set
            {
                _canPlay = value;
                OnPropertyChanged(nameof(CanPlay));
            }
        }
        
        private bool _showMapButton;

        [Binding]
        public bool ShowMapButton
        {
            get => _showMapButton;
            set
            {
                _showMapButton = value;
                OnPropertyChanged(nameof(ShowMapButton));
            }
        }
        
        private void OnEnable()
        {
            ShowMapButton = true;
            DeskCat.FindIt.Scripts.Core.Model.GlobalSetting.InitializeSetting();
            if (InGameTimer == "") Global.StageTimer = 600;
        }
        public void SetTimer()
        {
            if (InGameTimer == "") Global.StageTimer = 600;
            else
            {
                Debug.Log("Set StageTimer to: " + InGameTimer);
                Global.StageTimer = int.Parse(InGameTimer);
            }
        }
        [Binding]
        public void OnClickStartButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            PlayerPrefs.SetInt("IsIntro1", 1);
            PlayerPrefs.Save();
            var main = Global.CurrentScene as OutGame.MainMenu;
            main.OnClickStartButton();
        }
        [Binding]
        public void OnClickOptionButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            Global.UIManager.OpenPage<OptionPage>();
        }
        [Binding]
        public void OnClickMapButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            CanPlay = false;
            // var main = Global.CurrentScene as OutGame.MainMenu;
            // main.OnClickMapButton();

            ShowMapButton = false;


            Global.UIManager.OpenPage<MapSelectPage>();
        }
        [Binding]
        public void OnClickCollectionButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            Global.UIManager.OpenPage<CollectionPage>();
        }
        [Binding]
        public void OnClickMainMenuButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            var main = Global.CurrentScene as OutGame.MainMenu;
            main.OnClickMainMenuButton();
        }
        [Binding]
        public void OnClickResetButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            SceneManager.LoadScene(0);
        }
        [Binding]
        public void OnClickExitButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}