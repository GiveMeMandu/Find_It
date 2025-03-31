using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestData
{
    public QuestState state;
    public int questStepIndex;
    public QuestStepState[] questStepStates;
    public int completionCount;
    public long lastCompletionTimeTicks;  // DateTime 대신 ticks 값 저장

    // 기본 생성자
    public QuestData()
    {
        state = QuestState.REQUIREMENTS_NOT_MET;
        questStepIndex = 0;
        questStepStates = new QuestStepState[0];
        completionCount = 0;
        lastCompletionTimeTicks = DateTime.MinValue.Ticks;
    }

    // 매개변수를 받는 생성자
    public QuestData(QuestState state, int questStepIndex, QuestStepState[] questStepStates, DateTime lastCompletionTime, int completionCount)
    {
        this.state = state;
        this.questStepIndex = questStepIndex;
        this.questStepStates = questStepStates;
        this.lastCompletionTimeTicks = lastCompletionTime.Ticks;
        this.completionCount = completionCount;
    }

    // DateTime 변환 프로퍼티
    public DateTime LastCompletionTime
    {
        get => new DateTime(lastCompletionTimeTicks);
        set => lastCompletionTimeTicks = value.Ticks;
    }

    public enum QuestType{
        DailyQuest,
        WeeklyQuest,
        GuideQuest
    }
}
