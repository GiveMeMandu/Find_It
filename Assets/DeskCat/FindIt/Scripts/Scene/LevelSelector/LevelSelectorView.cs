using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeskCat.FindIt.Scripts.Scene.LevelSelector
{
    public class LevelSelectorView : MonoBehaviour
    {
        public Button BackToTitleBtn;

        private void Start()
        {
            BackToTitleBtn.onClick.AddListener(BackToTitleFunc);
        }

        private void BackToTitleFunc()
        {
            SceneManager.LoadScene("Cover");
        }
    }
}