using DeskCat.FindIt.Scripts.Core.Main.System;
using DeskCat.FindIt.Scripts.Core.Main.Utility.Animation;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Label("감지 카메라 (비어 있으면 Camera.main 사용)")]
    [ShowIf("enableClickEffect")]
    public Camera clickCamera;

    [Label("스프라이트 → 이펙트 매핑 SO")]
    [ShowIf("enableClickEffect")]
    [Tooltip("클릭된 오브젝트의 스프라이트를 기준으로 이펙트를 결정합니다. 매핑이 없으면 hitEffectPrefabs로 폴백합니다.")]
    public SpriteEffectMapSO spriteEffectMap;

    // ─── 내부 상태 ───────────────────────────────────────
    private MouseUIController _mouseUIController;
    private bool _initialized = false;

    // ─── Unity 생명 주기 ──────────────────────────────────

    private void Start()
    {
        if (enableClickEffect)
        {
            _mouseUIController = FindAnyObjectByType<MouseUIController>();
            SubscribeClickEvents();
        }

        _initialized = true;
    }

    private void OnEnable()
    {
        // Start 이후 재활성화 시 재구독
        if (_initialized && enableClickEffect)
            SubscribeClickEvents();
    }

    private void OnDisable()
    {
        UnsubscribeClickEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeClickEvents();
    }

    // ─── 구독 관리 ───────────────────────────────────────

    private void SubscribeClickEvents()
    {
        if (_mouseUIController == null)
            _mouseUIController = FindAnyObjectByType<MouseUIController>();

        if (_mouseUIController != null)
            _mouseUIController.OnMouseDownEvent.AddListener(HandleClick);
    }

    private void UnsubscribeClickEvents()
    {
        if (_mouseUIController != null)
            _mouseUIController.OnMouseDownEvent.RemoveListener(HandleClick);
    }

    // ─── 클릭 이펙트 처리 ────────────────────────────────

    /// <summary>
    /// MouseUIController.OnMouseDownEvent 콜백 - 클릭 위치를 기준으로 히트/미스 이펙트를 생성합니다.
    /// 스프라이트 매핑 SO가 있으면 클릭된 오브젝트의 스프라이트로 이펙트를 결정하고,
    /// 없으면 hitEffectPrefabs / missEffectPrefabs로 폴백합니다.
    /// </summary>
    private void HandleClick()
    {
        if (!enableClickEffect) return;

        Camera cam = clickCamera != null ? clickCamera : Camera.main;
        if (cam == null) return;

        // 현재 마우스/터치 스크린 좌표 취득
        Vector2 screenPos = Mouse.current?.position.value ?? (Vector2)Input.mousePosition;

        // 스크린 → 월드 변환 (z=0 평면 기준)
        Vector3 worldPoint = cam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane));
        worldPoint.z = 0f;

        GameObject hitObject = FindClickableObject(cam, screenPos, worldPoint);

        if (hitObject != null)
        {
            // 스프라이트 매핑 SO가 있으면 스프라이트 기반 이펙트 조회
            if (spriteEffectMap != null)
            {
                Sprite sprite = GetSpriteFromObject(hitObject);
                List<GameObject> mappedEffects = sprite != null ? spriteEffectMap.GetEffects(sprite) : null;
                if (mappedEffects != null && mappedEffects.Count > 0)
                {
                    SpawnEffectsAtPosition(mappedEffects, worldPoint);
                    return;
                }
            }

            // 폴백: 기존 hitEffectPrefabs
            SpawnEffectsAtPosition(hitEffectPrefabs, worldPoint);
        }
        else
        {
            SpawnEffectsAtPosition(missEffectPrefabs, worldPoint);
        }
    }

    /// <summary>
    /// 해당 위치에 HiddenObj 또는 LeanClickEvent 컴포넌트를 가진 오브젝트를 반환합니다.
    /// 없으면 null을 반환합니다.
    /// </summary>
    private GameObject FindClickableObject(Camera cam, Vector2 screenPos, Vector3 worldPoint)
    {
        // 2D 물리 검사
        Collider2D[] hits2D = Physics2D.OverlapPointAll(worldPoint);
        foreach (var col in hits2D)
        {
            if (col == null) continue;
            if (col.GetComponent<HiddenObj>() != null || col.GetComponent<LeanClickEvent>() != null)
                return col.gameObject;
        }

        // 3D 물리 검사 (Raycast)
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit3D))
        {
            if (hit3D.collider != null &&
                (hit3D.collider.GetComponent<HiddenObj>() != null ||
                 hit3D.collider.GetComponent<LeanClickEvent>() != null))
                return hit3D.collider.gameObject;
        }

        return null;
    }

    /// <summary>
    /// 오브젝트에서 스프라이트를 추출합니다.
    /// HiddenObj → UISprite → spriteRenderer.sprite → SpriteRenderer → UI.Image 순으로 시도합니다.
    /// </summary>
    private Sprite GetSpriteFromObject(GameObject obj)
    {
        // HiddenObj가 있으면 UISprite → spriteRenderer.sprite 순으로 시도
        HiddenObj hiddenObj = obj.GetComponent<HiddenObj>();
        if (hiddenObj != null)
        {
            if (hiddenObj.UISprite != null)
                return hiddenObj.UISprite;
            if (hiddenObj.spriteRenderer != null && hiddenObj.spriteRenderer.sprite != null)
                return hiddenObj.spriteRenderer.sprite;
        }

        // 직접 SpriteRenderer 검사
        if (obj.TryGetComponent(out SpriteRenderer sr) && sr.sprite != null)
            return sr.sprite;

        // UI Image 검사
        if (obj.TryGetComponent(out UnityEngine.UI.Image img) && img.sprite != null)
            return img.sprite;

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
            var instance = Instantiate(prefab, position, Quaternion.identity);

            if (enableScaleSync)
            {
                switch (scaleSyncMode)
                {
                    case ScaleSyncMode.MatchSource:
                        if (gameObject.TryGetComponent(out BGScaleLerp bGScaleLerp))
                        {
                            instance.transform.localScale = bGScaleLerp.ToScale;
                        }
                        else
                        {
                            Vector3 sourceScale = (customTransformPosition != null)
                                ? customTransformPosition.localScale
                                : transform.localScale;
                            instance.transform.localScale = sourceScale;
                        }
                        break;
                    case ScaleSyncMode.Custom:
                        instance.transform.localScale = customScale;
                        break;
                    case ScaleSyncMode.None:
                    default:
                        break;
                }
            }
        }
    }

}
