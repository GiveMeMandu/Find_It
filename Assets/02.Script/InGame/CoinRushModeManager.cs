using UnityEngine;
using UnityEngine.UI;
using DeskCat.FindIt.Scripts.Core.Main.System;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 코인러쉬 모드 - 60초 등 짧은 시간 동안 맵에 무작위로 생성되는 코인 오브젝트를 찾아 최대한 많은 재화를 획득하는 이벤트성 모드입니다.
/// 광고 시청 시 획득 코인을 2배로 지급하는 보상 로직을 추가합니다.
/// </summary>
public class CoinRushModeManager : ModeManager
{
    [Header("Coin Rush Settings")]
    public float rushDuration = 60f;
    public GameObject coinPrefab;
    public int maxCoinsOnScreen = 10;
    public float coinSpawnInterval = 2f;
    public int coinValue = 10;
    public float coinLifetime = 8f;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI coinsCollectedText;
    public TextMeshProUGUI totalScoreText;
    public Button doubleRewardButton;
    public GameObject rushEndUI;

    private float remainingTime;
    private int coinsCollected = 0;
    private int totalScore = 0;
    private bool rushActive = false;
    private float nextSpawnTime = 0f;

    private List<GameObject> activeCoins = new List<GameObject>();
    private List<Transform> spawnPoints = new List<Transform>();

    public override void InitializeMode()
    {
        currentMode = GameMode.COIN_RUSH;
        Debug.Log("[CoinRushModeManager] 코인러쉬 모드 초기화 완료");

        remainingTime = rushDuration;
        coinsCollected = 0;
        totalScore = 0;
        rushActive = true;

        SetupSpawnPoints();
        UpdateUI();
        SetupDoubleRewardButton();
    }

