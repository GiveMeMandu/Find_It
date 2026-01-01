using System.ComponentModel;
using UnityEngine;
using SRF;
using SRDebugger;
using UnityEngine.Scripting;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class SROptions
{
    
    [Category("챕터")]
    [DisplayName("모든 챕터 클리어")]
    public void ClearAllChapters()
    {
        // ChapterSelectView에서 모든 스테이지 씬 이름을 가져옴
        var allSceneNames = UnityWeld.ChapterSelectView.GetAllStageSceneNamesStatic();
        
        if (allSceneNames.Count == 0)
        {
            Debug.LogWarning("클리어할 스테이지를 찾을 수 없습니다. ChapterSelectView가 씬에 있는지 확인하세요.");
            return;
        }
        
        // UserDataManager를 통해 모든 스테이지를 클리어 상태로 설정
        var userDataManager = Manager.Global.UserDataManager;
        if (userDataManager != null)
        {
            foreach (var sceneName in allSceneNames)
            {
                userDataManager.SetStageClear(sceneName);
            }
            Debug.Log($"총 {allSceneNames.Count}개의 스테이지를 클리어 상태로 설정했습니다.");
        }
        else
        {
            Debug.LogError("UserDataManager를 찾을 수 없습니다.");
        }
    }
}