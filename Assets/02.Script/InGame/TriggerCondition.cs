using System;

namespace Stage
{
    [Serializable]
    public abstract class TriggerCondition : TriggerElement
    {
        public abstract bool Satisfy(TriggerContext context);
    }
}