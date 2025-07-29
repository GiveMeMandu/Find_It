using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class IngamePositionCorrectionBehaviour : PlayableBehaviour
{
    [Header("위치 보정 타겟")]
    public Transform targetTransform;
    
    [Header("보정 모드")]
    public CorrectionMode correctionMode = CorrectionMode.ToZero;
    
    [Header("기준 위치 설정")]
    public Vector3 referencePosition = Vector3.zero;
    public Transform referenceTransform; // ToReference 모드용 Transform 참조
    
    [Header("커스텀 오프셋 설정")]
    public Vector3 customOffset = Vector3.zero; // CustomOffset 모드용 오프셋
    
    [Header("보정 방식")]
    public CorrectionType correctionType = CorrectionType.OnStart;
    public bool smoothCorrection = false;
    public float correctionDuration = 0.1f;
    public AnimationCurve correctionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("애니메이션 감지")]
    public bool detectAnimationStart = true;
    public string[] targetAnimationClips; // 특정 애니메이션만 보정
    
    [Header("기타 설정")]
    public bool useLocalPosition = true;
    public bool restoreOriginalPosition = true;
    public bool onlyCorrectXY = true; // 2D용: Z축 제외
    
    private Vector3 originalPosition;
    private Vector3 animationStartPosition;
    private Vector3 correctionOffset;
    private bool isInitialized = false;
    private bool correctionApplied = false;
    private float correctionTime = 0f;
    private Animator targetAnimator;
    
    public enum CorrectionMode
    {
        ToZero,           // (0,0)으로 보정
        ToReference,      // 지정된 기준 위치로 보정
        KeepCurrent,      // 현재 위치 유지 (애니메이션 오프셋만 제거)
        CustomOffset      // 커스텀 오프셋 적용
    }
    
    public enum CorrectionType
    {
        OnStart,          // 클립 시작 시 즉시 보정
        Continuous,       // 지속적으로 보정 (매 프레임)
        OnAnimationStart  // 애니메이션 시작 감지 시 보정
    }
    
    public override void OnPlayableCreate(Playable playable)
    {
        isInitialized = false;
        correctionApplied = false;
        correctionTime = 0f;
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
        correctionTime = 0f;
        
        if (correctionType == CorrectionType.OnStart)
        {
            ApplyCorrection();
        }
    }
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (targetTransform == null || !isInitialized) 
        {
            Debug.LogWarning("PositionCorrection: targetTransform이 할당되지 않았습니다!");
            return;
        }
        
        float deltaTime = (float)info.deltaTime;
        correctionTime += deltaTime;
        
        // 애니메이션 시작 감지
        if (correctionType == CorrectionType.OnAnimationStart && 
            detectAnimationStart && !correctionApplied)
        {
            if (DetectAnimationStart())
            {
                ApplyCorrection();
            }
        }
        
        // 지속적 보정
        if (correctionType == CorrectionType.Continuous)
        {
            ApplyCorrection();
        }
        
        // 부드러운 보정 처리
        if (smoothCorrection && correctionApplied && correctionTime < correctionDuration)
        {
            ApplySmoothCorrection();
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
    
    private void ApplyCorrection()
    {
        if (correctionApplied && !smoothCorrection && correctionType != CorrectionType.Continuous) 
            return;
        
        Vector3 currentPosition = useLocalPosition ? targetTransform.localPosition : targetTransform.position;
        Vector3 targetPosition = CalculateTargetPosition(currentPosition);
        
        if (smoothCorrection && !correctionApplied)
        {
            // 부드러운 보정 시작
            correctionOffset = targetPosition - currentPosition;
            correctionApplied = true;
            correctionTime = 0f;
        }
        else if (!smoothCorrection)
        {
            // 즉시 보정
            SetPosition(targetPosition);
            correctionApplied = true;
        }
    }
    
    private void ApplySmoothCorrection()
    {
        float progress = Mathf.Clamp01(correctionTime / correctionDuration);
        float curveValue = correctionCurve.Evaluate(progress);
        
        Vector3 currentPosition = useLocalPosition ? targetTransform.localPosition : targetTransform.position;
        Vector3 targetPosition = CalculateTargetPosition(currentPosition);
        Vector3 smoothedPosition = Vector3.Lerp(originalPosition, targetPosition, curveValue);
        
        SetPosition(smoothedPosition);
    }
    
    private Vector3 CalculateTargetPosition(Vector3 currentPosition)
    {
        Vector3 targetPosition = currentPosition;
        
        switch (correctionMode)
        {
            case CorrectionMode.ToZero:
                targetPosition = Vector3.zero;
                if (onlyCorrectXY)
                {
                    targetPosition.z = currentPosition.z;
                }
                break;
                
                                      case CorrectionMode.ToReference:
                 // Transform 참조가 있으면 우선 사용, 없으면 Vector3 값 사용
                 if (referenceTransform != null)
                 {
                     Vector3 originalPos = useLocalPosition ? referenceTransform.localPosition : referenceTransform.position;
                     targetPosition = originalPos;
                     Debug.Log($"[ToReference ✅] 참조 Transform: {referenceTransform.name}");
                     Debug.Log($"[ToReference ✅] useLocalPosition: {useLocalPosition}");
                     Debug.Log($"[ToReference ✅] 참조 위치: {originalPos} → 보정 위치: {targetPosition}");
                 }
                 else
                 {
                     targetPosition = referencePosition;
                     Debug.LogWarning($"[ToReference ❌] Transform 참조가 null! Vector3 백업 사용: {referencePosition}");
                 }
                 
                 if (onlyCorrectXY)
                 {
                     targetPosition.z = currentPosition.z;
                     Debug.Log($"[ToReference] Z축 보정 제외 적용: {targetPosition}");
                 }
                 break;
                
            case CorrectionMode.KeepCurrent:
                // 애니메이션 오프셋만 제거하고 현재 위치 유지
                Vector3 animationOffset = currentPosition - animationStartPosition;
                targetPosition = originalPosition;
                if (onlyCorrectXY)
                {
                    targetPosition.z = currentPosition.z;
                }
                break;
                
            case CorrectionMode.CustomOffset:
                // 원본 위치에 커스텀 오프셋 적용
                targetPosition = originalPosition + customOffset;
                if (onlyCorrectXY)
                {
                    targetPosition.z = currentPosition.z;
                }
                break;
        }
        
        return targetPosition;
    }
    
    private void SetPosition(Vector3 position)
    {
        if (useLocalPosition)
            targetTransform.localPosition = position;
        else
            targetTransform.position = position;
    }
    
    public override void OnGraphStop(Playable playable)
    {
        // 옵션에 따라 클립이 끝나면 원래 위치로 복원
        if (restoreOriginalPosition && targetTransform != null && isInitialized)
        {
            SetPosition(originalPosition);
        }
    }
} 