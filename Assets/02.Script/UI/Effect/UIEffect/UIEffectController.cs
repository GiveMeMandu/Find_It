using System.Threading;
using Coffee.UIEffects;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIEffectController : MonoBehaviour
{
    private UIEffect _uiEffect;
    private Graphic _graphic;

    private void Awake()
    {
        _graphic = GetComponent<Graphic>();
        _uiEffect = GetComponent<UIEffect>();
    }

    public UIEffect GetOrCreateEffect()
    {
        if (_uiEffect == null)
        {
            _uiEffect = gameObject.AddComponent<UIEffect>();
        }
        return _uiEffect;
    }

    /// <summary>
    /// Preset 효과를 로드합니다.
    /// </summary>
    public void LoadPreset(string presetName)
    {
        GetOrCreateEffect().LoadPreset(presetName);
    }

    #region UnityEvent 용 편의 메서드 (Fire & Forget)
    
    public void PlayTransition(float duration) => PlayTransitionAsync(0f, 1f, duration, this.GetCancellationTokenOnDestroy()).Forget();
    public void PlayTransitionReverse(float duration) => PlayTransitionAsync(1f, 0f, duration, this.GetCancellationTokenOnDestroy()).Forget();
    
    public void PlayToneIntensity(float duration) => PlayToneIntensityAsync(0f, 1f, duration, this.GetCancellationTokenOnDestroy()).Forget();
    public void PlayToneIntensityReverse(float duration) => PlayToneIntensityAsync(1f, 0f, duration, this.GetCancellationTokenOnDestroy()).Forget();

    public void PlayColorIntensity(float duration) => PlayColorIntensityAsync(0f, 1f, duration, this.GetCancellationTokenOnDestroy()).Forget();
    public void PlayColorIntensityReverse(float duration) => PlayColorIntensityAsync(1f, 0f, duration, this.GetCancellationTokenOnDestroy()).Forget();

    public void PlaySamplingIntensity(float duration) => PlaySamplingIntensityAsync(0f, 1f, duration, this.GetCancellationTokenOnDestroy()).Forget();
    public void PlaySamplingIntensityReverse(float duration) => PlaySamplingIntensityAsync(1f, 0f, duration, this.GetCancellationTokenOnDestroy()).Forget();

    #endregion

    /// <summary>
    /// Transition 값을 설정된 지속시간(duration)동안 시작값(startValue)에서 끝값(endValue)으로 애니메이팅합니다.
    /// </summary>
    public async UniTask PlayTransitionAsync(float startValue, float endValue, float duration, CancellationToken cancellationToken = default)
    {
        var effect = GetOrCreateEffect();
        effect.transitionRate = startValue;

        float time = 0f;
        while (time < duration)
        {
            if (cancellationToken.IsCancellationRequested) return;

            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            effect.transitionRate = Mathf.Lerp(startValue, endValue, t);

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        effect.transitionRate = endValue;
    }

    /// <summary>
    /// Tone Intensity 값을 설정된 지속시간(duration)동안 시작값(startValue)에서 끝값(endValue)으로 애니메이팅합니다.
    /// </summary>
    public async UniTask PlayToneIntensityAsync(float startValue, float endValue, float duration, CancellationToken cancellationToken = default)
    {
        var effect = GetOrCreateEffect();
        effect.toneIntensity = startValue;

        float time = 0f;
        while (time < duration)
        {
            if (cancellationToken.IsCancellationRequested) return;

            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            effect.toneIntensity = Mathf.Lerp(startValue, endValue, t);

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        effect.toneIntensity = endValue;
    }

    /// <summary>
    /// Color Intensity 값을 설정된 지속시간(duration)동안 시작값(startValue)에서 끝값(endValue)으로 애니메이팅합니다.
    /// </summary>
    public async UniTask PlayColorIntensityAsync(float startValue, float endValue, float duration, CancellationToken cancellationToken = default)
    {
        var effect = GetOrCreateEffect();
        effect.colorIntensity = startValue;

        float time = 0f;
        while (time < duration)
        {
            if (cancellationToken.IsCancellationRequested) return;

            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            effect.colorIntensity = Mathf.Lerp(startValue, endValue, t);

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        effect.colorIntensity = endValue;
    }

    /// <summary>
    /// Sampling(Blur) Intensity 값을 설정된 지속시간(duration)동안 시작값(startValue)에서 끝값(endValue)으로 애니메이팅합니다.
    /// </summary>
    public async UniTask PlaySamplingIntensityAsync(float startValue, float endValue, float duration, CancellationToken cancellationToken = default)
    {
        var effect = GetOrCreateEffect();
        effect.samplingIntensity = startValue;

        float time = 0f;
        while (time < duration)
        {
            if (cancellationToken.IsCancellationRequested) return;

            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            effect.samplingIntensity = Mathf.Lerp(startValue, endValue, t);

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        effect.samplingIntensity = endValue;
    }
}
