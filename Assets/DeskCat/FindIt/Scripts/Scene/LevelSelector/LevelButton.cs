using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeskCat.FindIt.Scripts.Scene.LevelSelector
{
    public class LevelButton : MonoBehaviour
    {
        public string LevelName;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(LevelName));
        }
    }
}