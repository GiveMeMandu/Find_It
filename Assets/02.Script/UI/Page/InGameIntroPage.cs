using System;
using Manager;
using OutGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using System.Threading;
using Data;

namespace UI.Page
{
    [Binding]
    public class InGameIntroPage : PageViewModel
    {
        private string _curStage;

        [Binding]
        public string CurStage
        {
            get => _curStage;
            set
            {
                _curStage = value;
                OnPropertyChanged(nameof(CurStage));
            }
        }
        private string _stageName;

        [Binding]
        public string StageName
        {
            get => _stageName;
            set
            {
                _stageName = value;
                OnPropertyChanged(nameof(StageName));
            }
        }
        private CancellationTokenSource _cts;

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
            _curStage = Global.CurrentScene.SceneName.ToString();

            // 전체 스테이지 이름 사용
            string stageKey = _curStage;
            _stageName = I2.Loc.LocalizationManager.GetTranslation("SceneName/" + stageKey);

            // 위에 표기를 위해 _ 대신 - 쓰기
            _curStage = _curStage.Replace('_', '-');

            DisableInputAndDestroyAsync(_cts.Token).Forget();
        }

        private void OnDisable()
        {
            // 항상 입력을 먼저 활성화 (안전장치)
            try
            {
                Global.InputManager?.EnableAllInput();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InGameIntroPage] Error enabling input in OnDisable: {ex.Message}");
            }
            
            // CancellationToken 정리
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTaskVoid DisableInputAndDestroyAsync(CancellationToken cancellationToken)
        {
            try
            {
                Global.InputManager.DisableAllInput();

                await UniTask.WaitForSeconds(3, cancellationToken: cancellationToken);
                
                Global.InputManager.EnableAllInput();
                Global.UIManager.ClosePage();
                Global.UIManager.OpenPage<InGameTutorialPage>();
            }
            catch (OperationCanceledException)
            {
                // CancellationToken이 취소된 경우 (OnDisable에서 이미 처리됨)
                Debug.Log("[InGameIntroPage] DisableInputAndDestroyAsync cancelled");
            }
            catch (Exception ex)
            {
                // 예외 발생 시에도 입력 복구
                Debug.LogError($"[InGameIntroPage] Error in DisableInputAndDestroyAsync: {ex.Message}");
                Global.InputManager.EnableAllInput();
            }
        }

        [Binding]
        public void OnClickStartButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            Debug.Log("Start button clicked");
        }

        [Binding]
        public void OnClickSkipIntroButton()
        {
            Global.SoundManager.PlaySFX(SFXEnum.ClickUI);
            Debug.Log("Skip intro button clicked");
        }
    }
}
