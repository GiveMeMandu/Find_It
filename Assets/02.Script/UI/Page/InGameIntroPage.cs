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

        public override bool BlockEscape => true; // 게임 종료 페이지에서는 Escape 키 입력 차단

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();

            int chapterIdx = Global.StageManager.CurrentChapterIndex + 1;
            int stageIdx = Global.StageManager.CurrentStageIndex + 1;
            CurStage = $"stage{chapterIdx}_{stageIdx}";

            // 전체 스테이지 이름 사용
            string stageKey = Global.StageManager.CurrentStageSceneName;
            if (string.IsNullOrEmpty(stageKey) && Global.CurrentScene != null)
            {
                stageKey = Global.CurrentScene.SceneName.ToString();
            }

            StageName = I2.Loc.LocalizationManager.GetTranslation("SceneName/" + Global.CurrentScene.SceneName.ToString());

            Global.InputManager.DisableGameInputOnly();
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

                await UniTask.WaitForSeconds(3, cancellationToken: cancellationToken);

                Global.UIManager.ClosePage();
                // Global.InputManager.EnableAllInput();
                // Global.UIManager.OpenPage<InGameTutorialPage>();
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
                // Global.InputManager.EnableAllInput();
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

        public override void Init(params object[] parameters)
        {
        }
    }
}
