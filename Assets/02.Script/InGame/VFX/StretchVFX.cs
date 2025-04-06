using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using Sirenix.OdinInspector;

namespace Effect
{
    public class StretchVFX : VFXObject
    {
        [Header("애니메이션 설정")]
        [Tooltip("X축 및 Y축 늘어남 배율"), Range(1f, 2f)]
        [SerializeField] private float stretchMultiplier = 1.1f;
        
        [Tooltip("X축 늘어남/복원 애니메이션 시간 (초)"), Range(0.01f, 1f)]
        [SerializeField] private float xAnimDuration = 0.1f;
        
        [Tooltip("Y축 복원 애니메이션 시간 (초)"), Range(0.01f, 1f)]
        [SerializeField] private float yRestoreDuration = 0.15f;
        
        [Tooltip("모든 애니메이션에 적용할 이징 타입")]
        [SerializeField] private Ease animationEase = Ease.OutSine;
        
        private Vector3 originalScale;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            originalScale = transform.localScale;
            
            // 애니메이션을 항상 재생 가능하도록 설정
            isPlayLock = false;
        }
        
        // 애니메이션 리셋
        private void ResetAnimation()
        {
            if (this == null || transform == null) return;
            transform.DOKill();
            transform.localScale = originalScale;
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
                // 정확하게 StretchEffect와 같은 애니메이션 패턴 사용
                // X축 늘어나기
                await transform.DOScaleX(originalScale.x * stretchMultiplier + effectAddValue, 0.1f * effectSpeed)
                    .SetEase(Ease.OutSine)
                    .WithCancellation(destroyCancellation.Token);
                
                // X축 복원 및 Y축 늘어나기 동시에
                await UniTask.WhenAll(
                    transform.DOScaleX(originalScale.x, 0.1f * effectSpeed)
                        .SetEase(Ease.OutSine)
                        .WithCancellation(destroyCancellation.Token),
                    transform.DOScaleY(originalScale.y * stretchMultiplier + effectAddValue, 0.1f * effectSpeed)
                        .SetEase(Ease.OutSine)
                        .WithCancellation(destroyCancellation.Token)
                );
                
                // Y축 복원
                await transform.DOScaleY(originalScale.y, 0.15f * effectSpeed)
                    .SetEase(Ease.OutSine)
                    .WithCancellation(destroyCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // 작업 취소 시 스케일 초기화
                ResetAnimation();
                throw;
            }
        }
        
        protected override async UniTask VFXOnceUI()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            // UI 스케일 기록
            Vector3 originalUIScale = rectTransform.localScale;
            
            // 기존 애니메이션 중지
            rectTransform.DOKill();
            rectTransform.localScale = originalUIScale;
            
            try
            {
                // X축 늘어나기
                await rectTransform.DOScaleX(originalUIScale.x * stretchMultiplier + effectAddValue, 0.1f * effectSpeed)
                    .SetEase(Ease.OutSine)
                    .WithCancellation(destroyCancellation.Token);
                
                // X축 복원 및 Y축 늘어나기 동시에
                await UniTask.WhenAll(
                    rectTransform.DOScaleX(originalUIScale.x, 0.1f).SetEase(Ease.OutSine)
                        .WithCancellation(destroyCancellation.Token),
                    rectTransform.DOScaleY(originalUIScale.y * stretchMultiplier + effectAddValue, 0.1f * effectSpeed)
                        .SetEase(Ease.OutSine)
                        .WithCancellation(destroyCancellation.Token)
                );
                
                // Y축 복원
                await rectTransform.DOScaleY(originalUIScale.y, 0.15f * effectSpeed)
                    .SetEase(Ease.OutSine)
                    .WithCancellation(destroyCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // 작업 취소 시 스케일 초기화
                if (rectTransform != null)
                {
                    rectTransform.DOKill();
                    rectTransform.localScale = originalUIScale;
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
