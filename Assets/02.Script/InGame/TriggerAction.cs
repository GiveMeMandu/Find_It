using System;
using NaughtyAttributes;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;
using I2.Loc;

namespace SnowRabbit.Stage
{

    [Serializable]
    public abstract class TriggerAction : TriggerElement
    {
        public abstract void Execute(TriggerContext context = null);
    }
    public class TriggerContext
    {
        public string key;
        public GameObject triggeredObject;
    }

    // VictoryTriggerAction
    [Serializable]
    [AddTypeMenu("승리")]
    public class VictoryTriggerAction : TriggerAction
    {
        public override void Execute(TriggerContext context = null)
        {
            // StageScope.Instance.StageProcessor.Victory();
        }
    }

    // DefeatTriggerAction
    [Serializable]
    [AddTypeMenu("패배")]
    public class DefeatTriggerAction : TriggerAction
    {
        public override void Execute(TriggerContext context = null)
        {
            // StageScope.Instance.StageProcessor.Defeat();
        }
    }

    [Serializable]
    [AddTypeMenu("지연 트리거")]
    public class DelayTriggerAction : TriggerAction
    {
        public float delay;
        public string triggerName;

        public override void Execute(TriggerContext context = null)
        {
            ExecuteAsync(context).Forget();
        }

        private async UniTask ExecuteAsync(TriggerContext context)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));

            if (!string.IsNullOrEmpty(triggerName))
            {
                // StageScope.Instance.StageProcessor.ActivateTrigger(triggerName);
            }
        }
    }


    // GameSequenceTriggerAction
    [Serializable]
    [AddTypeMenu("인게임/게임 시퀀스")]
    public class GameSequenceTriggerAction : TriggerAction
    {
        // public GameSequenceBase gameSequence;
        public string[] endTriggers;
        public override void Execute(TriggerContext context = null)
        {
            PlayGameSequence().Forget();
        }
        private async UniTask PlayGameSequence()
        {
            // await gameSequence.Play();
            foreach (var trigger in endTriggers)
            {
                // StageScope.Instance.StageProcessor.ActivateTrigger(trigger);
            }
        }
    }

    [Serializable]
    [AddTypeMenu("타임라인 실행")]
    public class TimelineTriggerAction : TriggerAction
    {
        public PlayableDirector timeline;
        [Tooltip("타임라인 종료 후 실행할 트리거")]
        public string[] endTriggers;

        public override void Execute(TriggerContext context = null)
        {
            if (timeline != null)
            {
                PlayTimeline().Forget();
            }
            else
            {
                Debug.LogWarning("Timeline이 설정되지 않았습니다.");
            }
        }

        private async UniTask PlayTimeline()
        {
            // using (StageScope.Instance.GameInputManager.CreateLockInputScope())
            // {
                // timeline.Play();
                await UniTask.WaitWhile(() => timeline.state == PlayState.Playing);
            // }
            // foreach (var trigger in endTriggers)
            // {
            //     StageScope.Instance.StageProcessor.ActivateTrigger(trigger);
            // }
        }
    }
    [Serializable]
    [AddTypeMenu("Unity Event 실행")]
    public class UnityEventTriggerAction : TriggerAction
    {
        public UnityEngine.Events.UnityEvent onExecute;

        public override void Execute(TriggerContext context = null)
        {
            onExecute?.Invoke();
        }
    }
    [Serializable]
    [AddTypeMenu("Trigger 온오프")]
    public class TriggerEnableTriggerAction : TriggerAction
    {
        public string triggerName;
        public bool setEnable = true;
        public override void Execute(TriggerContext context = null)
        {
            // var trigger = StageScope.Instance.StageProcessor.Triggers.Find(t => t.Trigger.name == triggerName);
            // if (trigger == null)
            // {
            //     Debug.LogError($"TriggerEnableTriggerAction: {triggerName} 트리거를 찾을 수 없습니다.");
            //     return;
            // }
            // trigger.Trigger.isDisabled = !setEnable;
        }
    }



}