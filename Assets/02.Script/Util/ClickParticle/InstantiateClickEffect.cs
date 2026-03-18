using DeskCat.FindIt.Scripts.Core.Main.System;
using DeskCat.FindIt.Scripts.Core.Main.Utility.Animation;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using Pooling;
using Lean.Touch;

public class InstantiateClickEffect : MonoBehaviour
{
    [Label("커스텀 위치에 생성")]
    public Transform customTransformPosition; // 커스텀 위치에 생성할 때 사용할 트랜스폼
    [Label("스케일 동기화")]
    public bool enableScaleSync = false;

    public enum ScaleSyncMode
    {
        None,
        MatchSource,
        Custom
    }
    [Label("스케일 옵션")]
    [ShowIf("enableScaleSync")]
    public ScaleSyncMode scaleSyncMode = ScaleSyncMode.MatchSource;

    [Label("커스텀 스케일")]
    [ShowIf("enableScaleSync")]
    public Vector3 customScale = Vector3.one;

    [Header("클릭 감지 이펙트")]
    [Label("클릭 이펙트 활성화")]
    public bool enableClickEffect = false;

    [Label("HiddenObj / LeanClickEvent 클릭 시 이펙트 (터치 히트)")]
    [ShowIf("enableClickEffect")]
    public List<GameObject> hitEffectPrefabs = new List<GameObject>();

    [Label("빈 영역 클릭 시 이펙트 (터치 미스)")]
    [ShowIf("enableClickEffect")]
    public List<GameObject> missEffectPrefabs = new List<GameObject>();

    [Label("HiddenObj 클릭 시 이펙트 무시")]
    [ShowIf("enableClickEffect")]
    public bool ignoreHiddenObjClick = true;

    [Label("감지 카메라 (비어 있으면 Camera.main 사용)")]
    [ShowIf("enableClickEffect")]
    public Camera clickCamera;

    [Label("스프라이트 → 이펙트 매핑 SO")]
    [ShowIf("enableClickEffect")]
    [Tooltip("클릭된 오브젝트의 스프라이트를 기준으로 파티클 타입을 결정합니다.")]
    public SpriteEffectMapSO spriteEffectMap;

    [Header("디버깅")]
    [Label("디버그 로그 활성화")]
    public bool enableDebug = false;

    // ─── 내부 상태 ───────────────────────────────────────
    private bool _initialized = false;
    private float _lastEffectTime; // 마지막 이펙트 생성 시간
    private const float EFFECT_COOLDOWN = 0.1f; // 이펙트 생성 쿨다운

    private bool _hitProcessedThisFrame = false;
    private bool _tapDetectedThisFrame = false;
    private Vector2 _lastTapPos;

    // ─── Unity 생명 주기 ──────────────────────────────────

    private void OnEnable()
    {
        LeanTouch.OnFingerTap += HandleFingerTap;
        LeanClickEvent.OnGlobalClickSuccess += HandleHitFromLeanClickEvent;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerTap -= HandleFingerTap;
        LeanClickEvent.OnGlobalClickSuccess -= HandleHitFromLeanClickEvent;
    }

    private void OnDestroy()
    {
        LeanTouch.OnFingerTap -= HandleFingerTap;
        LeanClickEvent.OnGlobalClickSuccess -= HandleHitFromLeanClickEvent;
    }

    private void LateUpdate()
    {
        if (_tapDetectedThisFrame)
        {
            if (!_hitProcessedThisFrame)
            {
                // 히트가 없었으므로 Miss 처리
                if (Time.time - _lastEffectTime >= EFFECT_COOLDOWN)
                {
                    _lastEffectTime = Time.time;
                    HandleMissEffect(_lastTapPos);
                }
            }
            
            // 상태 초기화
            _tapDetectedThisFrame = false;
            _hitProcessedThisFrame = false;
        }
        else
        {
            _hitProcessedThisFrame = false;
        }
    }

    // ─── 이벤트 처리 ───────────────────────────────────────

    private void HandleFingerTap(LeanFinger finger)
    {
        if (!enableClickEffect) return;

        if (enableDebug) Debug.Log("<b>[InstantiateClickEffect]</b> HandleFingerTap: Tap detected.");
        _tapDetectedThisFrame = true;
        _lastTapPos = finger.ScreenPosition;
    }

