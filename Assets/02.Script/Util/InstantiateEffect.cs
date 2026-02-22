using DeskCat.FindIt.Scripts.Core.Main.Utility.Animation;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateEffect : MonoBehaviour
{
    [Label("이펙트 프리팹들")]
    public List<GameObject> effectPrefabs = new List<GameObject>(); // 인스펙터에서 할당할 이펙트 프리팹들
    [Label("커스텀 위치에 생성")]
    public Transform customTransformPosition; // 커스텀 위치에 생성할 때 사용할 트랜스폼
    public Vector3 positionOffset; // 위치 오프셋 (커스텀 위치에서 추가로 적용할 오프셋)
    public enum EffectSpawnTiming
    {
        OnStart,
        OnEnable,
        OnDisable,
        Manual
    }
    [Label("이펙트 생성 타이밍")]
    public EffectSpawnTiming spawnTiming = EffectSpawnTiming.Manual;
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
            Vector3 basePos = (customTransformPosition != null) ? customTransformPosition.position + positionOffset : transform.position + positionOffset;
            foreach (var prefab in effectPrefabs)
            {
                if (prefab == null) continue;
                var instance = Instantiate(prefab, basePos, Quaternion.identity);

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
}
