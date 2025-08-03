using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class 파티클재생클립 : PlayableAsset, ITimelineClipAsset
{
    public IngameParticlePlayBehaviour template = new IngameParticlePlayBehaviour();
    
    [Header("파티클 타겟")]
    public ExposedReference<ParticleSystem> targetParticleSystem;
    public ExposedReference<Transform> particleTransform;
    
    [Header("재생 모드")]
    public IngameParticlePlayBehaviour.PlayMode playMode = IngameParticlePlayBehaviour.PlayMode.PlayOnStart;
    
    [Header("재생 설정")]
    public bool autoStop = true;
    public float playDuration = 1f;
    public float delayTime = 0f;
    public bool loop = false;
    
    [Header("위치 설정")]
    public bool useCustomPosition = false;
    public Vector3 customPosition = Vector3.zero;
    public ExposedReference<Transform> positionReference;
    public Transform directPositionReference; // 직접 참조용
    
    [Header("스케일 설정")]
    public bool useCustomScale = false;
    public Vector3 customScale = Vector3.one;
    
    [Header("회전 설정")]
    public bool useCustomRotation = false;
    public Vector3 customRotation = Vector3.zero;
    
    [Header("애니메이션 감지")]
    public bool detectAnimationStart = false;
    public string[] targetAnimationClips;
    
    [Header("기타 설정")]
    public bool restoreOriginalSettings = true;
    public bool onlyPlayOnce = true;
    
    [Header("에디터 모드 설정")]
    [Tooltip("에디터에서 Timeline 재생 시 원래 설정 복원 여부")]
    public bool restoreOriginalSettingsInEditor = true;
    
    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<IngameParticlePlayBehaviour>.Create(graph, template);
        IngameParticlePlayBehaviour clone = playable.GetBehaviour();
        
        // ParticleSystem 할당
        clone.targetParticleSystem = targetParticleSystem.Resolve(graph.GetResolver());
        
        // Transform 할당
        clone.particleTransform = particleTransform.Resolve(graph.GetResolver());
        
        // 재생 설정 복사
        clone.playMode = playMode;
        clone.autoStop = autoStop;
        clone.playDuration = playDuration;
        clone.delayTime = delayTime;
        clone.loop = loop;
        
        // 위치 설정 복사
        clone.useCustomPosition = useCustomPosition;
        clone.customPosition = customPosition;
        
        // ExposedReference를 우선 사용, 없으면 직접 참조 사용
        Transform resolvedPositionRef = positionReference.Resolve(graph.GetResolver());
        if (resolvedPositionRef != null)
        {
            clone.positionReference = resolvedPositionRef;
        }
        else
        {
            clone.positionReference = directPositionReference;
        }
        
        // 스케일 설정 복사
        clone.useCustomScale = useCustomScale;
        clone.customScale = customScale;
        
        // 회전 설정 복사
        clone.useCustomRotation = useCustomRotation;
        clone.customRotation = customRotation;
        
        // 애니메이션 감지 설정 복사
        clone.detectAnimationStart = detectAnimationStart;
        clone.targetAnimationClips = targetAnimationClips;
        
        // 기타 설정 복사
        clone.restoreOriginalSettings = restoreOriginalSettings;
        clone.onlyPlayOnce = onlyPlayOnce;
        clone.restoreOriginalSettingsInEditor = restoreOriginalSettingsInEditor;
        
        return playable;
    }
} 