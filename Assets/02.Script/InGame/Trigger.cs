using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace SnowRabbit.Stage
{
    [Serializable]
    public class Trigger
    {
        [ShowNativeProperty]
        public string DisplayName => name + (isDisabled ? " (비활성화)" : "");
        public string name;
        public bool isPersistent;
        public bool isDisabled;
        public bool isTutorialTrigger;

        [SerializeReference]
        [SubclassSelector]
        public List<TriggerEvent> events = new();

        [SerializeReference]
        [SubclassSelector]
        public List<TriggerCondition> conditions = new();

        [SerializeReference]
        [SubclassSelector]
        public List<TriggerAction> actions = new();
        public bool SatisfyConditions(TriggerContext triggerContext)
        {
            foreach (var e in events)
            {
                if (!e.Satisfy(triggerContext))
                {
                    return false;
                }
            }

            foreach (var condition in conditions)
            {
                if (!condition.Satisfy(triggerContext))
                {
                    return false;
                }
            }

            return true;
        }
    }

    [Serializable]
    public abstract class TriggerElement
    {
        public string name;
    }

    public class TriggerInstance
    {
        public Trigger Trigger { get; }
        public bool IsTutorialTrigger { get; }

        public int TriggeredCount { get; set; }

        public bool IsActive => (Trigger.isPersistent || !IsTriggered) && !Trigger.isDisabled;
        public bool IsTriggered => TriggeredCount > 0;

        public TriggerInstance(Trigger trigger, bool ignoreTutorialTrigger = false)
        {
            Trigger = trigger;

            if (!ignoreTutorialTrigger)
            {
                IsTutorialTrigger = Trigger.isTutorialTrigger;
            }
        }

        public void Reset()
        {
            if (!IsTutorialTrigger)
            {
                TriggeredCount = 0;
            }
        }

        public void Execute(TriggerContext context)
        {
            if (!IsActive)
            {
                Debug.LogError($"Inactive trigger execute request: {Trigger.name}");
                return;
            }

            try
            {
                TriggeredCount++;

                foreach (var action in Trigger.actions)
                {
                    action.Execute(context);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"트리거 <color=yellow>{Trigger.name}</color> 실행 중 에러가 발생했습니다: {exception}");
            }
        }
    }
}