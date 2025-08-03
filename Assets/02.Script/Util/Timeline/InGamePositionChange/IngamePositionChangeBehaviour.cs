using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using DG.Tweening;

[Serializable]
public class IngamePositionChangeBehaviour : PlayableBehaviour
{
    [Header("위치 변경 타겟")]
    public Transform targetTransform;
    
    [Header("변경 모드")]
    public ChangeMode changeMode = ChangeMode.ToPosition;
    
    [Header("목표 위치 설정")]
    public Vector3 targetPosition = Vector3.zero;
    public Transform targetTransformReference; // ToTransform 모드용 Transform 참조
    
    [Header("커스텀 오프셋 설정")]
    public Vector3 customOffset = Vector3.zero; // Offset 모드용 오프셋
    
    [Header("변경 방식")]
    public ChangeType changeType = ChangeType.OnStart;
    public bool smoothChange = false;
    public float changeDuration = 0.5f;
    public AnimationCurve changeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("이동 방식 설정")]
    public MovementType movementType = MovementType.AnimationCurve;
    
    [Header("Tween 설정")]
    public TweenType tweenType = TweenType.Linear;
    public Ease easeType = Ease.InOutQuad;
    
    [Header("AnimationCurve 설정")]
    public AnimationCurve customCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("애니메이션 감지")]
    public bool detectAnimationStart = true;
    public string[] targetAnimationClips; // 특정 애니메이션만 변경
    
    [Header("기타 설정")]
    public bool useLocalPosition = true;
    public bool restoreOriginalPosition = true;
    public bool onlyChangeXY = true; // 2D용: Z축 제외
    
    [Header("애니메이션 오버라이드 설정")]
    [Tooltip("애니메이션 클립이 재생 중에도 위치 수정을 강제로 적용")]
    public bool overrideAnimationPosition = true;
    
    [Header("에디터 모드 설정")]
    [Tooltip("에디터에서 Timeline 재생 시 원위치 복귀 여부")]
    public bool restoreOriginalPositionInEditor = true;
    
    private Vector3 originalPosition;
    private Vector3 animationStartPosition;
    private Vector3 changeOffset;
    private bool isInitialized = false;
    private bool changeApplied = false;
    private float changeTime = 0f;
    private Animator targetAnimator;
    
    public enum ChangeMode
    {
        ToPosition,        // 지정된 위치로 변경
        ToTransform,       // 지정된 Transform 위치로 변경
        Offset,           // 현재 위치에 오프셋 적용
        Relative          // 상대적 위치 변경
    }
    
    public enum ChangeType
    {
        OnStart,          // 클립 시작 시 즉시 변경
        Continuous,       // 지속적으로 변경 (매 프레임)
        OnAnimationStart  // 애니메이션 시작 감지 시 변경
    }
    
    public enum MovementType
    {
        AnimationCurve,   // AnimationCurve 사용
        Tween            // DOTween 사용
    }
    
    public enum TweenType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Bounce,
        Elastic,
        Back
    }
    
    public enum Ease
    {
        Linear,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InQuart, OutQuart, InOutQuart,
        InQuint, OutQuint, InOutQuint,
        InSine, OutSine, InOutSine,
        InExpo, OutExpo, InOutExpo,
        InCirc, OutCirc, InOutCirc,
        InBack, OutBack, InOutBack,
        InBounce, OutBounce, InOutBounce,
        InElastic, OutElastic, InOutElastic
    }
    
    public override void OnPlayableCreate(Playable playable)
    {
        isInitialized = false;
        changeApplied = false;
        changeTime = 0f;
    }
    
    public override void OnGraphStart(Playable playable)
    {
        if (targetTransform != null && !isInitialized)
        {
            originalPosition = useLocalPosition ? targetTransform.localPosition : targetTransform.position;
            targetAnimator = targetTransform.GetComponent<Animator>();
            isInitialized = true;
            
            // 애니메이션 시작 위치 감지
            if (detectAnimationStart && targetAnimator != null)
            {
                animationStartPosition = originalPosition;
            }
        }
    }
    
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        changeTime = 0f;
        
        if (changeType == ChangeType.OnStart)
        {
            ApplyChange();
        }
    }
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (targetTransform == null || !isInitialized) 
        {
            Debug.LogWarning("PositionChange: targetTransform이 할당되지 않았습니다!");
            return;
        }
        
        float deltaTime = (float)info.deltaTime;
        changeTime += deltaTime;
        
        // 클립의 진행률 계산 (0~1)
        double clipTime = playable.GetTime();
        double clipDuration = playable.GetDuration();
        float clipProgress = (float)(clipTime / clipDuration);
        
        // 애니메이션 시작 감지
        if (changeType == ChangeType.OnAnimationStart && 
            detectAnimationStart && !changeApplied)
        {
            if (DetectAnimationStart())
            {
                ApplyChange();
            }
        }
        
        // 지속적 변경
        if (changeType == ChangeType.Continuous)
        {
            // 매 프레임마다 위치를 강제로 재설정 (애니메이션 오버라이드)
            Vector3 currentPosition = useLocalPosition ? targetTransform.localPosition : targetTransform.position;
            Vector3 newPosition = CalculateNewPosition(currentPosition);
            SetPosition(newPosition);
        }
        
        // 클립 길이 기반 부드러운 이동 (새로운 기능)
        if (changeType == ChangeType.OnStart && smoothChange)
        {
            ApplyClipBasedSmoothChange(clipProgress);
        }
        // 기존 부드러운 변경 처리
        else if (smoothChange && changeApplied && changeTime < changeDuration)
        {
            ApplySmoothChange();
        }
    }
    
    private bool DetectAnimationStart()
    {
        if (targetAnimator == null) return false;
        
        // 현재 애니메이션 클립 이름 확인
        AnimatorClipInfo[] clipInfos = targetAnimator.GetCurrentAnimatorClipInfo(0);
        if (clipInfos.Length > 0)
        {
            string currentClipName = clipInfos[0].clip.name;
            
            // 특정 애니메이션만 감지하는 경우
            if (targetAnimationClips != null && targetAnimationClips.Length > 0)
            {
                foreach (string targetClip in targetAnimationClips)
                {
                    if (currentClipName.Contains(targetClip))
                    {
                        return true;
                    }
                }
                return false;
            }
            
            return true; // 모든 애니메이션 감지
        }
        
        return false;
    }
    
    private void ApplyChange()
    {
        if (changeApplied && !smoothChange && changeType != ChangeType.Continuous) 
            return;
        
        Vector3 currentPosition = useLocalPosition ? targetTransform.localPosition : targetTransform.position;
        Vector3 newPosition = CalculateNewPosition(currentPosition);
        
        if (smoothChange && !changeApplied)
        {
            // 부드러운 변경 시작
            changeOffset = newPosition - currentPosition;
            changeApplied = true;
            changeTime = 0f;
        }
        else if (!smoothChange)
        {
            // 즉시 변경
            SetPosition(newPosition);
            changeApplied = true;
        }
    }
    
    private void ApplySmoothChange()
    {
        float progress = Mathf.Clamp01(changeTime / changeDuration);
        float curveValue = changeCurve.Evaluate(progress);
        
        Vector3 currentPosition = useLocalPosition ? targetTransform.localPosition : targetTransform.position;
        Vector3 newPosition = CalculateNewPosition(currentPosition);
        Vector3 smoothedPosition = Vector3.Lerp(originalPosition, newPosition, curveValue);
        
        SetPosition(smoothedPosition);
    }

    private void ApplyClipBasedSmoothChange(float clipProgress)
    {
        // 이동 방식에 따른 진행률 계산
        float curveValue = CalculateProgressValue(clipProgress);
        
        Vector3 startPosition = originalPosition;
        Vector3 endPosition = CalculateNewPosition(originalPosition);
        Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, curveValue);
        
        SetPosition(currentPosition);
        
        // 클립이 끝나면 정확한 목표 위치로 설정
        if (clipProgress >= 1.0f)
        {
            SetPosition(endPosition);
        }
    }
    
    private float CalculateProgressValue(float progress)
    {
        switch (movementType)
        {
            case MovementType.AnimationCurve:
                return customCurve.Evaluate(progress);
                
            case MovementType.Tween:
                return CalculateTweenProgress(progress);
                
            default:
                return progress;
        }
    }
    
    private float CalculateTweenProgress(float progress)
    {
        switch (tweenType)
        {
            case TweenType.Linear:
                return progress;
                
            case TweenType.EaseIn:
                return EaseIn(progress, easeType);
                
            case TweenType.EaseOut:
                return EaseOut(progress, easeType);
                
            case TweenType.EaseInOut:
                return EaseInOut(progress, easeType);
                
            case TweenType.Bounce:
                return Bounce(progress);
                
            case TweenType.Elastic:
                return Elastic(progress);
                
            case TweenType.Back:
                return Back(progress);
                
            default:
                return progress;
        }
    }
    
    // Tween 함수들
    private float EaseIn(float t, Ease ease)
    {
        switch (ease)
        {
            case Ease.InQuad: return t * t;
            case Ease.InCubic: return t * t * t;
            case Ease.InQuart: return t * t * t * t;
            case Ease.InQuint: return t * t * t * t * t;
            case Ease.InSine: return 1f - Mathf.Cos(t * Mathf.PI / 2f);
            case Ease.InExpo: return t == 0f ? 0f : Mathf.Pow(2f, 10f * t - 10f);
            case Ease.InCirc: return 1f - Mathf.Sqrt(1f - t * t);
            case Ease.InBack: return t * t * (2.70158f * t - 1.70158f);
            case Ease.InBounce: return 1f - EaseOut(1f - t, Ease.OutBounce);
            case Ease.InElastic: return t == 0f ? 0f : t == 1f ? 1f : -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * (2f * Mathf.PI) / 3f);
            default: return t;
        }
    }
    
    private float EaseOut(float t, Ease ease)
    {
        switch (ease)
        {
            case Ease.OutQuad: return 1f - (1f - t) * (1f - t);
            case Ease.OutCubic: return 1f - Mathf.Pow(1f - t, 3f);
            case Ease.OutQuart: return 1f - Mathf.Pow(1f - t, 4f);
            case Ease.OutQuint: return 1f - Mathf.Pow(1f - t, 5f);
            case Ease.OutSine: return Mathf.Sin(t * Mathf.PI / 2f);
            case Ease.OutExpo: return t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
            case Ease.OutCirc: return Mathf.Sqrt(1f - Mathf.Pow(t - 1f, 2f));
            case Ease.OutBack: return 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
            case Ease.OutBounce: return Bounce(t);
            case Ease.OutElastic: return t == 0f ? 0f : t == 1f ? 1f : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * (2f * Mathf.PI) / 3f) + 1f;
            default: return t;
        }
    }
    
    private float EaseInOut(float t, Ease ease)
    {
        if (t < 0.5f)
            return EaseIn(t * 2f, ease) / 2f;
        else
            return EaseOut((t - 0.5f) * 2f, ease) / 2f + 0.5f;
    }
    
    private float Bounce(float t)
    {
        if (t < 1f / 2.75f)
            return 7.5625f * t * t;
        else if (t < 2f / 2.75f)
            return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
        else if (t < 2.5f / 2.75f)
            return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
        else
            return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
    }
    
    private float Elastic(float t)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;
        return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * (2f * Mathf.PI) / 3f);
    }
    
    private float Back(float t)
    {
        return t * t * (2.70158f * t - 1.70158f);
    }
    
    private Vector3 CalculateNewPosition(Vector3 currentPosition)
    {
        Vector3 newPosition = currentPosition;
        
        switch (changeMode)
        {
            case ChangeMode.ToPosition:
                newPosition = targetPosition;
                if (onlyChangeXY)
                {
                    newPosition.z = currentPosition.z;
                }
                break;
                
            case ChangeMode.ToTransform:
                // Transform 참조가 있으면 우선 사용, 없으면 Vector3 값 사용
                if (targetTransformReference != null)
                {
                    Vector3 referencePos = useLocalPosition ? targetTransformReference.localPosition : targetTransformReference.position;
                    newPosition = referencePos;
                    Debug.Log($"[ToTransform ✅] 참조 Transform: {targetTransformReference.name}");
                    Debug.Log($"[ToTransform ✅] useLocalPosition: {useLocalPosition}");
                    Debug.Log($"[ToTransform ✅] 참조 위치: {referencePos} → 변경 위치: {newPosition}");
                }
                else
                {
                    newPosition = targetPosition;
                    Debug.LogWarning($"[ToTransform ❌] Transform 참조가 null! Vector3 백업 사용: {targetPosition}");
                }
                
                if (onlyChangeXY)
                {
                    newPosition.z = currentPosition.z;
                    Debug.Log($"[ToTransform] Z축 변경 제외 적용: {newPosition}");
                }
                break;
                
            case ChangeMode.Offset:
                // 현재 위치에 오프셋 적용
                newPosition = currentPosition + customOffset;
                if (onlyChangeXY)
                {
                    newPosition.z = currentPosition.z;
                }
                break;
                
            case ChangeMode.Relative:
                // 원본 위치 기준으로 상대적 변경
                newPosition = originalPosition + customOffset;
                if (onlyChangeXY)
                {
                    newPosition.z = currentPosition.z;
                }
                break;
        }
        
        return newPosition;
    }
    
    private void SetPosition(Vector3 position)
    {
        // 애니메이션을 오버라이드하여 위치 강제 설정
        if (useLocalPosition)
        {
            targetTransform.localPosition = position;
        }
        else
        {
            targetTransform.position = position;
        }
        
        // 애니메이션의 위치 영향을 무시하기 위해 추가 처리
        if (targetAnimator != null)
        {
            targetAnimator.applyRootMotion = false;
        }
    }
    
    public override void OnGraphStop(Playable playable)
    {
        // 에디터에서 플레이 중이 아닐 때와 런타임/플레이 중을 구분해서 원위치 복원 처리
        bool shouldRestore = false;
        
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // 에디터에서 플레이 중이 아닐 때만 에디터 전용 설정 사용
            shouldRestore = restoreOriginalPositionInEditor;
        }
        else
        {
            // 에디터에서 플레이 중일 때는 런타임 설정 사용
            shouldRestore = restoreOriginalPosition;
        }
        #else
        // 빌드된 게임에서는 런타임 설정 사용
        shouldRestore = restoreOriginalPosition;
        #endif
        
        if (shouldRestore && targetTransform != null && isInitialized)
        {
            SetPosition(originalPosition);
            string mode = Application.isPlaying ? "플레이 중" : "에디터 모드";
            Debug.Log($"[위치변경] 원위치 복원 완료: {originalPosition} ({mode})");
        }
    }
} 