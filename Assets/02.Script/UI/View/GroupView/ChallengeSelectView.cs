using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Data;
using Manager;
using UnityWeld;
using UnityWeld.Binding;
using UI;

public enum ChallengeType
{
    None = -1,
    HiddenPicture = 0,  // 숨은 그림찾기
    TimeChallenge = 1,  // 타임 챌린지
    Puzzle = 2          // 퍼즐 맞추기
}

[Binding]
public class ChallengeSelectView : ViewModel
{
    [Header("참조")]
    [SerializeField] private MapSelectView mapSelectView;

    [Header("챌린지 버튼들")]
    [SerializeField] private List<Button> challengeButtons = new List<Button>();

    // 현재 선택된 챌린지 타입
    [SerializeField] private ChallengeType _selectedChallengeTypeField = ChallengeType.None;

    /// <summary>
    /// 현재 선택된 챌린지 타입 (Inspector에서 확인 가능)
    /// </summary>
    public ChallengeType _selectedChallengeType
    {
        get => _selectedChallengeTypeField;
        set
        {
            if (_selectedChallengeTypeField != value)
            {
                _selectedChallengeTypeField = value;
            }
        }
    }

    /// <summary>
    /// 현재 선택된 챌린지 타입
    /// </summary>
    public ChallengeType SelectedChallengeType => _selectedChallengeType;

    // 각 챌린지 선택 상태 바인딩 프로퍼티들
    private bool _isHiddenPictureSelected;
    private bool _isTimeChallengeSelected;
    private bool _isPuzzleSelected;

    [Binding]
    public bool IsHiddenPictureSelected
    {
        get => _isHiddenPictureSelected;
        private set
        {
            _isHiddenPictureSelected = value;
            OnPropertyChanged(nameof(IsHiddenPictureSelected));
        }
    }

    [Binding]
    public bool IsTimeChallengeSelected
    {
        get => _isTimeChallengeSelected;
        private set
        {
            _isTimeChallengeSelected = value;
            OnPropertyChanged(nameof(IsTimeChallengeSelected));
        }
    }

