using System;
using System.Collections.Generic;
using System.Threading;
using Coffee.UIEffects;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BushWipe : MonoBehaviour
{
    public bool IsPlaying => _isPlaying;

    [Header("수풀 이미지 목록")]
    [SerializeField] private List<Sprite> bushSprites = new();

    [Header("크기 설정")]
    [SerializeField] private Vector2 scaleRange = new(0.8f, 1.4f);

    [Header("회전 설정")]
    [SerializeField] private Vector2 rotationRange = new(-30f, 30f);

    [Header("색상 설정 (HSV)")]
    [HideIf("useRGBRange")]
    [Tooltip("색조(Hue) 범위 0~1, 예: 황록 = 0.20~0.25")]
    [SerializeField] private Vector2 hueRange = new(0.20f, 0.25f);
    [HideIf("useRGBRange")]
    [Tooltip("채도(Saturation) 범위 0~1")]
    [SerializeField] private Vector2 saturationRange = new(0.45f, 0.70f);
    [HideIf("useRGBRange")]
    [Tooltip("명도(Value) 범위 0~1")]
    [SerializeField] private Vector2 valueRange = new(0.75f, 1.00f);

    [Header("색상 설정 (RGB)")]
    [SerializeField] private bool useRGBRange = false;
    [ShowIf("useRGBRange")]
    [SerializeField] private Color colorMin = Color.white;
    [ShowIf("useRGBRange")]
    [SerializeField] private Color colorMax = Color.white;

    [Header("배치 설정")]
    [Tooltip("수풀 격자 간격 (캔버스 픽셀)")]
    [SerializeField] private float spacing = 10f;
    [Tooltip("격자 내 랜덤 위치 오프셋 범위 (캔버스 픽셀)")]
    [SerializeField] private float positionOffsetRange = 20f;

    [Header("애니메이션 설정")]
    [SerializeField] private float animDuration = 0.7f;
    [Tooltip("각 수풀 간 딜레이 (0 = 동시 이동)")]
    [SerializeField] private float staggerDelay = 0f;
    [SerializeField] private Ease animEase = Ease.OutQuart;
    [Tooltip("화면 밖 시작/종료 오프셋 거리 (캔버스 픽셀)")]
    [SerializeField] private float outScreenDistance = 2000f;

    [Header("컨테이너 (전체화면 RectTransform)")]
    [SerializeField] private RectTransform container;

    [Header("함께 조절할 페이드 이미지")]
    [SerializeField] private Image fadeImage;

    private readonly List<RectTransform> _bushInstances = new();
    private readonly List<Vector2> _targetPositions = new();
    private readonly List<Vector2> _outDirections = new();
    private bool _isPlaying;
    private CancellationTokenSource _cts;

    public UnityEvent OnBushWipeInComplete;
    public UnityEvent OnBushWipeOutComplete;

    private void GenerateBushes()
    {
        KillAllTweens();
        foreach (var bush in _bushInstances)
        {
            if (bush != null) Destroy(bush.gameObject);
        }
        _bushInstances.Clear();
        _targetPositions.Clear();
        _outDirections.Clear();

        if (bushSprites == null || bushSprites.Count == 0 || container == null) return;

        Vector2 screenSize = container.rect.size;
        if (screenSize.sqrMagnitude < 1f) return;

        float avgW = 0f, avgH = 0f;
        int validCount = 0;
        foreach (var s in bushSprites)
        {
            if (s == null) continue;
            avgW += s.rect.width;
            avgH += s.rect.height;
            validCount++;
        }
        if (validCount == 0) return;
        avgW /= validCount;
        avgH /= validCount;

        // 최대 스케일 기준으로 셀 크기 계산해 빈틈 방지
        float cellW = avgW * scaleRange.y + spacing;
        float cellH = avgH * scaleRange.y + spacing;

        // 화면 가득 채우고 랜덤 오프셋 흡수할 여유 행/열 추가
        int cols = Mathf.CeilToInt(screenSize.x / cellW) + 2;
        int rows = Mathf.CeilToInt(screenSize.y / cellH) + 2;

        float startX = -(cols * cellW) / 2f + cellW / 2f;
        float startY = -(rows * cellH) / 2f + cellH / 2f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Sprite sprite = bushSprites[UnityEngine.Random.Range(0, bushSprites.Count)];
                if (sprite == null) continue;

                float x = startX + col * cellW + UnityEngine.Random.Range(-positionOffsetRange, positionOffsetRange);
                float y = startY + row * cellH + UnityEngine.Random.Range(-positionOffsetRange, positionOffsetRange);
                Vector2 targetPos = new(x, y);

                var go = new GameObject($"Bush_{row}_{col}");
                go.transform.SetParent(container, false);

                var img = go.AddComponent<Image>();
                img.sprite = sprite;
                img.SetNativeSize();
                img.raycastTarget = false;
                if (useRGBRange)
                {
                    img.color = Color.Lerp(colorMin, colorMax, UnityEngine.Random.value);
                }
                else
                {
                    img.color = Color.HSVToRGB(
                        UnityEngine.Random.Range(hueRange.x, hueRange.y),
                        UnityEngine.Random.Range(saturationRange.x, saturationRange.y),
                        UnityEngine.Random.Range(valueRange.x, valueRange.y)
                    );
                }

                var rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition = targetPos;
                rt.localScale = Vector3.one * UnityEngine.Random.Range(scaleRange.x, scaleRange.y);
                rt.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(rotationRange.x, rotationRange.y));

                // 중앙에서 수풀 위치 방향 (화면 밖 이동에 사용)
                Vector2 dir = targetPos.sqrMagnitude > 1f
                    ? targetPos.normalized
                    : UnityEngine.Random.insideUnitCircle.normalized;

                _bushInstances.Add(rt);
                _targetPositions.Add(targetPos);
                _outDirections.Add(dir);


                // var uiEffect = img.AddComponent<UIEffect>();
                // uiEffect.shadowMode = ShadowMode.Shadow3;
                // uiEffect.shadowDistance = new Vector2(-9.8f, -4.66f);
                // uiEffect.shadowFade = 0.347f;
                // uiEffect.shadowColor = new Color(24f/255f, 64f/255f, 17f/255f, 1);
            }
        }
    }

    private void KillAllTweens()
    {
        foreach (var bush in _bushInstances)
        {
            if (bush != null) bush.DOKill();
        }
    }

    [Button("WipeIn 테스트 (외곽→중앙)")]
    public void WipeIn() => WipeInAsync().Forget();

    [Button("WipeOut 테스트 (중앙→외곽)")]
    public void WipeOut() => WipeOutAsync().Forget();

    /// <summary>외곽 → 중앙: 수풀이 화면 밖에서 들어와 화면 전체를 덮음</summary>
    public async UniTask WipeInAsync(float delayInSeconds = 0f, CancellationToken cancellationToken = default)
    {
        if (delayInSeconds > 0) await UniTask.Delay(TimeSpan.FromSeconds(delayInSeconds), cancellationToken: cancellationToken);

        CancelInternal();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _cts.Token;

        _isPlaying = true;
        container.gameObject.SetActive(true);

        GenerateBushes();

        float totalDuration = animDuration + (_bushInstances.Count > 0 ? (_bushInstances.Count - 1) * staggerDelay : 0f);
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.DOKill();
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f);
            fadeImage.DOFade(1f, totalDuration).SetEase(animEase).SetUpdate(true);
        }

        // 모든 수풀을 각자 방향 기준 화면 밖 시작 위치로 배치
        for (int i = 0; i < _bushInstances.Count; i++)
        {
            if (_bushInstances[i] == null) continue;
            _bushInstances[i].anchoredPosition = _targetPositions[i] + _outDirections[i] * outScreenDistance;
        }

        await UniTask.Yield(PlayerLoopTiming.Update, token);
        if (token.IsCancellationRequested) { _isPlaying = false; return; }
        var tasks = new List<UniTask<AsyncUnit>>(_bushInstances.Count);
        for (int i = 0; i < _bushInstances.Count; i++)
        {
            if (_bushInstances[i] == null) continue;
            var tween = _bushInstances[i].DOAnchorPos(_targetPositions[i], animDuration)
                .SetEase(animEase)
                .SetDelay(staggerDelay * i);
            tasks.Add(tween.ToUniTask(token));
        }

        try
        {
            await UniTask.WhenAll(tasks).AttachExternalCancellation(token);
        }
        catch (OperationCanceledException) { }

        _isPlaying = false;
        OnBushWipeInComplete?.Invoke();
    }

    /// <summary>중앙 → 외곽: 수풀이 화면 밖으로 나가며 화면을 드러냄</summary>
    public async UniTask WipeOutAsync(float delayInSeconds = 0f, CancellationToken cancellationToken = default)
    {
        if (delayInSeconds > 0) await UniTask.Delay(TimeSpan.FromSeconds(delayInSeconds), cancellationToken: cancellationToken);

        CancelInternal();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _cts.Token;

        _isPlaying = true;

        // WipeIn 없이 단독 호출 시 수풀 생성 후 배치
        if (_bushInstances.Count == 0)
        {
            container.gameObject.SetActive(true);
            GenerateBushes();
            for (int i = 0; i < _bushInstances.Count; i++)
            {
                if (_bushInstances[i] == null) continue;
                _bushInstances[i].anchoredPosition = _targetPositions[i];
            }
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            if (token.IsCancellationRequested) { _isPlaying = false; return; }
        }

        float totalDuration = animDuration + (_bushInstances.Count > 0 ? (_bushInstances.Count - 1) * staggerDelay : 0f);
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.DOKill();
            fadeImage.DOFade(0f, totalDuration).SetEase(animEase).SetUpdate(true);
        }

        var tasks = new List<UniTask<AsyncUnit>>(_bushInstances.Count);
        for (int i = 0; i < _bushInstances.Count; i++)
        {
            if (_bushInstances[i] == null) continue;
            Vector2 endPos = _targetPositions[i] + _outDirections[i] * outScreenDistance;
            var tween = _bushInstances[i].DOAnchorPos(endPos, animDuration)
                .SetEase(animEase)
                .SetDelay(staggerDelay * i);
            tasks.Add(tween.ToUniTask(token));
        }

        try
        {
            await UniTask.WhenAll(tasks).AttachExternalCancellation(token);
        }
        catch (OperationCanceledException) { }

        if (fadeImage != null) fadeImage.gameObject.SetActive(false);
        container.gameObject.SetActive(false);
        _isPlaying = false;
        }

        public void Cancel()
        {
        CancelInternal();
        KillAllTweens();
        if (fadeImage != null)
        {
            fadeImage.DOKill();
            fadeImage.gameObject.SetActive(false);
        }
        _isPlaying = false;
        }

    private void CancelInternal()
    {
        if (_cts == null) return;
        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }

    private void OnDestroy()
    {
        Cancel();
    }
}
