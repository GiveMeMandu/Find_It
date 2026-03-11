using System;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Random = UnityEngine.Random;

namespace Effect
{
    [System.Flags]
    public enum SquashAxis
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4
    }

    public enum SquashPivot
    {
        Center,
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public class SquishyEffect : VFXObject
    {
        [BoxGroup("기본 설정")]
        [Label("스쿼시 축")]
        [SerializeField]
        private SquashAxis squashAxis = SquashAxis.Y;

        [BoxGroup("기본 설정")]
        [Label("애니메이션 모드")]
        [SerializeField]
        private AnimationMode animationMode = AnimationMode.DOTween;

        [BoxGroup("기본 설정")]
        [Label("재생 확률 (%)")]
        [SerializeField]
        [Range(0f, 100f)]
        private float chanceToPlay = 100f;

        [BoxGroup("기본 설정")]
        [Label("커스텀 초기 스케일 사용")]
        [SerializeField]
        private bool useCustomOriginalScale = false;

        [BoxGroup("기본 설정")]
        [Label("커스텀 초기 스케일 (축별)")]
        [ShowIf("useCustomOriginalScale")]
        [SerializeField]
        private Vector3 customOriginalScale = Vector3.one;

        [Header("DOTween 모드 설정")]
        [BoxGroup("DOTween 설정")]
        [Label("수평 스쿼시 비율")]
        [SerializeField]
        [Range(0f, 2f)]
        private float squashHorizontalRatio = 0.7f;

        [BoxGroup("DOTween 설정")]
        [Label("수직 스쿼시 비율")]
        [SerializeField]
        [Range(0f, 2f)]
        private float squashVerticalRatio = 0.7f;

        [BoxGroup("랜덤 설정")]
        [Label("랜덤 사용")]
        [SerializeField]
        private bool useRandom = false;
        
        [BoxGroup("DOTween 설정")]
        [Label("스쿼시 랜덤 범위")]
        [SerializeField]
        [Range(0f, 0.5f)]
        private float squashRatioRandomRange = 0f;

        [BoxGroup("DOTween 설정")]
        [Label("스쿼시 지속시간")]
        [SerializeField]
        [Range(0.01f, 1f)]
        private float squashDuration = 0.1f;

        [BoxGroup("DOTween 설정")]
        [Label("Y축 지연시간")]
        [SerializeField]
        [Range(0f, 1f)]
        private float yAxisDelay = 0f;

        [BoxGroup("Tween 설정")]
        [Label("스쿼시 Ease")]
        [SerializeField]
        private Ease squashEase = Ease.OutSine;

        [BoxGroup("Tween 설정")]
        [Label("복원 Ease")]
        [SerializeField]
        private Ease restoreEase = Ease.OutSine;

        [BoxGroup("Tween 설정")]
        [Label("복원 지속시간 배율")]
        [SerializeField]
        [Range(0.5f, 3f)]
        private float restoreDurationMultiplier = 1.5f;

        [BoxGroup("커브 설정")]
        [Label("애니메이션 지속시간")]
        [SerializeField]
        [Range(0.01f, 2f)]
        private float animationDuration = 0.25f;

        [BoxGroup("커브 설정")]
        [Label("초기 스케일")]
        [SerializeField]
        private float initialScale = 1f;

        [BoxGroup("커브 설정")]
        [Label("최대 스케일")]
        [SerializeField]
        private float maximumScale = 1.3f;

        [BoxGroup("커브 설정")]
        [Label("스쿼시 커브")]
        [SerializeField]
        private AnimationCurve squashCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 1f),
            new Keyframe(1f, 0f)
        );

        [BoxGroup("피벗 설정")]
        [Label("피벗 변경 사용")]
        [SerializeField]
        private bool usePivotOffset = false;

        [BoxGroup("피벗 설정")]
        [Label("스쿼시 피벗")]
        [SerializeField]
        private SquashPivot squashPivot = SquashPivot.Center;

        [BoxGroup("피벗 설정")]
        [Label("피벗 이동 배율")]
        [SerializeField]
        [Range(0f, 2f)]
        private float pivotOffsetMultiplier = 1f;

        [BoxGroup("랜덤 설정")]
        [Label("수평 스쿼시 랜덤 최소값")]
        [SerializeField]
        [Range(0f, 2f)]
        private float squashHorizontalRandomMin = 0.1f;

        [BoxGroup("랜덤 설정")]
        [Label("수직 스쿼시 랜덤 최소값")]
        [SerializeField]
        [Range(0f, 2f)]
        private float squashVerticalRandomMin = 0.1f;

        [BoxGroup("랜덤 설정")]
        [Label("스쿼시 지속시간 랜덤 최소값")]
        [SerializeField]
        [Range(0.01f, 1f)]
        private float squashDurationRandomMin = 0.01f;

        public enum AnimationMode
        {
            DOTween,
            AnimationCurve
        }

        private Vector3 originalScale;
        private Vector3 originalPosition;
        private Vector2 originalAnchoredPosition;

        // 랜덤값 저장 변수
        private float randomSquashHorizontal;
        private float randomSquashVertical;
        private float randomDuration;

        // 축 체크 프로퍼티
        private bool affectX => (squashAxis & SquashAxis.X) != 0;
        private bool affectY => (squashAxis & SquashAxis.Y) != 0;
        private bool affectZ => (squashAxis & SquashAxis.Z) != 0;

        protected override void OnEnable()
        {
            base.OnEnable();
            // 커스텀 초기 스케일이 설정된 경우 해당 값으로 스케일을 강제 설정
            if (useCustomOriginalScale)
            {
                originalScale = customOriginalScale;
                transform.localScale = customOriginalScale;
            }
            else
            {
                originalScale = transform.localScale;
            }
            originalPosition = transform.position;
            if (isUIEffect)
            {
                var rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                    originalAnchoredPosition = rectTransform.anchoredPosition;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            transform.DOKill(false);
            if (transform != null)
            {
                transform.localScale = originalScale;
                if (usePivotOffset && animationMode == AnimationMode.DOTween)
                {
                    if (isUIEffect)
                    {
                        var rectTransform = GetComponent<RectTransform>();
                        if (rectTransform != null)
                            rectTransform.anchoredPosition = originalAnchoredPosition;
                    }
                    else
                    {
                        transform.position = originalPosition;
                    }
                }
            }
        }

        protected override async UniTask VFXOnceInGame()
        {
            if (Random.Range(0f, 100f) > chanceToPlay)
                return;

            if (animationMode == AnimationMode.DOTween)
            {
                CalculateRandomValues();
                if (usePivotOffset)
                {
                    await SquashWithPivotInGame();
                }
                else
                {
                    await SquashNormalInGame();
                }
            }
            else
            {
                await SquashWithCurveInGame();
            }
        }

        protected override async UniTask VFXOnceUI()
        {
            if (Random.Range(0f, 100f) > chanceToPlay)
                return;

            if (animationMode == AnimationMode.DOTween)
            {
                CalculateRandomValues();
                if (usePivotOffset)
                {
                    await SquashWithPivotUI();
                }
                else
                {
                    await SquashNormalUI();
                }
            }
            else
            {
                await SquashWithCurveUI();
            }
        }

        private void CalculateRandomValues()
        {
            if (!useRandom)
            {
                randomSquashHorizontal = squashHorizontalRatio;
                randomSquashVertical = squashVerticalRatio;
                randomDuration = squashDuration;
                return;
            }
            randomSquashHorizontal = squashHorizontalRatio + Random.Range(-squashRatioRandomRange, squashRatioRandomRange);
            randomSquashVertical = squashVerticalRatio + Random.Range(-squashRatioRandomRange, squashRatioRandomRange);
            randomDuration = squashDuration + Random.Range(-squashRatioRandomRange, squashRatioRandomRange);
            randomSquashHorizontal = Mathf.Max(squashHorizontalRandomMin, randomSquashHorizontal);
            randomSquashVertical = Mathf.Max(squashVerticalRandomMin, randomSquashVertical);
            randomDuration = Mathf.Max(squashDurationRandomMin, randomDuration);
        }

        private async UniTask SquashWithCurveInGame()
        {
            transform.localScale = originalScale;
            float duration = useRandom ? randomDuration : animationDuration;
            try
            {
                await DOTween.To(
                    () => 0f,
                    (t) =>
                    {
                        float curveValue = squashCurve.Evaluate(t);
                        float remappedValue = initialScale + (curveValue * (maximumScale - initialScale));
                        float minimumThreshold = 0.0001f;
                        if (Mathf.Abs(remappedValue) < minimumThreshold)
                            remappedValue = minimumThreshold;
                        Vector3 newScale = originalScale;
                        if (affectX) newScale.x = originalScale.x * remappedValue;
                        if (affectY) newScale.y = originalScale.y * remappedValue;
                        if (affectZ) newScale.z = originalScale.z * remappedValue;
                        transform.localScale = newScale;
                    },
                    1f,
                    duration * effectSpeed
                ).SetEase(Ease.Linear).WithCancellation(destroyCancellation.Token);
            }
            finally
            {
                if (this != null && transform != null)
                {
                    transform.DOKill(false);
                    transform.localScale = originalScale;
                }
            }
        }

        private async UniTask SquashWithCurveUI()
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.localScale = originalScale;
            float duration = useRandom ? randomDuration : animationDuration;
            try
            {
                await DOTween.To(
                    () => 0f,
                    (t) =>
                    {
                        float curveValue = squashCurve.Evaluate(t);
                        float remappedValue = initialScale + (curveValue * (maximumScale - initialScale));
                        float minimumThreshold = 0.0001f;
                        if (Mathf.Abs(remappedValue) < minimumThreshold)
                            remappedValue = minimumThreshold;
                        Vector3 newScale = originalScale;
                        if (affectX) newScale.x = originalScale.x * remappedValue;
                        if (affectY) newScale.y = originalScale.y * remappedValue;
                        if (affectZ) newScale.z = originalScale.z * remappedValue;
                        rectTransform.localScale = newScale;
                    },
                    1f,
                    duration * effectSpeed
                ).SetEase(Ease.Linear).WithCancellation(destroyCancellation.Token);
            }
            finally
            {
                if (this != null && rectTransform != null)
                {
                    rectTransform.DOKill(false);
                    rectTransform.localScale = originalScale;
                }
            }
        }

        private async UniTask SquashNormalInGame()
        {
            transform.localScale = originalScale;
            try
            {
                await transform.DOScaleX(originalScale.x * randomSquashHorizontal, randomDuration * effectSpeed)
                    .SetEase(squashEase).WithCancellation(destroyCancellation.Token);
                if (yAxisDelay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(yAxisDelay * effectSpeed),
                        cancellationToken: destroyCancellation.Token);
                }
                await transform.DOScaleY(originalScale.y * randomSquashVertical, randomDuration * effectSpeed)
                    .SetEase(squashEase).WithCancellation(destroyCancellation.Token);
                await UniTask.WhenAll(
                    transform.DOScaleX(originalScale.x, randomDuration * effectSpeed * restoreDurationMultiplier)
                        .SetEase(restoreEase).WithCancellation(destroyCancellation.Token),
                    transform.DOScaleY(originalScale.y, randomDuration * effectSpeed * restoreDurationMultiplier)
                        .SetEase(restoreEase).WithCancellation(destroyCancellation.Token)
                );
            }
            finally
            {
                if (this != null && transform != null)
                {
                    transform.DOKill(false);
                    transform.localScale = originalScale;
                }
            }
        }

        private async UniTask SquashNormalUI()
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.localScale = originalScale;
            try
            {
                await rectTransform.DOScaleX(originalScale.x * randomSquashHorizontal, randomDuration * effectSpeed)
                    .SetEase(squashEase).WithCancellation(destroyCancellation.Token);
                if (yAxisDelay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(yAxisDelay * effectSpeed),
                        cancellationToken: destroyCancellation.Token);
                }
                await rectTransform.DOScaleY(originalScale.y * randomSquashVertical, randomDuration * effectSpeed)
                    .SetEase(squashEase).WithCancellation(destroyCancellation.Token);
                await UniTask.WhenAll(
                    rectTransform.DOScaleX(originalScale.x, randomDuration * effectSpeed * restoreDurationMultiplier)
                        .SetEase(restoreEase).WithCancellation(destroyCancellation.Token),
                    rectTransform.DOScaleY(originalScale.y, randomDuration * effectSpeed * restoreDurationMultiplier)
                        .SetEase(restoreEase).WithCancellation(destroyCancellation.Token)
                );
            }
            finally
            {
                if (this != null && rectTransform != null)
                {
                    rectTransform.DOKill(false);
                    rectTransform.localScale = originalScale;
                }
            }
        }

        private async UniTask SquashWithPivotInGame()
        {
            transform.localScale = originalScale;
            transform.position = originalPosition;
            try
            {
                Vector3 targetScaleX = new Vector3(originalScale.x * randomSquashHorizontal, originalScale.y, originalScale.z);
                Vector3 offsetX = CalculatePositionOffset(originalScale, targetScaleX);
                await UniTask.WhenAll(
                    transform.DOScale(targetScaleX, randomDuration * effectSpeed).SetEase(squashEase).WithCancellation(destroyCancellation.Token),
                    transform.DOMove(originalPosition + offsetX, randomDuration * effectSpeed).SetEase(squashEase).WithCancellation(destroyCancellation.Token)
                );
                if (yAxisDelay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(yAxisDelay * effectSpeed),
                        cancellationToken: destroyCancellation.Token);
                }
                Vector3 targetScaleY = new Vector3(originalScale.x * randomSquashHorizontal, originalScale.y * randomSquashVertical, originalScale.z);
                Vector3 offsetY = CalculatePositionOffset(originalScale, targetScaleY);
                await UniTask.WhenAll(
                    transform.DOScale(targetScaleY, randomDuration * effectSpeed).SetEase(squashEase).WithCancellation(destroyCancellation.Token),
                    transform.DOMove(originalPosition + offsetY, randomDuration * effectSpeed).SetEase(squashEase).WithCancellation(destroyCancellation.Token)
                );
                await UniTask.WhenAll(
                    transform.DOScale(originalScale, randomDuration * effectSpeed * restoreDurationMultiplier).SetEase(restoreEase).WithCancellation(destroyCancellation.Token),
                    transform.DOMove(originalPosition, randomDuration * effectSpeed * restoreDurationMultiplier).SetEase(restoreEase).WithCancellation(destroyCancellation.Token)
                );
            }
            finally
            {
                if (this != null && transform != null)
                {
                    transform.DOKill(false);
                    transform.localScale = originalScale;
                    transform.position = originalPosition;
                }
            }
        }

        private async UniTask SquashWithPivotUI()
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.localScale = originalScale;
            rectTransform.anchoredPosition = originalAnchoredPosition;
            try
            {
                Vector3 targetScaleX = new Vector3(originalScale.x * randomSquashHorizontal, originalScale.y, originalScale.z);
                Vector3 offsetX = CalculatePositionOffset(originalScale, targetScaleX);
                await UniTask.WhenAll(
                    rectTransform.DOScale(targetScaleX, randomDuration * effectSpeed).SetEase(squashEase).WithCancellation(destroyCancellation.Token),
                    rectTransform.DOAnchorPos(originalAnchoredPosition + (Vector2)offsetX, randomDuration * effectSpeed).SetEase(squashEase).WithCancellation(destroyCancellation.Token)
                );
                if (yAxisDelay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(yAxisDelay * effectSpeed),
                        cancellationToken: destroyCancellation.Token);
                }
                Vector3 targetScaleY = new Vector3(originalScale.x * randomSquashHorizontal, originalScale.y * randomSquashVertical, originalScale.z);
                Vector3 offsetY = CalculatePositionOffset(originalScale, targetScaleY);
                await UniTask.WhenAll(
                    rectTransform.DOScale(targetScaleY, randomDuration * effectSpeed).SetEase(squashEase).WithCancellation(destroyCancellation.Token),
                    rectTransform.DOAnchorPos(originalAnchoredPosition + (Vector2)offsetY, randomDuration * effectSpeed).SetEase(squashEase).WithCancellation(destroyCancellation.Token)
                );
                await UniTask.WhenAll(
                    rectTransform.DOScale(originalScale, randomDuration * effectSpeed * restoreDurationMultiplier).SetEase(restoreEase).WithCancellation(destroyCancellation.Token),
                    rectTransform.DOAnchorPos(originalAnchoredPosition, randomDuration * effectSpeed * restoreDurationMultiplier).SetEase(restoreEase).WithCancellation(destroyCancellation.Token)
                );
            }
            finally
            {
                if (this != null && rectTransform != null)
                {
                    rectTransform.DOKill(false);
                    rectTransform.localScale = originalScale;
                    rectTransform.anchoredPosition = originalAnchoredPosition;
                }
            }
        }

        private Vector3 CalculatePositionOffset(Vector3 originalScale, Vector3 targetScale)
        {
            Vector3 scaleChange = targetScale - originalScale;
            Vector3 currentSize = GetCurrentObjectSize();
            Vector3 offset = Vector3.zero;
            switch (squashPivot)
            {
                case SquashPivot.Center:
                    offset = Vector3.zero;
                    break;
                case SquashPivot.Top:
                    offset.y = -(scaleChange.y * currentSize.y * 0.5f) * pivotOffsetMultiplier;
                    break;
                case SquashPivot.Bottom:
                    offset.y = (scaleChange.y * currentSize.y * 0.5f) * pivotOffsetMultiplier;
                    break;
                case SquashPivot.Left:
                    offset.x = (scaleChange.x * currentSize.x * 0.5f) * pivotOffsetMultiplier;
                    break;
                case SquashPivot.Right:
                    offset.x = -(scaleChange.x * currentSize.x * 0.5f) * pivotOffsetMultiplier;
                    break;
                case SquashPivot.TopLeft:
                    offset.x = (scaleChange.x * currentSize.x * 0.5f) * pivotOffsetMultiplier;
                    offset.y = -(scaleChange.y * currentSize.y * 0.5f) * pivotOffsetMultiplier;
                    break;
                case SquashPivot.TopRight:
                    offset.x = -(scaleChange.x * currentSize.x * 0.5f) * pivotOffsetMultiplier;
                    offset.y = -(scaleChange.y * currentSize.y * 0.5f) * pivotOffsetMultiplier;
                    break;
                case SquashPivot.BottomLeft:
                    offset.x = (scaleChange.x * currentSize.x * 0.5f) * pivotOffsetMultiplier;
                    offset.y = (scaleChange.y * currentSize.y * 0.5f) * pivotOffsetMultiplier;
                    break;
                case SquashPivot.BottomRight:
                    offset.x = -(scaleChange.x * currentSize.x * 0.5f) * pivotOffsetMultiplier;
                    offset.y = (scaleChange.y * currentSize.y * 0.5f) * pivotOffsetMultiplier;
                    break;
            }
            return offset;
        }

        private Vector3 GetCurrentObjectSize()
        {
            if (isUIEffect)
            {
                var rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    return rectTransform.rect.size;
                }
            }
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.size;
            }
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds.size;
            }
            var collider2D = GetComponent<Collider2D>();
            if (collider2D != null)
            {
                return collider2D.bounds.size;
            }
            return Vector3.one;
        }
    }
}
