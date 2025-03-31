using System.Collections;
using System.Collections.Generic;
using Data;
using SO;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestInfoSO", menuName = "퀘스트/QuestInfoSO", order = 1)]
public class QuestInfoSO : ScriptableObject
{
    [BoxGroup("설정")] [LabelText("ID, 파일 이름에 따라 강제로 달라짐")]
    [field: SerializeField] public string id { get; private set; }
    [BoxGroup("설정")] [LabelText("Sort Priority, 퀘스트 목록 정렬 우선순위")]
    [field: SerializeField] public int SortPriority { get; private set; }

    [BoxGroup("설정")] [LabelText("퀘스트 이름")] [TermsPopup("Quest/Name")]
    public string displayName;
    [BoxGroup("설정")] [LabelText("퀘스트 설명")] [TermsPopup("Quest/Info")]
    public string displayInfo;
    [BoxGroup("설정")] [LabelText("퀘스트 분류")] 
    public QuestData.QuestType questType;

    [BoxGroup("설정")] [LabelText("퀘스트 해금에 필요한 스테이지 조건")]
    public SceneName sceneRequiremnet;
    [BoxGroup("설정")] [LabelText("퀘스트 해금에 필요한 선행퀘스트들")]
    public QuestInfoSO[] questPrerequisites;

    [BoxGroup("설정")] [LabelText("퀘스트 단계 프리팹")]
    public GameObject[] questStepPrefabs;
    [BoxGroup("설정")] [LabelText("보상 정보")]
    public RewardSO[] rewards;
    [BoxGroup("설정")] [LabelText("퀘스트 완료시 바로 보상 수령")]
    public bool isImmediateReward;

    // ensure the id is always the name of the Scriptable Object asset
    private void OnValidate()
    {
        #if UNITY_EDITOR
        id = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}
