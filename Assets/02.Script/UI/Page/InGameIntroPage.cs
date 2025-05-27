using System;
using Manager;
using OutGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using System.Threading;

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
            
            // Stage1_1 -> Stage1로 변환 (언더스코어 이후 제거)
            string stageKey = _curStage;
            int underscoreIndex = _curStage.IndexOf('_');
            if (underscoreIndex > 0)
            {
                stageKey = _curStage.Substring(0, underscoreIndex);
            }
            
            _stageName = I2.Loc.LocalizationManager.GetTranslation("SceneName/" + stageKey);
            
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
