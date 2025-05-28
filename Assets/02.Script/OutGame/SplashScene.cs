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
            
            // 모바일 최적화
            Application.targetFrameRate = 60;
            
            // SceneManager.LoadScene(Data.SceneName.Start.ToString(), LoadSceneMode.Single);
            SplashScreenTask().Forget();
        }
        
        async UniTaskVoid SplashScreenTask()
        {
            try
            {
                Debug.Log("Starting splash screen task...");
                
                AsyncOperation asyncOp = SceneManager.LoadSceneAsync(Data.SceneName.Start.ToString(), LoadSceneMode.Single);
                asyncOp.allowSceneActivation = false;
                
                // 페이드 인
                await _canvasGroup.DOFade(1, 0.5f).AsyncWaitForCompletion();
                await UniTask.Delay(250, true);
                
                // 로딩 대기 (타임아웃 추가)
                float timeoutDuration = 15f; // 15초로 증가 (모바일은 더 오래 걸릴 수 있음)
                float elapsedTime = 0f;
                
                Debug.Log("Waiting for scene to load...");
                while (asyncOp.progress < 0.9f && elapsedTime < timeoutDuration)
                {
                    await UniTask.Delay(100, true);
                    elapsedTime += 0.1f;
                    
                    if (elapsedTime % 1f < 0.1f) // 1초마다 로그
                    {
                        Debug.Log($"Loading progress: {asyncOp.progress:F2}, Elapsed: {elapsedTime:F1}s");
                    }
                }
                
                if (elapsedTime >= timeoutDuration)
                {
                    Debug.LogWarning("Scene loading timeout! Force activating scene...");
                }
                
                // Global 생성 (타임아웃 적용)
                await CreateGlobalWithTimeout();
                
                await UniTask.Delay(500, true);
                
                // 씬 활성화
                Debug.Log("Activating scene...");
                asyncOp.allowSceneActivation = true;
                
                // 씬 전환 완료 대기 (추가 안전장치)
                float sceneActivationTimeout = 5f;
                float sceneActivationElapsed = 0f;
                
                while (!asyncOp.isDone && sceneActivationElapsed < sceneActivationTimeout)
                {
                    await UniTask.Delay(100, true);
                    sceneActivationElapsed += 0.1f;
                }
                
                if (!asyncOp.isDone)
                {
                    Debug.LogWarning("Scene activation timeout! Using fallback...");
                    // 강제 씬 전환
                    SceneManager.LoadScene(Data.SceneName.Start.ToString(), LoadSceneMode.Single);
                }
                else
                {
                    Debug.Log("Scene loaded successfully!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SplashScreenTask failed: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
                
                // 예외 발생 시 강제로 씬 전환
                Debug.Log("Using fallback scene loading...");
                SceneManager.LoadScene(Data.SceneName.Start.ToString(), LoadSceneMode.Single);
            }
        }
        
        private UniTask CreateGlobalWithTimeout()
        {
            try
            {
                Debug.Log("Creating Global...");
                
                // 메인 스레드에서 직접 Global 생성 (이미 메인 스레드에 있음)
                try
                {
                    Global.Create(true);
                    Debug.Log("Global created successfully");
                    Debug.Log("Global initialization completed successfully");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Global creation failed: {e.Message}");
                    Debug.LogWarning("Global creation failed, but continuing...");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"CreateGlobalWithTimeout failed: {e.Message}");
                Debug.LogWarning("Continuing without Global initialization...");
            }
            
            return UniTask.CompletedTask;
        }
    }
}
