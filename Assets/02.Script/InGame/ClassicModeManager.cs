using UnityEngine;
using DeskCat.FindIt.Scripts.Core.Main.System;

/// <summary>
/// 기본 모드 - 표준 숨은그림찾기 로직으로, 정해진 수의 오브젝트를 시간제한 없이 찾습니다.
/// 모든 오브젝트를 성공적으로 찾으면 클리어되고 기본 재화를 지급합니다.
/// </summary>
public class ClassicModeManager : ModeManager
{
    [Header("Classic Mode Settings")]
    public int baseReward = 100;

    public override void InitializeMode()
    {
        currentMode = GameMode.CLASSIC;
        Debug.Log("[ClassicModeManager] 클래식 모드 초기화 완료");

        // LevelManager 이벤트 구독
        if (levelManager != null)
        {
            levelManager.OnFoundObj += OnHiddenObjectFound;
            
            // 필요하다면 모든 HiddenObjUI에 접근 가능
            var allUIs = levelManager.GetAllHiddenObjUIs();
            Debug.Log($"[ClassicModeManager] 사용 가능한 UI 수: {allUIs.Count}개");
        }
    }

    private void OnHiddenObjectFound(object sender, HiddenObj foundObj)
    {
        OnObjectFound(foundObj);
    }

    public override void OnGameStart()
    {
        base.OnGameStart();
        Debug.Log("[ClassicModeManager] 클래식 모드 시작 - 제한 시간 없이 오브젝트를 찾아보세요!");
    }

    public override void OnObjectFound(HiddenObj foundObj)
    {
        base.OnObjectFound(foundObj);
        // 기본 모드에서는 특별한 처리 없음
    }

    public override void OnGameEnd()
    {
        base.OnGameEnd();
        GiveReward();
    }

    private void GiveReward()
    {
        int totalFound = levelManager.GetTotalHiddenObjCount() - levelManager.GetLeftHiddenObjCount();
        int reward = baseReward * totalFound;

        Debug.Log($"[ClassicModeManager] 보상 지급: {totalFound}개 발견 -> {reward} 코인");
        // 실제 재화 지급 로직 구현 필요
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (levelManager != null)
        {
            levelManager.OnFoundObj -= OnHiddenObjectFound;
        }
    }
}