using UnityEngine;
using UnityEngine.UI;
using DeskCat.FindIt.Scripts.Core.Main.System;
using TMPro;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using Data;
using UI;
using Manager;

/// <summary>
/// 타임챌린지 모드 - 60초 등 정해진 제한 시간 내에 모든 오브젝트를 찾아야 하는 모드로, 시간이 종료되면 실패합니다.
/// 난이도 조정을 위해 힌트 사용을 제한하거나 보너스 시간을 추가할 수 있습니다.
/// </summary>
public class TimeChallengeModeManager : ModeManager
{
    [Header("Time Challenge Settings")]
    public float timeLimit = 60f;
    public int maxHints = 3;
    public float bonusTimePerObject = 5f;

    [Tooltip("체크 시 레벨매니저의 ScrollView(찾을 물건 목록 UI)를 숨깁니다")]
    public bool hideScrollView = false;

    [Header("Timer ViewModels")]
    [Tooltip("List of TimerCountViewModel instances to start when the level begins (seconds)")]
    public List<TimerCountViewModel> TimerViewModels;
    public int defaultSeconds = 600; // 기본 타이머 시간 (초)
    private int timersCompleted = 0;
    private bool forcedEndTriggered = false;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI hintsText;
    public Button hintButton;
    public GameObject timeUpUI;

    private float remainingTime;
    private int hintsUsed = 0;
    private bool gameEnded = false;

    public override void InitializeMode()
    {
        currentMode = GameMode.TIME_CHALLENGE;
        Debug.Log("[TimeChallengeModeManager] 타임챌린지 모드 초기화 완료");

        remainingTime = timeLimit;
        hintsUsed = 0;
        gameEnded = false;
        timersCompleted = 0;
        forcedEndTriggered = false;

        if (levelManager != null)
        {
            levelManager.OnFoundObj += OnHiddenObjectFound;
        }

        // ScrollView 숨김 처리
        if (hideScrollView)
        {
            HideScrollView();
        }

        // TimerViewModels 초기화 및 시작
        InitializeTimers();

        UpdateUI();
        SetupHintButton();
    }

    private void InitializeTimers()
    {
        // Start timers for all assigned TimerCountViewModel instances (default 10 minutes = 600 seconds)
        if (TimerViewModels != null)
        {
            foreach (var timerVm in TimerViewModels)
            {
                timerVm.gameObject.SetActive(true);
                if (timerVm == null) continue;
                // Subscribe to completion event
                timerVm.OnTimerComplete += OnSingleTimerComplete;
                // defaultSeconds = Global.StageTimer;
                timerVm.Initialize(defaultSeconds);
            }
        }
    }

    private void OnSingleTimerComplete()
    {
        // Fire-and-forget the async handler
        _ = HandleTimerCompletionAsync();
    }

    private async UniTaskVoid HandleTimerCompletionAsync()
    {
        timersCompleted++;

        Debug.Log($"[TimeChallengeModeManager] Timer completed ({timersCompleted}/{(TimerViewModels?.Count ?? 0)})");

        if (forcedEndTriggered) return;

        if (TimerViewModels != null && timersCompleted >= TimerViewModels.Count)
        {
            forcedEndTriggered = true;
            Debug.Log("[TimeChallengeModeManager] All timers finished — forcing game end sequence.");

            // Run any registered async end tasks first
            if (levelManager != null && levelManager.OnEndEvent.Count > 0)
            {
                foreach (var func in levelManager.OnEndEvent)
                {
                    try
                    {
                        await func();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[TimeChallengeModeManager] Error executing end event: {ex}");
                    }
                }
            }

            TimeUp();
        }
    }

    private void SetupHintButton()
    {
        if (hintButton != null)
        {
            hintButton.onClick.AddListener(UseHint);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!gameEnded && remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerUI();

            if (remainingTime <= 0)
            {
                TimeUp();
            }
        }
    }

