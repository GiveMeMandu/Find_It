using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWeld.Binding;
using UI.Page;
using Cysharp.Threading.Tasks;
using Manager;

namespace UI.Page
{
    [Binding]
    public class IngameMissionCompleteVisualPage : PageViewModel
    {
        private string _titleText = "Mission Found!";
        [Binding]
        public string TitleText
        {
            get => _titleText;
            set
            {
                _titleText = value;
                OnPropertyChanged(nameof(TitleText));
            }
        }

        private string _missionSetName;
        [Binding]
        public string MissionSetName
        {
            get => _missionSetName;
            set
            {
                _missionSetName = value;
                OnPropertyChanged(nameof(MissionSetName));
            }
        }

        private GameObject _targetObject;
        private float _cameraZoomSize;
        private bool _enableCameraEffect;
        [Binding]
        public bool EnableCameraEffect
        {
            get => _enableCameraEffect;
            set
            {
                _enableCameraEffect = value;
                OnPropertyChanged(nameof(EnableCameraEffect));
            }
        }

        [Binding]
        public void OnClickClose()
        {
            Global.UIManager.ClosePage(this);
        }

        public void Initialize(string missionSetName, GameObject targetObject, bool enableCameraEffect, float cameraZoomSize = 0f)
        {
            MissionSetName = missionSetName;
            _targetObject = targetObject;
            _cameraZoomSize = cameraZoomSize;
            EnableCameraEffect = enableCameraEffect;
            TitleText = "Mission Found";

            PlaySequence().Forget();
        }

        private async UniTaskVoid PlaySequence()
        {
            // 카메라 연출이 활성화된 경우
            if (EnableCameraEffect)
            {
                // ItemSetCameraViewModel 찾아서 연출 시작
                var cameraViewModel = GetComponentInChildren<ItemSetCameraViewModel>(true);
                if (cameraViewModel != null)
                {
                    cameraViewModel.gameObject.SetActive(true);
                    cameraViewModel.ItemSetName = MissionSetName;
                    
                    // 카메라 연출 시작 및 완료 대기 (줌 크기 전달)
                    await cameraViewModel.PlayCameraEffect(_targetObject, _cameraZoomSize, () =>
                    {
                        // 연출 완료 후 페이지 닫기
                        Global.UIManager.ClosePage(this);
                    });
                }
                else
                {
                    Debug.LogWarning("[IngameMissionCompleteVisualPage] ItemSetCameraViewModel not found. Closing page immediately.");
                    Global.UIManager.ClosePage(this);
                }
            }
            else
            {
                // 기본 연출: 블러 없이 3초 후 자동 닫기
                await UniTask.Delay(3000);
                Global.UIManager.ClosePage(this);
            }
        }
    }
}
