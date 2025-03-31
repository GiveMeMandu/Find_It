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

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
            CurStage = Global.CurrentScene.SceneName.ToString();
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
                await UniTask.Delay(3000, cancellationToken: cancellationToken);
            }
            finally
            {
                Global.InputManager.EnableAllInput();
            }

            Global.UIManager.ClosePage();
            _closeTaskSource.SetResult(true);
        }

        public UniTask WaitForClose()
        {
            return _closeTaskSource.Task.AsUniTask();
        }

        [Binding]
        public void OnClickStartButton()
        {
            Debug.Log("Start button clicked");
        }

        [Binding]
        public void OnClickSkipIntroButton()
        {
            Debug.Log("Skip intro button clicked");
        }
    }
}
