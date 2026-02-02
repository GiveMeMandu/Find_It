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

        private bool _isFocusIconActive;
        [Binding]
        public bool IsFocusIconActive
        {
            get => _isFocusIconActive;
            set
            {
                _isFocusIconActive = value;
                OnPropertyChanged(nameof(IsFocusIconActive));
            }
        }

        private GameObject _targetObject;

        [Binding]
        public void OnClickClose()
        {
            // 연출 중에는 닫기 불가하도록 할 수도 있음
            Global.UIManager.ClosePage(this);
        }

        public void Initialize(string missionSetName, GameObject targetObject)
        {
            MissionSetName = missionSetName;
            _targetObject = targetObject;
            TitleText = "Mission Found"; // 기본값 설정
            IsFocusIconActive = false;

            PlaySequence().Forget();
        }

        private async UniTaskVoid PlaySequence()
        {
            // 1. 카메라 이동 & 블러 세팅
            if (_targetObject != null && Util.CameraSetting.CameraView2D.Instance != null)
            {
                // 블러 시작 (흐리게)
                if (Global.UIEffectManager != null && Global.UIEffectManager.BlurController != null)
                {
                    Global.UIEffectManager.BlurController.TurnOnBlur(1f); // 강한 블러
                }

                // 카메라 이동 (1초)
                await Util.CameraSetting.CameraView2D.Instance.MoveCameraToPositionAsync(_targetObject.transform.position, 1f);
            }

            // 2. 초점 맞추기 (블러 1 -> 0) & 초점 아이콘 활성화
            IsFocusIconActive = true;
            if (Global.UIEffectManager != null && Global.UIEffectManager.BlurController != null)
            {
                // 1초 동안 블러 해제 (초점 맞추는 연출)
                Global.UIEffectManager.BlurController.BlurFadeOut(1f).Forget();
                await UniTask.Delay(1000);
            }
            else
            {
                await UniTask.Delay(1000);
            }

            // 3. 2초 대기
            await UniTask.Delay(2000);

            // 4. 사진 찍기 및 저장
            await CaptureAndSaveScreenshot();

            // 연출 종료 후 페이지 닫기? (기획에 명시되진 않았으나 보통 닫음)
            // Global.UIManager.ClosePage(this); 
        }

        private async UniTask CaptureAndSaveScreenshot()
        {
            // UI 숨기기 처리가 필요하다면 여기서 수행 (Canvas 끄기 등)
            // 여기서는 UI 포함해서 찍는다고 가정하거나, 기획 의도에 맞게 구현

            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"Mission_{MissionSetName}_{timestamp}.png";
            
            // 에디터/PC 저장
            ScreenCapture.CaptureScreenshot(filename);
            
            Debug.Log($"[Screenshot] Saved to {filename}");

            // 모바일 갤러리 갱신 등은 별도 처리가 필요함
            // 여기서는 캡처 완료를 알리는 찰칵 소리 등을 추가할 수 있음
            if (Global.SoundManager != null)
            {
                // Global.SoundManager.PlaySFX(SFXEnum.CameraShutter); // 예시
            }

            await UniTask.Yield();
        }
    }
}
