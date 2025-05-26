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
    private ChallengeType _selectedChallengeType = ChallengeType.None;
    
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
        set
        {
            _isHiddenPictureSelected = value;
            OnPropertyChanged(nameof(IsHiddenPictureSelected));
        }
    }
    
    [Binding]
    public bool IsTimeChallengeSelected
    {
        get => _isTimeChallengeSelected;
        set
        {
            _isTimeChallengeSelected = value;
            OnPropertyChanged(nameof(IsTimeChallengeSelected));
        }
    }
    
    [Binding]
    public bool IsPuzzleSelected
    {
        get => _isPuzzleSelected;
        set
        {
            _isPuzzleSelected = value;
            OnPropertyChanged(nameof(IsPuzzleSelected));
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupChallengeButtons();
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
        SelectChallenge(ChallengeType.HiddenPicture);
        UpdateChallengeSelectionProperties();
    }
    
    /// <summary>
    /// 챌린지 타입을 선택합니다
    /// </summary>
    /// <param name="challengeType">선택할 챌린지 타입</param>
    [Binding]
    public void SelectChallenge(ChallengeType challengeType)
    {
        _selectedChallengeType = challengeType;
        
        // 선택된 챌린지에 따른 UI 업데이트나 피드백 처리
        Debug.Log($"챌린지 선택됨: {challengeType}");
        
        // 바인딩 프로퍼티 업데이트
        UpdateChallengeSelectionProperties();
    }
    
    /// <summary>
    /// 챌린지 선택 상태 프로퍼티들을 업데이트합니다
    /// </summary>
    private void UpdateChallengeSelectionProperties()
    {
        IsHiddenPictureSelected = (_selectedChallengeType == ChallengeType.HiddenPicture);
        IsTimeChallengeSelected = (_selectedChallengeType == ChallengeType.TimeChallenge);
        IsPuzzleSelected = (_selectedChallengeType == ChallengeType.Puzzle);
    }
    
    /// <summary>
    /// 챌린지 선택을 해제합니다
    /// </summary>
    [Binding]
    public void ClearChallengeSelection()
    {
        _selectedChallengeType = ChallengeType.None;
        UpdateChallengeSelectionProperties();
        Debug.Log("챌린지 선택 해제됨");
    }
    
    /// <summary>
    /// 현재 선택된 챌린지를 실행합니다
    /// </summary>
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
    
    /// <summary>
    /// Stage1 시리즈 씬인지 확인합니다
    /// </summary>
    /// <param name="sceneName">확인할 씬 이름</param>
    /// <returns>Stage1 시리즈이면 true</returns>
    private bool IsStage1Scene(SceneName sceneName)
    {
        return sceneName == SceneName.Stage1_1 || 
               sceneName == SceneName.Stage1_2 || 
               sceneName == SceneName.Stage1_3;
    }
    
    /// <summary>
    /// Stage2 시리즈 씬인지 확인합니다
    /// </summary>
    /// <param name="sceneName">확인할 씬 이름</param>
    /// <returns>Stage2 시리즈이면 true</returns>
    private bool IsStage2Scene(SceneName sceneName)
    {
        return sceneName == SceneName.Stage2_1 || 
               sceneName == SceneName.Stage2_2 || 
               sceneName == SceneName.Stage2_3;
    }
    
    /// <summary>
    /// CloudRabbitStage 시리즈 씬인지 확인합니다
    /// </summary>
    /// <param name="sceneName">확인할 씬 이름</param>
    /// <returns>CloudRabbitStage 시리즈이면 true</returns>
    private bool IsCloudRabbitStageScene(SceneName sceneName)
    {
        return sceneName == SceneName.CloudRabbitStage1_1 || 
               sceneName == SceneName.CloudRabbitStage1_2 || 
               sceneName == SceneName.CloudRabbitStage1_3 ||
               sceneName == SceneName.CloudRabbitStage2_1 || 
               sceneName == SceneName.CloudRabbitStage2_2 || 
               sceneName == SceneName.CloudRabbitStage2_3;
    }
    
    /// <summary>
    /// 타임 챌린지를 실행합니다 (CLOUD_RABBIT_STAGE, MapSelectView 인덱스 기반)
    /// </summary>
    private void ExecuteTimeChallenge()
    {
        if (mapSelectView == null) return;
        
        int selectedStageIndex = mapSelectView.CurrentStageIndex;
        SceneName selectedSceneName = mapSelectView.CurrentSceneName;
        
        // CloudRabbitStage 시리즈에 따라 해당하는 CLOUD_RABBIT_STAGE 인덱스 계산
        int cloudRabbitStageIndex = GetCloudRabbitStageIndex(selectedSceneName, selectedStageIndex);
        
        if (cloudRabbitStageIndex != -1)
        {
            // 직접 LoadingSceneManager 호출 (+4 로직 우회)
            LoadingSceneManager.LoadScene(cloudRabbitStageIndex);
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
        Debug.Log("=== ExecutePuzzleChallenge 실행 ===");
        Debug.Log($"SceneNum.PUZZLE 값: {SceneNum.PUZZLE}");
        
        // 직접 LoadingSceneManager 호출 (+4 로직 우회)
        LoadingSceneManager.LoadScene(SceneNum.PUZZLE);
        Debug.Log($"LoadingSceneManager.LoadScene({SceneNum.PUZZLE}) 호출 완료");
    }
    
    /// <summary>
    /// 선택된 씬과 스테이지에 따라 해당하는 CLOUD_RABBIT_STAGE 인덱스를 반환합니다
    /// </summary>
    /// <param name="sceneName">선택된 씬 이름</param>
    /// <param name="stageIndex">선택된 스테이지 인덱스</param>
    /// <returns>CLOUD_RABBIT_STAGE 인덱스, 해당 없으면 -1</returns>
    private int GetCloudRabbitStageIndex(SceneName sceneName, int stageIndex)
    {
        // 새로운 SceneName enum에 맞게 직접 매핑
        switch (sceneName)
        {
            // CloudRabbitStage1 시리즈
            case SceneName.CloudRabbitStage1_1:
                return SceneNum.CLOUD_RABBIT_STAGE1_1;
            case SceneName.CloudRabbitStage1_2:
                return SceneNum.CLOUD_RABBIT_STAGE1_2;
            case SceneName.CloudRabbitStage1_3:
                return SceneNum.CLOUD_RABBIT_STAGE1_3;
                
            // CloudRabbitStage2 시리즈
            case SceneName.CloudRabbitStage2_1:
                return SceneNum.CLOUD_RABBIT_STAGE2_1;
            case SceneName.CloudRabbitStage2_2:
                return SceneNum.CLOUD_RABBIT_STAGE2_2;
            case SceneName.CloudRabbitStage2_3:
                return SceneNum.CLOUD_RABBIT_STAGE2_3;
        }
        
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
