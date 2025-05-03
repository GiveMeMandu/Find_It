using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Data;
using Manager;
using Cysharp.Threading.Tasks;

public class CloudRabbitGameManager : MMSingleton<CloudRabbitGameManager>
{
    [System.Serializable]
    public class CloudRabbitData
    {
        public SceneName sceneName;
        public int stageIndex;
        public Sprite backgroundImage;
        public List<RabbitInfo> rabbitList;
    }

    [System.Serializable]
    public class RabbitInfo
    {
        public Vector2 position;
        public float size = 1f;
        public bool isFound = false;
    }

    [SerializeField] private CloudRabbitData[] cloudRabbitDataList;
    private CloudRabbitData currentData;
    private bool isGamePaused = false;
    private bool isGameCompleted = false;
    private InputManager inputManager;

    public CloudRabbitData[] CloudRabbitDataList => cloudRabbitDataList;
    public bool IsGamePaused => isGamePaused;
    public CloudRabbitData CurrentData => currentData;
    
    public event Action OnGameCompleted;
    public event Action OnGameStarted;
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action<RabbitInfo> OnRabbitFound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputManager = FindAnyObjectByType<InputManager>();
        if (inputManager != null)
        {
            inputManager.OnTouchPressAction += HandleTouchPress;
        }
    }

    private void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnTouchPressAction -= HandleTouchPress;
        }
    }

    public void InitializeGame(SceneName sceneName, int stageIndex)
    {
        isGameCompleted = false;
        
        // sceneName과 stageIndex에 맞는 데이터 찾기
        currentData = cloudRabbitDataList.FirstOrDefault(d => d.sceneName == sceneName && d.stageIndex == stageIndex);
        if (currentData == null) return;

        // 토끼 초기화
        foreach (var rabbit in currentData.rabbitList)
        {
            rabbit.isFound = false;
        }

        OnGameStarted?.Invoke();
    }

    private void HandleTouchPress(object sender, InputManager.TouchData touchData)
    {
        if (isGamePaused || isGameCompleted || currentData == null) return;

        // 터치 위치에서 토끼 확인
        CheckForRabbit(touchData.WorldPosition);
    }

    private void CheckForRabbit(Vector2 touchPosition)
    {
        for (int i = 0; i < currentData.rabbitList.Count; i++)
        {
            RabbitInfo rabbit = currentData.rabbitList[i];
            if (rabbit.isFound) continue;

            // 터치 위치와 토끼 위치 사이의 거리 계산
            float distance = Vector2.Distance(touchPosition, rabbit.position);
            float hitRange = rabbit.size * 0.5f; // 토끼 크기의 절반을 히트 범위로 사용

            if (distance <= hitRange)
            {
                // 토끼 발견
                rabbit.isFound = true;
                OnRabbitFound?.Invoke(rabbit);
                
                // 모든 토끼를 찾았는지 확인
                CheckCompletion();
                break;
            }
        }
    }

    private void CheckCompletion()
    {
        if (currentData.rabbitList.All(r => r.isFound))
        {
            isGameCompleted = true;
            OnGameCompleted?.Invoke();
        }
    }

    public void PauseGame()
    {
        if (!isGamePaused)
        {
            isGamePaused = true;
            inputManager?.DisableAllInput();
            OnGamePaused?.Invoke();
        }
    }

    public void ResumeGame()
    {
        if (isGamePaused)
        {
            isGamePaused = false;
            inputManager?.EnableAllInput();
            OnGameResumed?.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
