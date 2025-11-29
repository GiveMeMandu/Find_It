using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Sirenix.OdinInspector;

/// <summary>
/// 눈 깜빡이는 효과를 구현하는 스크립트
/// 이미지의 Y 크기를 조절하여 눈이 뜨고 감는 효과를 연출
/// </summary>
public class EyeBlinkScreen : MonoBehaviour
{
    public bool IsPlaying => isBlinking;

    [Header("눈 깜빡임 설정")]
    [SerializeField] private Image eyeImage; // 눈 깜빡임 이미지

    [Header("애니메이션 설정")]
    [SerializeField] private AnimationCurve blinkCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // 깜빡임 커브 (0에서 1로)
    [SerializeField] private float blinkDuration = 0.3f; // 한 번 깜빡이는 시간
    [SerializeField] private float eyeOpenHeight = 1080f; // 눈 뜬 상태의 높이 (화면 전체)
    [SerializeField] private float eyeClosedHeight = 0f; // 눈 감은 상태의 높이 (0 = 완전히 감음)

    [Header("자동 깜빡임 설정")]
    [SerializeField] private bool autoBlinkEnabled = true; // 자동 깜빡임 활성화
    [SerializeField] private float minBlinkInterval = 2f; // 최소 깜빡임 간격
    [SerializeField] private float maxBlinkInterval = 5f; // 최대 깜빡임 간격

    [Header("그룹 설정")]
    [SerializeField] private GameObject effectGroup; // 효과 실행 시 활성화할 그룹 오브젝트

    private RectTransform eyeRectTransform;
    private bool isBlinking = false;
    private CancellationTokenSource autoBlinkCancellationSource;

    void Start()
    {
        InitializeEyeImage();

        if (autoBlinkEnabled)
        {
            StartAutoBlinking();
        }
    }

    void OnDestroy()
    {
        StopAutoBlinking();
        // Ensure the effect group is deactivated when this component is destroyed
        if (effectGroup != null)
            effectGroup.SetActive(false);
    }

    /// <summary>
    /// 눈 이미지 초기화
    /// </summary>
    private void InitializeEyeImage()
    {
        if (eyeImage == null) return;

        eyeRectTransform = eyeImage.GetComponent<RectTransform>();

        // 초기 상태를 눈 뜬 상태로 설정
        SetEyeHeight(eyeOpenHeight);

        // 이미지가 화면 중앙에 위치하도록 설정 (Y축 중심)
        eyeRectTransform.anchorMin = new Vector2(0, 0.5f);
        eyeRectTransform.anchorMax = new Vector2(1, 0.5f);
        eyeRectTransform.offsetMin = Vector2.zero;
        eyeRectTransform.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 눈의 높이 설정
    /// </summary>
    private void SetEyeHeight(float height)
    {
        if (eyeRectTransform == null) return;

        Vector2 sizeDelta = eyeRectTransform.sizeDelta;
        sizeDelta.y = height;
        eyeRectTransform.sizeDelta = sizeDelta;
    }

    [Button("테스트 한번 깜빡임")]
    public async UniTask BlinkAsync(CancellationToken cancellationToken = default)
    {
        Debug.Log("BlinkAsync ");
        if (isBlinking || eyeImage == null) return;

        // Activate effect group while blinking (safely deactivate in finally)
        if (effectGroup != null)
            effectGroup.SetActive(true);

        isBlinking = true;
        float elapsedTime = 0f;

        try
        {
            while (elapsedTime < blinkDuration)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                float normalizedTime = elapsedTime / blinkDuration;
                float curveValue = blinkCurve.Evaluate(normalizedTime);

                // 커브 값에 따라 높이 설정 (0일 때 눈 감음, 1일 때 눈 뜸)
                float currentHeight = Mathf.Lerp(eyeClosedHeight, eyeOpenHeight, curveValue);
                SetEyeHeight(currentHeight);

                elapsedTime += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            // 최종적으로 눈 뜬 상태로 복귀
            SetEyeHeight(eyeOpenHeight);
        }
        finally
        {
            isBlinking = false;
            if (effectGroup != null)
                effectGroup.SetActive(false);
        }
    }
    public void Blink()
    {
        BlinkAsync().Forget();
    }
    public async UniTask CloseEyesAsync(float duration = 0.5f, CancellationToken cancellationToken = default)
    {
        if (eyeImage == null) return;

        isBlinking = true;

        if (effectGroup != null)
            effectGroup.SetActive(true);

        float elapsedTime = 0f;
        float startHeight = eyeRectTransform.sizeDelta.y;

        try
        {
            while (elapsedTime < duration)
            {
                if (cancellationToken.IsCancellationRequested) return;

                float normalizedTime = elapsedTime / duration;
                float currentHeight = Mathf.Lerp(startHeight, eyeClosedHeight, normalizedTime);
                SetEyeHeight(currentHeight);

                elapsedTime += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            SetEyeHeight(eyeClosedHeight);
        }
        finally
        {
            isBlinking = false;
            if (effectGroup != null)
                effectGroup.SetActive(false);
        }
    }

    public async UniTask OpenEyesAsync(float duration = 0.5f, CancellationToken cancellationToken = default)
    {
        if (eyeImage == null) return;

        isBlinking = true;

        if (effectGroup != null)
            effectGroup.SetActive(true);

        float elapsedTime = 0f;
        float startHeight = eyeRectTransform.sizeDelta.y;

        try
        {
            while (elapsedTime < duration)
            {
                if (cancellationToken.IsCancellationRequested) return;

                float normalizedTime = elapsedTime / duration;
                float currentHeight = Mathf.Lerp(startHeight, eyeOpenHeight, normalizedTime);
                SetEyeHeight(currentHeight);

                elapsedTime += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            SetEyeHeight(eyeOpenHeight);
        }
        finally
        {
            isBlinking = false;
            if (effectGroup != null)
                effectGroup.SetActive(false);
        }
    }

    /// <summary>
    /// 눈 감기 (동기 호출용)
    /// </summary>
    public void CloseEyes(float duration = 0.5f)
    {
        CloseEyesAsync(duration).Forget();
    }

    [Button("테스트 눈 뜨기")]
    public void OpenEyes(float duration = 0.5f)
    {
        OpenEyesAsync(duration).Forget();
    }
    [Button("테스트 자동 깜빡임")]
    public void StartAutoBlinking()
    {
        StopAutoBlinking();
        autoBlinkCancellationSource = new CancellationTokenSource();
        AutoBlinkLoop(autoBlinkCancellationSource.Token).Forget();
    }

    public void StopAutoBlinking()
    {
        if (autoBlinkCancellationSource != null)
        {
            autoBlinkCancellationSource.Cancel();
            autoBlinkCancellationSource.Dispose();
            autoBlinkCancellationSource = null;
        }
    }

    private async UniTaskVoid AutoBlinkLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // 랜덤한 간격으로 대기
            float waitTime = UnityEngine.Random.Range(minBlinkInterval, maxBlinkInterval);
            await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested) return;

            // 깜빡이기 실행
            await BlinkAsync(cancellationToken);
        }
    }

