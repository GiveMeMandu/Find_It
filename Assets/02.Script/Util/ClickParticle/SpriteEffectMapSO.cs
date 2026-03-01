using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스프라이트와 이펙트 프리팹을 매핑하는 ScriptableObject.
/// 클릭된 오브젝트의 스프라이트를 기준으로 해당하는 이펙트를 생성할 때 사용합니다.
/// </summary>
[CreateAssetMenu(fileName = "SpriteEffectMap", menuName = "클릭/SpriteEffectMap", order = 0)]
public class SpriteEffectMapSO : ScriptableObject
{
    public enum ParticleType
    {
        None,
        FruitJuicy,    // 딸기, 열매 등 과일류 (과즙 튀는 효과)
        Leaf,   // 나무, 덤불, 풀숲
        EarthDust,     // 흙, 당근 밭 (먼지나 흙덩이 튀는 효과)
        WoodyChop,     // 그루터기, 통나무 (나무 파편 효과)
        MagicalGlow,   // 숲의 신비로운 분위기, 집 입구 (반짝임 효과)
        FoodSweet,     // 케이크, 간식류 (하트나 달콤한 시각 효과)
        StoneCrunch,   // 돌길, 돌담 (돌 가루 효과)
        FabricSoft,    // 토끼 옷, 돗자리 (부드러운 실이나 솜 효과)
        DustPuff,       // 먼지, 모래 (작은 먼지 구름 효과)
        WaterSplash,    // 연못, 물웅덩이 (물 튀는 효과) bubbleBlastUnderWater, watersplashTinySharp
    }

    [System.Serializable]
    public class SpriteEffectEntry
    {
        [Tooltip("매핑 기준 스프라이트")]
        public Sprite sprite;

        [Tooltip("해당 스프라이트 클릭 시 생성할 이펙트 프리팹 리스트")]
        public List<GameObject> effectPrefabs = new List<GameObject>();
    }

    [Tooltip("스프라이트 → 이펙트 매핑 목록")]
    public List<SpriteEffectEntry> entries = new List<SpriteEffectEntry>();

    /// <summary>
    /// 주어진 스프라이트에 해당하는 이펙트 프리팹 리스트를 반환합니다.
    /// 매핑이 없으면 null을 반환합니다.
    /// </summary>
    public List<GameObject> GetEffects(Sprite sprite)
    {
        if (sprite == null) return null;

        foreach (var entry in entries)
        {
            if (entry.sprite == sprite)
                return entry.effectPrefabs;
        }

        return null;
    }
}
