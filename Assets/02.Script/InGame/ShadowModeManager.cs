using UnityEngine;
using UnityEngine.UI;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// 새도우 모드 - 찾아야 할 오브젝트 리스트의 썸네일을 실루엣(검은색 윤곽선)으로만 보여주어 난이도를 높입니다.
/// 플레이어는 실루엣만으로 실제 맵의 오브젝트를 유추하여 터치해야 합니다.
/// </summary>
public class ShadowModeManager : ModeManager
{
    public override void InitializeMode()
    {
        currentMode = GameMode.SHADOW;
        Debug.Log("[ShadowModeManager] 새도우 모드 초기화 완료");

        if (levelManager != null)
        {
            levelManager.OnFoundObj += OnHiddenObjectFound;
        }

        if (TimeChallengeManager.Instance != null)
        {
            TimeChallengeManager.Instance.OnFoundRabbit += OnHiddenObjectFound;
        }

        SetupShadowMode();
    }

    private void SetupShadowMode()
    {
        // UI가 생성된 직후 즉시 실루엣 모드를 적용하여 깜빡임 방지
        // 만약 초기화 시점에 UI가 아직 없다면 약간의 지연 후에 재시도
        EnableSilhouetteModeAsync().Forget();
    }

    private async UniTaskVoid EnableSilhouetteModeAsync()
    {
        // 초기화 시점에는 다른 매니저들이 UI를 생성 중일 수 있으므로 
        // 1프레임 대기 후 적용 (초기 진입 시 깜빡임은 허용)
        await UniTask.Yield(PlayerLoopTiming.Update);
        
        EnableSilhouetteMode();
    }

    private void EnableSilhouetteMode()
    {
        // LevelManager의 UI 목록 다시 가져오기 (TimeChallengeManager가 등록한 것 포함)
        var uiList = levelManager.GetAllHiddenObjUIs();
        
        foreach (var hiddenObjUI in uiList)
        {
            if (hiddenObjUI != null)
            {
                hiddenObjUI.SetSilhouetteMode(true);
            }
        }
        
        Debug.Log($"[ShadowModeManager] {uiList.Count}개의 UI가 실루엣 모드로 전환되었습니다");
    }

    private void OnHiddenObjectFound(object sender, HiddenObj foundObj)
    {
        OnObjectFound(foundObj);
        
        // 찾은 오브젝트에 해당하는 UI의 실루엣 모드 해제는 HiddenObjUI.Found()에서 자동으로 처리됨
        // 하지만 UI 리스트가 재생성될 수 있으므로 실루엣 모드를 다시 적용해야 함
        
        // 중요: UI 업데이트 이벤트는 동기적으로 호출되므로, 즉시 실루엣 모드를 적용하여 깜빡임을 방지
        EnableSilhouetteMode();

        Debug.Log($"[ShadowModeManager] 실루엣으로부터 오브젝트 발견: {foundObj.name}");
    }

    public override void OnGameStart()
    {
        base.OnGameStart();
        Debug.Log("[ShadowModeManager] 새도우 모드 시작 - 실루엣만으로 오브젝트를 식별하세요!");
    }

    public override void OnObjectFound(HiddenObj foundObj)
    {
        base.OnObjectFound(foundObj);
        Debug.Log($"[ShadowModeManager] 실루엣 오브젝트 정확히 식별: {foundObj.name}");
    }

    public override void OnGameEnd()
    {
        base.OnGameEnd();
        Debug.Log("[ShadowModeManager] 모든 실루엣이 식별되었습니다! 마스터 탐정!");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (levelManager != null)
        {
            levelManager.OnFoundObj -= OnHiddenObjectFound;
        }

        if (TimeChallengeManager.Instance != null)
        {
            TimeChallengeManager.Instance.OnFoundRabbit -= OnHiddenObjectFound;
        }
    }
}