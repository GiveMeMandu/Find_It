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
    public class InGameMissionCompletePage : PageViewModel
    {
        private string _missionName;

        [Binding] 
        public string MissionName
        {
            get => _missionName;
            set
            {
                _missionName = value;
                OnPropertyChanged(nameof(MissionName));
            }
        }
        private CancellationTokenSource _cts;
        private TaskCompletionSource<bool> _closeTaskSource;

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
            
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
