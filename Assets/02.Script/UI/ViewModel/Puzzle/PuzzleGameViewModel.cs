using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;
using System;
using Manager;
using Data;
using Unity.VisualScripting;

namespace UI
{
    [Binding]
    public class PuzzleGameViewModel : ViewModel
    {
        [SerializeField] private MiniGameProgressBarViewModel progressBar;
        [SerializeField] private float gameDuration = 180f; // 3분

        private Sprite _puzzlePreview;
        private bool _isGameStarted;
        private bool _isGamePaused;
        private bool _isGameCompleted;

        public event EventHandler OnGameCompleted;
        public event EventHandler OnGameStarted;
        public event EventHandler OnGamePaused;
        public event EventHandler OnGameResumed;

        [Binding]
        public Sprite PuzzlePreview
        {
            get => _puzzlePreview;
            set
            {
                _puzzlePreview = value;
                OnPropertyChanged(nameof(PuzzlePreview));
            }
        }

        [Binding]
        public bool IsGameStarted
        {
            get => _isGameStarted;
            set
            {
                _isGameStarted = value;
                OnPropertyChanged(nameof(IsGameStarted));
            }
        }

        [Binding]
        public bool IsGamePaused
        {
            get => _isGamePaused;
            set
            {
                _isGamePaused = value;
                OnPropertyChanged(nameof(IsGamePaused));
            }
        }

        [Binding]
        public bool IsGameCompleted
        {
            get => _isGameCompleted;
            set
            {
                _isGameCompleted = value;
                OnPropertyChanged(nameof(IsGameCompleted));
            }
        }

        public void Initialize(PuzzleData puzzleData)
        {
            if (puzzleData == null) return;
            
            PuzzlePreview = puzzleData.puzzleImage;
            IsGameStarted = false;
            IsGamePaused = false;
            IsGameCompleted = false;

            // 타이머 초기화 및 시작
            if (progressBar != null)
            {
                progressBar.StartTimer(gameDuration);
            }
        }
        private void OnEnable()
        {
            // PuzzleGameManager 이벤트 구독
            PuzzleGameManager.Instance.OnPuzzleCompleted += HandlePuzzleCompleted;
            PuzzleGameManager.Instance.OnPuzzlePaused += HandlePuzzlePaused;
            PuzzleGameManager.Instance.OnPuzzleResumed += HandlePuzzleResumed;
            
            if (progressBar != null)
            {
                progressBar.OnProgressBarEmpty += HandleGameOver;
            }
        }

        private void OnDisable()
        {
            if (PuzzleGameManager.Instance != null)
            {
                PuzzleGameManager.Instance.OnPuzzleCompleted -= HandlePuzzleCompleted;
                PuzzleGameManager.Instance.OnPuzzlePaused -= HandlePuzzlePaused;
                PuzzleGameManager.Instance.OnPuzzleResumed -= HandlePuzzleResumed;
            }

            if (progressBar != null)
            {
                progressBar.OnProgressBarEmpty -= HandleGameOver;
                progressBar.StopTimer();
            }
        }

        private void HandlePuzzleCompleted()
        {
            IsGameCompleted = true;
            progressBar?.StopTimer();
            // 퍼즐 완성 시 추가 처리가 필요하다면 여기에 추가
        }

        private void HandlePuzzlePaused()
        {
            IsGamePaused = true;
            progressBar?.StopTimer();
        }

        private void HandlePuzzleResumed()
        {
            IsGamePaused = false;
            progressBar?.SetupTimer(gameDuration);
        }

        private void HandleGameOver(object sender, EventArgs e)
        {
            // 시간 초과 처리
            IsGameCompleted = true;
            PuzzleGameManager.Instance.PauseGame();
            // 실패 UI 표시 등 추가 처리
        }

        [Binding]
        public void StartGame()
        {
            IsGameStarted = true;
            OnGameStarted?.Invoke(this, EventArgs.Empty);
        }

        [Binding]
        public void OnClickPauseButton()
        {
            PuzzleGameManager.Instance.PauseGame();
        }

        [Binding]
        public void OnClickResumeButton()
        {
            PuzzleGameManager.Instance.ResumeGame();
        }

        public void CompleteGame()
        {
            IsGameStarted = false;
            OnGameCompleted?.Invoke(this, EventArgs.Empty);
        }

    }
}
