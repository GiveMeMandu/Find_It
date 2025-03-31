using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class Quest
{
    // static info
    public QuestInfoSO info;

    // state info
    public QuestState state;
    private int currentQuestStepIndex;
    private QuestStepState[] questStepStates;

    // additional fields for repeatable quests
    public DateTime lastCompletionTime;
    public int completionCount;

    public Quest(QuestInfoSO questInfo)
    {
        this.info = questInfo;
        this.state = QuestState.REQUIREMENTS_NOT_MET;
        this.currentQuestStepIndex = 0;
        this.questStepStates = new QuestStepState[info.questStepPrefabs.Length];
        if(this.info.questType == QuestData.QuestType.DailyQuest)
        {
            this.state = QuestState.IN_PROGRESS;
        }
        for (int i = 0; i < questStepStates.Length; i++)
        {
            questStepStates[i] = new QuestStepState();
        }
    }

    public Quest(QuestInfoSO questInfo, QuestState questState, int currentQuestStepIndex, QuestStepState[] questStepStates, DateTime lastCompletionTime, int completionCount)
    {
        this.info = questInfo;
        this.state = questState;
        this.currentQuestStepIndex = currentQuestStepIndex;
        this.questStepStates = questStepStates;
        this.lastCompletionTime = lastCompletionTime;
        this.completionCount = completionCount;

        // if the quest step states and prefabs are different lengths,
        // something has changed during development and the saved data is out of sync.
        if (this.questStepStates.Length != this.info.questStepPrefabs.Length)
        {
            Debug.LogWarning("Quest Step Prefabs and Quest Step States are "
                + "of different lengths. This indicates something changed "
                + "with the QuestInfo and the saved data is now out of sync. "
                + "Reset your data - as this might cause issues. QuestId: " + this.info.id);
        }
    }

    public void MoveToNextStep()
    {
        currentQuestStepIndex++;
    }

    public bool CurrentStepExists()
    {
        return (currentQuestStepIndex < info.questStepPrefabs.Length);
    }

    public void InstantiateCurrentQuestStep(Transform parentTransform)
    {
        GameObject questStepPrefab = GetCurrentQuestStepPrefab();
        if (questStepPrefab != null)
        {
            QuestStep questStep = UnityEngine.Object.Instantiate<GameObject>(questStepPrefab, parentTransform)
                .GetComponent<QuestStep>();
            questStep.InitializeQuestStep(info.id, currentQuestStepIndex, questStepStates[currentQuestStepIndex].state);
        }
    }

    private GameObject GetCurrentQuestStepPrefab()
    {
        GameObject questStepPrefab = null;
        if (CurrentStepExists())
        {
            questStepPrefab = info.questStepPrefabs[currentQuestStepIndex];
        }
        else 
        {
            Debug.LogWarning("Tried to get quest step prefab, but stepIndex was out of range indicating that "
                + "there's no current step: QuestId=" + info.id + ", stepIndex=" + currentQuestStepIndex);
        }
        return questStepPrefab;
    }

    public void StoreQuestStepState(QuestStepState questStepState, int stepIndex)
    {
        if (stepIndex < questStepStates.Length)
        {
            questStepStates[stepIndex].state = questStepState.state;
            questStepStates[stepIndex].status = questStepState.status;
        }
        else 
        {
            Debug.LogWarning("Tried to access quest step data, but stepIndex was out of range: "
                + "Quest Id = " + info.id + ", Step Index = " + stepIndex);
        }
    }

    public QuestData GetQuestData()
    {
        return new QuestData(
            state, 
            currentQuestStepIndex, 
            questStepStates,
            lastCompletionTime,    // 추가
            completionCount        // 추가
        );
    }

    public string GetFullStatusText()
    {
        string fullStatus = "";

        if (state == QuestState.REQUIREMENTS_NOT_MET)
        {
            fullStatus = "Requirements are not yet met to start this quest.";
        }
        else if (state == QuestState.CAN_START)
        {
            fullStatus = "This quest can be started!";
        }
        else 
        {
            if (questStepStates != null && questStepStates.Length > 0)
            {
                // display all previous quests with strikethroughs
                for (int i = 0; i < currentQuestStepIndex && i < questStepStates.Length; i++)
                {
                    if (questStepStates[i] != null)
                    {
                        fullStatus += "<s>" + questStepStates[i].status + "</s>\n";
                    }
                }
                
                // display the current step, if it exists
                if (CurrentStepExists() && currentQuestStepIndex < questStepStates.Length)
                {
                    if (questStepStates[currentQuestStepIndex] != null)
                    {
                        fullStatus += questStepStates[currentQuestStepIndex].status;
                    }
                }
            }

            // when the quest is completed or turned in
            if (state == QuestState.CAN_FINISH)
            {
                // fullStatus += "The quest is ready to be turned in.";
                fullStatus = "";
            }
            else if (state == QuestState.FINISHED)
            {
                //The quest has been completed!
                fullStatus = LocalizationManager.GetTranslation("Quest/Clear");
            }
        }

        return fullStatus;
    }

    public void ResetQuestProgress()
    {
        // reset current progress
        currentQuestStepIndex = 0;
        
        // reset each step state
        for (int i = 0; i < questStepStates.Length; i++)
        {
            questStepStates[i] = new QuestStepState();
        }
        
        // reset quest state
        state = QuestState.CAN_START;
    }
}
