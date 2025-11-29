using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Sirenix.OdinInspector;

/// <summary>
/// 상하 레터박스 효과를 구현하는 스크립트
/// Top과 Bottom 이미지의 높이를 조절하여 시네마틱 효과 연출
/// </summary>
public class LetterBox : MonoBehaviour
{
    public bool IsPlaying => isAnimating;

    [Header("레터박스 설정")]
    [SerializeField] private Image topLetterBox; // 상단 레터박스 이미지
    [SerializeField] private Image bottomLetterBox; // 하단 레터박스 이미지

    [Header("애니메이션 설정")]
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // 보이기 커브
    [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f); // 숨기기 커브
    public float animationDuration = 0.5f; // 애니메이션 지속 시간
    [SerializeField] private float letterBoxHeight = 100f; // 레터박스 높이

    [Header("그룹 설정")]
    [SerializeField] private GameObject effectGroup; // 효과 실행 시 활성화할 그룹 오브젝트

    private RectTransform topRectTransform;
    private RectTransform bottomRectTransform;
    private bool isAnimating = false;

    void Start()
    {
        InitializeLetterBox();
    }

    void OnDestroy()
    {
        // 컴포넌트 파괴 시 효과 그룹 비활성화
        if (effectGroup != null)
            effectGroup.SetActive(false);
    }

    /// <summary>
    /// 레터박스 초기화
    /// </summary>
    private void InitializeLetterBox()
    {
        if (topLetterBox != null)
        {
            topRectTransform = topLetterBox.GetComponent<RectTransform>();
            SetupTopLetterBox();
        }

        if (bottomLetterBox != null)
        {
            bottomRectTransform = bottomLetterBox.GetComponent<RectTransform>();
            SetupBottomLetterBox();
        }

        // 초기 상태를 숨김으로 설정
        SetLetterBoxHeight(0f);
    }

    /// <summary>
    /// 상단 레터박스 설정
    /// </summary>
    private void SetupTopLetterBox()
    {
        if (topRectTransform == null) return;

        // 상단에 고정
        topRectTransform.anchorMin = new Vector2(0, 1);
        topRectTransform.anchorMax = new Vector2(1, 1);
        topRectTransform.offsetMin = Vector2.zero;
        topRectTransform.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 하단 레터박스 설정
    /// </summary>
    private void SetupBottomLetterBox()
    {
        if (bottomRectTransform == null) return;

        // 하단에 고정
        bottomRectTransform.anchorMin = new Vector2(0, 0);
        bottomRectTransform.anchorMax = new Vector2(1, 0);
        bottomRectTransform.offsetMin = Vector2.zero;
        bottomRectTransform.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 레터박스 높이 설정
    /// </summary>
    private void SetLetterBoxHeight(float height)
    {
        if (topRectTransform != null)
        {
            Vector2 sizeDelta = topRectTransform.sizeDelta;
            sizeDelta.y = height;
            topRectTransform.sizeDelta = sizeDelta;
        }

        if (bottomRectTransform != null)
        {
            Vector2 sizeDelta = bottomRectTransform.sizeDelta;
            sizeDelta.y = height;
            bottomRectTransform.sizeDelta = sizeDelta;
        }
    }
    [Button("테스트 레터박스 보이기")]
    public async UniTask ShowAsync(CancellationToken cancellationToken = default)
    {
        await ShowAsync(animationDuration, cancellationToken);
    }

    /// <summary>
    /// 레터박스 보이기 (커스텀 지속시간)
    /// </summary>
    public async UniTask ShowAsync(float customDuration, CancellationToken cancellationToken = default)
    {
        if (isAnimating) return;

        // 효과 그룹 활성화
        if (effectGroup != null)
            effectGroup.SetActive(true);

        isAnimating = true;
        float elapsedTime = 0f;

        try
        {
            while (elapsedTime < customDuration)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                float normalizedTime = elapsedTime / customDuration;
                float curveValue = showCurve.Evaluate(normalizedTime);
                float currentHeight = Mathf.Lerp(0f, letterBoxHeight, curveValue);
                SetLetterBoxHeight(currentHeight);

                elapsedTime += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            SetLetterBoxHeight(letterBoxHeight);
            // ShowAsync 완료 후에도 isAnimating을 true로 유지 (Hide될 때까지)
        }
        catch
        {
            isAnimating = false;
        }
    }

    [Button("테스트 레터박스 숨기기")]
    public async UniTask HideAsync(CancellationToken cancellationToken = default)
    {
        await HideAsync(animationDuration, cancellationToken);
    }

    /// <summary>
    /// 레터박스 숨기기 (커스텀 지속시간)
    /// </summary>
    public async UniTask HideAsync(float customDuration, CancellationToken cancellationToken = default)
    {
        // isAnimating이 true인 상태에서만 hide 실행 가능 (Show 상태에서만 Hide 가능)
        if (!isAnimating) return;

        float elapsedTime = 0f;

        try
        {
            while (elapsedTime < customDuration)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                float normalizedTime = elapsedTime / customDuration;
                float curveValue = hideCurve.Evaluate(normalizedTime);
                // hideCurve가 1에서 0으로 가므로, 직접 letterBoxHeight에 곱해줍니다
                float currentHeight = letterBoxHeight * curveValue;
                SetLetterBoxHeight(currentHeight);

                elapsedTime += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            SetLetterBoxHeight(0f);
        }
        finally
        {
            isAnimating = false;
            if (effectGroup != null)
                effectGroup.SetActive(false);
        }
    }

    /// <summary>
    /// 레터박스 보이기 (동기 호출용)
    /// </summary>
    public void Show()
    {
        ShowAsync().Forget();
    }

    /// <summary>
    /// 레터박스 숨기기 (동기 호출용)
    /// </summary>
    public void Hide()
    {
        HideAsync().Forget();
    }

    /// <summary>
    /// 레터박스 높이 즉시 설정 (외부 호출용)
    /// </summary>
    public void SetLetterBoxHeightPublic(float height)
    {
        SetLetterBoxHeight(height);
    }
}