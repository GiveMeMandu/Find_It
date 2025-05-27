using UnityEngine;
using Data;
using UnityWeld;

public class MainMenuSelectedManager : MonoBehaviour
{
    /// <summary>
    /// 현재 선택된 씬 이름
    /// </summary>
    public SceneName SelectedSceneName { get; private set; }
    
    /// <summary>
    /// 현재 선택된 스테이지 인덱스
    /// </summary>
    public int SelectedStageIndex { get; private set; }
    
    /// <summary>
    /// 현재 선택된 스테이지 정보
    /// </summary>
    public SceneInfo SelectedStageInfo { get; private set; }
    
    /// <summary>
    /// 선택된 스테이지 정보를 저장합니다
    /// </summary>
    /// <param name="sceneName">선택된 씬 이름</param>
    /// <param name="stageIndex">선택된 스테이지 인덱스</param>
    /// <param name="stageInfo">선택된 스테이지 정보</param>
    public void SetSelectedStage(SceneName sceneName, int stageIndex, SceneInfo stageInfo)
    {
        SelectedSceneName = sceneName;
        SelectedStageIndex = stageIndex;
        SelectedStageInfo = stageInfo;
        
        Debug.Log($"[MainMenuSelectedManager] Stage selected - Scene: {sceneName}, StageIndex: {stageIndex}, Stage: {stageInfo?.stageIndex}");
    }
    
    /// <summary>
    /// 현재 선택된 스테이지가 있는지 확인합니다
    /// </summary>
    /// <returns>스테이지가 선택되었으면 true</returns>
    public bool HasSelectedStage()
    {
        return SelectedStageInfo != null;
    }
    
    /// <summary>
    /// 저장된 스테이지 정보를 초기화합니다
    /// </summary>
    public void ClearSelectedStage()
    {
        SelectedSceneName = SceneName.Stage1_1; // 기본값
        SelectedStageIndex = -1; // 기본값
        SelectedStageInfo = null;
        
        Debug.Log("[MainMenuSelectedManager] Selected stage cleared");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
