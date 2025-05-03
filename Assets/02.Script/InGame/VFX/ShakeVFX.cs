using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Effect
{
    public class ShakeVFX : VFXObject
    {
        [Header("흔들림 설정")]
        [Tooltip("Z축 최대 회전 각도 (도)"), Range(1f, 30f)]
        [SerializeField] private float maxRotationAngle = 2f;
        
        [Tooltip("흔들림 횟수"), Range(1, 10)]
        [SerializeField] private int shakeCount = 1;
        
        [Tooltip("개별 흔들림 시간 (초)"), Range(0.01f, 0.5f)]
        [SerializeField] private float shakeDuration = 0.1f;
        
        [Tooltip("복원 애니메이션 시간 (초)"), Range(0.01f, 1f)]
        [SerializeField] private float restoreDuration = 0.1f;
        
        [Tooltip("흔들림 감소 계수 (높을수록 빠르게 감소)"), Range(0f, 1f)]
        [SerializeField] private float decayFactor = 0.3f;
        
        [Tooltip("애니메이션 이징 타입")]
        [SerializeField] private Ease animationEase = Ease.OutSine;
        
        private Quaternion originalRotation;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            originalRotation = transform.localRotation;
            
            // 애니메이션을 항상 재생 가능하도록 설정
            isPlayLock = false;
        }
        
        // 애니메이션 리셋
        private void ResetAnimation()
        {
            if (this == null || transform == null) return;
            transform.DOKill();
            transform.localRotation = originalRotation;
        }
        
        // 매개변수 없는 PlayVFX 오버라이드
        public new void PlayVFX()
        {
            // 애니메이션 상태 초기화
            ResetAnimation();
            
            // 부모 클래스 메서드 호출
            base.PlayVFX();
        }
        
        // PlayVFXForce 오버라이드
        public new void PlayVFXForce()
        {
            // 애니메이션 상태 초기화
            ResetAnimation();
            
            // 부모 클래스 메서드 호출
            base.PlayVFXForce();
        }
        
        protected override async UniTask VFXOnceInGame()
        {
            // 기존 애니메이션 중지 및 초기화
            ResetAnimation();
            
            try
            {
                float currentAngle = maxRotationAngle + effectAddValue;
                
                // 지정된 횟수만큼 반복
                for (int i = 0; i < shakeCount; i++)
                {
                    // 감쇠 적용
                    currentAngle *= (1f - (decayFactor * i / shakeCount));
                    
                    // 왼쪽으로 회전
                    await transform.DORotate(new Vector3(0, 0, currentAngle), shakeDuration * effectSpeed)
                        .SetEase(animationEase)
                        .WithCancellation(destroyCancellation.Token);
                    
                    // 오른쪽으로 회전
                    await transform.DORotate(new Vector3(0, 0, -currentAngle), shakeDuration * effectSpeed * 2)
                        .SetEase(animationEase)
                        .WithCancellation(destroyCancellation.Token);
                    
                    // 다시 왼쪽으로 회전 (마지막이 아닌 경우만)
                    if (i < shakeCount - 1)
                    {
                        await transform.DORotate(new Vector3(0, 0, currentAngle), shakeDuration * effectSpeed)
                            .SetEase(animationEase)
                            .WithCancellation(destroyCancellation.Token);
                    }
                }
                
                // 원래 회전으로 복원
                await transform.DORotate(originalRotation.eulerAngles, restoreDuration * effectSpeed)
                    .SetEase(Ease.OutElastic, 0.5f, 0.2f)
                    .WithCancellation(destroyCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // 작업 취소 시 회전 초기화
                ResetAnimation();
                throw;
            }
        }
        
        protected override async UniTask VFXOnceUI()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            // UI 회전 기록
            Quaternion originalUIRotation = rectTransform.localRotation;
            
            // 기존 애니메이션 중지
            rectTransform.DOKill();
            rectTransform.localRotation = originalUIRotation;
            
            try
            {
                float currentAngle = maxRotationAngle + effectAddValue;
                
                // 지정된 횟수만큼 반복
                for (int i = 0; i < shakeCount; i++)
                {
                    // 감쇠 적용
                    currentAngle *= (1f - (decayFactor * i / shakeCount));
                    
                    // 왼쪽으로 회전
                    await rectTransform.DORotate(new Vector3(0, 0, currentAngle), shakeDuration * effectSpeed)
                        .SetEase(animationEase)
                        .WithCancellation(destroyCancellation.Token);
                    
                    // 오른쪽으로 회전
                    await rectTransform.DORotate(new Vector3(0, 0, -currentAngle), shakeDuration * effectSpeed * 2)
                        .SetEase(animationEase)
                        .WithCancellation(destroyCancellation.Token);
                    
                    // 다시 왼쪽으로 회전 (마지막이 아닌 경우만)
                    if (i < shakeCount - 1)
                    {
                        await rectTransform.DORotate(new Vector3(0, 0, currentAngle), shakeDuration * effectSpeed)
                            .SetEase(animationEase)
                            .WithCancellation(destroyCancellation.Token);
                    }
                }
                
                // 원래 회전으로 복원
                await rectTransform.DORotate(originalUIRotation.eulerAngles, restoreDuration * effectSpeed)
                    .SetEase(Ease.OutElastic, 0.5f, 0.2f)
                    .WithCancellation(destroyCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // 작업 취소 시 회전 초기화
                if (rectTransform != null)
                {
                    rectTransform.DOKill();
                    rectTransform.localRotation = originalUIRotation;
                }
                throw;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ResetAnimation();
        }
        
        // 에디터에서 현재 설정으로 미리보기 실행
        [Button("미리보기"), GUIColor(0.4f, 0.8f, 1f)]
        private void PreviewInEditor()
        {
            #if UNITY_EDITOR
            if (Application.isPlaying)
            {
                ResetAnimation();
                VFXOnceInGame().Forget();
            }
            else
            {
                // 에디터 모드에서 미리보기 실행
                EditorPreviewAnimation();
            }
            #endif
        }
        
        #if UNITY_EDITOR
        // 에디터 모드에서 미리보기 애니메이션을 실행하는 함수
        private void EditorPreviewAnimation()
        {
            // 현재 회전 저장
            originalRotation = transform.localRotation;
            
            // 애니메이션 취소 및 초기화
            transform.DOKill();
            
            // 에디터 업데이트를 통해 애니메이션 효과를 시뮬레이션
            EditorApplication.update += EditorAnimationUpdate;
            
            // 애니메이션 단계 및 타이머 초기화
            _editorAnimStep = 0;
            _editorAnimSubStep = 0;
            _editorAnimTimer = 0f;
            _currentAngle = maxRotationAngle + effectAddValue;
        }
        
        private int _editorAnimStep = 0;
        private int _editorAnimSubStep = 0;
        private float _editorAnimTimer = 0f;
        private float _currentAngle = 0f;
        private const float EDITOR_TIME_STEP = 0.01f; // 에디터 업데이트 시간 간격
        
        private void EditorAnimationUpdate()
        {
            if (this == null || transform == null)
            {
                EditorApplication.update -= EditorAnimationUpdate;
                return;
            }
            
            _editorAnimTimer += EDITOR_TIME_STEP;
            
            // 현재 흔들림 스텝에 따라 애니메이션 처리
            if (_editorAnimStep < shakeCount)
            {
                float currentShakeAngle = _currentAngle * (1f - (decayFactor * _editorAnimStep / shakeCount));
                
                switch (_editorAnimSubStep)
                {
                    case 0: // 왼쪽으로 회전
                        if (_editorAnimTimer <= shakeDuration * effectSpeed)
                        {
                            float t = _editorAnimTimer / (shakeDuration * effectSpeed);
                            float easedT = DOVirtual.EasedValue(0, 1, t, animationEase);
                            Vector3 newRotation = Vector3.Lerp(originalRotation.eulerAngles, new Vector3(0, 0, currentShakeAngle), easedT);
                            transform.localRotation = Quaternion.Euler(newRotation);
                        }
                        else
                        {
                            _editorAnimTimer = 0f;
                            _editorAnimSubStep = 1;
                        }
                        break;
                    
                    case 1: // 오른쪽으로 회전
                        if (_editorAnimTimer <= shakeDuration * effectSpeed * 2)
                        {
                            float t = _editorAnimTimer / (shakeDuration * effectSpeed * 2);
                            float easedT = DOVirtual.EasedValue(0, 1, t, animationEase);
                            Vector3 startRot = new Vector3(0, 0, currentShakeAngle);
                            Vector3 endRot = new Vector3(0, 0, -currentShakeAngle);
                            Vector3 newRotation = Vector3.Lerp(startRot, endRot, easedT);
                            transform.localRotation = Quaternion.Euler(newRotation);
                        }
                        else
                        {
                            _editorAnimTimer = 0f;
                            _editorAnimSubStep = 2;
                        }
                        break;
                    
                    case 2: // 마지막이 아니면 다시 왼쪽으로
                        if (_editorAnimStep < shakeCount - 1)
                        {
                            if (_editorAnimTimer <= shakeDuration * effectSpeed)
                            {
                                float t = _editorAnimTimer / (shakeDuration * effectSpeed);
                                float easedT = DOVirtual.EasedValue(0, 1, t, animationEase);
                                Vector3 startRot = new Vector3(0, 0, -currentShakeAngle);
                                Vector3 endRot = new Vector3(0, 0, currentShakeAngle);
                                Vector3 newRotation = Vector3.Lerp(startRot, endRot, easedT);
                                transform.localRotation = Quaternion.Euler(newRotation);
                            }
                            else
                            {
                                _editorAnimTimer = 0f;
                                _editorAnimSubStep = 0;
                                _editorAnimStep++;
                            }
                        }
                        else
                        {
                            _editorAnimTimer = 0f;
                            _editorAnimStep = shakeCount; // 복원 단계로 진행
                        }
                        break;
                }
            }
            else
            {
                // 복원 애니메이션
                if (_editorAnimTimer <= restoreDuration * effectSpeed)
                {
                    float t = _editorAnimTimer / (restoreDuration * effectSpeed);
                    // OutElastic 이징을 시뮬레이션
                    float overshoot = 0.5f;
                    float period = 0.2f;
                    float easedT = DOVirtual.EasedValue(0, 1, t, Ease.OutElastic, overshoot, period);
                    
                    Vector3 startRot = new Vector3(0, 0, -_currentAngle * (1f - (decayFactor * (shakeCount - 1) / shakeCount)));
                    Vector3 endRot = originalRotation.eulerAngles;
                    Vector3 newRotation = Vector3.Lerp(startRot, endRot, easedT);
                    transform.localRotation = Quaternion.Euler(newRotation);
                }
                else
                {
                    // 애니메이션 종료
                    transform.localRotation = originalRotation;
                    EditorApplication.update -= EditorAnimationUpdate;
                }
            }
            
            // 에디터 업데이트 요청
            SceneView.RepaintAll();
        }
        #endif
    }
}