    private void SetupSpawnPoints()
    {
        // 화면의 여러 위치에 스폰 포인트 생성 또는 기존 위치 활용
        // 실제로는 맵의 적절한 위치들을 spawnPoints에 추가해야 함

        // 예시: 화면 곳곳에 스폰 포인트 생성
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            for (int i = 0; i < 20; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(-8f, 8f),
                    Random.Range(-4f, 4f),
                    0f
                );

                GameObject spawnPoint = new GameObject($"CoinSpawnPoint_{i}");
                spawnPoint.transform.position = randomPos;
                spawnPoints.Add(spawnPoint.transform);
            }
        }
    }

    private void SetupDoubleRewardButton()
    {
        if (doubleRewardButton != null)
        {
            doubleRewardButton.onClick.AddListener(WatchAdForDoubleReward);
            doubleRewardButton.gameObject.SetActive(false); // 러시 종료 후 활성화
        }
    }

    protected override void Update()
    {
        base.Update();

        if (rushActive)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerUI();

            if (remainingTime <= 0)
            {
                EndCoinRush();
            }
            else if (Time.time >= nextSpawnTime)
            {
                SpawnCoin();
                nextSpawnTime = Time.time + coinSpawnInterval;
            }
        }

        // 활성화된 코인들의 수명 관리
        ManageActiveCoins();
    }

    private void ManageActiveCoins()
    {
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            if (activeCoins[i] == null)
            {
                activeCoins.RemoveAt(i);
            }
        }
    }

    private void SpawnCoin()
    {
        if (activeCoins.Count >= maxCoinsOnScreen || spawnPoints.Count == 0) return;

        // 랜덤 스폰 포인트 선택
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        if (coinPrefab != null)
        {
            GameObject coin = Instantiate(coinPrefab, spawnPoint.position, Quaternion.identity);

            // 코인에 클릭 가능한 컴포넌트 추가
            CoinRushCoin coinComponent = coin.GetComponent<CoinRushCoin>();
            if (coinComponent == null)
            {
                coinComponent = coin.AddComponent<CoinRushCoin>();
            }

            coinComponent.Initialize(coinValue, coinLifetime, OnCoinCollected);
            activeCoins.Add(coin);

            Debug.Log($"[CoinRushModeManager] 코인 스폰 위치: {spawnPoint.position}");
        }
    }

    private void OnCoinCollected(int value)
    {
        if (!rushActive) return;

        coinsCollected++;
        totalScore += value;

        Debug.Log($"[CoinRushModeManager] 코인 획득! 가치: {value}, 총점: {totalScore}");
        UpdateUI();
    }

    private void UpdateUI()
    {
        UpdateTimerUI();

        if (coinsCollectedText != null)
        {
            coinsCollectedText.text = $"Coins: {coinsCollected}";
        }

        if (totalScoreText != null)
        {
            totalScoreText.text = $"Score: {totalScore}";
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";

            if (remainingTime <= 10f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.yellow;
            }
        }
    }

    private void EndCoinRush()
    {
        rushActive = false;

        // 남은 코인들 제거
        foreach (GameObject coin in activeCoins)
        {
            if (coin != null)
            {
                Destroy(coin);
            }
        }
        activeCoins.Clear();

        Debug.Log($"[CoinRushModeManager] 코인러쉬 종료! 획득 코인: {coinsCollected}, 최종 점수: {totalScore}");

        if (rushEndUI != null)
        {
            rushEndUI.SetActive(true);
        }

        if (doubleRewardButton != null)
        {
            doubleRewardButton.gameObject.SetActive(true);
        }

        OnGameEnd();
    }

    private void WatchAdForDoubleReward()
    {
        // 광고 시청 로직 (실제로는 광고 SDK와 연동)
        Debug.Log("[CoinRushModeManager] 보상 2배 광고 시청 중...");

        // 광고 시청 완료 후 보상 2배 지급
        int doubledReward = totalScore * 2;
        Debug.Log($"[CoinRushModeManager] 보상 2배 지급 완료: {totalScore} -> {doubledReward}");

        totalScore = doubledReward;
        UpdateUI();

        if (doubleRewardButton != null)
        {
            doubleRewardButton.gameObject.SetActive(false);
        }
    }

    public override void OnGameStart()
    {
        base.OnGameStart();
        Debug.Log($"[CoinRushModeManager] 코인러쉬 시작! {rushDuration}초 동안 가능한 한 많은 코인을 모으세요!");
    }

    public override void OnGameEnd()
    {
        base.OnGameEnd();
        Debug.Log($"[CoinRushModeManager] 최종 점수: {totalScore} 코인 획득");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // 스폰 포인트들 정리
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null && spawnPoint.gameObject != null)
            {
                Destroy(spawnPoint.gameObject);
            }
        }

        if (doubleRewardButton != null)
        {
            doubleRewardButton.onClick.RemoveListener(WatchAdForDoubleReward);
        }
    }
}

/// <summary>
/// 코인러쉬 모드에서 사용되는 개별 코인 클래스
/// </summary>
public class CoinRushCoin : MonoBehaviour
{
    private int value;
    private float lifetime;
    private System.Action<int> onCollected;
    private float spawnTime;

    public void Initialize(int coinValue, float coinLifetime, System.Action<int> collectCallback)
    {
        value = coinValue;
        lifetime = coinLifetime;
        onCollected = collectCallback;
        spawnTime = Time.time;

        // 클릭 가능하도록 콜라이더 추가
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<CircleCollider2D>();
        }
    }

    private void Update()
    {
        // 수명 체크
        if (Time.time - spawnTime >= lifetime)
        {
            DestroyCoin();
        }

        // 깜빡이는 효과 (수명이 얼마 남지 않았을 때)
        if (Time.time - spawnTime >= lifetime * 0.7f)
        {
            float blinkRate = 5f;
            bool visible = Mathf.Sin(Time.time * blinkRate) > 0;
            GetComponent<Renderer>().enabled = visible;
        }
    }

    private void OnMouseDown()
    {
        CollectCoin();
    }

    private void CollectCoin()
    {
        onCollected?.Invoke(value);
        DestroyCoin();
    }

    private void DestroyCoin()
    {
        Destroy(gameObject);
    }
}