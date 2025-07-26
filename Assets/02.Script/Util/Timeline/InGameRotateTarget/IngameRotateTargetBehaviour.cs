using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class IngameRotateTargetBehaviour : PlayableBehaviour
{
    public Transform rotateTarget;
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 90f; // 초당 회전 각도
    public bool useLocalRotation = true;
    
    [Header("회전 모드")]
    public RotationMode rotationMode = RotationMode.Continuous;
    public AnimationCurve rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    
    [Header("시작/종료 설정")]
    public Vector3 startRotation;
    public Vector3 endRotation;
    public bool setStartRotationOnPlay = false;
    public bool setEndRotationOnComplete = false;
    
    private Vector3 originalRotation;
    private bool isInitialized = false;
    
    public enum RotationMode
    {
        Continuous,     // 연속 회전
        CurveBased,     // 커브 기반 회전
        StartToEnd      // 시작에서 끝으로 회전
    }
    
    public override void OnPlayableCreate(Playable playable)
    {
        isInitialized = false;
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
        if (rotateTarget != null && setStartRotationOnPlay)
        {
            if (useLocalRotation)
                rotateTarget.localEulerAngles = startRotation;
            else
                rotateTarget.eulerAngles = startRotation;
        }
    }
    
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (rotateTarget != null && setEndRotationOnComplete)
        {
            if (useLocalRotation)
                rotateTarget.localEulerAngles = endRotation;
            else
                rotateTarget.eulerAngles = endRotation;
        }
    }
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (rotateTarget == null) 
        {
            Debug.LogWarning("IngameRotateTarget: rotateTarget이 할당되지 않았습니다!");
            return;
        }
        
        float progress = (float)(playable.GetTime() / playable.GetDuration());
        progress = Mathf.Clamp01(progress);
        
        Vector3 currentRotation = useLocalRotation ? rotateTarget.localEulerAngles : rotateTarget.eulerAngles;
        Vector3 targetRotation = currentRotation;
        
        switch (rotationMode)
        {
            case RotationMode.Continuous:
                float rotationAmount = rotationSpeed * (float)info.deltaTime;
                targetRotation += rotationAxis * rotationAmount;
                break;
                
            case RotationMode.CurveBased:
                float curveValue = rotationCurve.Evaluate(progress);
                targetRotation = Vector3.Lerp(startRotation, endRotation, curveValue);
                break;
                
            case RotationMode.StartToEnd:
                targetRotation = Vector3.Lerp(startRotation, endRotation, progress);
                break;
        }
        
        if (useLocalRotation)
            rotateTarget.localEulerAngles = targetRotation;
        else
            rotateTarget.eulerAngles = targetRotation;
    }
    
    public override void OnGraphStop(Playable playable)
    {
        if (rotateTarget != null && isInitialized)
        {
            // 원래 회전값으로 복원 (선택사항)
            // if (useLocalRotation)
            //     rotateTarget.localEulerAngles = originalRotation;
            // else
            //     rotateTarget.eulerAngles = originalRotation;
        }
    }
} 