using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Util.CameraSetting;

namespace Effect
{
    public enum ParallaxPreset
    {
        [InspectorName("원경 (느린 이동)")]
        Far,
        [InspectorName("중경 (보통 이동)")]
        Middle,
        [InspectorName("근경 (빠른 이동)")]
        Near,
        [InspectorName("커스텀 (수동 설정)")]
        Custom
    }

    public class ParallaxVFX : VFXObject
    {
        [Header("Parallax 설정")]
        [Tooltip("Parallax 프리셋 선택")]
        [SerializeField] private ParallaxPreset preset = ParallaxPreset.Middle;
        
        [Tooltip("자동 시작 여부")]
        [SerializeField] private bool autoStart = true;
        
        [ShowIf("preset", ParallaxPreset.Custom)]
        [Tooltip("수평 이동 강도 (0~1)"), Range(0f, 1f)]
        [SerializeField] private float horizontalStrength = 0.3f;
        
        [ShowIf("preset", ParallaxPreset.Custom)]
        [Tooltip("수직 이동 강도 (0~1)"), Range(0f, 1f)]
        [SerializeField] private float verticalStrength = 0.2f;
        
        [ShowIf("preset", ParallaxPreset.Custom)]
        [Tooltip("부드러운 이동 속도"), Range(1f, 20f)]
        [SerializeField] private float smoothSpeed = 5f;
        
        private Camera mainCamera;
        private Vector3 previousCameraPosition;
        private Vector3 originalPosition;
        private bool isRunning = false;
        
        // 프리셋별 설정값
        private float GetHorizontalStrength()
        {
            return preset switch
            {
                ParallaxPreset.Far => 0.1f,
                ParallaxPreset.Middle => 0.3f,
                ParallaxPreset.Near => 0.5f,
                ParallaxPreset.Custom => horizontalStrength,
                _ => 0.3f
            };
        }
        
        private float GetVerticalStrength()
        {
            return preset switch
            {
                ParallaxPreset.Far => 0.05f,
                ParallaxPreset.Middle => 0.2f,
                ParallaxPreset.Near => 0.4f,
                ParallaxPreset.Custom => verticalStrength,
                _ => 0.2f
            };
        }
        
        private float GetSmoothSpeed()
        {
            return preset switch
            {
                ParallaxPreset.Far => 3f,
                ParallaxPreset.Middle => 5f,
                ParallaxPreset.Near => 8f,
                ParallaxPreset.Custom => smoothSpeed,
                _ => 5f
            };
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            // 카메라 찾기
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogWarning($"[ParallaxVFX] 메인 카메라를 찾을 수 없습니다.");
                    return;
                }
            }
            
            // 원래 위치 저장
            originalPosition = transform.localPosition;
            previousCameraPosition = mainCamera.transform.position;
            
            // 자동 시작
            if (autoStart && !isRunning)
            {
                PlayVFX();
            }
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            isRunning = false;
            
            // 원래 위치로 복원
            if (transform != null)
            {
                transform.localPosition = originalPosition;
            }
        }
        
        protected override async UniTask VFXOnceInGame()
        {
            if (mainCamera == null)
            {
                Debug.LogWarning($"[ParallaxVFX] 메인 카메라가 없어 Parallax 효과를 실행할 수 없습니다.");
                return;
            }
            
            isRunning = true;
            
            try
            {
                while (isRunning && this != null && gameObject != null && gameObject.activeSelf)
                {
                    // 카메라 이동량 계산
                    Vector3 cameraMovement = mainCamera.transform.position - previousCameraPosition;
                    
                    // Parallax 오프셋 계산
                    Vector3 parallaxOffset = new Vector3(
                        cameraMovement.x * GetHorizontalStrength(),
                        cameraMovement.y * GetVerticalStrength(),
                        0f
                    );
                    
                    // 목표 위치 계산
                    Vector3 targetPosition = transform.localPosition + parallaxOffset;
                    
                    // 부드러운 이동
                    transform.localPosition = Vector3.Lerp(
                        transform.localPosition,
                        targetPosition,
                        Time.deltaTime * GetSmoothSpeed()
                    );
                    
                    // 이전 카메라 위치 업데이트
                    previousCameraPosition = mainCamera.transform.position;
                    
                    // 다음 프레임까지 대기
                    await UniTask.Yield(cancellationToken: destroyCancellation.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 취소 시 정상 종료
                isRunning = false;
            }
            finally
            {
                isRunning = false;
            }
        }
        
        protected override async UniTask VFXOnceUI()
        {
            // UI에서는 사용하지 않음
            await UniTask.Yield();
        }
        
        // 수동으로 시작
        public new void PlayVFX()
        {
            if (!isRunning)
            {
                base.PlayVFX();
            }
        }
        
        // 수동으로 중지
        public new void StopVFX()
        {
            isRunning = false;
            base.StopVFX();
            
            // 원래 위치로 복원
            if (transform != null)
            {
                transform.localPosition = originalPosition;
            }
        }
        
        // 프리셋 변경 시 즉시 적용
        private void OnValidate()
        {
            #if UNITY_EDITOR
            if (Application.isPlaying && isRunning)
            {
                // 실행 중일 때 프리셋 변경하면 즉시 반영
                previousCameraPosition = mainCamera != null ? mainCamera.transform.position : Vector3.zero;
            }
            #endif
        }
        
        #if UNITY_EDITOR
        [Button("미리보기 (Play 모드에서만)"), GUIColor(0.4f, 0.8f, 1f)]
        private void PreviewInEditor()
        {
            if (Application.isPlaying)
            {
                if (isRunning)
                {
                    StopVFX();
                }
                else
                {
                    PlayVFX();
                }
            }
            else
            {
                Debug.LogWarning("[ParallaxVFX] Play 모드에서만 미리보기가 가능합니다.");
            }
        }
        
        [Button("위치 초기화"), GUIColor(1f, 0.8f, 0.4f)]
        private void ResetPosition()
        {
            if (Application.isPlaying)
            {
                transform.localPosition = originalPosition;
                if (mainCamera != null)
                {
                    previousCameraPosition = mainCamera.transform.position;
                }
            }
            else
            {
                Debug.LogWarning("[ParallaxVFX] Play 모드에서만 초기화가 가능합니다.");
            }
        }
        #endif
    }
}
