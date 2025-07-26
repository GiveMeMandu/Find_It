using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class IngameRotateTargetClip : PlayableAsset, ITimelineClipAsset
{
    public IngameRotateTargetBehaviour template = new IngameRotateTargetBehaviour();
    
    public ExposedReference<Transform> rotateTarget;
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 90f;
    public bool useLocalRotation = true;
    
    [Header("회전 모드")]
    public IngameRotateTargetBehaviour.RotationMode rotationMode = IngameRotateTargetBehaviour.RotationMode.Continuous;
    public AnimationCurve rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    
    [Header("시작/종료 설정")]
    public Vector3 startRotation;
    public Vector3 endRotation;
    public bool setStartRotationOnPlay = false;
    public bool setEndRotationOnComplete = false;

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<IngameRotateTargetBehaviour>.Create(graph, template);
        IngameRotateTargetBehaviour clone = playable.GetBehaviour();
        
        // ExposedReference 해결
        clone.rotateTarget = rotateTarget.Resolve(graph.GetResolver());
        
        // 나머지 설정값 복사
        clone.rotationAxis = rotationAxis;
        clone.rotationSpeed = rotationSpeed;
        clone.useLocalRotation = useLocalRotation;
        clone.rotationMode = rotationMode;
        clone.rotationCurve = rotationCurve;
        clone.startRotation = startRotation;
        clone.endRotation = endRotation;
        clone.setStartRotationOnPlay = setStartRotationOnPlay;
        clone.setEndRotationOnComplete = setEndRotationOnComplete;
        
        return playable;
    }
} 