    [Binding]
    public bool IsPuzzleSelected
    {
        get => _isPuzzleSelected;
        private set
        {
            _isPuzzleSelected = value;
            OnPropertyChanged(nameof(IsPuzzleSelected));
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupChallengeButtons();
        // 기본값을 HiddenPicture로 설정
        SelectChallenge(ChallengeType.HiddenPicture);
    }
    /// <summary>
    /// 챌린지 버튼들의 클릭 이벤트를 설정합니다
    /// </summary>
    private void SetupChallengeButtons()
    {
        if (challengeButtons == null || challengeButtons.Count < 3) return;

        // 0번 버튼: 숨은 그림찾기 (Stage1~2)
        if (challengeButtons[0] != null)
        {
            challengeButtons[0].onClick.RemoveAllListeners();
            challengeButtons[0].onClick.AddListener(() => SelectChallenge(ChallengeType.HiddenPicture));
        }

        // 1번 버튼: 타임 챌린지 (CLOUD_RABBIT_STAGE)
        if (challengeButtons[1] != null)
        {
            challengeButtons[1].onClick.RemoveAllListeners();
            challengeButtons[1].onClick.AddListener(() => SelectChallenge(ChallengeType.TimeChallenge));
        }

        // 2번 버튼: 퍼즐 맞추기 (PUZZLE)
        if (challengeButtons[2] != null)
        {
            challengeButtons[2].onClick.RemoveAllListeners();
            challengeButtons[2].onClick.AddListener(() => SelectChallenge(ChallengeType.Puzzle));
        }

        UpdateChallengeSelectionProperties();
    }

    public void SelectChallenge(ChallengeType challengeType)
    {
        _selectedChallengeType = challengeType;

        // 바인딩 프로퍼티 업데이트
        UpdateChallengeSelectionProperties();
    }
    private void UpdateChallengeSelectionProperties()
    {
        IsHiddenPictureSelected = (_selectedChallengeType == ChallengeType.HiddenPicture);
        IsTimeChallengeSelected = (_selectedChallengeType == ChallengeType.TimeChallenge);
        IsPuzzleSelected = (_selectedChallengeType == ChallengeType.Puzzle);
    }

    public void ExecuteSelectedChallenge()
    {
        if (_selectedChallengeType == ChallengeType.None)
        {
            Debug.LogWarning("선택된 챌린지가 없습니다.");
            return;
        }

        switch (_selectedChallengeType)
        {
            case ChallengeType.HiddenPicture:
                ExecuteHiddenPictureChallenge();
                break;

            case ChallengeType.TimeChallenge:
                ExecuteTimeChallenge();
                break;

            case ChallengeType.Puzzle:
                ExecutePuzzleChallenge();
                break;
        }
    }

    /// <summary>
    /// 숨은 그림찾기 챌린지를 실행합니다 (Stage1~2)
    /// </summary>
    private void ExecuteHiddenPictureChallenge()
    {
        if (mapSelectView == null) return;

        SceneName selectedSceneName = mapSelectView.CurrentSceneName;
        int buttonIndex = GetCurrentButtonIndex();

        Debug.Log($"[ChallengeSelectView] HiddenPicture - Scene: {selectedSceneName}, ButtonIndex: {buttonIndex}");

        // Stage1_1~Stage1_3 또는 Stage2_1~Stage2_3만 허용
        // 일반 게임과 동일한 계산 (숨은 그림찾기는 원본 스테이지와 동일)
        int targetSceneIndex = CalculateNormalSceneIndex(selectedSceneName, buttonIndex);
        Debug.Log($"[ChallengeSelectView] Loading HiddenPicture scene: {targetSceneIndex}");

        var main = Global.CurrentScene as OutGame.MainMenu;
        main?.OnClickStartButton(targetSceneIndex);
    }

    /// <summary>
    /// 일반 게임과 동일한 씬 인덱스 계산 (숨은 그림찾기용)
    /// </summary>
    private int CalculateNormalSceneIndex(SceneName sceneName, int buttonIndex)
    {
        switch (sceneName)
        {
            case SceneName.Stage1_1:
            case SceneName.Stage1_2:
            case SceneName.Stage1_3:
                // Stage1 시리즈: 4, 5, 6
                return 4 + buttonIndex;
            case SceneName.Stage2_1:
            case SceneName.Stage2_2:
            case SceneName.Stage2_3:
                // Stage2 시리즈: 7, 8, 9
                return 7 + buttonIndex;
            case SceneName.Stage3_1:
            case SceneName.Stage3_2:
            case SceneName.Stage3_3:
                // Stage3 시리즈: 10, 11, 12
                return 10 + buttonIndex;
            default:
                Debug.LogWarning($"Unknown scene name: {sceneName}");
                return 4; // 기본값으로 Stage1_1
        }
    }
    private void ExecuteTimeChallenge()
    {
        if (mapSelectView == null) return;

        SceneName selectedSceneName = mapSelectView.CurrentSceneName;
        int buttonIndex = GetCurrentButtonIndex();

        Debug.Log($"[ChallengeSelectView] TimeChallenge - Scene: {selectedSceneName}, ButtonIndex: {buttonIndex}");

        // 현재 선택된 씬과 버튼 인덱스로 타임 챌린지 씬 계산
        int timeChallengeStageIndex = GetTimeChallengeStageIndex(selectedSceneName, buttonIndex);

        if (timeChallengeStageIndex != -1)
        {
            Debug.Log($"[ChallengeSelectView] Loading TimeChallenge scene: {timeChallengeStageIndex}");
            LoadingSceneManager.LoadScene(timeChallengeStageIndex);
        }
        else
        {
            Debug.LogWarning("타임 챌린지에 해당하는 스테이지를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// MapSelectView에서 현재 선택된 버튼 인덱스를 가져옵니다
    /// </summary>
    /// <returns>현재 씬 내에서의 버튼 인덱스 (0, 1, 2...)</returns>
    private int GetCurrentButtonIndex()
    {
        if (mapSelectView == null) return 0;

        var currentSceneStageIndices = mapSelectView.GetCurrentSceneStageIndices();
        int currentStageIndex = mapSelectView.CurrentStageIndex;

        for (int i = 0; i < currentSceneStageIndices.Count; i++)
        {
            if (currentSceneStageIndices[i] == currentStageIndex)
            {
                return i;
            }
        }

        return 0; // 기본값
    }

    /// <summary>
    /// 퍼즐 맞추기 챌린지를 실행합니다 (PUZZLE)
    /// </summary>
    private void ExecutePuzzleChallenge()
    {
        if (mapSelectView == null) return;

        // MainMenuSelectedManager에 현재 선택된 스테이지 정보 저장
        if (Global.MainMenuSelectedManager != null)
        {
            SceneName selectedSceneName = mapSelectView.CurrentSceneName;
            int selectedStageIndex = mapSelectView.CurrentStageIndex;
            SceneInfo selectedStageInfo = mapSelectView.CurrentStageInfo;

            Global.MainMenuSelectedManager.SetSelectedStage(selectedSceneName, selectedStageIndex, selectedStageInfo);
            Debug.Log($"[ChallengeSelectView] Puzzle challenge - Stage info saved - Scene: {selectedSceneName}, StageIndex: {selectedStageIndex}");
        }

        // 직접 LoadingSceneManager 호출 (+4 로직 우회)
        LoadingSceneManager.LoadScene(SceneNum.PUZZLE);
    }

    private int GetTimeChallengeStageIndex(SceneName sceneName, int buttonIndex)
    {
        // 현재 선택된 씬과 버튼 인덱스를 기반으로 타임 챌린지 씬 계산
        switch (sceneName)
        {
            case SceneName.Stage1_1:
            case SceneName.Stage1_2:
            case SceneName.Stage1_3:
                // Stage1 + 버튼 인덱스 → TimeChallenge_STAGE1 시리즈 (13, 14, 15)
                return SceneNum.TimeChallenge_STAGE1_1 + buttonIndex;

            case SceneName.Stage2_1:
            case SceneName.Stage2_2:
            case SceneName.Stage2_3:
                // Stage2 + 버튼 인덱스 → TimeChallenge_STAGE2 시리즈 (16, 17, 18)
                return SceneNum.TimeChallenge_STAGE2_1 + buttonIndex;

            // TimeChallenge 시리즈에서 직접 선택된 경우 (향후 확장용)
            case SceneName.TimeChallenge_STAGE1_1:
            case SceneName.TimeChallenge_STAGE1_2:
            case SceneName.TimeChallenge_STAGE1_3:
                return SceneNum.TimeChallenge_STAGE1_1 + buttonIndex;

            case SceneName.TimeChallenge_STAGE2_1:
            case SceneName.TimeChallenge_STAGE2_2:
            case SceneName.TimeChallenge_STAGE2_3:
                return SceneNum.TimeChallenge_STAGE2_1 + buttonIndex;

            case SceneName.TimeChallenge_STAGE3_1:
            case SceneName.TimeChallenge_STAGE3_2:
            case SceneName.TimeChallenge_STAGE3_3:
                return SceneNum.TimeChallenge_STAGE3_1 + buttonIndex;

            default:
                Debug.LogWarning($"타임 챌린지에서 지원하지 않는 씬: {sceneName}");
                return -1;
        }
    }

    /// <summary>
    /// MapSelectView 참조를 설정합니다
    /// </summary>
    /// <param name="mapSelectView">참조할 MapSelectView</param>
    public void SetMapSelectView(MapSelectView mapSelectView)
    {
        this.mapSelectView = mapSelectView;
    }

    /// <summary>
    /// 통합 게임 시작 메서드 - 챌린지 선택 여부에 따라 적절한 메서드 실행
    /// </summary>
    [Binding]
    public void StartGame()
    {
        if (_selectedChallengeType == ChallengeType.None)
        {
            // 챌린지가 선택되지 않은 경우 - MapSelectView의 기본 시작 방식 사용
            if (mapSelectView != null)
            {
                mapSelectView.OnClickStartButton();
            }
            else
            {
                Debug.LogWarning("MapSelectView 참조가 없습니다.");
            }
        }
        else
        {
            // 챌린지가 선택된 경우 - 선택된 챌린지 실행
            ExecuteSelectedChallenge();
        }

    }
}
