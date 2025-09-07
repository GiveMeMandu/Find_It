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
