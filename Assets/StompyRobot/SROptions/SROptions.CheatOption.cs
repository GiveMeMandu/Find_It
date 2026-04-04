using System.ComponentModel;
using UnityEngine;
using SRF;
using SRDebugger;
using UnityEngine.Scripting;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Manager;


#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class SROptions
{
    
    [Category("챕터")]
    [DisplayName("모든 챕터 클리어")]
    public void ClearAllChapters()
    {
        // StageManager에서 모든 스테이지 씬 이름을 가져옴
        var stageManager = Manager.Global.StageManager;
        if (stageManager == null)
        {
            Debug.LogWarning("StageManager를 찾을 수 없습니다.");
            return;
        }
        
        var allSceneNames = stageManager.GetAllStageSceneNames();
        
        if (allSceneNames.Count == 0)
        {
            Debug.LogWarning("클리어할 스테이지를 찾을 수 없습니다.");
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

    [DisplayName("데이터 초기화")]
    public void ResetData()
    {
        var userDataManager = Manager.Global.UserDataManager;
        if (userDataManager != null)
        {
            userDataManager.Reset();
            Debug.Log("모든 유저 데이터를 초기화했습니다.");
        }
        else
        {
            Debug.LogError("UserDataManager를 찾을 수 없습니다.");
        }
    }
    [DisplayName("모든 컬렉션 획득")]
    public void GetAllCollections()
    {
        var collectionManager = Manager.Global.CollectionManager;
        var userDataManager = Manager.Global.UserDataManager;
        if (collectionManager != null && userDataManager != null)
        {
            var allCollections = collectionManager.GetAllCollections();
            foreach (var collection in allCollections)
            {
                userDataManager.AddCollection(collection, 99); // 테스트를 위해 99개씩 지급
            }
            Debug.Log($"총 {allCollections.Count}개의 컬렉션을 99개씩 획득했습니다.");
        }
        else
        {
            Debug.LogError("CollectionManager 또는 UserDataManager를 찾을 수 없습니다.");
        }
    }
    [DisplayName("게임 클리어")]
    public void ClearGame()
    {
        var levelManager = LevelManager.Instance;
        if (levelManager == null)
        {
            Debug.LogError("LevelManager를 찾을 수 없습니다.");
            return;
        }

        levelManager.FindAllHidden();
    }
    [DisplayName("모든 아이템 지급")]
    public void AddItem()
    {
        var itemManager = Manager.Global.ItemManager;
        if (itemManager != null)
        {
            itemManager.AddItem(ItemType.Hint, 10); // 힌트 아이템 10개 지급
            itemManager.AddItem(ItemType.Compass, 10); // 나침반 아이템 10개 지급
            itemManager.AddItem(ItemType.Stopwatch, 10); // 초시계 아이템 10개 지급
            Debug.Log("모든 아이템 10개를 지급했습니다.");
        }
        else
        {
            Debug.LogError("ItemManager를 찾을 수 없습니다.");
        }
    }
    [DisplayName("힌트 아이템 지급")]
    public void AddHintItem()
    {
        var itemManager = Manager.Global.ItemManager;
        if (itemManager != null)
        {
            itemManager.AddItem(ItemType.Hint, 10); // 힌트 아이템 10개 지급
            Debug.Log("힌트 아이템 10개를 지급했습니다.");
        }
        else
        {
            Debug.LogError("ItemManager를 찾을 수 없습니다.");
        }
    }
    [DisplayName("나침반 아이템 지급")]
    public void AddCompassItem()
    {
        var itemManager = Manager.Global.ItemManager;
        if (itemManager != null)
        {
            itemManager.AddItem(ItemType.Compass, 10); // 나침반 아이템 10개 지급
            Debug.Log("나침반 아이템 10개를 지급했습니다.");
        }
        else
        {
            Debug.LogError("ItemManager를 찾을 수 없습니다.");
        }
    }
    [DisplayName("초시계 아이템 지급")]
    public void AddStopwatchItem()
    {
        var itemManager = Manager.Global.ItemManager;
        if (itemManager != null)
        {
            itemManager.AddItem(ItemType.Stopwatch, 10); // 초시계 아이템 10개 지급
            Debug.Log("초시계 아이템 10개를 지급했습니다.");
        }
        else
        {
            Debug.LogError("ItemManager를 찾을 수 없습니다.");
        }
    }
}