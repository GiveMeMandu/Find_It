using System;
using UnityEngine;

namespace SnowRabbit.Stage
{
    [Serializable]
    public abstract class TriggerEvent : TriggerElement
    {
        public virtual bool Satisfy(TriggerContext context)
        {
            return true;
        }
    }

    [Serializable]
    [AddTypeMenu("게임 시작")]
    public class GameStartTriggerEvent : TriggerEvent
    {
    }

    [Serializable]
    [AddTypeMenu("플레이어 사망")]
    public class PlayerDeathTriggerEvent : TriggerEvent
    {
        public override bool Satisfy(TriggerContext context)
        {
            return true;
        }
    }
    
    // 웨이브 완료
    [Serializable]
    [AddTypeMenu("웨이브 완료")]
    public class WaveCompleteTriggerEvent : TriggerEvent
    {
        public string waveName;
        public override bool Satisfy(TriggerContext context)
        {
            return context.key == waveName;
        }
    }
    [Serializable]
    [AddTypeMenu("스포너 스폰 완료")]
    public class SpawnerSpawnEndTriggerEvent : TriggerEvent
    {
        public string spawnerName;
        public override bool Satisfy(TriggerContext context)
        {
            return context.key == spawnerName;
        }
    }
    [Serializable]
    [AddTypeMenu("스포너 처치 완료")]
    public class SpawnerCompleteTriggerEvent : TriggerEvent
    {
        public string spawnerName;
        public override bool Satisfy(TriggerContext context)
        {
            return context.key == spawnerName;
        }
    }

    // [Serializable]
    // [AddTypeMenu("상호작용")]
    // public class InteractionZoneTriggerEvent : TriggerEvent
    // {
    //     public InteractionZone interactionZone;
        
    //     public override bool Satisfy(TriggerContext context)
    //     {
    //         if(interactionZone.gameObject == context.triggeredObject)
    //         {
    //             return true;
    //         }
    //         return false;
    //     }
    // // }

    // [Serializable]
    // [AddTypeMenu("심플 트리거 존 진입")]
    // public class SimpleTriggerZoneEnterEvent : TriggerEvent
    // {
    //     public SimpleTriggerZone triggerZone;
        
    //     public override bool Satisfy(TriggerContext context)
    //     {
    //         if(triggerZone.gameObject == context.triggeredObject)
    //         {
    //             return true;
    //         }
    //         return false;
    //     }
    // }

}