    /// <summary>
    /// 눈의 높이 설정 (외부 호출용)
    /// </summary>
    public void SetEyeHeightPublic(float height)
    {
        SetEyeHeight(height);
    }

    /// <summary>
    /// 커스텀 커브를 사용한 눈뜨기 애니메이션
    /// </summary>
    public async UniTask OpenEyesWithCurveAsync(float duration, AnimationCurve curve, CancellationToken cancellationToken = default)
    {
        if (eyeImage == null) return;

        isBlinking = true;

        if (effectGroup != null)
            effectGroup.SetActive(true);

        float elapsedTime = 0f;

        try
        {
            while (elapsedTime < duration)
            {
                if (cancellationToken.IsCancellationRequested) return;

                float normalizedTime = elapsedTime / duration;
                float curveValue = curve.Evaluate(normalizedTime);

                // 커브 값에 따라 높이 설정 (0일 때 감음, 1일 때 뜸)
                float currentHeight = Mathf.Lerp(eyeClosedHeight, eyeOpenHeight, curveValue);
                SetEyeHeight(currentHeight);

                elapsedTime += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            SetEyeHeight(eyeOpenHeight);
        }
        finally
        {
            isBlinking = false;
            if (effectGroup != null)
                effectGroup.SetActive(false);
        }
    }

    /// <summary>
    /// 커스텀 커브를 사용한 눈감기 애니메이션
    /// </summary>
    public async UniTask CloseEyesWithCurveAsync(float duration, AnimationCurve curve, CancellationToken cancellationToken = default)
    {
        if (eyeImage == null) return;

        isBlinking = true;

        if (effectGroup != null)
            effectGroup.SetActive(true);

        float elapsedTime = 0f;

        try
        {
            while (elapsedTime < duration)
            {
                if (cancellationToken.IsCancellationRequested) return;

                float normalizedTime = elapsedTime / duration;
                float curveValue = curve.Evaluate(normalizedTime);

                // 커브 값에 따라 높이 설정 (0일 때 뜸, 1일 때 감음)
                float currentHeight = Mathf.Lerp(eyeOpenHeight, eyeClosedHeight, curveValue);
                SetEyeHeight(currentHeight);

                elapsedTime += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            SetEyeHeight(eyeClosedHeight);
        }
        finally
        {
            isBlinking = false;
            if (effectGroup != null)
                effectGroup.SetActive(false);
        }
    }

    /// <summary>
    /// 자동 깜빡임 활성화/비활성화
    /// </summary>
    public void SetAutoBlinkEnabled(bool enabled)
    {
        autoBlinkEnabled = enabled;

        if (enabled)
        {
            StartAutoBlinking();
        }
        else
        {
            StopAutoBlinking();
        }
    }
}