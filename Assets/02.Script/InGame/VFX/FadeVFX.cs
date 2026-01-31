using UnityEngine;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Effect
{
    public class FadeVFX : VFXObject
    {
        public enum FadeMode
        {
            Single,     // Start -> End
            PingPong    // Start -> End -> Start
        }

        [Header("페이드 설정")]
        [Tooltip("페이드 동작 모드")]
        [SerializeField] private FadeMode fadeMode = FadeMode.Single;

        [Tooltip("시작 알파값 (0~1)"), Range(0f, 1f)]
        [SerializeField] private float startAlpha = 1f;

        [Tooltip("목표 알파값 (0~1)"), Range(0f, 1f)]
        [SerializeField] private float endAlpha = 0f;

        [Tooltip("페이드 시간 (초)"), Range(0.01f, 5f)]
        [SerializeField] private float duration = 0.5f;

        [Tooltip("애니메이션 이징 타입")]
        [SerializeField] private Ease animationEase = Ease.Linear;

        [Tooltip("시작 시 현재 알파값 무시하고 강제 설정 여부")]
        [SerializeField] private bool forceStartAlpha = true;

        [Header("루프 설정")]
        [Tooltip("루프 시 왕복 재생 여부 (Single 모드일 때만 유효, 1->0, 0->1 반복)")]
        [SerializeField] private bool useYoyoLoop = false;

        private float _originalAlpha;
        private bool _isYoyoReverse = false;
        private SpriteRenderer _spriteRenderer;
        private CanvasGroup _canvasGroup;
        private Graphic _graphic;

        protected override void OnEnable()
        {
            base.OnEnable();
            InitializeComponents();
            if (forceStartAlpha)
            {
                ApplyAlpha(startAlpha);
            }
            // 애니메이션을 항상 재생 가능하도록 설정
            isPlayLock = false;
        }

        private void InitializeComponents()
        {
            if (isUIEffect)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _graphic = GetComponent<Graphic>();
                }
            }
            else
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            
            // 초기 알파값 저장 (필요 시 복원을 위해)
            _originalAlpha = GetCurrentAlpha();
        }

        private float GetCurrentAlpha()
        {
            if (isUIEffect)
            {
                if (_canvasGroup != null) return _canvasGroup.alpha;
                if (_graphic != null) return _graphic.color.a;
            }
            else
            {
                if (_spriteRenderer != null) return _spriteRenderer.color.a;
            }
            return 1f;
        }

        private void ApplyAlpha(float alpha)
        {
            if (isUIEffect)
            {
                if (_canvasGroup != null) _canvasGroup.alpha = alpha;
                else if (_graphic != null)
                {
                    Color c = _graphic.color;
                    c.a = alpha;
                    _graphic.color = c;
                }
            }
            else
            {
                if (_spriteRenderer != null)
                {
                    Color c = _spriteRenderer.color;
                    c.a = alpha;
                    _spriteRenderer.color = c;
                }
            }
        }

        // 애니메이션 리셋
        private void ResetAnimation()
        {
            if (this == null || transform == null) return;
            
            _isYoyoReverse = false;

            // 트윈 킬
            if (isUIEffect)
            {
                if (_canvasGroup != null) _canvasGroup.DOKill();
                else if (_graphic != null) _graphic.DOKill();
            }
            else
            {
                if (_spriteRenderer != null) _spriteRenderer.DOKill();
            }

            // 시작 값으로 초기화 (강제 설정인 경우)
            if (forceStartAlpha)
            {
                ApplyAlpha(startAlpha);
            }
            else
            {
                // 강제가 아니면 원래 알파값으로 복원? 
                // 보통 Fade는 연속될 수 있으므로 현재 상태 유지 혹은 StartAlpha로 가는게 맞음.
                // 여기서는 Play 시점에 명확히 정의하므로 Reset은 StartAlpha로 보냄.
                ApplyAlpha(startAlpha); 
            }
        }

        public new void PlayVFX()
        {
            ResetAnimation();
            base.PlayVFX();
        }

        public new void PlayVFXForce()
        {
            ResetAnimation();
            base.PlayVFXForce();
        }

        protected override async UniTask VFXOnceInGame()
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null) return;

            // 시작 전 초기화
            _spriteRenderer.DOKill();

            float currentStart = (_isYoyoReverse && fadeMode == FadeMode.Single) ? endAlpha : startAlpha;
            float currentEnd = (_isYoyoReverse && fadeMode == FadeMode.Single) ? startAlpha : endAlpha;

            if (forceStartAlpha) ApplyAlpha(currentStart);

            try
            {
                // 1단계: Start -> End
                await _spriteRenderer.DOFade(currentEnd, duration * effectSpeed)
                    .SetEase(animationEase)
                    .WithCancellation(destroyCancellation.Token);

                // 2단계: PingPong인 경우 End -> Start (YoyoLoop가 아닐 때만 혹은 핑퐁 로직 유지)
                // PingPong은 자체적으로 돌아오므로 YoyoLoop 로직을 타지 않게 하거나, 
                // YoyoLoop는 Single 모드 전용으로 안내했으므로 Single일 때만 반전 처리.
                if (fadeMode == FadeMode.PingPong)
                {
                    await _spriteRenderer.DOFade(startAlpha, duration * effectSpeed)
                        .SetEase(animationEase)
                        .WithCancellation(destroyCancellation.Token);
                }
                else if (useYoyoLoop)
                {
                    _isYoyoReverse = !_isYoyoReverse;
                }
            }
            catch (OperationCanceledException)
            {
                ResetAnimation();
                throw;
            }
        }

        protected override async UniTask VFXOnceUI()
        {
            InitializeComponents(); // 컴포넌트 재확인
            
            if (_canvasGroup == null && _graphic == null) return;

            // 트윈 대상 식별
            // object target = _canvasGroup != null ? (object)_canvasGroup : (object)_graphic;
            
            // 시작 전 초기화
            if (_canvasGroup != null) _canvasGroup.DOKill();
            else _graphic.DOKill();

            float currentStart = (_isYoyoReverse && fadeMode == FadeMode.Single) ? endAlpha : startAlpha;
            float currentEnd = (_isYoyoReverse && fadeMode == FadeMode.Single) ? startAlpha : endAlpha;

            if (forceStartAlpha) ApplyAlpha(currentStart);

            try
            {
                // 1단계: Start -> End
                if (_canvasGroup != null)
                {
                    await _canvasGroup.DOFade(currentEnd, duration * effectSpeed)
                        .SetEase(animationEase)
                        .WithCancellation(destroyCancellation.Token);
                }
                else
                {
                    await _graphic.DOFade(currentEnd, duration * effectSpeed)
                        .SetEase(animationEase)
                        .WithCancellation(destroyCancellation.Token);
                }

                // 2단계: PingPong인 경우 End -> Start
                if (fadeMode == FadeMode.PingPong)
                {
                    if (_canvasGroup != null)
                    {
                        await _canvasGroup.DOFade(startAlpha, duration * effectSpeed)
                            .SetEase(animationEase)
                            .WithCancellation(destroyCancellation.Token);
                    }
                    else
                    {
                        await _graphic.DOFade(startAlpha, duration * effectSpeed)
                            .SetEase(animationEase)
                            .WithCancellation(destroyCancellation.Token);
                    }
                }
                else if (useYoyoLoop)
                {
                    _isYoyoReverse = !_isYoyoReverse;
                }
            }
            catch (OperationCanceledException)
            {
                // UI는 취소 시 리셋할지, 유지할지 결정해야 함. 보통 리셋.
                if (forceStartAlpha) ApplyAlpha(startAlpha);
                throw;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // 비활성화 시 트윈 킬
             if (isUIEffect)
            {
                if (_canvasGroup != null) _canvasGroup.DOKill();
                else if (_graphic != null) _graphic.DOKill();
            }
            else
            {
                if (_spriteRenderer != null) _spriteRenderer.DOKill();
            }
        }

        // 에디터 미리보기
        [Button("미리보기"), GUIColor(0.4f, 0.8f, 1f)]
        private void PreviewInEditor()
        {
            #if UNITY_EDITOR
            if (Application.isPlaying)
            {
                ResetAnimation();
                if(isUIEffect) VFXOnceUI().Forget();
                else VFXOnceInGame().Forget();
            }
            else
            {
                EditorPreviewAnimation();
            }
            #endif
        }

        #if UNITY_EDITOR
        private void EditorPreviewAnimation()
        {
            InitializeComponents();
            if (!isUIEffect && _spriteRenderer == null) return;
            if (isUIEffect && _canvasGroup == null && _graphic == null) return;

            // 초기화
            if (forceStartAlpha) ApplyAlpha(startAlpha);
            
            // 에디터 업데이트 등록
            EditorApplication.update += EditorAnimationUpdate;
            _editorAnimTimer = 0f;
            _editorIsPingPongPhase = false;
        }

        private float _editorAnimTimer = 0f;
        private bool _editorIsPingPongPhase = false;

        private void EditorAnimationUpdate()
        {
            if (this == null || transform == null)
            {
                EditorApplication.update -= EditorAnimationUpdate;
                return;
            }

            _editorAnimTimer += 0.01f; // 대략적인 델타타임 (정확하지 않음)
            
            float currentDuration = duration * effectSpeed;
            if (currentDuration <= 0) currentDuration = 0.01f;

            float t = _editorAnimTimer / currentDuration;
            
            if (t <= 1f)
            {
                float easedT = DOVirtual.EasedValue(0, 1, t, animationEase);
                float currentVal;
                
                if (!_editorIsPingPongPhase)
                {
                    // Start -> End
                    currentVal = Mathf.Lerp(startAlpha, endAlpha, easedT);
                }
                else
                {
                    // End -> Start
                    currentVal = Mathf.Lerp(endAlpha, startAlpha, easedT);
                }
                
                ApplyAlpha(currentVal);
            }
            else
            {
                // 단계 종료
                if (fadeMode == FadeMode.PingPong && !_editorIsPingPongPhase)
                {
                    // 핑퐁 2단계 진입
                    _editorIsPingPongPhase = true;
                    _editorAnimTimer = 0f;
                    ApplyAlpha(endAlpha);
                }
                else
                {
                    // 애니메이션 종료
                    float finalAlpha = (fadeMode == FadeMode.PingPong) ? startAlpha : endAlpha;
                    ApplyAlpha(finalAlpha);
                    EditorApplication.update -= EditorAnimationUpdate;
                }
            }
            
            // 씬 뷰 갱신
            // EditorUtility.SetDirty(this); // 필요 시
        }
        #endif
    }
}
