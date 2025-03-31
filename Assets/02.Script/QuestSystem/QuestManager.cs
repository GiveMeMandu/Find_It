using System;
using System.Collections;
using System.Collections.Generic;
using BunnyCafe.Events;
using UnityEngine;

namespace Manager
{
    public partial class QuestManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private bool loadQuestState = true;

        public QuestEvents QuestEvents;
        private Dictionary<string, Quest> questMap;

        public int AllQuestCount() => questMap.Count;
        public List<Quest> GetDailyQuests(){
            List<Quest> DailyQuestList = new List<Quest>();
            foreach (Quest quest in questMap.Values)
            {
                if (quest.info.questType == QuestData.QuestType.DailyQuest)
                {
                    DailyQuestList.Add(quest);
                }
            }
            return DailyQuestList;
        }
        public List<Quest> GetWeeklyQuests(){
            List<Quest> WeeklyQuestList = new List<Quest>();
            foreach (Quest quest in questMap.Values)
            {
                if (quest.info.questType == QuestData.QuestType.WeeklyQuest)
                {
                    WeeklyQuestList.Add(quest);
                }
            }
            return WeeklyQuestList;
        }
        
        private void Awake()
        {
            questMap = CreateQuestMap();
            QuestEvents = new QuestEvents();
        }

        private void OnEnable()
        {
            QuestEvents.onStartQuest += StartQuest;
            QuestEvents.onAdvanceQuest += AdvanceQuest;
            QuestEvents.onFinishQuest += FinishQuest;
            QuestEvents.onQuestStepStateChange += QuestStepStateChange;
        }

        private void Start()
        {
            foreach (Quest quest in questMap.Values)
            {
                if (quest.state == QuestState.IN_PROGRESS)
                {
                    quest.InstantiateCurrentQuestStep(this.transform);
                }
                QuestEvents.QuestStateChange(quest);
            }

            Global.DailyCheckManager.SubscribeToDayChanged(OnDayChanged);
        }

        private void OnDisable()
        {
            QuestEvents.onStartQuest -= StartQuest;
            QuestEvents.onAdvanceQuest -= AdvanceQuest;
            QuestEvents.onFinishQuest -= FinishQuest;
            QuestEvents.onQuestStepStateChange -= QuestStepStateChange;
            Global.DailyCheckManager.OnDayChanged -= OnDayChanged;
        }

        private void OnDayChanged(object sender, DateTime e)
        {
            CheckAndResetQuests(e);
        }

