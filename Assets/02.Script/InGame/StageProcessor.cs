using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SnowRabbit.Stage
{
    public class StageProcessor : MonoBehaviour
    {
        public enum StageStateType
        {
            Initialize,
            Playing,
            Result,
        }
        public StageStateType StageState { get; private set; } = StageStateType.Initialize;
        public List<TriggerInstance> Triggers { get; } = new();
        private StageData StageData;
        // private StageData StageData => StageScope.Instance.StageData;

        public string DebugTriggerName;
        [Button("디버그 - 트리거 실행")]
        public void Debug_ActivateTrigger()
        {
            if (!string.IsNullOrEmpty(DebugTriggerName))
            {
                ActivateTrigger(DebugTriggerName);
            }
        }

        protected void Awake()
        {
            SetUp();
        }

        // Start is called before the first frame update
        void Start()
        {
            StartGameTask().Forget();
        }

        // Update is called once per frame
        void Update()
        {
            // UIManager uiManager = StageScope.Instance.UIManager;
            // if (uiManager.EscapePressedThisFrame && StageState == StageStateType.Playing)
            // {
            //     if (uiManager.CurrentPage == null && uiManager.CurrentPopup == null)
            //     {
            //         uiManager.EscapePressedThisFrame = false;
            //         uiManager.OpenPage<SettingsPage>();
            //     }
            // }
        }

        private void SetUp()
        {
            Triggers.Clear();
            // TODO: 튜토리얼 스킵 처리 필요.
            Triggers.AddRange(StageData.triggers.Select(t => new TriggerInstance(t, false)));
        }

        public void ActivateTrigger(string triggerName, TriggerContext triggerContext = null)
        {
            if (triggerContext == null)
            {
                triggerContext = new TriggerContext();
            }
            foreach (var triggerInstance in Triggers)
            {
                if (triggerInstance.Trigger.name == triggerName)
                {
                    // 트리거 컨디션 체크
                    bool canExecute = true;
                    foreach (var condition in triggerInstance.Trigger.conditions)
                    {
                        if (!condition.Satisfy(triggerContext))
                        {
                            canExecute = false;
                            break;
                        }
                    }

                    // 컨디션이 모두 만족되면 액션 실행
                    if (canExecute)
                    {
                        foreach (var action in triggerInstance.Trigger.actions)
                        {
                            Debug.Log($"트리거 실행 ({action.GetType()}): {triggerInstance.Trigger.name}");
                            action.Execute(triggerContext);
                        }
                    }
                }

            }
        }
        public void OnWaveComplete(string waveName)
        {
            TryExecuteTriggers<WaveCompleteTriggerEvent>(
                new TriggerContext()
                {
                    key = waveName
                });
        }
        public void OnSpawnerSpawnEnd(string spawnerName)
        {
            TryExecuteTriggers<SpawnerSpawnEndTriggerEvent>(
                new TriggerContext()
                {
                    key = spawnerName
                });
        }
        public void OnSpawnerClear(string spawnerName)
        {
            TryExecuteTriggers<SpawnerCompleteTriggerEvent>(
                new TriggerContext()
                {
                    key = spawnerName
                });
        }
        public void OnPlayerDeath()
        {
            TryExecuteTriggers<PlayerDeathTriggerEvent>();
        }
        async UniTask StartGameTask()
        {
            // TODO : 플레이어 등장 연출
            await UniTask.Yield();
            StageState = StageStateType.Playing;
            TryExecuteTriggers<GameStartTriggerEvent>();
        }

        public void Victory()
        {
            StageState = StageStateType.Result;

            // string sceneName = SceneManager.GetActiveScene().name;
            // var currentStageGameData = GameDataManager.Instance.Database.StageGameDataTable.FindByScenePath(sceneName).First;
            // if (currentStageGameData == null)
            // {
            //     Debug.LogError($"VictoryTriggerAction: {sceneName} 스테이지 데이터를 찾을 수 없습니다.");
            //     return;
            // }
            // // var currentEpisodeGameData = GameDataManager.Instance.Database.EpisodeGameDataTable.FindById(currentStageGameData.EpisodeId);
            // string nextSceneName = "LobbyScene";
            // if (GameDataManager.Instance.Database.StageGameDataTable.TryFindById(currentStageGameData.Id + 1, out var nextStageGameData))
            // {
            //     nextSceneName = nextStageGameData.ScenePath;
            // }
            // StageScope.Instance.UIManager.LoadScene(nextSceneName).Forget();
        }

        public void Defeat()
        {
            StageState = StageStateType.Result;
            // StageScope.Instance.UIManager.LoadScene("LobbyScene").Forget();
        }

        public void TryExecuteTriggers<T>(TriggerContext triggerContext = null) where T : TriggerEvent
        {
            foreach (var triggerInstance in Triggers)
            {
                if (!triggerInstance.IsActive)
                {
                    continue;
                }

                if (triggerInstance.Trigger.isTutorialTrigger)
                {
                    continue;
                }

                foreach (var evt in triggerInstance.Trigger.events)
                {
                    if (evt is T)
                    {
                        if (triggerContext == null)
                        {
                            triggerContext = new TriggerContext();
                        }
                        if (!triggerInstance.Trigger.SatisfyConditions(triggerContext))
                        {
                            continue;
                        }
                        Debug.Log($"트리거 실행 ({evt.GetType()}): {triggerInstance.Trigger.name}");
                        triggerInstance.Execute(triggerContext);
                        break;
                    }
                }
            }
        }
    }
}
