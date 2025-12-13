using UnityEngine;
using DeskCat.FindIt.Scripts.Core.Main.System;

public abstract class ModeManager : AutoTaskControl
{
    public enum GameMode
    {
        CLASSIC,        // 기본 모드
        FOG,            // 구름(안개) 모드
        TIME_CHALLENGE, // 타임챌린지 모드
        COIN_RUSH,      // 코인러쉬 모드
        SHADOW,         // 새도우 모드
        DARK            // 어두운 모드
    }

    [Header("Mode Settings")]
    public GameMode currentMode = GameMode.CLASSIC;

    protected LevelManager levelManager => LevelManager.Instance;

    protected virtual void Start()
    {
    }

    public abstract void InitializeMode();

    public virtual void OnGameStart()
    {
        Debug.Log($"[ModeManager] 게임 시작 - 모드: {currentMode}");
    }

    public virtual void OnObjectFound(HiddenObj foundObj)
    {
        Debug.Log($"[ModeManager] 오브젝트 발견: {foundObj.name}");
    }

    public virtual void OnGameEnd()
    {
        Debug.Log($"[ModeManager] 게임 종료 - 모드: {currentMode}");
    }

    protected virtual void Update()
    {
        // 각 모드별로 필요한 업데이트 로직 구현
    }
}
