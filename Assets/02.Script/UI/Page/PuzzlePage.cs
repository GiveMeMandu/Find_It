using Data;
using Manager;
using UI.Effect;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class PuzzlePage : PageViewModel
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
        public void OnClickOptionButton()
        {
            Global.UIManager.OpenPage<OptionPage>();
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