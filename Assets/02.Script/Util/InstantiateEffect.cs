using NaughtyAttributes;
using UnityEngine;

public class InstantiateEffect : MonoBehaviour
{
    public GameObject effectPrefab; // 인스펙터에서 할당할 이펙트 프리팹
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
        if (effectPrefab != null)
        {
            if (customTransformPosition != null)
                Instantiate(effectPrefab, customTransformPosition.position + positionOffset, Quaternion.identity);
            else
                Instantiate(effectPrefab, transform.position + positionOffset, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Effect Prefab이 할당되지 않았습니다.");
        }
    }
}