        private void CheckAndResetQuests(DateTime currentTime)
        {
            try
            {
                foreach (Quest quest in questMap.Values)
                {
                    if (quest.info.questType == QuestData.QuestType.DailyQuest)
                    {
                        ResetDailyQuest(quest);
                    }
                    if (quest.info.questType == QuestData.QuestType.WeeklyQuest)
                    {
                        ResetWeeklyQuest(quest);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"퀘스트 리셋 중 오류 발생: {ex}");
            }
        }

        private void ResetDailyQuest(Quest quest)
        {
            // 이전 상태 저장 (통계 등을 위해 필요한 경우)
            SaveQuestHistory(quest);
            
            // 퀘스트 데이터 초기화
            quest.ResetQuestProgress();
            
            // 퀘스트 재시작
            StartQuest(quest.info.id);
        }

        private void ResetWeeklyQuest(Quest quest)
        {
            // 현재 시간이 마지막 완료 시간과 다른 주인지 확인
            DateTime currentTime = DateTime.Now;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday; // 한 주의 시작을 월요일로 설정

            // 마지막 완료 시간과 현재 시간이 다른 주인 경우에만 초기화
            if (IsNewWeek(quest.lastCompletionTime, currentTime, firstDayOfWeek) || quest.lastCompletionTime == DateTime.MinValue)
            {
                // 이전 상태 저장 (통계 등을 위해 필요한 경우)
                SaveQuestHistory(quest);
                
                // 퀘스트 데이터 초기화
                quest.ResetQuestProgress();
                
                // 퀘스트 재시작
                StartQuest(quest.info.id);
            }
            // 마지막 완료 시간이 없는 경우 (처음인 경우)
            if (quest.lastCompletionTime == DateTime.MinValue)
            {
                quest.lastCompletionTime = currentTime;
            }
        }

        private bool IsNewWeek(DateTime lastCompletion, DateTime currentTime, DayOfWeek firstDayOfWeek)
        {
            // 현재 날짜의 주 시작일 계산
            DateTime currentWeekStart = currentTime.Date;
            while (currentWeekStart.DayOfWeek != firstDayOfWeek)
                currentWeekStart = currentWeekStart.AddDays(-1);

            // 마지막 완료 시간의 주 시작일 계산
            DateTime lastWeekStart = lastCompletion.Date;
            while (lastWeekStart.DayOfWeek != firstDayOfWeek)
                lastWeekStart = lastWeekStart.AddDays(-1);

            // Debug.Log($"주간 체크 - 현재 주 시작: {currentWeekStart}, 마지막 완료 주 시작: {lastWeekStart}");
            
            // 다른 주인지 확인
            return currentWeekStart > lastWeekStart;
        }

        private void ChangeQuestState(string id, QuestState state)
        {
            Quest quest = GetQuestById(id);
            quest.state = state;
            QuestEvents.QuestStateChange(quest);
        }

        private bool CheckRequirementsMet(Quest quest)
        {
            // start true and prove to be false
            bool meetsRequirements = true;

            // check player level requirements
            if (Global.UserDataManager.userStorage.curScene == quest.info.sceneRequiremnet)
            {
                meetsRequirements = false;
            }

            // check quest prerequisites for completion
            foreach (QuestInfoSO prerequisiteQuestInfo in quest.info.questPrerequisites)
            {
                if (GetQuestById(prerequisiteQuestInfo.id).state != QuestState.FINISHED)
                {
                    meetsRequirements = false;
                    // add this break statement here so that we don't continue on to the next quest, since we've proven meetsRequirements to be false at this point.
                    break;
                }
            }

            return meetsRequirements;
        }

        private void Update()
        {
            // loop through ALL quests
            foreach (Quest quest in questMap.Values)
            {
                // if we're now meeting the requirements, switch over to the CAN_START state
                if (quest.state == QuestState.REQUIREMENTS_NOT_MET && CheckRequirementsMet(quest))
                {
                    ChangeQuestState(quest.info.id, QuestState.CAN_START);
                }
            }
        }

        private void StartQuest(string id)
        {
            Quest quest = GetQuestById(id);
            quest.InstantiateCurrentQuestStep(this.transform);
            ChangeQuestState(quest.info.id, QuestState.IN_PROGRESS);
        }

        private void AdvanceQuest(string id)
        {
            Quest quest = GetQuestById(id);

            // move on to the next step
            quest.MoveToNextStep();

            // if there are more steps, instantiate the next one
            if (quest.CurrentStepExists())
            {
                quest.InstantiateCurrentQuestStep(this.transform);
            }
            // if there are no more steps, then we've finished all of them for this quest
            else
            {
                if (quest.info.isImmediateReward)
                {
                    FinishQuest(quest.info.id);
                }
                else
                {
                    ChangeQuestState(quest.info.id, QuestState.CAN_FINISH);
                }
            }
        }

        private void FinishQuest(string id)
        {
            Quest quest = GetQuestById(id);
            ClaimRewards(quest);
            ChangeQuestState(quest.info.id, QuestState.FINISHED);
        }

        private void ClaimRewards(Quest quest)
        {
            Global.RewardManager.ClaimRewards(quest.info.rewards);
        }

        private void QuestStepStateChange(string id, int stepIndex, QuestStepState questStepState)
        {
            Quest quest = GetQuestById(id);
            quest.StoreQuestStepState(questStepState, stepIndex);
            ChangeQuestState(id, quest.state);
        }

        private Dictionary<string, Quest> CreateQuestMap()
        {
            // loads all QuestInfoSO Scriptable Objects under the Assets/Resources/Quests folder
            QuestInfoSO[] allQuests = Resources.LoadAll<QuestInfoSO>("Quests");
            
            // SortPriority 기준으로 오름차순 정렬
            System.Array.Sort(allQuests, (a, b) => a.SortPriority.CompareTo(b.SortPriority));
            
            // Create the quest map
            Dictionary<string, Quest> idToQuestMap = new Dictionary<string, Quest>();
            foreach (QuestInfoSO questInfo in allQuests)
            {
                if (idToQuestMap.ContainsKey(questInfo.id))
                {
                    Debug.LogWarning("Duplicate ID found when creating quest map: " + questInfo.id);
                }
                idToQuestMap.Add(questInfo.id, LoadQuest(questInfo));
            }
            return idToQuestMap;
        }

        private Quest GetQuestById(string id)
        {
            Quest quest = questMap[id];
            if (quest == null)
            {
                Debug.LogError("ID not found in the Quest Map: " + id);
            }
            return quest;
        }

        private void OnApplicationQuit()
        {
            foreach (Quest quest in questMap.Values)
            {
                SaveQuest(quest);
            }
        }

        private void SaveQuest(Quest quest)
        {
            try
            {
                QuestData questData = quest.GetQuestData();
                // serialize using JsonUtility, but use whatever you want here (like JSON.NET)
                string serializedData = JsonUtility.ToJson(questData);
                // saving to PlayerPrefs is just a quick example for this tutorial video,
                // you probably don't want to save this info there long-term.
                // instead, use an actual Save & Load system and write to a file, the cloud, etc..
                PlayerPrefs.SetString(quest.info.id, serializedData);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to save quest with id " + quest.info.id + ": " + e);
            }
        }

        private Quest LoadQuest(QuestInfoSO questInfo)
        {
            Quest quest = null;
            try
            {
                // load quest from saved data
                if (PlayerPrefs.HasKey(questInfo.id) && loadQuestState)
                {
                    string serializedData = PlayerPrefs.GetString(questInfo.id);
                    QuestData questData = JsonUtility.FromJson<QuestData>(serializedData);
                    quest = new Quest(questInfo, questData.state, questData.questStepIndex, questData.questStepStates, questData.LastCompletionTime, questData.completionCount);
                }
                // otherwise, initialize a new quest
                else
                {
                    quest = new Quest(questInfo);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load quest with id " + quest.info.id + ": " + e);
            }
            return quest;
        }

        private void SaveQuestHistory(Quest quest)
        {
            // 필요한 경우 이전 퀘스트 완료 기록 저장
            // 예: 통계, 보상 지급 이력 등
        }
    }
}