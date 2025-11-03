using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Scene에 배치된 ModeManager들을 자동으로 캐싱하고
/// 선택된 GameMode에 해당하는 ModeManager를 반환하거나 초기화하는 헬퍼 컴포넌트입니다.
/// ModeManager들은 같은 GameObject에 붙이거나 자식으로 두면 자동으로 수집됩니다.
/// </summary>
public class ModeSelector : MonoBehaviour
{
    [InfoBox("초기 선택모드 여기만 바꾸면 모드 바꿀 수 있음!", EInfoBoxType.Normal)]
    [Tooltip("명시적으로 할당된 ModeManager 리스트(없으면 자동수집).")]
    public List<ModeManager> modeManagers = new List<ModeManager>();

    // GameMode -> ModeManager 매핑
    private Dictionary<ModeManager.GameMode, ModeManager> modeMap = new Dictionary<ModeManager.GameMode, ModeManager>();

    [Header("초기 선택 모드")]
    public ModeManager.GameMode selectedMode = ModeManager.GameMode.CLASSIC;

    private void Awake()
    {
        CacheModeManagers();
    }

    /// <summary>
    /// 현재 GameObject와 자식에서 ModeManager 컴포넌트들을 찾아 캐싱합니다.
    /// 명시적으로 할당된 `modeManagers`가 있으면 그것을 우선으로 사용합니다.
    /// </summary>
    public void CacheModeManagers()
    {
        modeMap.Clear();

        // 만약 인스펙터에서 수동으로 넣지 않았다면 자동 수집
        if (modeManagers == null || modeManagers.Count == 0)
        {
            // 같은 GameObject와 자식에서 모두 찾도록 함
            modeManagers = new List<ModeManager>(GetComponents<ModeManager>());
        }

        foreach (var m in modeManagers)
        {
            if (m == null) continue;
            // 같은 모드가 여러 개 있으면 마지막으로 발견된 것으로 덮어씀
            modeMap[m.currentMode] = m;
        }
    }

    public ModeManager GetModeManager(ModeManager.GameMode mode)
    {
        modeMap.TryGetValue(mode, out var manager);
        return manager;
    }

    public ModeManager GetSelectedModeManager()
    {
        return GetModeManager(selectedMode);
    }

    /// <summary>
    /// 현재 선택된 모드를 즉시 InitializeMode() 호출로 초기화합니다.
    /// </summary>
    public void InitializeSelectedMode()
    {
        var m = GetSelectedModeManager();
        if (m != null)
        {
            m.InitializeMode();
        }
        else
        {
            Debug.LogWarning($"[ModeSelector] 선택된 모드({selectedMode})에 해당하는 ModeManager를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 선택 모드를 변경합니다. 필요시 즉시 초기화하려면 true를 전달하세요.
    /// </summary>
    public void SetSelectedMode(ModeManager.GameMode mode, bool initializeNow = false)
    {
        selectedMode = mode;
        if (initializeNow)
            InitializeSelectedMode();
    }
}
