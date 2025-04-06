using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;

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
            #endif
        }
    }
}
