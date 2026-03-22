using UnityEngine;
using Util.CameraSetting;

namespace InGame.Tutorial
{
    public class TutorialMoveKey : TutorialBase
    {
        [Header("이동 목표 설정")]
        [SerializeField] private float _requiredMoveSpace = 1.0f; // 완료로 인정할 이동 거리 (월드 좌표 기준)
        
        [Header("입력 제어")]
        [SerializeField] private bool _disableZoomInput = true; // 튜토리얼 중 카메라 줌 비활성화 여부

        private bool _isMoved = false;
        private Vector3 _initialCameraPosition;
        private bool _previousZoomState;

        public override void Enter()
        {
            _isMoved = false;
            
            if (CameraView2D.Instance != null && Camera.main != null)
            {
                _initialCameraPosition = Camera.main.transform.position;
                
                // 줌만 막고 이동(Pan)은 허용
                _previousZoomState = CameraView2D.Instance._enableZoom;
                if (_disableZoomInput)
                {
                    CameraView2D.Instance._enableZoom = false;
                }
            }
        }

        public override void Execute(TutorialController controller)
        {
            if (_isMoved)
            {
                controller.SetNextTutorial();
                return;
            }

            if (CameraView2D.Instance != null && Camera.main != null)
            {
                // 다른 터치 로직에 의해 줌이 강제로 켜지는 현상 방지
                if (_disableZoomInput)
                {
                    CameraView2D.Instance._enableZoom = false;
                }

                // 현재 카메라 위치와 초기 튜토리얼 시작 시점의 카메라 위치 사이의 거리를 계산
                Vector3 currentCameraPosition = Camera.main.transform.position;
                currentCameraPosition.z = 0f; // 2D 뷰 환경인 경우 z값 무시
                Vector3 initPos = _initialCameraPosition;
                initPos.z = 0f;
                
                float moveDistance = Vector3.Distance(initPos, currentCameraPosition);
                
                // 설정된 이동 거리 목표를 달성했다면 튜토리얼 완료 조건 충족
                if (moveDistance >= _requiredMoveSpace)
                {
                    _isMoved = true;
                }
            }
        }

        public override void Exit()
        {
            // 종료 시 이전 Zoom 상태로 복구
            if (CameraView2D.Instance != null)
            {
                CameraView2D.Instance._enableZoom = _previousZoomState;
            }
        }
    }
}
