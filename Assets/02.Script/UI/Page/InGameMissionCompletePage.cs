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
        private float _missionNameAlpha;

        [Binding]
        public float MissionNameAlpha
        {
            get => _missionNameAlpha;
            set
            {
                _missionNameAlpha = value;
                OnPropertyChanged(nameof(MissionNameAlpha));
            }
        }   

        private string _missionStatus;

        [Binding]
        public string MissionStatus
        {
            get => _missionStatus;
            set 
            {
                _missionStatus = value;
                OnPropertyChanged(nameof(MissionStatus));
            }
        }

        private Sprite _missionSetIcon;

        [Binding]
        public Sprite MissionSetIcon
        {
            get => _missionSetIcon;
            set
            {
                _missionSetIcon = value;
                OnPropertyChanged(nameof(MissionSetIcon));
            }
        }

        private string _missionSetFoundLeft;

        [Binding]
        public string MissionSetFoundLeft
        {
            get => _missionSetFoundLeft;
            set
            {
                _missionSetFoundLeft = value;
                OnPropertyChanged(nameof(MissionSetFoundLeft));
            }
        }

        private bool _isSetFoundComplete;

        [Binding]
        public bool IsSetFoundComplete
        {
            get => _isSetFoundComplete;
            set
            {
                _isSetFoundComplete = value;
                OnPropertyChanged(nameof(IsSetFoundComplete));
            }
        }
        
        private bool _isGroupComplete;

        [Binding]
        public bool IsGroupComplete
        {
            get => _isGroupComplete;
            set
            {
                _isGroupComplete = value;
                OnPropertyChanged(nameof(IsGroupComplete));
            }
        }

        private string _missionNameDivider;

        [Binding]
        public string MissionNameDivider
        {
            get => _missionNameDivider;
            set
            {
                _missionNameDivider = value;
                OnPropertyChanged(nameof(MissionNameDivider));
            }
        }

        private Vector2 _sizeDelta;

        [Binding]
        public Vector2 SizeDelta
        {
            get => _sizeDelta;
            set
            {
                _sizeDelta = value;
                OnPropertyChanged(nameof(SizeDelta));
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
            
        }

        private async UniTaskVoid DisableInputAndDestroyAsync(CancellationToken cancellationToken)
        {
            
            try
            {
                await UniTask.Delay(3000, cancellationToken: cancellationToken);
            }
            finally
            {
                Global.InputManager.EnableAllInput();
            }

            Global.UIManager.ClosePage(this);
            _closeTaskSource.SetResult(true);
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

        public void Initialize(string missionName, string missionNameDivider, Sprite missionSetIcon, 
            string missionSetFoundLeft, bool isSetFoundComplete, bool isGroupComplete, 
            string missionStatus, float missionNameAlpha)
        {
            MissionName = missionName;
            MissionNameDivider = missionNameDivider;
            
            // 아이콘 설정 시 35x35 크기에 맞춰 비율 조정
            if (missionSetIcon != null)
            {
                MissionSetIcon = missionSetIcon;
                // 스프라이트의 원본 크기를 가져옵니다
                Rect spriteRect = missionSetIcon.rect;
                float originalWidth = spriteRect.width;
                float originalHeight = spriteRect.height;
                
                // 35x35 크기에 맞춰 비율 계산
                float targetSize = 35f;
                if (originalWidth >= originalHeight)
                {
                    float scale = targetSize / originalWidth;
                    SizeDelta = new Vector2(targetSize, originalHeight * scale);
                }
                else
                {
                    float scale = targetSize / originalHeight;
                    SizeDelta = new Vector2(originalWidth * scale, targetSize);
                }
            }
            
            MissionSetFoundLeft = missionSetFoundLeft;
            IsSetFoundComplete = isSetFoundComplete;
            IsGroupComplete = isGroupComplete;
            MissionStatus = missionStatus;
            MissionNameAlpha = missionNameAlpha;
        }

        // 특정 크기로 조절이 필요한 경우를 위한 보조 메서드
        private Vector2 CalculateAspectRatio(float originalWidth, float originalHeight, float targetWidth)
        {
            float aspectRatio = originalWidth / originalHeight;
            float newHeight = targetWidth / aspectRatio;
            return new Vector2(targetWidth, newHeight);
        }
    }
}
