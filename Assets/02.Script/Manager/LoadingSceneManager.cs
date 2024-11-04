using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Manager
{
    public static class SceneNum
    {
        public const int BootStrap = 0;
        public const int MAIN = 1;
        public const int LOADING = 2;
        public const int InGame1 = 3;
        public const int InGame2 = 4;
        public const int InGame3 = 5;
        public const int OUTSIDE = 6;
        public const int AniPang = 7;

    }
    public class LoadingSceneManager : MonoBehaviour
    {
        public Slider progressBar;
        public static int nextScene;

        private void Start()
        {
            if(nextScene == 0) 
                nextScene = (int)SceneNum.InGame1;
            StartCoroutine(LoadSceneSide());
        }

        public static void LoadScene(int sceneName)
        {
            nextScene = sceneName;

            SceneManager.LoadScene(SceneNum.LOADING); //로딩 씬
        }
        public static void LoadScene()
        {
            SceneManager.LoadScene(SceneNum.LOADING); //로딩 씬
        }
        private IEnumerator LoadSceneSide()
        {
            yield return null;

            AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
            op.allowSceneActivation = false;

            float timer = 0.0f;
            while (!op.isDone)
            {
                yield return null;

                timer += Time.deltaTime * 0.25f;

                if (op.progress < 0.9f)
                {
                    progressBar.value = Mathf.Lerp(progressBar.value, op.progress, timer);
                    if (progressBar.value >= op.progress)
                    {
                        timer = 0f;
                    }
                }
                else
                {
                    progressBar.value = Mathf.Lerp(progressBar.value, 1f, timer);

                    if (progressBar.value == 1.0f)
                    {
                        op.allowSceneActivation = true;

                        yield break;
                    }
                }
            }
        }
    }
}