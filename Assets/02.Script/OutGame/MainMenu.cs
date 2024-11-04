using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Manager;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UI.Page;

namespace OutGame
{
    public class MainMenu : SceneBase
    {
        public static float Speed = 1;
        public Button flashingBtn;
        // public TextMeshProUGUI flashingText;
        
        private MainMenuPage mainMenuPage = null;
        protected override void Start()
        {
            base.Start();
            mainMenuPage = Global.UIManager.OpenPage<MainMenuPage>();
            if (mainMenuPage != null)
                mainMenuPage.CanPlay = true;
            // SplashScreenTask().Forget();
        }

        // async UniTaskVoid SplashScreenTask()
        // {
        //     AsyncOperation asyncOp = SceneManager.LoadSceneAsync("InGame1", LoadSceneMode.Single);
        //     asyncOp.allowSceneActivation = false;
        //     await flashingText.DOFade(1, 0.5f).AsyncWaitForCompletion();
        //     await UniTask.Delay(1000, true);
            
        //     await UniTask.WaitUntil(() => asyncOp.progress >= 0.9f);
        //     Global.Create(true);
            
        //     await UniTask.Delay(1000, true);
        //     await flashingText.DOFade(0, 0.5f).AsyncWaitForCompletion();
        //     asyncOp.allowSceneActivation = true;

        //     if(mainMenuPage != null)
        //         mainMenuPage.CanPlay = true;
        // }
        // public IEnumerator BlinkText()
        // {
        //     while (true)
        //     {
        //         flashingText.color = new Color(0, 0, 0, 0);
        //         yield return new WaitForSeconds(.5f);
        //         flashingText.color = new Color(1, 1, 1, 1);
        //         flashingBtn.interactable = true;
        //         yield return new WaitForSeconds(.5f);
        //     }
        // }
    }
}