using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;

namespace UI.Page
{
    [Binding]
    public class TitlePage : PageViewModel
    {
        [Binding]
        public void OnClickStartButton()
        {
            SceneManager.LoadScene("CharacterController");
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