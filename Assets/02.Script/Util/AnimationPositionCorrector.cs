using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D 애니메이션의 위치 오프셋을 자동으로 보정하는 컴포넌트
/// Timeline 없이도 사용 가능한 Runtime 보정 시스템
/// </summary>
public class AnimationPositionCorrector : MonoBehaviour
{
    [Header("보정 설정")]
    [SerializeField] private CorrectionMode correctionMode = CorrectionMode.ToZero;
    [SerializeField] private Vector3 referencePosition = Vector3.zero;
    [SerializeField] private Transform referenceTransform; // ToReference 모드용 Transform 참조
    [SerializeField] private Vector3 customOffset = Vector3.zero; // CustomOffset 모드용 오프셋
    [SerializeField] private bool useLocalPosition = true;
    [SerializeField] private bool onlyCorrectXY = true;
    
    [Header("애니메이션 감지")]
    [SerializeField] private bool autoDetectAnimations = true;
    [SerializeField] private string[] targetAnimationNames;
    [SerializeField] private bool ignoreIdleAnimations = true;
    
    [Header("보정 방식")]
    [SerializeField] private bool smoothCorrection = false;
    [SerializeField] private float correctionDuration = 0.1f;
    [SerializeField] private AnimationCurve correctionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = false;
    [SerializeField] private bool showGizmos = false;
    
    private Animator animator;
    private Vector3 originalPosition;
    private Vector3 lastPosition;
    private string currentAnimationName = "";
    private string previousAnimationName = "";
    private bool correctionInProgress = false;
    private Coroutine correctionCoroutine;
    
    private readonly HashSet<string> idleKeywords = new HashSet<string> 
    { 
        "idle", "wait", "stand", "기본", "대기", "서있기" 
    };
    
