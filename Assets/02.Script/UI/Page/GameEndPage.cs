using UnityEngine;
using UnityWeld.Binding;
using UI;
using System;
using Manager;

[Binding]
public class GameEndPage : PageViewModel
{
    private string _gameTime;
    private string _foundObjectCount;
    private string _foundRabbitCount;
    private string _stageCompleteText;
    private int _starCount;
    
    [Binding]
    public string GameTime
    {
        get => _gameTime;
        set
        {
            _gameTime = value;
            OnPropertyChanged(nameof(GameTime));
        }
    }
    
    [Binding]
    public string FoundObjectCount
    {
        get => _foundObjectCount;
        set
        {
            _foundObjectCount = value;
            OnPropertyChanged(nameof(FoundObjectCount));
        }
    }
    
    [Binding]
    public string FoundRabbitCount
    {
        get => _foundRabbitCount;
        set
        {
            _foundRabbitCount = value;
            OnPropertyChanged(nameof(FoundRabbitCount));
        }
    }
    
    [Binding]
    public string StageCompleteText
    {
        get => _stageCompleteText;
        set
        {
            _stageCompleteText = value;
            OnPropertyChanged(nameof(StageCompleteText));
        }
    }
    
    [Binding]
    public int StarCount
    {
        get => _starCount;
        set
        {
            _starCount = value;
            OnPropertyChanged(nameof(StarCount));
        }
    }
    
    // 별 개별 표시를 위한 프로퍼티들
    [Binding]
    public bool IsStar1Active => StarCount >= 1;
    
    [Binding]
    public bool IsStar2Active => StarCount >= 2;
    
    [Binding]
    public bool IsStar3Active => StarCount >= 3;
    
    /// <summary>
    /// 게임 결과 데이터를 설정합니다
    /// </summary>
    /// <param name="gameTime">게임 플레이 시간</param>
    /// <param name="foundObjects">찾은 오브젝트 수</param>
    /// <param name="totalObjects">전체 오브젝트 수</param>
    /// <param name="foundRabbits">찾은 토끼 수</param>
    /// <param name="totalRabbits">전체 토끼 수</param>
    /// <param name="stageName">스테이지 이름</param>
    /// <param name="starCount">획득한 별 개수</param>
    public void SetGameResult(TimeSpan gameTime, int foundObjects, int totalObjects, 
                             int foundRabbits, int totalRabbits, string stageName, int starCount)
    {
        GameTime = gameTime.Hours > 0
            ? gameTime.ToString(@"hh\:mm\:ss")
            : gameTime.ToString(@"mm\:ss");
            
        FoundObjectCount = $"{foundObjects} / {totalObjects}";
        FoundRabbitCount = $"{foundRabbits} / {totalRabbits}";
        StageCompleteText = $"{stageName} CLEAR!";
        StarCount = starCount;
        
        // 별 상태 업데이트를 위해 모든 별 관련 프로퍼티 알림
        OnPropertyChanged(nameof(IsStar1Active));
        OnPropertyChanged(nameof(IsStar2Active));
        OnPropertyChanged(nameof(IsStar3Active));
    }
    
    /// <summary>
    /// 퍼즐 게임 결과를 설정합니다 (간단한 버전)
    /// </summary>
    /// <param name="gameTime">게임 플레이 시간</param>
    /// <param name="stageName">스테이지 이름</param>
    public void SetPuzzleGameResult(TimeSpan gameTime, string stageName)
    {
        GameTime = gameTime.Hours > 0
            ? gameTime.ToString(@"hh\:mm\:ss")
            : gameTime.ToString(@"mm\:ss");
            
        StageCompleteText = $"{stageName} CLEAR!";
        StarCount = 3; // 퍼즐 완성 시 항상 3개 별
        
        // 퍼즐 게임에서는 오브젝트/토끼 카운트 숨김
        FoundObjectCount = "";
        FoundRabbitCount = "";
        
        // 별 상태 업데이트
        OnPropertyChanged(nameof(IsStar1Active));
        OnPropertyChanged(nameof(IsStar2Active));
        OnPropertyChanged(nameof(IsStar3Active));
    }
    
    [Binding]
    public void OnClickNextStage()
    {
        // 다음 스테이지로 이동 또는 메인 메뉴로 이동
        LoadingSceneManager.LoadScene(Data.SceneNum.SELECT);
    }
    
    [Binding]
    public void OnClickRestart()
    {
        // 현재 스테이지 재시작
        if (Global.MainMenuSelectedManager != null && Global.MainMenuSelectedManager.HasSelectedStage())
        {
            // 현재 씬 재로드
            LoadingSceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            // 메인 메뉴로 이동
            LoadingSceneManager.LoadScene(Data.SceneNum.SELECT);
        }
    }
    
    [Binding]
    public void OnClickMainMenu()
    {
        // 메인 메뉴로 이동
        LoadingSceneManager.LoadScene(Data.SceneNum.SELECT);
    }
}
