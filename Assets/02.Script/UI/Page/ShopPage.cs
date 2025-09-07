using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;
using Data;

namespace UI.Page
{
    [Binding]
    public class ShopPage : PageViewModel
    {
        [Binding]
        public void OnClickStartButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
        }
        
        [Binding]
        public void OnClickOptionButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
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