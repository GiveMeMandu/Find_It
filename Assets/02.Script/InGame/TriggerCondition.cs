using System;

namespace SnowRabbit.Stage
{
    [Serializable]
    public abstract class TriggerCondition : TriggerElement
    {
        public abstract bool Satisfy(TriggerContext context);
    }
}