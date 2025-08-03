using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class IngameParticlePlayBehaviour : PlayableBehaviour
{
    [Header("파티클 타겟")]
    public ParticleSystem targetParticleSystem;
    public Transform particleTransform; // 파티클 위치 설정용
    
    [Header("재생 모드")]
    public PlayMode playMode = PlayMode.PlayOnStart;
    
    [Header("재생 설정")]
    public bool autoStop = true;
    public float playDuration = 1f;
    public float delayTime = 0f; // PlayOnCustomTime 모드용 지연 시간
    public bool loop = false;
    
    [Header("위치 설정")]
    public bool useCustomPosition = false;
    public Vector3 customPosition = Vector3.zero;
    public Transform positionReference; // 위치 참조용 Transform
    
    [Header("스케일 설정")]
    public bool useCustomScale = false;
    public Vector3 customScale = Vector3.one;
    
    [Header("회전 설정")]
    public bool useCustomRotation = false;
    public Vector3 customRotation = Vector3.zero;
    
    [Header("애니메이션 감지")]
    public bool detectAnimationStart = false;
    public string[] targetAnimationClips; // 특정 애니메이션만 감지
    
    [Header("기타 설정")]
    public bool restoreOriginalSettings = true;
    public bool onlyPlayOnce = true; // 한 번만 재생
    
    [Header("에디터 모드 설정")]
    [Tooltip("에디터에서 Timeline 재생 시 원래 설정 복원 여부")]
    public bool restoreOriginalSettingsInEditor = true;
    
    private bool isInitialized = false;
    private bool particlePlayed = false;
    private float playTime = 0f;
    private Animator targetAnimator;
    
    // 원래 설정 저장용
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    
    public enum PlayMode
    {
        PlayOnStart,          // 클립 시작 시 즉시 재생
        PlayOnAnimationStart, // 애니메이션 시작 감지 시 재생
        PlayOnCustomTime      // 지정된 시간에 재생
    }
    
    public override void OnPlayableCreate(Playable playable)
    {
        isInitialized = false;
        particlePlayed = false;
        playTime = 0f;
    }
    
    public override void OnGraphStart(Playable playable)
    {
        if (targetParticleSystem != null && !isInitialized)
        {
            // 원래 설정 저장
            originalPosition = targetParticleSystem.transform.position;
            originalScale = targetParticleSystem.transform.localScale;
            originalRotation = targetParticleSystem.transform.rotation;
            
            // 애니메이터 찾기
            if (particleTransform != null)
            {
                targetAnimator = particleTransform.GetComponent<Animator>();
            }
            else
            {
                targetAnimator = targetParticleSystem.GetComponent<Animator>();
            }
            
            isInitialized = true;
        }
    }
    
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        playTime = 0f;
        particlePlayed = false; // 클립이 다시 시작될 때 재생 상태 초기화
        
        if (playMode == PlayMode.PlayOnStart)
        {
            PlayParticle();
        }
    }
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (targetParticleSystem == null || !isInitialized) 
        {
            Debug.LogWarning("ParticlePlay: targetParticleSystem이 할당되지 않았습니다!");
            return;
        }
        
        float deltaTime = (float)info.deltaTime;
        playTime += deltaTime;
        
        // 애니메이션 시작 감지
        if (playMode == PlayMode.PlayOnAnimationStart && 
            detectAnimationStart && !particlePlayed)
        {
            if (DetectAnimationStart())
            {
                PlayParticle();
            }
        }
        
        // 지정된 시간에 재생
        if (playMode == PlayMode.PlayOnCustomTime && 
            !particlePlayed && playTime >= delayTime)
        {
            PlayParticle();
        }
        
        // 자동 정지 처리
        if (autoStop && particlePlayed && !loop && playTime >= playDuration)
        {
            StopParticle();
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
    
    private void PlayParticle()
    {
        if (particlePlayed && onlyPlayOnce) return;
        
        // 위치 설정
        if (useCustomPosition)
        {
            Vector3 targetPosition = customPosition;
            if (positionReference != null)
            {
                targetPosition = positionReference.position;
            }
            targetParticleSystem.transform.position = targetPosition;
        }
        
        // 스케일 설정
        if (useCustomScale)
        {
            targetParticleSystem.transform.localScale = customScale;
        }
        
        // 회전 설정
        if (useCustomRotation)
        {
            targetParticleSystem.transform.rotation = Quaternion.Euler(customRotation);
        }
        
        // 파티클 재생
        targetParticleSystem.Play();
        particlePlayed = true;
        
        Debug.Log($"[파티클재생] {targetParticleSystem.name} 재생 시작 (모드: {playMode})");
    }
    
    private void StopParticle()
    {
        if (targetParticleSystem.isPlaying)
        {
            targetParticleSystem.Stop();
            Debug.Log($"[파티클재생] {targetParticleSystem.name} 정지");
        }
    }
    
    public override void OnGraphStop(Playable playable)
    {
        // 에디터에서 플레이 중이 아닐 때와 런타임/플레이 중을 구분해서 원래 설정 복원 처리
        bool shouldRestore = false;
        
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // 에디터에서 플레이 중이 아닐 때만 에디터 전용 설정 사용
            shouldRestore = restoreOriginalSettingsInEditor;
        }
        else
        {
            // 에디터에서 플레이 중일 때는 런타임 설정 사용
            shouldRestore = restoreOriginalSettings;
        }
        #else
        // 빌드된 게임에서는 런타임 설정 사용
        shouldRestore = restoreOriginalSettings;
        #endif
        
        if (shouldRestore && targetParticleSystem != null && isInitialized)
        {
            // 원래 설정 복원
            targetParticleSystem.transform.position = originalPosition;
            targetParticleSystem.transform.localScale = originalScale;
            targetParticleSystem.transform.rotation = originalRotation;
            
            // 파티클 정지
            if (targetParticleSystem.isPlaying)
            {
                targetParticleSystem.Stop();
            }
            
            string mode = Application.isPlaying ? "플레이 중" : "에디터 모드";
            Debug.Log($"[파티클재생] 원래 설정 복원 완료 ({mode})");
        }
    }
} 