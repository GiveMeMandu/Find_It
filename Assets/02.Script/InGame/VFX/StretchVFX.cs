using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
                await transform.DOScaleX(originalScale.x * stretchMultiplier + effectAddValue, xAnimDuration * effectSpeed)
                    .SetEase(animationEase)
                    .WithCancellation(destroyCancellation.Token);
                
                // X축 복원 및 Y축 늘어나기 동시에
                await UniTask.WhenAll(
                    transform.DOScaleX(originalScale.x, xAnimDuration * effectSpeed)
                        .SetEase(animationEase)
                        .WithCancellation(destroyCancellation.Token),
                    transform.DOScaleY(originalScale.y * stretchMultiplier + effectAddValue, xAnimDuration * effectSpeed)
                        .SetEase(animationEase)
                        .WithCancellation(destroyCancellation.Token)
                );
                
                // Y축 복원
                await transform.DOScaleY(originalScale.y, yRestoreDuration * effectSpeed)
                    .SetEase(animationEase)
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
                await rectTransform.DOScaleX(originalUIScale.x * stretchMultiplier + effectAddValue, xAnimDuration * effectSpeed)
                    .SetEase(animationEase)
                    .WithCancellation(destroyCancellation.Token);
                
                // X축 복원 및 Y축 늘어나기 동시에
                await UniTask.WhenAll(
                    rectTransform.DOScaleX(originalUIScale.x, xAnimDuration * effectSpeed)
                        .SetEase(animationEase)
                        .WithCancellation(destroyCancellation.Token),
                    rectTransform.DOScaleY(originalUIScale.y * stretchMultiplier + effectAddValue, xAnimDuration * effectSpeed)
                        .SetEase(animationEase)
                        .WithCancellation(destroyCancellation.Token)
                );
                
                // Y축 복원
                await rectTransform.DOScaleY(originalUIScale.y, yRestoreDuration * effectSpeed)
                    .SetEase(animationEase)
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
            // 현재 스케일 저장
            Vector3 currentScale = transform.localScale;
            originalScale = currentScale;
            
            // 애니메이션 취소 및 초기화
            transform.DOKill();
            
            EditorApplication.update += EditorAnimationUpdate;
            
            // 애니메이션 단계 및 타이머 초기화
            _editorAnimStep = 0;
            _editorAnimTimer = 0f;
        }
        
        private int _editorAnimStep = 0;
        private float _editorAnimTimer = 0f;
        private const float EDITOR_TIME_STEP = 0.01f; // 에디터 업데이트 시간 간격
        
        private void EditorAnimationUpdate()
        {
            if (this == null || transform == null)
            {
                EditorApplication.update -= EditorAnimationUpdate;
                return;
            }
            
            _editorAnimTimer += EDITOR_TIME_STEP;
            
            switch (_editorAnimStep)
            {
                case 0: // X축 늘어나기
                    if (_editorAnimTimer <= xAnimDuration * effectSpeed)
                    {
                        float t = _editorAnimTimer / (xAnimDuration * effectSpeed);
                        float easedT = DOVirtual.EasedValue(0, 1, t, animationEase);
                        float newXScale = Mathf.Lerp(originalScale.x, originalScale.x * stretchMultiplier + effectAddValue, easedT);
                        transform.localScale = new Vector3(newXScale, originalScale.y, originalScale.z);
                    }
                    else
                    {
                        _editorAnimStep = 1;
                        _editorAnimTimer = 0f;
                    }
                    break;
                
                case 1: // X축 복원 및 Y축 늘어나기
                    if (_editorAnimTimer <= xAnimDuration * effectSpeed)
                    {
                        float t = _editorAnimTimer / (xAnimDuration * effectSpeed);
                        float easedT = DOVirtual.EasedValue(0, 1, t, animationEase);
                        float newXScale = Mathf.Lerp(originalScale.x * stretchMultiplier + effectAddValue, originalScale.x, easedT);
                        float newYScale = Mathf.Lerp(originalScale.y, originalScale.y * stretchMultiplier + effectAddValue, easedT);
                        transform.localScale = new Vector3(newXScale, newYScale, originalScale.z);
                    }
                    else
                    {
                        _editorAnimStep = 2;
                        _editorAnimTimer = 0f;
                    }
                    break;
                
                case 2: // Y축 복원
                    if (_editorAnimTimer <= yRestoreDuration * effectSpeed)
                    {
                        float t = _editorAnimTimer / (yRestoreDuration * effectSpeed);
                        float easedT = DOVirtual.EasedValue(0, 1, t, animationEase);
                        float newYScale = Mathf.Lerp(originalScale.y * stretchMultiplier + effectAddValue, originalScale.y, easedT);
                        transform.localScale = new Vector3(originalScale.x, newYScale, originalScale.z);
                    }
                    else
                    {
                        // 애니메이션 종료
                        transform.localScale = originalScale;
                        EditorApplication.update -= EditorAnimationUpdate;
                    }
                    break;
            }
            
            // 에디터 업데이트 요청
            SceneView.RepaintAll();
        }
        #endif
    }
}
