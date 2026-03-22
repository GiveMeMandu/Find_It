using UnityEngine;
using Util.CameraSetting;

namespace InGame.Tutorial
{
    public class TutorialZoomGuide : TutorialBase
    {
        [Header("줌 목표 설정")]
        [SerializeField] private float _requiredZoomDelta = 0.5f; // 완료로 인정할 줌 크기 변화량
        
        [Header("입력 제어")]
        [SerializeField] private bool _disablePanInput = true; // 튜토리얼 중 카메라 이동(Pan) 비활성화 여부

        private bool _isZoomed = false;
        private float _initialZoomSize;
        private bool _previousPanState;

        public override void Enter()
        {
            _isZoomed = false;
            
            if (CameraView2D.Instance != null)
            {
                _initialZoomSize = CameraView2D.Instance.CurrentOrthographicSize;
                
                // 직렬화 문제로 false로 로드되는 현상 방지 및 강제 적용
                _previousPanState = CameraView2D.Instance._enablePan;
                CameraView2D.Instance._enablePan = false;
            }
        }

        public override void Execute(TutorialController controller)
        {
            if (_isZoomed)
            {
                controller.SetNextTutorial();
                return;
            }

            if (CameraView2D.Instance != null)
            {
                // 다른 UI 스크립트나 터치 이벤트 등으로 인해 _enablePan이 다시 true로 풀리는 것을 방지
                if (_disablePanInput)
                {
                    CameraView2D.Instance._enablePan = false;
                }

                float currentZoom = CameraView2D.Instance.CurrentOrthographicSize;
                
                // 설정된 줌 변화량만큼 줌 인/아웃이 발생했다면 튜토리얼 완료 조건 충족
                if (Mathf.Abs(currentZoom - _initialZoomSize) >= _requiredZoomDelta)
                {
                    _isZoomed = true;
                }
            }
        }

        public override void Exit()
        {
            // 종료 시 이전 Pan 상태로 복구
            if (CameraView2D.Instance != null)
            {
                CameraView2D.Instance._enablePan = _previousPanState;
            }
        }
    }
}