    public enum CorrectionMode
    {
        ToZero,           // (0,0)으로 보정
        ToReference,      // 지정된 기준 위치로 보정
        KeepOriginal,     // 컴포넌트 시작 시 위치 유지
        CustomOffset,     // 커스텀 오프셋 적용
        Manual            // 수동 호출 시에만 보정
    }
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"AnimationPositionCorrector: {name}에 Animator가 없습니다!");
            enabled = false;
            return;
        }
        
        originalPosition = useLocalPosition ? transform.localPosition : transform.position;
        lastPosition = originalPosition;
    }
    
    private void Start()
    {
        if (showDebugLogs)
        {
            Debug.Log($"AnimationPositionCorrector 시작: {name}, 원본 위치: {originalPosition}");
        }
    }
    
    private void Update()
    {
        if (!autoDetectAnimations || animator == null) return;
        
        DetectAnimationChange();
    }
    
    private void DetectAnimationChange()
    {
        // 현재 애니메이션 클립 정보 가져오기
        AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfos.Length == 0) return;
        
        currentAnimationName = clipInfos[0].clip.name;
        
        // 애니메이션 변경 감지
        if (currentAnimationName != previousAnimationName)
        {
            if (showDebugLogs)
            {
                Debug.Log($"애니메이션 변경 감지: {previousAnimationName} → {currentAnimationName}");
            }
            
            OnAnimationChanged(currentAnimationName);
            previousAnimationName = currentAnimationName;
        }
    }
    
    private void OnAnimationChanged(string animationName)
    {
        // Idle 애니메이션 무시 옵션
        if (ignoreIdleAnimations && IsIdleAnimation(animationName))
        {
            if (showDebugLogs)
            {
                Debug.Log($"Idle 애니메이션 무시: {animationName}");
            }
            return;
        }
        
        // 특정 애니메이션만 처리하는 경우
        if (targetAnimationNames != null && targetAnimationNames.Length > 0)
        {
            bool isTargetAnimation = false;
            foreach (string targetName in targetAnimationNames)
            {
                if (animationName.Contains(targetName))
                {
                    isTargetAnimation = true;
                    break;
                }
            }
            
            if (!isTargetAnimation)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"대상 애니메이션이 아님: {animationName}");
                }
                return;
            }
        }
        
        // 위치 보정 실행
        CorrectPosition();
    }
    
    private bool IsIdleAnimation(string animationName)
    {
        string lowerName = animationName.ToLower();
        foreach (string keyword in idleKeywords)
        {
            if (lowerName.Contains(keyword))
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// 위치 보정을 수동으로 실행
    /// </summary>
    public void CorrectPosition()
    {
        if (correctionInProgress)
        {
            if (correctionCoroutine != null)
            {
                StopCoroutine(correctionCoroutine);
            }
        }
        
        Vector3 targetPosition = CalculateTargetPosition();
        
        if (smoothCorrection)
        {
            correctionCoroutine = StartCoroutine(SmoothCorrectPosition(targetPosition));
        }
        else
        {
            SetPosition(targetPosition);
            if (showDebugLogs)
            {
                Debug.Log($"위치 즉시 보정: {targetPosition}");
            }
        }
    }
    
    private Vector3 CalculateTargetPosition()
    {
        Vector3 currentPos = useLocalPosition ? transform.localPosition : transform.position;
        Vector3 targetPos = currentPos;
        
        switch (correctionMode)
        {
            case CorrectionMode.ToZero:
                targetPos = Vector3.zero;
                break;
                
            case CorrectionMode.ToReference:
                // Transform 참조가 있으면 우선 사용, 없으면 Vector3 값 사용
                if (referenceTransform != null)
                {
                    targetPos = useLocalPosition ? referenceTransform.localPosition : referenceTransform.position;
                }
                else
                {
                    targetPos = referencePosition;
                }
                break;
                
            case CorrectionMode.KeepOriginal:
                targetPos = originalPosition;
                break;
                
            case CorrectionMode.CustomOffset:
                // 원본 위치에 커스텀 오프셋 적용
                targetPos = originalPosition + customOffset;
                break;
                
            case CorrectionMode.Manual:
                // 수동 모드에서는 현재 위치 유지
                break;
        }
        
        // Z축 보정 제외 옵션
        if (onlyCorrectXY)
        {
            targetPos.z = currentPos.z;
        }
        
        return targetPos;
    }
    
    private IEnumerator SmoothCorrectPosition(Vector3 targetPosition)
    {
        correctionInProgress = true;
        Vector3 startPosition = useLocalPosition ? transform.localPosition : transform.position;
        float elapsedTime = 0f;
        
        if (showDebugLogs)
        {
            Debug.Log($"부드러운 위치 보정 시작: {startPosition} → {targetPosition}");
        }
        
        while (elapsedTime < correctionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / correctionDuration);
            float curveValue = correctionCurve.Evaluate(progress);
            
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
            SetPosition(currentPosition);
            
            yield return null;
        }
        
        SetPosition(targetPosition);
        correctionInProgress = false;
        
        if (showDebugLogs)
        {
            Debug.Log($"부드러운 위치 보정 완료: {targetPosition}");
        }
    }
    
    private void SetPosition(Vector3 position)
    {
        if (useLocalPosition)
            transform.localPosition = position;
        else
            transform.position = position;
    }
    
    /// <summary>
    /// 기준 위치를 현재 위치로 설정
    /// </summary>
    public void SetReferencePositionToCurrent()
    {
        referencePosition = useLocalPosition ? transform.localPosition : transform.position;
        if (showDebugLogs)
        {
            Debug.Log($"기준 위치 설정: {referencePosition}");
        }
    }
    
    /// <summary>
    /// 원본 위치를 현재 위치로 재설정
    /// </summary>
    public void ResetOriginalPosition()
    {
        originalPosition = useLocalPosition ? transform.localPosition : transform.position;
        if (showDebugLogs)
        {
            Debug.Log($"원본 위치 재설정: {originalPosition}");
        }
    }
    
    /// <summary>
    /// 기준 Transform을 설정 (ToReference 모드용)
    /// </summary>
    public void SetReferenceTransform(Transform newReference)
    {
        referenceTransform = newReference;
        if (showDebugLogs)
        {
            Debug.Log($"기준 Transform 설정: {(newReference != null ? newReference.name : "null")}");
        }
    }
    
    /// <summary>
    /// 커스텀 오프셋을 설정 (CustomOffset 모드용)
    /// </summary>
    public void SetCustomOffset(Vector3 newOffset)
    {
        customOffset = newOffset;
        if (showDebugLogs)
        {
            Debug.Log($"커스텀 오프셋 설정: {customOffset}");
        }
    }
    
    /// <summary>
    /// 보정 모드를 변경
    /// </summary>
    public void SetCorrectionMode(CorrectionMode newMode)
    {
        correctionMode = newMode;
        if (showDebugLogs)
        {
            Debug.Log($"보정 모드 변경: {correctionMode}");
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // 원본 위치 표시 (파란색)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(originalPosition, 0.1f);
        
        // 보정 모드별 기준점 표시
        if (correctionMode == CorrectionMode.ToReference)
        {
            Gizmos.color = Color.green;
            Vector3 refPos = referenceTransform != null ? 
                (useLocalPosition ? referenceTransform.localPosition : referenceTransform.position) : 
                referencePosition;
            Gizmos.DrawWireSphere(refPos, 0.1f);
            
            // Transform 참조가 있으면 연결선 표시
            if (referenceTransform != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(refPos, refPos + Vector3.up * 0.2f);
            }
        }
        else if (correctionMode == CorrectionMode.CustomOffset)
        {
            // 커스텀 오프셋 적용된 위치 표시 (마젠타)
            Gizmos.color = Color.magenta;
            Vector3 offsetPos = originalPosition + customOffset;
            Gizmos.DrawWireSphere(offsetPos, 0.1f);
            
            // 오프셋 방향 표시
            if (customOffset.magnitude > 0.01f)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(originalPosition, offsetPos);
            }
        }
        
        // 현재 위치에서 목표 위치로의 선 (빨간색)
        if (Application.isPlaying)
        {
            Vector3 currentPos = useLocalPosition ? transform.localPosition : transform.position;
            Vector3 targetPos = CalculateTargetPosition();
            
            if (Vector3.Distance(currentPos, targetPos) > 0.01f)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(currentPos, targetPos);
                Gizmos.DrawWireSphere(targetPos, 0.05f);
            }
        }
    }
} 