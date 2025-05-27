using System;
using Cysharp.Threading.Tasks;
using DeskCat.FindIt.Scripts.Core.Main.System;
using UnityEngine;
using UnityWeld.Binding;

namespace UI
{
    [Binding]
    public class TimeChallengeViewModel : BaseViewModel
    {
        // 바인딩 필드들
        private string _rabbitCountText;
        private float _rabbitCountFillAmount;
        private string _timerText;
        private float _timerFillAmount;
        private string _finalRabbitCountText;
        private string _finalTimeText;
        private string _resultText;
        private bool _isGameEndUIVisible;
        private bool _isTimeUpUIVisible;
        private bool _isNextLevelButtonVisible;
        
        private TimeChallengeManager gameManager;
        
        // 타이머 관련 필드들
        private float currentTime;
        private float timeLimit;
        private bool isTimerActive = true;
        private DateTime startTime;
        
        // 토끼 카운트 텍스트 바인딩
        [Binding]
        public string RabbitCountText
        {
            get => _rabbitCountText;
            set
            {
                _rabbitCountText = value;
                OnPropertyChanged(nameof(RabbitCountText));
            }
        }
        
        // 토끼 카운트 Fill Amount 바인딩
        [Binding]
        public float RabbitCountFillAmount
        {
            get => _rabbitCountFillAmount;
            set
            {
                _rabbitCountFillAmount = value;
                OnPropertyChanged(nameof(RabbitCountFillAmount));
            }
        }
        
        // 타이머 텍스트 바인딩
        [Binding]
        public string TimerText
        {
            get => _timerText;
            set
            {
                _timerText = value;
                OnPropertyChanged(nameof(TimerText));
            }
        }
        
        // 타이머 Fill Amount 바인딩
        [Binding]
        public float TimerFillAmount
        {
            get => _timerFillAmount;
            set
            {
                _timerFillAmount = value;
                OnPropertyChanged(nameof(TimerFillAmount));
            }
        }
        
        // 최종 토끼 카운트 텍스트 바인딩
        [Binding]
        public string FinalRabbitCountText
        {
            get => _finalRabbitCountText;
            set
            {
                _finalRabbitCountText = value;
                OnPropertyChanged(nameof(FinalRabbitCountText));
            }
        }
        
        // 최종 시간 텍스트 바인딩
        [Binding]
        public string FinalTimeText
        {
            get => _finalTimeText;
            set
            {
                _finalTimeText = value;
                OnPropertyChanged(nameof(FinalTimeText));
            }
        }
        
        // 결과 텍스트 바인딩
        [Binding]
        public string ResultText
        {
            get => _resultText;
            set
            {
                _resultText = value;
                OnPropertyChanged(nameof(ResultText));
            }
        }
        
        // 게임 종료 UI 표시 여부 바인딩
        [Binding]
        public bool IsGameEndUIVisible
        {
            get => _isGameEndUIVisible;
            set
            {
                _isGameEndUIVisible = value;
                OnPropertyChanged(nameof(IsGameEndUIVisible));
            }
        }
        
        // 시간 초과 UI 표시 여부 바인딩
        [Binding]
        public bool IsTimeUpUIVisible
        {
            get => _isTimeUpUIVisible;
            set
            {
                _isTimeUpUIVisible = value;
                OnPropertyChanged(nameof(IsTimeUpUIVisible));
            }
        }
        
        // 다음 레벨 버튼 표시 여부 바인딩
        [Binding]
        public bool IsNextLevelButtonVisible
        {
            get => _isNextLevelButtonVisible;
            set
            {
                _isNextLevelButtonVisible = value;
                OnPropertyChanged(nameof(IsNextLevelButtonVisible));
            }
        }
        
        private void Start()
        {
            // TimeChallengeManager 인스턴스 가져오기
            gameManager = TimeChallengeManager.Instance;
            
            if (gameManager != null)
            {
                // 이벤트 구독
                gameManager.OnFoundRabbit += OnRabbitFound;
                gameManager.OnGameEnd += OnGameEnd;
                
                // 타이머 초기화
                InitializeTimer();
                InitializeAsync().Forget();
            }
            
            // 초기 UI 상태 설정
            InitializeUI();
        }

        private async UniTaskVoid InitializeAsync()
        {
            await UniTask.WaitUntil(() => gameManager.GetTotalRabbitCount() > 0);
            // 초기 UI 업데이트
            UpdateRabbitCountUI();
            UpdateTimerUI();
        }
        