    private void UpdateUI()
    {
        UpdateTimerUI();
        UpdateHintsUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";

            // 시간이 얼마 남지 않으면 빨간색으로 표시
            if (remainingTime <= 10f)
            {
                timerText.color = Color.red;
            }
            else if (remainingTime <= 30f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    private void UpdateHintsUI()
    {
        if (hintsText != null)
        {
            hintsText.text = $"Hints: {maxHints - hintsUsed}";
        }

        if (hintButton != null)
        {
            hintButton.interactable = hintsUsed < maxHints && !gameEnded;
        }
    }

    private void UseHint()
    {
        if (hintsUsed >= maxHints || gameEnded) return;

        hintsUsed++;

        // 힌트 로직: 아직 찾지 않은 오브젝트 중 하나를 강조 표시
        var leftObjects = levelManager.GetLeftHiddenObjCount();
        if (leftObjects > 0)
        {
            // LevelManager의 테스트 메서드를 활용하여 힌트 제공
            // 실제로는 오브젝트를 찾지 않고 위치만 강조 표시해야 함
            Debug.Log($"[TimeChallengeModeManager] 힌트 사용됨! 남은 힌트: {maxHints - hintsUsed}");

            // 힌트 효과 구현 (예: 오브젝트 주변에 반짝이는 효과)
            ShowHintEffect();
        }

        UpdateHintsUI();
    }

    private void ShowHintEffect()
    {
        // 힌트 효과 구현 - 실제로는 파티클이나 UI 효과로 오브젝트 위치 암시
        Debug.Log("[TimeChallengeModeManager] 숨겨진 오브젝트에 힌트 효과를 표시합니다");
    }

    private void OnHiddenObjectFound(object sender, HiddenObj foundObj)
    {
        if (gameEnded) return;

        OnObjectFound(foundObj);

        // 보너스 시간 추가
        remainingTime += bonusTimePerObject;
        Debug.Log($"[TimeChallengeModeManager] 보너스 시간 추가: +{bonusTimePerObject}초");

        // 모든 오브젝트를 찾았는지 확인
        if (levelManager.GetLeftHiddenObjCount() <= 0)
        {
            GameCompleted();
        }
    }

    private void TimeUp()
    {
        gameEnded = true;
        Debug.Log("[TimeChallengeModeManager] 시간이 종료되었습니다! 게임 실패");

        if (timeUpUI != null)
        {
            timeUpUI.SetActive(true);
        }

        OnGameEnd();
    }

    private void GameCompleted()
    {
        gameEnded = true;
        Debug.Log($"[TimeChallengeModeManager] 챌린지 완료! 남은 시간: {remainingTime:F1}초");
        OnGameEnd();
    }

    public override void OnGameStart()
    {
        base.OnGameStart();
        Debug.Log($"[TimeChallengeModeManager] 타임챌린지 시작 - 모든 오브젝트를 {timeLimit}초 내에 찾으세요!");
    }

    public override void OnObjectFound(HiddenObj foundObj)
    {
        base.OnObjectFound(foundObj);
        Debug.Log($"[TimeChallengeModeManager] 오브젝트 발견 - 남은 시간: {remainingTime:F1}초");
    }

    public override void OnGameEnd()
    {
        base.OnGameEnd();

        if (remainingTime > 0)
        {
            Debug.Log("[TimeChallengeModeManager] 챌린지 성공적으로 완료됨!");
        }
        else
        {
            Debug.Log("[TimeChallengeModeManager] 챌린지 실패 - 시간이 모두 소진되었습니다!");
        }
    }

    /// <summary>
    /// 레벨매니저의 ScrollView를 타임챌린지 모드로 전환합니다.
    /// contentContainer와 CountCircle을 숨기고 TimeChallengePanel을 표시합니다.
    /// </summary>
    private void HideScrollView()
    {
        if (levelManager == null) return;

        if (levelManager.HorizontalScrollView != null)
        {
            levelManager.HorizontalScrollView.SetTimeChallengeMode(true);
        }

        if (levelManager.VerticalScrollView != null)
        {
            levelManager.VerticalScrollView.SetTimeChallengeMode(true);
        }

        Debug.Log("[TimeChallengeModeManager] ScrollView가 타임챌린지 모드로 전환되었습니다");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (levelManager != null)
        {
            levelManager.OnFoundObj -= OnHiddenObjectFound;
        }

        if (hintButton != null)
        {
            hintButton.onClick.RemoveListener(UseHint);
        }

        // Unsubscribe from timer events to avoid leaks
        if (TimerViewModels != null)
        {
            foreach (var timerVm in TimerViewModels)
            {
                if (timerVm == null) continue;
                timerVm.OnTimerComplete -= OnSingleTimerComplete;
            }
        }
    }
}