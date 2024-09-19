using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (!PlayerPrefs.HasKey("IsTutorial"))
        {
            PlayerPrefs.SetInt("IsTutorial", 1);
            PlayerPrefs.Save();
        }
    }
}