    private void HandleHitFromLeanClickEvent(GameObject hitObject, Vector2 screenPos)
    {
        if (!enableClickEffect) return;

        // 히트 플래그 설정 (LateUpdate에서 Miss 발동을 막음)
        _hitProcessedThisFrame = true;
        _lastEffectTime = Time.time; // 마지막 이펙트 생성 시간 기록

        if (ignoreHiddenObjClick && hitObject.GetComponent<HiddenObj>() != null)
        {
            if (enableDebug) Debug.Log("<b>[InstantiateClickEffect]</b> HandleHit: Ignored due to HiddenObj.");
            return;
        }

        Camera cam = clickCamera != null ? clickCamera : Camera.main;
        if (cam == null)
        {
            if (enableDebug) Debug.LogError("<b>[InstantiateClickEffect]</b> HandleHitFromLeanClickEvent: Camera not found!");
            return;
        }

        // 스크린 → 월드 변환 (z=0 평면 기준)
        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane));
        worldPoint.z = 0f;

        if (enableDebug) Debug.Log($"<b>[InstantiateClickEffect]</b> HandleHit: Hit object reported by LeanClickEvent: '{hitObject.name}'");

        // 스프라이트 매핑 SO가 있으면 매핑된 이펙트 프리팹 리스트 조회
        if (spriteEffectMap != null)
        {
            Sprite sprite = GetSpriteFromObject(hitObject);
            SpriteEffectMapSO.ParticleType pType = SpriteEffectMapSO.ParticleType.None;

            if (sprite != null)
            {
                if (enableDebug) Debug.Log($"<b>[InstantiateClickEffect]</b> HandleHit: Sprite '{sprite.name}' successfully retrieved from '{hitObject.name}'.");
                pType = spriteEffectMap.GetParticleType(sprite);
            }
            else
            {
                if (enableDebug) Debug.LogWarning($"<b>[InstantiateClickEffect]</b> HandleHit: Could not retrieve a sprite from '{hitObject.name}'. Attempting to find particle type by object name.");
                pType = spriteEffectMap.GetParticleTypeByName(hitObject.name);
            }

            if (pType != SpriteEffectMapSO.ParticleType.None)
            {
                if (enableDebug) Debug.Log($"<b>[InstantiateClickEffect]</b> HandleHit: Resolved ParticleType is '{pType}'.");
                
                List<GameObject> mappedEffects = spriteEffectMap.GetEffectPrefabsByType(pType);
                if (mappedEffects != null && mappedEffects.Count > 0)
                {
                    if (enableDebug) Debug.Log($"<b>[InstantiateClickEffect]</b> HandleHit: Found {mappedEffects.Count} mapped effect(s) for ParticleType '{pType}'. Spawning effects.");
                    SpawnEffectsAtPosition(mappedEffects, worldPoint);
                    return;
                }
                else
                {
                    if (enableDebug) Debug.LogWarning($"<b>[InstantiateClickEffect]</b> HandleHit: No effect prefabs mapped for ParticleType '{pType}'. Falling back to default hit effects.");
                }
            }
            else
            {
                if (enableDebug) Debug.LogWarning($"<b>[InstantiateClickEffect]</b> HandleHit: ParticleType could not be resolved. Falling back to default hit effects.");
            }
        }

        // 폴백: 기존 hitEffectPrefabs
        if (enableDebug) Debug.Log("<b>[InstantiateClickEffect]</b> HandleHit: Spawning default hit effects.");
        SpawnEffectsAtPosition(hitEffectPrefabs, worldPoint);
    }

    private void HandleMissEffect(Vector2 screenPos)
    {
        if (!enableClickEffect) return;

        Camera cam = clickCamera != null ? clickCamera : Camera.main;
        if (cam == null) return;

        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane));
        worldPoint.z = 0f;

        if (enableDebug) Debug.Log("<b>[InstantiateClickEffect]</b> HandleMissEffect: No clickable object reported. Spawning miss effects.");
        SpawnEffectsAtPosition(missEffectPrefabs, worldPoint);
    }

    /// <summary>
    /// 오브젝트에서 스프라이트를 추출합니다.
    /// HiddenObj → UISprite → spriteRenderer.sprite → SpriteRenderer → UI.Image 순으로 시도합니다.
    /// </summary>
    private Sprite GetSpriteFromObject(GameObject obj)
    {
        // HiddenObj가 있으면 캐시/보조 로직이 포함된 GetUISprite를 우선 사용
        HiddenObj hiddenObj = obj.GetComponent<HiddenObj>();
        if (hiddenObj != null)
        {
            Sprite uiSprite = hiddenObj.GetUISprite();
            if (uiSprite != null)
            {
                if (enableDebug) Debug.Log($"<b>[GetSpriteFromObject]</b> Found sprite '{uiSprite.name}' via hiddenObj.GetUISprite().");
                return uiSprite;
            }

            if (hiddenObj.spriteRenderer != null && hiddenObj.spriteRenderer.sprite != null)
            {
                if (enableDebug) Debug.Log($"<b>[GetSpriteFromObject]</b> Found sprite '{hiddenObj.spriteRenderer.sprite.name}' via hiddenObj.spriteRenderer.");
                return hiddenObj.spriteRenderer.sprite;
            }

            // 일부 프리팹은 자식에 SpriteRenderer가 있으므로 추가로 탐색
            SpriteRenderer childSpriteRenderer = hiddenObj.GetComponentInChildren<SpriteRenderer>();
            if (childSpriteRenderer != null && childSpriteRenderer.sprite != null)
            {
                if (enableDebug) Debug.Log($"<b>[GetSpriteFromObject]</b> Found sprite '{childSpriteRenderer.sprite.name}' via hiddenObj.GetComponentInChildren<SpriteRenderer>().");
                return childSpriteRenderer.sprite;
            }
        }

        // 직접 SpriteRenderer 검사
        if (obj.TryGetComponent(out SpriteRenderer sr) && sr.sprite != null)
        {
            if (enableDebug) Debug.Log($"<b>[GetSpriteFromObject]</b> Found sprite '{sr.sprite.name}' via obj.GetComponent<SpriteRenderer>().");
            return sr.sprite;
        }

        // UI Image 검사
        if (obj.TryGetComponent(out UnityEngine.UI.Image img) && img.sprite != null)
        {
            if (enableDebug) Debug.Log($"<b>[GetSpriteFromObject]</b> Found sprite '{img.sprite.name}' via obj.GetComponent<Image>().");
            return img.sprite;
        }

        // 자식 오브젝트에서 SpriteRenderer 검사
        SpriteRenderer childSr = obj.GetComponentInChildren<SpriteRenderer>();
        if (childSr != null && childSr.sprite != null)
        {
            if (enableDebug) Debug.Log($"<b>[GetSpriteFromObject]</b> Found sprite '{childSr.sprite.name}' via obj.GetComponentInChildren<SpriteRenderer>().");
            return childSr.sprite;
        }

        // 자식 오브젝트에서 UI Image 검사
        UnityEngine.UI.Image childImg = obj.GetComponentInChildren<UnityEngine.UI.Image>();
        if (childImg != null && childImg.sprite != null)
        {
            if (enableDebug) Debug.Log($"<b>[GetSpriteFromObject]</b> Found sprite '{childImg.sprite.name}' via obj.GetComponentInChildren<Image>().");
            return childImg.sprite;
        }

        if (enableDebug) Debug.LogWarning($"<b>[GetSpriteFromObject]</b> Could not find any sprite on '{obj.name}'.");
        return null;
    }

    /// <summary>
    /// 지정된 월드 위치에 프리팹 리스트를 인스턴스화합니다.
    /// </summary>
    private void SpawnEffectsAtPosition(List<GameObject> prefabs, Vector3 position)
    {
        if (prefabs == null || prefabs.Count == 0) return;

        foreach (var prefab in prefabs)
        {
            if (prefab == null) continue;
            
            // PoolManager를 사용하여 PoolObject 가져오기
            var poolObject = PoolManager.Instance.Pull<PoolObject>(prefab, position, Quaternion.identity);
            
            if (poolObject == null) continue;

            if (enableScaleSync)
            {
                switch (scaleSyncMode)
                {
                    case ScaleSyncMode.MatchSource:
                        if (gameObject.TryGetComponent(out BGScaleLerp bGScaleLerp))
                        {
                            poolObject.transform.localScale = bGScaleLerp.ToScale;
                        }
                        else
                        {
                            Vector3 sourceScale = (customTransformPosition != null)
                                ? customTransformPosition.localScale
                                : transform.localScale;
                            poolObject.transform.localScale = sourceScale;
                        }
                        break;
                    case ScaleSyncMode.Custom:
                        poolObject.transform.localScale = customScale;
                        break;
                    case ScaleSyncMode.None:
                    default:
                        break;
                }
            }
        }
    }

}
