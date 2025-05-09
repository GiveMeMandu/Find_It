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
        private void OnEnable()
        {
            DeskCat.FindIt.Scripts.Core.Model.GlobalSetting.InitializeSetting();
        }
        [Binding]
        public void OnClickStartButton()
        {
            PlayerPrefs.SetInt("IsIntro1", 1);
            PlayerPrefs.Save();
            var main = Global.CurrentScene as OutGame.MainMenu;
            main.OnClickStartButton();
        }
        [Binding]
        public void OnClickOptionButton()
        {
            Global.UIManager.OpenPage<OptionPage>();
        }
        [Binding]
        public void OnClickMapButton()
        {
            CanPlay = false;
            var main = Global.CurrentScene as OutGame.MainMenu;
            main.OnClickMapButton();
        }
        [Binding]
        public void OnClickMainMenuButton()
        {
            var main = Global.CurrentScene as OutGame.MainMenu;
            main.OnClickMainMenuButton();
        }
        [Binding]
        public void OnClickResetButton()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            SceneManager.LoadScene(0);
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