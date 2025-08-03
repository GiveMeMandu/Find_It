using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class 위치변경클립 : PlayableAsset, ITimelineClipAsset
{
    public IngamePositionChangeBehaviour template = new IngamePositionChangeBehaviour();
    
    [Header("위치 변경 타겟")]
    public ExposedReference<Transform> targetTransform;
    
    [Header("변경 모드")]
    public IngamePositionChangeBehaviour.ChangeMode changeMode = IngamePositionChangeBehaviour.ChangeMode.ToPosition;
    
    [Header("목표 위치 설정")]
    public Vector3 targetPosition = Vector3.zero;
    public ExposedReference<Transform> targetTransformReference; // ToTransform 모드용 Transform 참조
    public Transform directTargetTransform; // 직접 참조용 (ExposedReference 대안)
    
    [Header("커스텀 오프셋 설정")]
    public Vector3 customOffset = Vector3.zero; // Offset 모드용 오프셋
    
    [Header("변경 방식")]
    public IngamePositionChangeBehaviour.ChangeType changeType = IngamePositionChangeBehaviour.ChangeType.OnStart;
    
    [Header("부드러운 변경")]
    public bool smoothChange = false;
    
    [Range(0.1f, 2f)]
    public float changeDuration = 0.5f;
    
    public AnimationCurve changeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("이동 방식 설정")]
    public IngamePositionChangeBehaviour.MovementType movementType = IngamePositionChangeBehaviour.MovementType.AnimationCurve;
    
    [Header("Tween 설정")]
    public IngamePositionChangeBehaviour.TweenType tweenType = IngamePositionChangeBehaviour.TweenType.Linear;
    public IngamePositionChangeBehaviour.Ease easeType = IngamePositionChangeBehaviour.Ease.InOutQuad;
    
    [Header("AnimationCurve 설정")]
    public AnimationCurve customCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("애니메이션 감지")]
    public bool detectAnimationStart = true;
    public string[] targetAnimationClips;
    
    [Header("기타 설정")]
    public bool useLocalPosition = true;
    public bool restoreOriginalPosition = false;
    public bool onlyChangeXY = true;
    
    [Header("애니메이션 오버라이드 설정")]
    [Tooltip("애니메이션 클립이 재생 중에도 위치 수정을 강제로 적용")]
    public bool overrideAnimationPosition = true;
    
    [Header("에디터 모드 설정")]
    [Tooltip("에디터에서 Timeline 재생 시 원위치 복귀 여부")]
    public bool restoreOriginalPositionInEditor = true;
    
    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<IngamePositionChangeBehaviour>.Create(graph, template);
        IngamePositionChangeBehaviour clone = playable.GetBehaviour();
        
        // Transform 할당
        clone.targetTransform = targetTransform.Resolve(graph.GetResolver());
        
        // 변경 설정 복사
        clone.changeMode = changeMode;
        clone.targetPosition = targetPosition;
        
        // ExposedReference를 우선 사용, 없으면 직접 참조 사용
        Transform resolvedTarget = targetTransformReference.Resolve(graph.GetResolver());
        if (resolvedTarget != null)
        {
            clone.targetTransformReference = resolvedTarget;
        }
        else
        {
            clone.targetTransformReference = directTargetTransform;
        }
        
        clone.customOffset = customOffset;
        clone.changeType = changeType;
        clone.smoothChange = smoothChange;
        clone.changeDuration = changeDuration;
        clone.changeCurve = changeCurve;
        clone.movementType = movementType;
        clone.tweenType = tweenType;
        clone.easeType = easeType;
        clone.customCurve = customCurve;
        clone.detectAnimationStart = detectAnimationStart;
        clone.targetAnimationClips = targetAnimationClips;
        clone.useLocalPosition = useLocalPosition;
        clone.restoreOriginalPosition = restoreOriginalPosition;
        clone.overrideAnimationPosition = overrideAnimationPosition;
        clone.restoreOriginalPositionInEditor = restoreOriginalPositionInEditor;
        clone.onlyChangeXY = onlyChangeXY;
        
        return playable;
    }
} 