        private void Update()
        {
            if (isTimerActive && gameManager != null && gameManager.IsGameActive())
            {
                UpdateTimer();
            }
        }
        
        private void OnDisable()
        {
            // 이벤트 구독 해제
            if (gameManager != null)
            {
                gameManager.OnFoundRabbit -= OnRabbitFound;
                gameManager.OnGameEnd -= OnGameEnd;
            }
        }
        
        private void InitializeTimer()
        {
            if (gameManager != null)
            {
                timeLimit = gameManager.TimeLimit;
                currentTime = timeLimit;
                startTime = DateTime.Now;
                isTimerActive = true;
            }
        }
        
        private void UpdateTimer()
        {
            currentTime -= Time.deltaTime;
            
            if (currentTime <= 0)
            {
                currentTime = 0;
                isTimerActive = false;
                
                // 시간 초과 처리
                if (gameManager != null)
                {
                    gameManager.TimeUp();
                }
            }
            
            UpdateTimerUI();
        }
        
        private void InitializeUI()
        {
            // 게임 종료 UI 비활성화
            IsGameEndUIVisible = false;
            IsTimeUpUIVisible = false;
            IsNextLevelButtonVisible = false;
        }
        
        private void OnRabbitFound(object sender, HiddenObj rabbit)
        {
            UpdateRabbitCountUI();
        }
        

        
        private void OnGameEnd(object sender, EventArgs e)
        {
            // 게임 종료 시 UI는 ShowGameResult에서 처리됨
        }
        
        private void UpdateRabbitCountUI()
        {
            if (gameManager != null)
            {
                int foundCount = gameManager.GetFoundRabbitCount();
                int totalCount = gameManager.GetTotalRabbitCount();
                
                RabbitCountText = $"{foundCount}/{totalCount}";
                RabbitCountFillAmount = totalCount > 0 ? (float)foundCount / totalCount : 0f;
            }
        }
        
        private void UpdateTimerUI()
        {
            TimerText = string.Format("{0:0}초", currentTime);
            TimerFillAmount = timeLimit > 0 ? currentTime / timeLimit : 0f;
        }
        
        public void ShowGameResult(bool isSuccess)
        {
            var endTime = DateTime.Now;
            var timeUsed = endTime.Subtract(startTime);
            
            // 결과 텍스트 설정
            if (isSuccess)
            {
                ResultText = gameManager.CurrentLevelName + " CLEAR!";
            }
            else
            {
                ResultText = "TIME UP!";
            }
            
            // 최종 토끼 카운트 표시
            FinalRabbitCountText = $"{gameManager.GetFoundRabbitCount()} / {gameManager.GetTotalRabbitCount()}";
            
            // 소요 시간 표시
            if (isSuccess)
            {
                FinalTimeText = timeUsed.Hours > 0
                    ? timeUsed.ToString(@"hh\:mm\:ss")
                    : timeUsed.ToString(@"mm\:ss");
            }
            else
            {
                FinalTimeText = "TIME OVER";
            }
            
            // 적절한 UI 표시
            IsGameEndUIVisible = isSuccess;
            IsTimeUpUIVisible = !isSuccess;
            
            // 다음 레벨 버튼 활성화 여부
            IsNextLevelButtonVisible = isSuccess;
        }
        
        // 버튼 클릭 이벤트 바인딩 메서드들
        [Binding]
        public void RestartGame()
        {
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }
        
        [Binding]
        public void LoadNextLevel()
        {
            if (gameManager != null)
            {
                gameManager.LoadNextLevel();
            }
        }
        
        // 아이템 효과용 메서드
        public void AddTime(float seconds)
        {
            if (isTimerActive && gameManager != null && gameManager.IsGameActive())
            {
                currentTime += seconds;
                // 최대 시간 제한 (원래 시간 제한의 2배까지)
                currentTime = Mathf.Min(currentTime, timeLimit * 2f);
                Debug.Log($"{seconds}초 시간 추가됨. 현재 시간: {currentTime:F1}초");
            }
        }
        
        // 외부에서 현재 시간을 가져올 수 있는 메서드
        public float GetRemainingTime() => currentTime;
        
        // 테스트용 메서드들
        [Binding]
        public void GenerateTestRabbits()
        {
            if (gameManager != null)
            {
                gameManager.GenerateTestRabbits();
            }
        }
        
        [Binding]
        public void ClearGeneratedRabbits()
        {
            if (gameManager != null)
            {
                gameManager.ClearGeneratedRabbits();
            }
        }
    }
}
