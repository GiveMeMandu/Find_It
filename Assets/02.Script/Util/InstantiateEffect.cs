using DeskCat.FindIt.Scripts.Core.Main.Utility.Animation;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateEffect : MonoBehaviour
{
    [LabelText("이펙트 프리팹들")]
    public List<GameObject> effectPrefabs = new List<GameObject>(); // 인스펙터에서 할당할 이펙트 프리팹들
    [LabelText("커스텀 위치에 생성")]
    public Transform customTransformPosition; // 커스텀 위치에 생성할 때 사용할 트랜스폼
    public Vector3 positionOffset; // 위치 오프셋 (커스텀 위치에서 추가로 적용할 오프셋)
    public enum EffectSpawnTiming
    {
        OnStart,
        OnEnable,
        OnDisable,
        Manual
    }
    [LabelText("이펙트 생성 타이밍")]
    public EffectSpawnTiming spawnTiming = EffectSpawnTiming.Manual;
    [LabelText("스케일 동기화")]
    public bool enableScaleSync = false;

    public enum ScaleSyncMode
    {
        None,
        MatchSource,
        Custom
    }
    [LabelText("스케일 옵션")]
    [ShowIf("enableScaleSync")]
    public ScaleSyncMode scaleSyncMode = ScaleSyncMode.MatchSource;

    [LabelText("커스텀 스케일")]
    [ShowIf("enableScaleSync")]
    public Vector3 customScale = Vector3.one;

    [LabelText("UI Transform 위치 사용")]
    public bool isUIPosition = false;

    [LabelText("카메라로부터 월드 거리")]
    [ShowIf("isUIPosition")]
    public float uiWorldDepth = 10f;

    [Header("Random Position")]
    [LabelText("랜덤 위치 활성화")]
    public bool enableRandomPosition = false;

    [LabelText("랜덤 위치 최솟값")]
    [ShowIf("enableRandomPosition")]
    public Vector3 randomPositionMin = Vector3.zero;

    [LabelText("랜덤 위치 최댓값")]
    [ShowIf("enableRandomPosition")]
    public Vector3 randomPositionMax = Vector3.zero;

    [Header("Random Rotation")]
    [LabelText("랜덤 회전 활성화")]
    public bool enableRandomRotation = false;

    public enum RandomRotationMode
    {
        FullRandom,
        CustomRange
    }

    [LabelText("랜덤 회전 모드")]
    [ShowIf("enableRandomRotation")]
    public RandomRotationMode randomRotationMode = RandomRotationMode.FullRandom;

    [LabelText("랜덤 회전 최솟값 (Euler)")]
    [ShowIf("enableRandomRotation")]
    public Vector3 randomRotationMin = Vector3.zero;

    [LabelText("랜덤 회전 최댓값 (Euler)")]
    [ShowIf("enableRandomRotation")]
    public Vector3 randomRotationMax = new Vector3(360f, 360f, 360f);

    private void Start()
    {
        if (spawnTiming == EffectSpawnTiming.OnStart)
            PlayEffect();
    }

    private void OnEnable()
    {
        if (spawnTiming == EffectSpawnTiming.OnEnable)
            PlayEffect();
    }

    private void OnDisable()
    {
        if (spawnTiming == EffectSpawnTiming.OnDisable)
            PlayEffect();
    }
    public void PlayEffect()
    {
        if (effectPrefabs != null && effectPrefabs.Count > 0)
        {
            Vector3 basePos = GetBasePosition();
            foreach (var prefab in effectPrefabs)
            {
                if (prefab == null) continue;

                Vector3 spawnPos = basePos + GetRandomPositionOffset();
                Quaternion spawnRot = GetRandomRotation();

                var instance = Instantiate(prefab, spawnPos, spawnRot);

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
                                Vector3 sourceScale = (customTransformPosition != null) ? customTransformPosition.localScale : transform.localScale;
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
        else
        {
            Debug.LogWarning("Effect Prefab이 할당되지 않았습니다.");
        }
    }

    private Vector3 GetBasePosition()
    {
        Transform source = (customTransformPosition != null) ? customTransformPosition : transform;

        if (isUIPosition)
        {
            // Screen Space - Overlay UI의 position은 스크린 픽셀 좌표이므로 월드 변환
            Vector3 screenPos = new Vector3(source.position.x, source.position.y, uiWorldDepth);
            return Camera.main.ScreenToWorldPoint(screenPos) + positionOffset;
        }

        return source.position + positionOffset;
    }

    private Vector3 GetRandomPositionOffset()
    {
        if (!enableRandomPosition) return Vector3.zero;

        return new Vector3(
            Random.Range(randomPositionMin.x, randomPositionMax.x),
            Random.Range(randomPositionMin.y, randomPositionMax.y),
            Random.Range(randomPositionMin.z, randomPositionMax.z)
        );
    }

    private Quaternion GetRandomRotation()
    {
        if (!enableRandomRotation) return Quaternion.identity;

        switch (randomRotationMode)
        {
            case RandomRotationMode.FullRandom:
                return Random.rotation;
            case RandomRotationMode.CustomRange:
                Vector3 euler = new Vector3(
                    Random.Range(randomRotationMin.x, randomRotationMax.x),
                    Random.Range(randomRotationMin.y, randomRotationMax.y),
                    Random.Range(randomRotationMin.z, randomRotationMax.z)
                );
                return Quaternion.Euler(euler);
            default:
                return Quaternion.identity;
        }
    }
}
