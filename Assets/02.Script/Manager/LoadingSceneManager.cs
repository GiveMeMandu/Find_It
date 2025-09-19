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
            shouldOpenReviewPage = false;
        }
        public static int nextScene;
        public static bool shouldOpenReviewPage = false;

        private void Start()
        {
            if(nextScene == 0) 
                nextScene = (int)SceneNum.START;
            LoadSceneSide().Forget();
        }

        public static void LoadScene(int sceneName)
        {
            nextScene = sceneName;
            
            // Start 씬으로 전환하는 경우 리뷰 페이지 열기 플래그 설정
            if (sceneName == (int)SceneNum.START)
            {
                shouldOpenReviewPage = true;
            }
            else
            {
                shouldOpenReviewPage = false;
            }

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
            
            // 첫 80%까지 1초 동안 부드럽게 증가
            while (elapsedTime < 1.0f)
            {
                await UniTask.DelayFrame(1, PlayerLoopTiming.LastPostLateUpdate);
                elapsedTime += Time.unscaledDeltaTime;
                
                if (Time.realtimeSinceStartup >= nextUpdateTime)
                {
                    float progress = Mathf.Lerp(0f, 0.8f, elapsedTime / 1.0f);
                    await UniTask.SwitchToMainThread();
                    
                    if (progressBar != null)
                        progressBar.value = progress;
                    if (percentText != null)
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
                    
                    if (progressBar != null)
                        progressBar.value = progress;
                    if (percentText != null)
                        percentText.text = $"{(progress * 100f):F0}%";
                }
            });

            // 마지막 90%에서 100%까지 0.5초 동안 부드럽게 증가
            elapsedTime = 0f;
            await UniTask.Create(async () =>
            {
                while (elapsedTime < 0.5f)
                {
                    await UniTask.DelayFrame(1, PlayerLoopTiming.LastUpdate);
                    elapsedTime += Time.unscaledDeltaTime;
                    float progress = Mathf.Lerp(0.9f, 1f, elapsedTime / 0.5f);
                    await UniTask.SwitchToMainThread();
                    
                    if (progressBar != null)
                        progressBar.value = progress;
                    if (percentText != null)
                        percentText.text = $"{(progress * 100f):F0}%";
                    
                    if (progress > 0.99f)
                    {
                        // 로딩 완료 시 텍스트 변경 및 애니메이션
                        if (loadingTextCTS != null)
                        {
                            loadingTextCTS.Cancel();
                            loadingTextCTS.Dispose();
                        }
                        
                        if (progressBar != null)
                            progressBar.value = 1;
                        if (percentText != null)
                            percentText.text = "100%";
                        if (loadingText != null)
                        {
                            loadingText.text = "Complete!";
                            loadingText.transform.localScale = Vector3.one * 1.7f;
                        }
                        
                        // 0.3초만 대기
                        await UniTask.Delay(300);
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
                    if (loadingText != null)
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