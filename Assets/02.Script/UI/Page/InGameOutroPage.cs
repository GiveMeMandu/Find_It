using System;
using Manager;
using OutGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using System.Threading;
using System.Threading.Tasks;
using Data;

namespace UI.Page
{
    [Binding]
    public class InGameOutroPage : PageViewModel
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
        private TaskCompletionSource<bool> _closeTaskSource;
        
        public override bool BlockEscape => true; // 게임 종료 페이지에서는 Escape 키 입력 차단

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
            CurStage = Global.CurrentScene.SceneName.ToString().Replace('_', '-');
            StageName = string.Format("{0} 클리어!", 
            I2.Loc.LocalizationManager.GetTranslation("SceneName/" + CurStage));
            
            _closeTaskSource = new TaskCompletionSource<bool>();
            DisableInputAndDestroyAsync(_cts.Token).Forget();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            
            Global.InputManager.EnableAllInput();
        }

        private async UniTaskVoid DisableInputAndDestroyAsync(CancellationToken cancellationToken)
        {
            Global.InputManager.DisableAllInput();
            
            try
            {
                bool isCancelled = await UniTask.Delay(3000, cancellationToken: cancellationToken).SuppressCancellationThrow();
            }
            finally
            {
                Global.InputManager.EnableAllInput();
                _closeTaskSource?.TrySetResult(true);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                Global.UIManager.ClosePage();
            }
        }

        public UniTask WaitForClose()
        {
            return _closeTaskSource.Task.AsUniTask();
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
