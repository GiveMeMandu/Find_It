using UnityEngine;
using UnityEngine.SceneManagement;

namespace VolFx
{
    [AddComponentMenu("")]
    public class LoadScene : MonoBehaviour
    {
        public string        _sceneName;
        public LoadSceneMode _mode;
        
        // =======================================================================
        public void Invoke()
        {
            SceneManager.LoadScene(_sceneName, LoadSceneMode.Single);
        }
    }
}