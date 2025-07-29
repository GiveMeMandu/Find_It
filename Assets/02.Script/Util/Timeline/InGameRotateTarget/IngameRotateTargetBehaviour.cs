using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class IngameRotateTargetBehaviour : PlayableBehaviour
{
    [Header("회전 타겟")]
    public Transform rotateTarget;
    
    [Header("2D 회전 모드")]
    public RotationMode2D rotationMode = RotationMode2D.Instant;
    
    [Header("즉시 회전 설정")]
    public float instantRotationAngle = 90f;
    
    [Header("시간 기반 회전 설정")]
    public float rotationDuration = 1f;
    public float targetRotationAngle = 360f;
    
    [Header("반전 회전 설정")]
    public int flipCount = 1;
    public float flipDuration = 0.5f;
    
    [Header("즉시 반전 설정")]
    public int instantFlipCount = 1;
    
    [Header("기타 설정")]
    public bool useLocalRotation = true;
    public bool restoreOriginalRotation = true;
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    private Vector3 originalRotation;
    private bool isInitialized = false;
    private float currentRotationTime = 0f;
    private bool instantFlipApplied = false;
    
    public enum RotationMode2D
    {
        Instant,        // 즉시 회전 (Z축)
        TimeBased,      // 시간 기반 회전 (Z축)
        Flip,           // 반전 회전 (Y축 180도씩)
        InstantFlip     // 즉시 반전 (Y축 180도씩 점프)
    }
    
    public override void OnPlayableCreate(Playable playable)
    {
        isInitialized = false;
        currentRotationTime = 0f;
        instantFlipApplied = false;
    }
    
    public override void OnGraphStart(Playable playable)
    {
        if (rotateTarget != null && !isInitialized)
        {
            originalRotation = useLocalRotation ? rotateTarget.localEulerAngles : rotateTarget.eulerAngles;
            isInitialized = true;
        }
    }
    
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        currentRotationTime = 0f;
        instantFlipApplied = false;
    }
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (rotateTarget == null) 
        {
            Debug.LogWarning("IngameRotateTarget: rotateTarget이 할당되지 않았습니다!");
            return;
        }
        
        float deltaTime = (float)info.deltaTime;
        currentRotationTime += deltaTime;
        
        Vector3 targetRotation = useLocalRotation ? rotateTarget.localEulerAngles : rotateTarget.eulerAngles;
        
        switch (rotationMode)
        {
            case RotationMode2D.Instant:
                // 즉시 회전 (Z축)
                targetRotation.z = originalRotation.z + instantRotationAngle;
                break;
                
            case RotationMode2D.TimeBased:
                // 시간 기반 회전 (Z축)
                float progress = Mathf.Clamp01(currentRotationTime / rotationDuration);
                float curveValue = rotationCurve.Evaluate(progress);
                targetRotation.z = originalRotation.z + (curveValue * targetRotationAngle);
                break;
                
            case RotationMode2D.Flip:
                // 반전 회전 (Y축 180도씩) - 시간 기반
                if (flipDuration > 0f) // 안전 검사
                {
                    float totalFlipDuration = flipCount * flipDuration;
                    
                    if (currentRotationTime < totalFlipDuration)
                    {
                        // 현재 어떤 반전 단계인지 계산
                        int currentFlipIndex = Mathf.FloorToInt(currentRotationTime / flipDuration);
                        currentFlipIndex = Mathf.Clamp(currentFlipIndex, 0, flipCount - 1);
                        
                        // 현재 반전 내에서의 진행도 (0~1)
                        float timeInCurrentFlip = currentRotationTime - (currentFlipIndex * flipDuration);
                        float flipProgress = Mathf.Clamp01(timeInCurrentFlip / flipDuration);
                        
                        // 애니메이션 커브 적용
                        float flipCurveValue = rotationCurve.Evaluate(flipProgress);
                        
                        // 각 반전마다 Y축 180도씩 회전
                        float baseAngle = originalRotation.y + (currentFlipIndex * 180f);
                        float additionalAngle = flipCurveValue * 180f;
                        
                        targetRotation.y = baseAngle + additionalAngle;
                    }
                    else
                    {
                        // 모든 반전 완료 - 최종 각도로 설정
                        targetRotation.y = originalRotation.y + (flipCount * 180f);
                    }
                }
                break;
                
            case RotationMode2D.InstantFlip:
                // 클립 길이 기반 즉시 반전 (Y축 180도씩) - 특정 타이밍에 즉시 점프
                double clipDuration = playable.GetDuration();
                
                if (clipDuration > 0 && instantFlipCount > 0)
                {
                    // 클립 길이를 반전 횟수로 나누어 각 반전 타이밍 계산
                    float flipInterval = (float)clipDuration / instantFlipCount;
                    
                    // 현재 몇 번째 반전까지 완료되었는지 계산
                    int completedFlips = Mathf.FloorToInt(currentRotationTime / flipInterval);
                    completedFlips = Mathf.Clamp(completedFlips, 0, instantFlipCount);
                    
                    // 완료된 반전 횟수에 따라 즉시 각도 설정
                    targetRotation.y = originalRotation.y + (completedFlips * 180f);
                }
                break;
        }
        
        // 회전 적용
        if (useLocalRotation)
            rotateTarget.localEulerAngles = targetRotation;
        else
            rotateTarget.eulerAngles = targetRotation;
    }
    
    public override void OnGraphStop(Playable playable)
    {
        // 옵션에 따라 클립이 끝나면 원래 회전값으로 복원
        if (restoreOriginalRotation && rotateTarget != null && isInitialized)
        {
            if (useLocalRotation)
                rotateTarget.localEulerAngles = originalRotation;
            else
                rotateTarget.eulerAngles = originalRotation;
        }
    }
} 