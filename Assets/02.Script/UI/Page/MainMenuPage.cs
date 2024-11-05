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
            if (PlayerPrefs.GetInt("IsTutorial") == 1)
            {
                LoadingSceneManager.LoadScene(SceneNum.STAGE1);
            }
            DeskCat.FindIt.Scripts.Core.Model.GlobalSetting.InitializeSetting();

            
        }
        [Binding]
        public void OnClickStartButton()
        {
            if (CanPlay)
                LoadingSceneManager.LoadScene(SceneNum.SELECT);
        }
        [Binding]
        public void OnClickIntroButton()
        {
            PlayerPrefs.SetInt("IsTutorial", 1);
            SceneManager.LoadScene("3_Stage1");
        }
        [Binding]
        public void OnClickOptionButton()
        {

        }
        [Binding]
        public void OnClickMapButton()
        {
            CanPlay = false;
            var main = Global.CurrentScene as OutGame.MainMenu;
            main.OnClickMapButton();
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