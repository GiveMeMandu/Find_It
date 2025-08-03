using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class 위치보정클립 : PlayableAsset, ITimelineClipAsset
{
    public IngamePositionCorrectionBehaviour template = new IngamePositionCorrectionBehaviour();
    
    [Header("위치 보정 타겟")]
    public ExposedReference<Transform> targetTransform;
    
    [Header("보정 모드")]
    public IngamePositionCorrectionBehaviour.CorrectionMode correctionMode = IngamePositionCorrectionBehaviour.CorrectionMode.ToZero;
    
    [Header("기준 위치 설정")]
    public Vector3 referencePosition = Vector3.zero;
    public ExposedReference<Transform> referenceTransform; // ToReference 모드용 Transform 참조
    public Transform directReferenceTransform; // 직접 참조용 (ExposedReference 대안)
    
    [Header("커스텀 오프셋 설정")]
    public Vector3 customOffset = Vector3.zero; // CustomOffset 모드용 오프셋
    
    [Header("보정 방식")]
    public IngamePositionCorrectionBehaviour.CorrectionType correctionType = IngamePositionCorrectionBehaviour.CorrectionType.OnStart;
    
    [Header("부드러운 보정")]
    public bool smoothCorrection = false;
    
    [Range(0.1f, 2f)]
    public float correctionDuration = 0.1f;
    
    public AnimationCurve correctionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("애니메이션 감지")]
    public bool detectAnimationStart = true;
    public string[] targetAnimationClips;
    
    [Header("기타 설정")]
    public bool useLocalPosition = true;
    public bool restoreOriginalPosition = false;
    public bool onlyCorrectXY = true;
    
    [Header("에디터 모드 설정")]
    [Tooltip("에디터에서 Timeline 재생 시 원위치 복귀 여부")]
    public bool restoreOriginalPositionInEditor = true;
    
    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<IngamePositionCorrectionBehaviour>.Create(graph, template);
        IngamePositionCorrectionBehaviour clone = playable.GetBehaviour();
        
        // Transform 할당
        clone.targetTransform = targetTransform.Resolve(graph.GetResolver());
        
        // 보정 설정 복사
        clone.correctionMode = correctionMode;
        clone.referencePosition = referencePosition;
        
        // ExposedReference를 우선 사용, 없으면 직접 참조 사용
        Transform resolvedReference = referenceTransform.Resolve(graph.GetResolver());
        if (resolvedReference != null)
        {
            clone.referenceTransform = resolvedReference;
        }
        else
        {
            clone.referenceTransform = directReferenceTransform;
        }
        
        clone.customOffset = customOffset;
        clone.correctionType = correctionType;
        clone.smoothCorrection = smoothCorrection;
        clone.correctionDuration = correctionDuration;
        clone.correctionCurve = correctionCurve;
        clone.detectAnimationStart = detectAnimationStart;
        clone.targetAnimationClips = targetAnimationClips;
        clone.useLocalPosition = useLocalPosition;
        clone.restoreOriginalPosition = restoreOriginalPosition;
        clone.restoreOriginalPositionInEditor = restoreOriginalPositionInEditor;
        clone.onlyCorrectXY = onlyCorrectXY;
        
        return playable;
    }
} 