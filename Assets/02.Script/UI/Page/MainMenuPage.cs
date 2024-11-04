using Manager;
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
        [Binding]
        public void OnClickStartButton()
        {
            if(CanPlay)
                LoadingSceneManager.LoadScene(SceneNum.InGame1);
        }
        [Binding]
        public void OnClickOptionButton()
        {
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