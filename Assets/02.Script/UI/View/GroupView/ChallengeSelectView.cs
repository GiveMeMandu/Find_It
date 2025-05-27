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
    /// 숨은 그림찾기 챌린지를 실행합니다 (Stage1~2, MapSelectView 인덱스 기반)
    /// </summary>
    private void ExecuteHiddenPictureChallenge()
    {
        if (mapSelectView == null) return;
        
        int selectedStageIndex = mapSelectView.CurrentStageIndex;
        SceneName selectedSceneName = mapSelectView.CurrentSceneName;
        
        // Stage1_1~Stage1_3 또는 Stage2_1~Stage2_3만 허용
        if (IsStage1Scene(selectedSceneName) || IsStage2Scene(selectedSceneName))
        {
            // MapSelectView의 기본 로직 사용 (stageIndex + 4)
            var main = Global.CurrentScene as OutGame.MainMenu;
            main?.OnClickStartButton(selectedStageIndex);
        }
        else
        {
            Debug.LogWarning("숨은 그림찾기는 Stage1 또는 Stage2 시리즈에서만 가능합니다.");
        }
    }
    
    private bool IsStage1Scene(SceneName sceneName)
    {
        return sceneName == SceneName.Stage1_1 || 
               sceneName == SceneName.Stage1_2 || 
               sceneName == SceneName.Stage1_3;
    }
    
    private bool IsStage2Scene(SceneName sceneName)
    {
        return sceneName == SceneName.Stage2_1 || 
               sceneName == SceneName.Stage2_2 || 
               sceneName == SceneName.Stage2_3;
    }
    
    private bool IsTimeChallengeStageScene(SceneName sceneName)
    {
        return sceneName == SceneName.TimeChallenge_STAGE1_1 || 
               sceneName == SceneName.TimeChallenge_STAGE1_2 || 
               sceneName == SceneName.TimeChallenge_STAGE1_3 ||
               sceneName == SceneName.TimeChallenge_STAGE2_1 || 
               sceneName == SceneName.TimeChallenge_STAGE2_2 || 
               sceneName == SceneName.TimeChallenge_STAGE2_3 ||
               sceneName == SceneName.TimeChallenge_STAGE3_1 || 
               sceneName == SceneName.TimeChallenge_STAGE3_2 || 
               sceneName == SceneName.TimeChallenge_STAGE3_3;
    }
    
    private void ExecuteTimeChallenge()
    {
        if (mapSelectView == null) return;
        
        int selectedStageIndex = mapSelectView.CurrentStageIndex;
        SceneName selectedSceneName = mapSelectView.CurrentSceneName;
        
        // TimeChallenge 시리즈에 따라 해당하는 TimeChallenge_STAGE 인덱스 계산
        int timeChallengeStageIndex = GetTimeChallengeStageIndex(selectedSceneName, selectedStageIndex);
        
        if (timeChallengeStageIndex != -1)
        {
            // 직접 LoadingSceneManager 호출 (+4 로직 우회)
            LoadingSceneManager.LoadScene(timeChallengeStageIndex);
        }
        else
        {
            Debug.LogWarning("타임 챌린지에 해당하는 스테이지를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 퍼즐 맞추기 챌린지를 실행합니다 (PUZZLE)
    /// </summary>
    private void ExecutePuzzleChallenge()
    {
        // 직접 LoadingSceneManager 호출 (+4 로직 우회)
        LoadingSceneManager.LoadScene(SceneNum.PUZZLE);
    }
    
    private int GetTimeChallengeStageIndex(SceneName sceneName, int stageIndex)
    {
        // 챌린지 타입에 따른 매핑
        switch (_selectedChallengeType)
        {
            case ChallengeType.TimeChallenge:
                // Stage1, Stage2 씬에서 선택된 경우 해당하는 TimeChallenge 씬으로 매핑
                switch (sceneName)
                {
                    // Stage1 시리즈 → TimeChallenge_STAGE1 시리즈
                    case SceneName.Stage1_1:
                        return SceneNum.TimeChallenge_STAGE1_1;
                    case SceneName.Stage1_2:
                        return SceneNum.TimeChallenge_STAGE1_2;
                    case SceneName.Stage1_3:
                        return SceneNum.TimeChallenge_STAGE1_3;
                        
                    // Stage2 시리즈 → TimeChallenge_STAGE2 시리즈
                    case SceneName.Stage2_1:
                        return SceneNum.TimeChallenge_STAGE2_1;
                    case SceneName.Stage2_2:
                        return SceneNum.TimeChallenge_STAGE2_2;
                    case SceneName.Stage2_3:
                        return SceneNum.TimeChallenge_STAGE2_3;
                        
                    // TimeChallenge 시리즈는 그대로 매핑
                    case SceneName.TimeChallenge_STAGE1_1:
                        return SceneNum.TimeChallenge_STAGE1_1;
                    case SceneName.TimeChallenge_STAGE1_2:
                        return SceneNum.TimeChallenge_STAGE1_2;
                    case SceneName.TimeChallenge_STAGE1_3:
                        return SceneNum.TimeChallenge_STAGE1_3;
                    case SceneName.TimeChallenge_STAGE2_1:
                        return SceneNum.TimeChallenge_STAGE2_1;
                    case SceneName.TimeChallenge_STAGE2_2:
                        return SceneNum.TimeChallenge_STAGE2_2;
                    case SceneName.TimeChallenge_STAGE2_3:
                        return SceneNum.TimeChallenge_STAGE2_3;
                    case SceneName.TimeChallenge_STAGE3_1:
                        return SceneNum.TimeChallenge_STAGE3_1;
                    case SceneName.TimeChallenge_STAGE3_2:
                        return SceneNum.TimeChallenge_STAGE3_2;
                    case SceneName.TimeChallenge_STAGE3_3:
                        return SceneNum.TimeChallenge_STAGE3_3;
                }
                break;
                
            // 다른 챌린지 타입들도 필요에 따라 추가 가능
            case ChallengeType.HiddenPicture:
                // 숨은 그림찾기는 원본 스테이지 그대로 사용
                Debug.LogWarning("숨은 그림찾기는 GetTimeChallengeStageIndex를 사용하지 않아야 합니다.");
                break;
                
            case ChallengeType.Puzzle:
                // 퍼즐은 고정된 PUZZLE 씬 사용
                Debug.LogWarning("퍼즐은 GetTimeChallengeStageIndex를 사용하지 않아야 합니다.");
                break;
        }
        
        Debug.LogWarning($"매핑되지 않은 조합: 씬={sceneName}, 챌린지={_selectedChallengeType}");
        return -1;
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
