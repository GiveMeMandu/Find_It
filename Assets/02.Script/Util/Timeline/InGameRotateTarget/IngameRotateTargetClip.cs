using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class 회전클립 : PlayableAsset, ITimelineClipAsset
{
    public IngameRotateTargetBehaviour template = new IngameRotateTargetBehaviour();
    
    [Header("회전 타겟")]
    public ExposedReference<Transform> rotateTarget;
    
    [Header("2D 회전 모드")]
    public IngameRotateTargetBehaviour.RotationMode2D rotationMode = IngameRotateTargetBehaviour.RotationMode2D.Instant;
    
    [Header("즉시 회전 설정")]
    [Range(0f, 360f)]
    public float instantRotationAngle = 90f;
    
    [Header("시간 기반 회전 설정")]
    [Range(0.1f, 10f)]
    public float rotationDuration = 1f;
    
    [Range(0f, 720f)]
    public float targetRotationAngle = 360f;
    
    [Header("반전 회전 설정")]
    [Range(1, 10)]
    public int flipCount = 1;
    
    [Range(0.1f, 5f)]
    public float flipDuration = 0.5f;
    
    [Header("즉시 반전 설정")]
    [Range(0, 10)]
    public int instantFlipCount = 1;
    
    [Header("기타 설정")]
    public bool useLocalRotation = true;
    public bool restoreOriginalRotation = true;
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("애니메이션 오버라이드 설정")]
    [Tooltip("애니메이션 클립이 재생 중에도 회전 수정을 강제로 적용")]
    public bool overrideAnimationRotation = true;

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<IngameRotateTargetBehaviour>.Create(graph, template);
        IngameRotateTargetBehaviour clone = playable.GetBehaviour();
        
        // 직접 Transform 할당
        clone.rotateTarget = rotateTarget.Resolve(graph.GetResolver());
        
        // 2D 회전 설정 복사
        clone.rotationMode = rotationMode;
        clone.instantRotationAngle = instantRotationAngle;
        clone.rotationDuration = rotationDuration;
        clone.targetRotationAngle = targetRotationAngle;
        clone.flipCount = flipCount;
        clone.flipDuration = flipDuration;
        clone.instantFlipCount = instantFlipCount;
        clone.useLocalRotation = useLocalRotation;
        clone.restoreOriginalRotation = restoreOriginalRotation;
        clone.rotationCurve = rotationCurve;
        clone.overrideAnimationRotation = overrideAnimationRotation;
        
        return playable;
    }
} 