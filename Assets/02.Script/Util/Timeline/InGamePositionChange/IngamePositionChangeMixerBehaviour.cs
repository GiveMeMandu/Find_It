using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class IngamePositionChangeMixerBehaviour : PlayableBehaviour
{
    // 여러 위치 변경 클립이 겹칠 때의 처리를 위한 믹서
    // 현재는 단순히 개별 클립이 독립적으로 동작하도록 구현
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        // 각 클립이 독립적으로 동작하므로 믹서에서는 특별한 처리 불필요
        // 필요시 여러 위치 변경 클립 간의 우선순위나 블렌딩 로직 추가 가능
    }
} 