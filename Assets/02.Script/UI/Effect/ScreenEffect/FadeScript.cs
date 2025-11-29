using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

/// <summary>
/// UniTask 기반으로 변환된 페이드 스크립트입니다.
/// 게임 전환/UI 오픈 등 게임 로직은 제거하고 순수 페이드(In/Out) 기능만 제공합니다.
/// </summary>
public class FadeScript : MonoBehaviour
{
    public Image panel;

    [Tooltip("페이드 기본 시간(초)")]
    public float duration = 1.5f;

    private Color fadeColor = Color.black; // 기본 페이드 색상 (검정)

    /// <summary>
    /// 페이드 색상 설정 (알파는 유지)
    /// </summary>
    [Button]
    public void SetFadeColor(Color color)
    {
        fadeColor = color;
        if (panel == null) return;
        Color panelColor = panel.color;
        panelColor.r = color.r;
        panelColor.g = color.g;
        panelColor.b = color.b;
        panel.color = panelColor;
    }

    // --- Async API (UniTask) ---

    /// <summary>
    /// 화면을 어둡게 만드는 페이드(In) — alpha 0 -> 1
    /// </summary>
    public async UniTask FadeInAsync(float? customDuration = null, CancellationToken cancellationToken = default, Action onComplete = null)
    {
        if (panel == null) return;
        panel.gameObject.SetActive(true);

        float t = 0f;
        float dur = customDuration ?? duration;
        Color color = panel.color;
        color.a = 0f;
        panel.color = color;

        while (t < 1f)
        {
            if (cancellationToken.IsCancellationRequested) return;
            t += (Time.deltaTime / Mathf.Max(0.0001f, dur));
            color.a = Mathf.Lerp(0f, 1f, t);
            panel.color = color;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        // 확실히 최종값 설정
        color.a = 1f;
        panel.color = color;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 화면을 밝게 만드는 페이드(Out) — alpha 1 -> 0
    /// </summary>
    public async UniTask FadeOutAsync(float? customDuration = null, CancellationToken cancellationToken = default, Action onComplete = null)
    {
        if (panel == null) return;
        panel.gameObject.SetActive(true);

        float t = 0f;
        float dur = customDuration ?? duration;
        Color color = panel.color;
        // 시작은 완전 불투명으로 간주
        color.a = Mathf.Max(color.a, 1f);
        panel.color = color;

        while (t < 1f)
        {
            if (cancellationToken.IsCancellationRequested) return;
            t += (Time.deltaTime / Mathf.Max(0.0001f, dur));
            color.a = Mathf.Lerp(1f, 0f, t);
            panel.color = color;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        color.a = 0f;
        panel.color = color;
        panel.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    // --- 편의 동기(유니티 이벤트) 호출자 ---

    /// <summary>
    /// 기존의 parameterless Fade()와 동일하게 동작 (Out: 화면 밝게)
    /// </summary>
    [Button]
    public void Fade()
    {
        FadeOutAsync().Forget();
    }

    [Button]
    public void Fade(Color color, Action onComplete = null)
    {
        SetFadeColor(color);
        FadeOutAsync(null, default, onComplete).Forget();
    }

    [Button]
    public void FadeIn(Action onComplete = null)
    {
        FadeInAsync(null, default, onComplete).Forget();
    }

    [Button]
    public void FadeIn(Color color, Action onComplete = null)
    {
        SetFadeColor(color);
        FadeInAsync(null, default, onComplete).Forget();
    }

    [Button]
    public void FadeOut(Action onComplete = null)
    {
        FadeOutAsync(null, default, onComplete).Forget();
    }

    [Button]
    public void FadeOut(Color color, Action onComplete = null)
    {
        SetFadeColor(color);
        FadeOutAsync(null, default, onComplete).Forget();
    }
}
