using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using Sirenix.OdinInspector;

namespace Effect
{
    public class GrowVFX : VFXObject
    {
        [Header("Growth Settings")]
        [SerializeField, Range(0f, 1f)] private float startScale = 0.3f;
        [SerializeField] private bool useCustomStartScale = false;
        [SerializeField, Range(0f, 2f)] private float customStartScale = 0.3f;
        [SerializeField, Range(0.1f, 2f)] private float growthDuration = 0.5f;
        [SerializeField, Range(0f, 10f)] private float growthDelay = 0f;
        [SerializeField] private Ease growthEase = Ease.OutBack;
        
        [Header("페이드 인 설정")]
        [Tooltip("페이드 인 적용 여부")]
        [SerializeField] private bool useFadeIn = false;
        
        [Tooltip("페이드 인 시간 (초)"), Range(0.1f, 3f)]
        [SerializeField] private float fadeInDuration = 0.3f;
        
        private Vector3 originalScale;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            originalScale = transform.localScale;
            if (useCustomStartScale)
            {
                transform.localScale = originalScale * customStartScale;
            }
            
            // 페이드 인 시작 시 투명하게 설정
            if (useFadeIn)
            {
                SetAlpha(0f);
            }
        }
        
        // 알파값 설정 (모든 렌더러에 적용)
        private void SetAlpha(float alpha)
        {
            // SpriteRenderer가 있는 경우
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
            
            // CanvasGroup이 있는 경우 (UI 요소)
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
        }
        
        protected override async UniTask VFXOnceInGame()
        {
            await base.VFXOnceInGame();
            
            try 
            {
                // 시작 크기로 설정 (custom 선택 시 입력한 값 사용)
                var appliedStart = useCustomStartScale ? customStartScale : startScale;
                transform.localScale = originalScale * appliedStart;

                // 모든 효과 동시에 실행
                List<UniTask> tasks = new List<UniTask>();
                
                // 페이드 인 효과
                if (useFadeIn)
                {
                    tasks.Add(FadeIn());
                }

                // 딜레이 후 성장 애니메이션
                tasks.Add(GrowAnimation(transform));

                // 모든 효과 동시 실행 및 완료 대기
                await UniTask.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Grow animation canceled");
                throw;
            }
            finally 
            {
                if (this != null && transform != null)
                {
                    transform.localScale = originalScale;
                }
            }
        }
        
        protected override async UniTask VFXOnceUI()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            try
            {
                // 시작 크기로 설정 (custom 선택 시 입력한 값 사용)
                var appliedStart = useCustomStartScale ? customStartScale : startScale;
                rectTransform.localScale = originalScale * appliedStart;
                
                // 모든 효과 동시에 실행
                List<UniTask> tasks = new List<UniTask>();
                
                // 페이드 인 효과
                if (useFadeIn)
                {
                    tasks.Add(FadeIn());
                }
                
                // 딜레이 후 성장 애니메이션
                tasks.Add(GrowAnimation(rectTransform));
                
                // 모든 효과 동시 실행 및 완료 대기
                await UniTask.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("UI Grow animation canceled");
                throw;
            }
            finally
            {
                if (this != null && rectTransform != null)
                {
                    rectTransform.localScale = originalScale;
                }
            }
        }
        
        // 성장 애니메이션 (InGame / UI 공통)
        private async UniTask GrowAnimation(Transform target)
        {
            // 딜레이 적용
            if (growthDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(growthDelay), cancellationToken: destroyCancellation.Token);
            }

            // 원래 크기로 성장
            await target.DOScale(originalScale, growthDuration)
                .SetEase(growthEase)
                .WithCancellation(destroyCancellation.Token);
        }
        
        // 페이드 인 효과
        private async UniTask FadeIn()
        {
            try
            {
                float currentTime = 0;
                
                while (currentTime < fadeInDuration)
                {
                    currentTime += Time.deltaTime;
                    float normalizedTime = currentTime / fadeInDuration;
                    float alpha = Mathf.Lerp(0f, 1f, normalizedTime);
                    
                    SetAlpha(alpha);
                    
                    await UniTask.Yield(destroyCancellation.Token);
                }
                
                // 완전히 불투명하게 설정
                SetAlpha(1f);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Fade in canceled");
                throw;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            transform.DOKill();
            transform.localScale = originalScale;
        }
        
        // 에디터에서 미리보기
        [Button("미리보기 재생"), GUIColor(0.4f, 0.8f, 1f)]
        private void PreviewInEditor()
        {
            #if UNITY_EDITOR
            if (Application.isPlaying)
            {
                PlayVFXForce();
            }
            #endif
        }
    }
}
