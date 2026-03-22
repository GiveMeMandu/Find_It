using Manager;
using UI;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace InGame.Tutorial
{
    public class TutorialZoom : TutorialBase
    {
        [Header("Target Settings")]
        [Tooltip("줌할 대상(Transform). 할당 시 해당 대상을 향해 카메라가 이동하며 줌을 진행합니다. 비워두면 제자리에서 줌만 합니다.")]
        public Transform targetTransform;

        [Header("Zoom Settings")]
        [Tooltip("도달할 목표 줌 수치 (값이 작을수록 확대)")]
        [Range(0.5f, 10f)]
        public float targetZoomSize = 3f;

        [Tooltip("줌 연출에 걸리는 시간 (초)")]
        public float zoomDuration = 1f;

        private bool canExecute = false;

        public override void Enter()
        {
            canExecute = false;
            
            if (Util.CameraSetting.CameraView2D.Instance != null)
            {
                ExecuteZoomAsync().Forget();
            }
            else
            {
                canExecute = true;
            }
        }

        private async UniTaskVoid ExecuteZoomAsync()
        {
            var cameraView = Util.CameraSetting.CameraView2D.Instance;
            
            // 타겟이 있으면 타겟의 위치로, 없으면 현재 카메라 위치 유지
            Vector3 targetPosition = targetTransform != null 
                ? targetTransform.position 
                : (Camera.main != null ? Camera.main.transform.position : Vector3.zero);
            
            await cameraView.MoveCameraAndZoomAsync(targetPosition, targetZoomSize, zoomDuration);
            
            canExecute = true;
        }

        public override void Execute(TutorialController controller)
        {
            if (!canExecute) return;
            
            controller.SetNextTutorial();
            canExecute = false;
        }

        public override void Exit()
        {
        }
    }
}

