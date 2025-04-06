using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Effect;

namespace Effect
{
    public class DropVFX : VFXObject
    {
        [Header("낙하 설정")]
        [Tooltip("초기 위치보다 높은 Y 오프셋 값"), Range(0.1f, 5f)]
        [SerializeField] private float startHeightOffset = 0.5f;
        
        [Tooltip("낙하 시간 (초)"), Range(0.1f, 3f)]
        [SerializeField] private float dropDuration = 0.5f;
        
        [Tooltip("낙하 이징 타입")]
        [SerializeField] private Ease dropEase = Ease.OutBounce;
        
        [Header("페이드 인 설정")]
        [Tooltip("페이드 인 적용 여부")]
        [SerializeField] private bool useFadeIn = true;
        
        [Tooltip("페이드 인 시간 (초)"), Range(0.1f, 3f)]
        [SerializeField] private float fadeInDuration = 0.3f;
        
        [Header("스트레치 설정")]
        [Tooltip("스트레치 효과 적용 여부")]
        [SerializeField] private bool useStretchEffect = true;
        
        [Tooltip("Y축 스트레치 배율 (착지 시)"), Range(0f, 2f)]
        [SerializeField] private float landingYStretch = 0.7f;
        
        [Tooltip("X축 스트레치 배율 (착지 시)"), Range(0f, 2f)]
        [SerializeField] private float landingXStretch = 1.3f;
        
        [Tooltip("스트레치 복원 시간 (초)"), Range(0.05f, 1f)]
        [SerializeField] private float stretchRestoreDuration = 0.2f;
        
        private Vector3 originalPosition;
        private Vector3 originalScale;
        
        // 초기화
        protected override void Start()
        {
            base.Start();
            originalScale = transform.localScale;
            originalPosition = transform.position;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            // 스케일 초기화
            originalScale = transform.localScale;
            
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
            try
            {
                // 시작 위치를 위로 올림
                Vector3 startPosition = new Vector3(
                    originalPosition.x,
                    originalPosition.y + startHeightOffset,
                    originalPosition.z
                );
                transform.position = startPosition;
                
                // 모든 효과 동시에 실행
                List<UniTask> tasks = new List<UniTask>();
                
                // 페이드 인 효과
                if (useFadeIn)
                {
                    tasks.Add(FadeIn());
                }
                
                // 낙하 애니메이션
                Tween dropTween = transform.DOMoveY(originalPosition.y, dropDuration)
                    .SetEase(dropEase);
                tasks.Add(dropTween.WithCancellation(destroyCancellation.Token));
                
                // 스트레치 효과 (낙하 중에 스트레치)
                if (useStretchEffect)
                {
                    tasks.Add(PlayStretchEffect());
                }
                
                // 모든 효과 동시 실행 및 완료 대기
                await UniTask.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // 작업 취소 처리
                Debug.Log("Drop animation canceled");
                throw;
            }
        }
        
        protected override async UniTask VFXOnceUI()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            try
            {
                // 시작 위치를 위로 올림
                Vector3 startPosition = new Vector3(
                    originalPosition.x,
                    originalPosition.y + startHeightOffset,
                    originalPosition.z
                );
                rectTransform.position = startPosition;
                
                // 모든 효과 동시에 실행
                List<UniTask> tasks = new List<UniTask>();
                
                // 페이드 인 효과
                if (useFadeIn)
                {
                    tasks.Add(FadeIn());
                }
                
                // 낙하 애니메이션
                Tween dropTween = rectTransform.DOMoveY(originalPosition.y, dropDuration)
                    .SetEase(dropEase);
                tasks.Add(dropTween.WithCancellation(destroyCancellation.Token));
                
                // 스트레치 효과 (낙하 중에 스트레치)
                if (useStretchEffect)
                {
                    tasks.Add(PlayUIStretchEffect(rectTransform));
                }
                
                // 모든 효과 동시 실행 및 완료 대기
                await UniTask.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // 작업 취소 처리
                Debug.Log("UI Drop animation canceled");
                throw;
            }
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
        
        // 스트레치 효과 (게임 오브젝트용)
        private async UniTask PlayStretchEffect()
        {
            try
            {
                // 낙하 시작과 함께 X축 늘어남
                await UniTask.Delay(TimeSpan.FromSeconds(dropDuration * 0.5f), cancellationToken: destroyCancellation.Token);
                
                // X축 늘어나고 Y축 줄어듬 (착지 효과)
                transform.localScale = new Vector3(
                    originalScale.x * landingXStretch,
                    originalScale.y * landingYStretch,
                    originalScale.z
                );
                
                // 원래 스케일로 복원
                await transform.DOScale(originalScale, stretchRestoreDuration)
                    .SetEase(Ease.OutElastic)
                    .WithCancellation(destroyCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Stretch effect canceled");
                throw;
            }
        }
        
        // 스트레치 효과 (UI용)
        private async UniTask PlayUIStretchEffect(RectTransform rectTransform)
        {
            try
            {
                // 낙하 시작과 함께 X축 늘어남
                await UniTask.Delay(TimeSpan.FromSeconds(dropDuration * 0.5f), cancellationToken: destroyCancellation.Token);
                
                // X축 늘어나고 Y축 줄어듬 (착지 효과)
                rectTransform.localScale = new Vector3(
                    originalScale.x * landingXStretch,
                    originalScale.y * landingYStretch,
                    originalScale.z
                );
                
                // 원래 스케일로 복원
                await rectTransform.DOScale(originalScale, stretchRestoreDuration)
                    .SetEase(Ease.OutElastic)
                    .WithCancellation(destroyCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("UI Stretch effect canceled");
                throw;
            }
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
