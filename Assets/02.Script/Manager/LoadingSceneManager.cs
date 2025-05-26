using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Data;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using TMPro;
using Sirenix.OdinInspector;
using System.Threading;
using System;

namespace Manager
{

    public class LoadingSceneManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI percentText;
        [LabelText("로딩 문구")] [SerializeField] private TextMeshProUGUI loadingText;
        public UnityEvent onLoadScene;
        public Slider progressBar;
        [RuntimeInitializeOnLoadMethod]
        public static void Initialize() 
        {
            nextScene = (int)SceneNum.START;
        }
        public static int nextScene;

        private void Start()
        {
            if(nextScene == 0) 
                nextScene = (int)SceneNum.START;
            LoadSceneSide().Forget();
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
        private async UniTaskVoid LoadSceneSide()
        {
            await UniTask.NextFrame();
            onLoadScene?.Invoke();

            // 로딩 텍스트 애니메이션 시작
            LoadingTextAnimation().Forget();

            var op = SceneManager.LoadSceneAsync(nextScene);
            op.allowSceneActivation = false;
            
            Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.Low;

            float elapsedTime = 0f;
            float nextUpdateTime = Time.realtimeSinceStartup;
            const float updateInterval = 0.016f;
            
            // 첫 80%까지 3초 동안 부드럽게 증가
            while (elapsedTime < 3.0f)
            {
                await UniTask.DelayFrame(1, PlayerLoopTiming.LastPostLateUpdate);
                elapsedTime += Time.unscaledDeltaTime;
                
                if (Time.realtimeSinceStartup >= nextUpdateTime)
                {
                    float progress = Mathf.Lerp(0f, 0.8f, elapsedTime / 3.0f);
                    await UniTask.SwitchToMainThread();
                    progressBar.value = progress;
                    percentText.text = $"{(progress * 100f):F0}%";
                    nextUpdateTime = Time.realtimeSinceStartup + updateInterval;
                }
            }

            // 실제 로딩 완료까지 대기 (80~90%)
            await UniTask.Create(async () =>
            {
                while (op.progress < 0.9f)
                {
                    await UniTask.DelayFrame(2, PlayerLoopTiming.LastUpdate);
                    await UniTask.SwitchToMainThread();
                    float progress = Mathf.Lerp(0.8f, 0.9f, op.progress / 0.9f);
                    progressBar.value = progress;
                    percentText.text = $"{(progress * 100f):F0}%";
                }
            });

            // 마지막 90%에서 100%까지 2초 동안 부드럽게 증가
            elapsedTime = 0f;
            await UniTask.Create(async () =>
            {
                while (elapsedTime < 2.0f)
                {
                    await UniTask.DelayFrame(1, PlayerLoopTiming.LastUpdate);
                    elapsedTime += Time.unscaledDeltaTime;
                    float progress = Mathf.Lerp(0.9f, 1f, elapsedTime / 2.0f);
                    await UniTask.SwitchToMainThread();
                    progressBar.value = progress;
                    percentText.text = $"{(progress * 100f):F0}%";
                    
                    if (progress > 0.99f)
                    {
                        // 로딩 완료 시 텍스트 변경 및 애니메이션
                        if (loadingTextCTS != null)
                        {
                            loadingTextCTS.Cancel();
                            loadingTextCTS.Dispose();
                        }
                        progressBar.value = 1;
                        percentText.text = "100%";
                        loadingText.text = "Complete!";
                        loadingText.transform.localScale = Vector3.one * 1.7f;
                        
                        // 1초 더 대기
                        await UniTask.Delay(1000);
                        op.allowSceneActivation = true;
                        break;
                    }
                }
            });
        }

        private CancellationTokenSource loadingTextCTS;

        private async UniTaskVoid LoadingTextAnimation()
        {
            if (loadingTextCTS != null)
            {
                loadingTextCTS.Cancel();
                loadingTextCTS.Dispose();
            }
            
            loadingTextCTS = new CancellationTokenSource();
            var token = loadingTextCTS.Token;
            
            string[] dots = { ".", "..", "..." };
            int dotIndex = 0;
            
            try
            {
                while (!token.IsCancellationRequested)
                {
                    loadingText.text = $"Now Loading{dots[dotIndex]}";
                    dotIndex = (dotIndex + 1) % dots.Length;
                    await UniTask.Delay(500, cancellationToken: token); // 0.5초 대기
                }
            }
            catch (OperationCanceledException)
            {
                // 취소 처리
            }
        }
    }
}