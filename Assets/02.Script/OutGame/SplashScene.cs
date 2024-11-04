using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OutGame
{
    public class SplashScene : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Start()
        {
            _canvasGroup.alpha = 0f;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            SplashScreenTask().Forget();
        }
        
        async UniTaskVoid SplashScreenTask()
        {
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync((int)SceneNum.LOADING, LoadSceneMode.Single);
            asyncOp.allowSceneActivation = false;
            await _canvasGroup.DOFade(1, 0.5f).AsyncWaitForCompletion();
            await UniTask.Delay(250, true);
            
            await UniTask.WaitUntil(() => asyncOp.progress >= 0.9f);
            Global.Create(true);
            
            await UniTask.Delay(500, true);
            // await _canvasGroup.DOFade(0, 0.5f).AsyncWaitForCompletion();
            asyncOp.allowSceneActivation = true;
        }
    }
}
