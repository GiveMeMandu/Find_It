using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Scene에 배치된 ModeManager들을 자동으로 캐싱하고
/// 선택된 GameMode에 해당하는 ModeManager를 반환하거나 초기화하는 헬퍼 컴포넌트입니다.
/// ModeManager들은 같은 GameObject에 붙이거나 자식으로 두면 자동으로 수집됩니다.
/// </summary>
public class ModeSelector : MonoBehaviour
{
    [InfoBox("초기 선택모드 여기만 바꾸면 모드 바꿀 수 있음!")]
    [Tooltip("명시적으로 할당된 ModeManager 리스트(없으면 자동수집).")]
    public List<ModeManager> modeManagers = new List<ModeManager>();

    // GameMode -> ModeManager 매핑
    private Dictionary<ModeManager.GameMode, ModeManager> modeMap = new Dictionary<ModeManager.GameMode, ModeManager>();

    [Header("초기 선택 모드")]
    [InfoBox("기본 모드 (항상 적용됨)")]
    public ModeManager.GameMode selectedMode = ModeManager.GameMode.CLASSIC;

    [LabelText("다중 모드 선택을 허용합니다.")]
    [InfoBox("다중 모드 활성화시 selectedMode와 함께 추가로 적용할 모드들")]
    public bool allowMultipleSelection = false;
    [ShowIf("allowMultipleSelection")]
    [LabelText("추가 모드 (selectedMode와 함께 적용)")]
    public List<ModeManager.GameMode> selectedModes = new List<ModeManager.GameMode>();

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
    /// 다중 선택이 가능할 때 선택된 모든 ModeManager들을 반환합니다.
    /// 다중 선택이 꺼져있으면 단일 선택된 ModeManager만 리스트로 반환합니다.
    /// </summary>
    public List<ModeManager> GetSelectedModeManagers()
    {
        var list = new List<ModeManager>();
        
        // 항상 selectedMode를 먼저 추가
        var mainManager = GetSelectedModeManager();
        if (mainManager != null)
            list.Add(mainManager);
        
        // 다중 선택 모드라면 추가 모드들도 함께 적용
        if (allowMultipleSelection && selectedModes != null)
        {
            foreach (var mode in selectedModes)
            {
                // selectedMode와 중복되지 않도록 체크
                if (mode == selectedMode) continue;
                
                var m = GetModeManager(mode);
                if (m != null && !list.Contains(m))
                    list.Add(m);
            }
        }
        
        return list;
    }

    /// <summary>
    /// 현재 선택된 모드를 즉시 InitializeMode() 호출로 초기화합니다.
    /// </summary>
    public void InitializeSelectedMode()
    {
        var managers = GetSelectedModeManagers();
        if (managers.Count == 0)
        {
            if (allowMultipleSelection)
            {
                Debug.LogWarning($"[ModeSelector] 선택된 모드들(기본: {selectedMode}, 추가: {string.Join(", ", selectedModes.ConvertAll(s => s.ToString()).ToArray())})에 해당하는 ModeManager를 찾을 수 없습니다.");
            }
            else
            {
                Debug.LogWarning($"[ModeSelector] 선택된 모드({selectedMode})에 해당하는 ModeManager를 찾을 수 없습니다.");
            }
            return;
        }

        // If CoinRush is among the selected modes, ensure TimeChallenge hides the ScrollView
        bool coinRushSelected = managers.Exists(m => m != null && m.currentMode == ModeManager.GameMode.COIN_RUSH);
        if (coinRushSelected)
        {
            var timeManager = GetModeManager(ModeManager.GameMode.TIME_CHALLENGE) as TimeChallengeModeManager;
            if (timeManager != null)
            {
                timeManager.hideScrollView = true;
                Debug.Log($"[ModeSelector] CoinRush selected — set {nameof(TimeChallengeModeManager)}.hideScrollView = true");
            }
        }

        foreach (var mm in managers)
        {
            mm.InitializeMode();
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

    /// <summary>
    /// 다중 선택 모드에서 추가로 선택할 모드들을 설정합니다.
    /// 이 모드들은 selectedMode와 함께 적용됩니다.
    /// </summary>
    public void SetSelectedModes(List<ModeManager.GameMode> modes, bool initializeNow = false)
    {
        selectedModes = modes != null ? new List<ModeManager.GameMode>(modes) : new List<ModeManager.GameMode>();
        if (initializeNow)
            InitializeSelectedMode();
    }
}
