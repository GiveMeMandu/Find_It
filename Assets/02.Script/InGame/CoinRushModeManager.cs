using UnityEngine;
using UnityEngine.UI;
using DeskCat.FindIt.Scripts.Core.Main.System;
using DeskCat.FindIt.Scripts.Core.Model;
using TMPro;
using System;
using System.Collections.Generic;
using Util.CameraSetting;
using Sirenix.OdinInspector;
using Manager;

using Random = UnityEngine.Random;

/// <summary>
/// 코인러쉬 모드 - 60초 등 짧은 시간 동안 맵에 무작위로 생성되는 코인 오브젝트를 찾아 최대한 많은 재화를 획득하는 이벤트성 모드입니다.
/// 광고 시청 시 획득 코인을 2배로 지급하는 보상 로직을 추가합니다.
/// LevelManager의 HiddenObj 방식과 유사하게 코인을 관리합니다.
/// </summary>
public class CoinRushModeManager : ModeManager
{
    [Header("Coin Rush Settings")]
    [LabelText("러쉬 지속시간 (초)")]
    public float rushDuration = 60f;

    [Header("Coin Generation")]
    [Tooltip("미리 배치된 코인들을 사용할지 여부")]
    [LabelText("프리셋 코인 사용")]
    public bool usePresetCoins = false;
    [Tooltip("미리 배치된 코인 오브젝트들 (usePresetCoins=true 일 때 사용)")]
    [LabelText("프리셋 코인들")]
    public HiddenObj[] presetCoins;

    [Tooltip("게임 시작시 모든 코인을 한번에 생성할지 여부 (usePresetCoins=false 일 때만 유효)")]
    [LabelText("시작 시 전체 생성")]
    public bool spawnAllCoinsAtStart = false;

    [Tooltip("런타임에 생성할 코인 프리팹 (usePresetCoins=false 일 때 사용)")]
    [LabelText("코인 프리팹")]
    public GameObject coinPrefab;
    [LabelText("최대 화면 코인 수")]
    public int maxCoinsOnScreen = 10;
    [LabelText("코인 소환 간격 (초)")]
    public float coinSpawnInterval = 2f;
    [Tooltip("배경 스프라이트 경계로부터의 안쪽 여백")]
    [LabelText("배경으로부터 스폰 영역 패딩")]
    public float spawnAreaPadding = 0.5f;

    [Header("Coin Spacing")]
    [Tooltip("코인 소환 시 다른 코인과의 최소 거리를 체크할지 여부")]
    [LabelText("코인 최소 간격 사용")]
    public bool useMinimumSpacing = true;
    [ShowIf("useMinimumSpacing")]
    [LabelText("코인 최소 거리")]
    [Tooltip("다른 코인과의 최소 거리")]
    public float minimumCoinDistance = 1.5f;
    [ShowIf("useMinimumSpacing")]
    [LabelText("최대 소환 시도 횟수")]
    [Tooltip("최소 거리 체크 최대 시도 횟수")]
    public int maxSpawnAttempts = 30;

    [Header("Coin Size")]
    [Tooltip("코인 크기를 랜덤으로 설정할지 여부")]
    [LabelText("랜덤 코인 크기 사용")]
    public bool useRandomSize = false;
    [ShowIf("useRandomSize")]
    [LabelText("최소 크기 배율")]
    [Tooltip("최소 크기 배율")]
    public float minSizeScale = 0.7f;
    [ShowIf("useRandomSize")]
    [LabelText("최대 크기 배율")]
    [Tooltip("최대 크기 배율")]
    public float maxSizeScale = 1.3f;
    [HideIf("useRandomSize")]
    [LabelText("고정 크기 배율")]
    [Tooltip("고정 크기 배율")]
    public float fixedSizeScale = 1.0f;

    [Header("Coin Properties")]
    [LabelText("코인 가치")]
    public int coinValue = 10;
    [LabelText("코인 수명 (초)")]
    public float coinLifetime = 8f;

    [Header("Background Animation")]
    [LabelText("기본 배경 애니메이션")]
    public GameObject DefaultBgAnimation;

    [Header("UI References")]
    [LabelText("타이머 텍스트")]
    public TextMeshProUGUI timerText;
    [LabelText("획득 코인 텍스트")]
    public TextMeshProUGUI coinsCollectedText;
    [LabelText("총점 텍스트")]
    public TextMeshProUGUI totalScoreText;
    [LabelText("보상 2배 버튼")]
    public Button doubleRewardButton;
    [LabelText("러시 종료 UI")]
    public GameObject rushEndUI;

    [Header("Sound Effects")]
    [LabelText("코인 발견 SFX")]
    public AudioSource coinFoundFx;

    [Header("coin ui들")]
    public List<IngameCoinLayer> ingameCoinLayers;

    private float remainingTime;
    private int coinsCollected = 0;
    private int totalScore = 0;
    private bool rushActive = false;
    private float nextSpawnTime = 0f;

    // HiddenObj 방식의 코인 관리 (LevelManager와 유사)
    private Dictionary<Guid, HiddenObj> coinObjDic = new Dictionary<Guid, HiddenObj>();
    private List<GameObject> activeCoinObjects = new List<GameObject>();

    public override void InitializeMode()
    {
        currentMode = GameMode.COIN_RUSH;
        Debug.Log("[CoinRushModeManager] 코인러쉬 모드 초기화 완료");

        remainingTime = rushDuration;
        coinsCollected = 0;
        totalScore = 0;
        rushActive = true;

        // 미리 세팅된 코인들 초기화
        if (usePresetCoins)
        {
            InitializePresetCoins();
        }
        // 게임 시작시 모든 코인을 한번에 생성
        else if (spawnAllCoinsAtStart)
        {
            SpawnAllCoinsAtStart();
        }

        UpdateUI();
        SetupDoubleRewardButton();
        // 활성화된 모드일 때 인게임 코인 UI 레이어들 활성화
        if (ingameCoinLayers != null)
        {
            foreach (var layer in ingameCoinLayers)
            {
                if (layer != null && layer.gameObject != null)
                    layer.gameObject.SetActive(true);
            }
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(16f, 9f, 0f));

        if (CameraView2D.Instance != null && CameraView2D.Instance.backgroundSprite != null)
        {
            bounds = CameraView2D.Instance.backgroundSprite.bounds;
        }

        float minX = bounds.min.x + spawnAreaPadding;
        float maxX = bounds.max.x - spawnAreaPadding;
        float minY = bounds.min.y + spawnAreaPadding;
        float maxY = bounds.max.y - spawnAreaPadding;

        // Ensure min <= max
        if (minX > maxX) { float temp = minX; minX = maxX; maxX = temp; }
        if (minY > maxY) { float temp = minY; minY = maxY; maxY = temp; }

        // 최소 거리 체크를 사용하는 경우
        if (useMinimumSpacing)
        {
            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                Vector3 position = new Vector3(
                    Random.Range(minX, maxX),
                    Random.Range(minY, maxY),
                    0f
                );

                // 다른 코인들과의 거리 체크
                if (IsPositionValid(position))
                {
                    return position;
                }
            }

            Debug.LogWarning($"[CoinRushModeManager] 최소 거리를 만족하는 위치를 찾지 못했습니다. ({maxSpawnAttempts}회 시도)");
        }

        // 최소 거리 체크를 사용하지 않거나, 적합한 위치를 찾지 못한 경우 랜덤 반환
        return new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            0f
        );
    }

    /// <summary>
    /// 해당 위치가 다른 코인들과 최소 거리를 유지하는지 확인
    /// </summary>
    private bool IsPositionValid(Vector3 position)
    {
        foreach (var coin in activeCoinObjects)
        {
            if (coin != null)
            {
                float distance = Vector3.Distance(position, coin.transform.position);
                if (distance < minimumCoinDistance)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void InitializePresetCoins()
    {
        if (presetCoins == null || presetCoins.Length == 0)
        {
            Debug.LogWarning("[CoinRushModeManager] usePresetCoins is true but no coins are assigned!");
            return;
        }

        Debug.Log($"[CoinRushModeManager] Initializing {presetCoins.Length} preset coins");

        foreach (var coinObj in presetCoins)
        {
            if (coinObj == null) continue;

            // HiddenObj 기본 설정
            coinObj.PlaySoundWhenFound = true;
            if (coinObj.TryGetComponent(out HideWhenFoundHelper hideWhenFoundHelper))
                coinObj.HideWhenFound = hideWhenFoundHelper.hideWhenFound;
            else
                coinObj.HideWhenFound = true;
            coinObj.hiddenObjFoundType = HiddenObjFoundType.Click;

            // 사운드 설정 (coinFoundFx를 AudioWhenClick으로 설정)
            if (coinFoundFx != null)
            {
                coinObj.AudioWhenClick = coinFoundFx.clip;
            }

            // BoxCollider2D 확인 및 추가
            if (!coinObj.TryGetComponent<BoxCollider2D>(out var boxCollider))
            {
                boxCollider = coinObj.gameObject.AddComponent<BoxCollider2D>();
                boxCollider.isTrigger = false;
                boxCollider.size = new Vector2(boxCollider.size.x * 1.5f, boxCollider.size.y * 1.5f);
            }

            // 배경 애니메이션 설정
            // BGAnimationHelper가 있으면 해당 설정을 우선 적용
            BGAnimationHelper bgAnimHelper = coinObj.GetComponent<BGAnimationHelper>();
            bool useBgAnim = bgAnimHelper == null || bgAnimHelper.UseBgAnimation;
            GameObject bgAnimPrefab = bgAnimHelper != null && bgAnimHelper.CustomBgAnimationPrefab != null
                ? bgAnimHelper.CustomBgAnimationPrefab
                : DefaultBgAnimation;

            if (useBgAnim && bgAnimPrefab != null && coinObj.BgAnimationTransform == null)
            {
                GameObject bgObj = Instantiate(bgAnimPrefab, coinObj.transform);
                coinObj.BgAnimationTransform = bgObj.transform;
                coinObj.SetBgAnimation(bgObj);
            }

            // Dictionary에 추가 및 클릭 이벤트 연결
            Guid guid = Guid.NewGuid();
            coinObjDic.Add(guid, coinObj);

            coinObj.TargetClickAction = () => { OnCoinClick(guid); };
            coinObj.IsFound = false;

            activeCoinObjects.Add(coinObj.gameObject);
        }

        Debug.Log($"[CoinRushModeManager] {coinObjDic.Count} preset coins initialized");
    }

    /// <summary>
    /// 게임 시작시 모든 코인을 한번에 생성
    /// </summary>
    private void SpawnAllCoinsAtStart()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("[CoinRushModeManager] spawnAllCoinsAtStart is true but coinPrefab is not assigned!");
            return;
        }

        Debug.Log($"[CoinRushModeManager] Spawning {maxCoinsOnScreen} coins at start");

        for (int i = 0; i < maxCoinsOnScreen; i++)
        {
            SpawnSingleCoin();
        }

        Debug.Log($"[CoinRushModeManager] {coinObjDic.Count} coins spawned at start");
    }

    /// <summary>
    /// 코인 하나를 생성 (SpawnCoin 로직을 재사용)
    /// </summary>
    private void SpawnSingleCoin()
    {
        Vector3 spawnPos = GetRandomSpawnPosition();

        GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);

        // HiddenObj 컴포넌트 추가 또는 가져오기
        HiddenObj hiddenObj = coin.GetComponent<HiddenObj>();
        if (hiddenObj == null)
        {
            hiddenObj = coin.AddComponent<HiddenObj>();
        }

        // HiddenObj 기본 설정 (LevelManager 방식)
        hiddenObj.PlaySoundWhenFound = true;
        if (hiddenObj.TryGetComponent(out HideWhenFoundHelper hideWhenFoundHelper))
            hiddenObj.HideWhenFound = hideWhenFoundHelper.hideWhenFound;
        else
            hiddenObj.HideWhenFound = true;
        hiddenObj.hiddenObjFoundType = HiddenObjFoundType.Click;

        // 사운드 설정 (coinFoundFx를 AudioWhenClick으로 설정)
        if (coinFoundFx != null)
        {
            hiddenObj.AudioWhenClick = coinFoundFx.clip;
        }

        // Collider 설정
        if (!coin.TryGetComponent<BoxCollider2D>(out var boxCollider))
        {
            boxCollider = coin.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = false;
        }

        // 배경 애니메이션 설정
        // BGAnimationHelper가 있으면 해당 설정을 우선 적용
        BGAnimationHelper bgAnimHelper = coin.GetComponent<BGAnimationHelper>();
        bool useBgAnim = bgAnimHelper == null || bgAnimHelper.UseBgAnimation;
        GameObject bgAnimPrefab = bgAnimHelper != null && bgAnimHelper.CustomBgAnimationPrefab != null
            ? bgAnimHelper.CustomBgAnimationPrefab
            : DefaultBgAnimation;

        if (useBgAnim && bgAnimPrefab != null)
        {
            GameObject bgObj = Instantiate(bgAnimPrefab, hiddenObj.transform);
            hiddenObj.BgAnimationTransform = bgObj.transform;
            hiddenObj.SetBgAnimation(bgObj);
        }

        // CoinRushCoin 컴포넌트 추가 (수명 및 시각 효과용)
        CoinRushCoin coinComponent = coin.GetComponent<CoinRushCoin>();
        if (coinComponent == null)
        {
            coinComponent = coin.AddComponent<CoinRushCoin>();
        }
        coinComponent.Initialize(coinValue, coinLifetime, null); // 콜백은 HiddenObj에서 처리

        // 코인 크기 설정
        ApplyCoinSize(coin.transform);

        // Dictionary에 추가 및 클릭 이벤트 연결
        Guid guid = Guid.NewGuid();
        coinObjDic.Add(guid, hiddenObj);

        // HiddenObj의 클릭 액션 설정 (LevelManager 방식)
        hiddenObj.TargetClickAction = () =>
        {
            coinComponent.CollectCoin();
            OnCoinClick(guid);
        };
        hiddenObj.IsFound = false;

        activeCoinObjects.Add(coin);
    }

    /// <summary>
    /// 코인의 크기를 설정합니다. (랜덤 또는 고정)
    /// </summary>
    private void ApplyCoinSize(Transform coinTransform)
    {
        float scale;

        if (useRandomSize)
        {
            scale = Random.Range(minSizeScale, maxSizeScale);
        }
        else
        {
            scale = fixedSizeScale;
        }

        coinTransform.localScale = Vector3.one * scale;
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
        // 수명이 다한 코인들을 찾아서 제거
        List<Guid> coinsToRemove = new List<Guid>();

        foreach (var kvp in coinObjDic)
        {
            var coinObj = kvp.Value;
            if (coinObj == null || coinObj.gameObject == null)
            {
                coinsToRemove.Add(kvp.Key);
                continue;
            }

            // CoinRushCoin 컴포넌트에서 수명 관리가 이루어지므로 여기서는 null 체크만
        }

        // Dictionary에서 제거
        foreach (var guid in coinsToRemove)
        {
            coinObjDic.Remove(guid);
        }

        // 오브젝트 리스트 정리
        activeCoinObjects.RemoveAll(obj => obj == null);
    }

    private void SpawnCoin()
    {
        // 미리 세팅된 코인을 사용하는 경우 런타임 생성 안 함
        if (usePresetCoins) return;

        // 게임 시작시 모든 코인 생성 옵션이 켜져있으면 개별 스폰 안 함
        if (spawnAllCoinsAtStart) return;

        if (coinObjDic.Count >= maxCoinsOnScreen) return;

        if (coinPrefab != null)
        {
            SpawnSingleCoin();

        }
    }

    private void OnCoinClick(Guid guid)
    {
        if (!rushActive) return;

        if (!coinObjDic.ContainsKey(guid))
        {
            Debug.LogWarning($"[CoinRushModeManager] OnCoinClick called with unknown guid: {guid}");
            return;
        }

        var hiddenObj = coinObjDic[guid];
        FoundCoinAction(guid, hiddenObj);
    }

    private void FoundCoinAction(Guid guid, HiddenObj hiddenObj)
    {
        // HitHiddenObject에서 이미 IsFound를 true로 설정하므로 여기서는 체크하지 않음
        // 사운드는 HiddenObj.HitHiddenObject에서 재생됨

        coinsCollected++;
        totalScore += coinValue;

        // Dictionary에서 제거
        coinObjDic.Remove(guid);

        // 코인 획득 즉시 CoinManager에 추가 및 저장
        if (Global.CoinManager != null)
        {
            Global.CoinManager.AddCoin(new System.Numerics.BigInteger(coinValue));
            Global.CoinManager.SaveCoinData();
        }

        Debug.Log($"[CoinRushModeManager] 코인 획득! 가치: {coinValue}, 총점: {totalScore}, 남은 코인: {coinObjDic.Count}");

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
        foreach (GameObject coin in activeCoinObjects)
        {
            if (coin != null)
            {
                Destroy(coin);
            }
        }
        activeCoinObjects.Clear();
        coinObjDic.Clear();

        Debug.Log($"[CoinRushModeManager] 코인러쉬 종료! 획득 코인: {coinsCollected}, 최종 점수: {totalScore}");

        if (rushEndUI != null)
        {
            rushEndUI.SetActive(true);
        }

        if (doubleRewardButton != null)
        {
            doubleRewardButton.gameObject.SetActive(true);
        }

        // 모드 종료 시 인게임 코인 UI 레이어들 비활성화
        if (ingameCoinLayers != null)
        {
            foreach (var layer in ingameCoinLayers)
            {
                if (layer != null && layer.gameObject != null)
                    layer.gameObject.SetActive(false);
            }
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

        // 코인은 획득할 때마다 실시간으로 저장되므로 여기서는 로그만 출력
        Debug.Log($"[CoinRushModeManager] 게임 종료 - 최종 점수: {totalScore}");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 생성된 코인들 정리
        foreach (GameObject coin in activeCoinObjects)
        {
            if (coin != null)
            {
                Destroy(coin);
            }
        }
        activeCoinObjects.Clear();
        coinObjDic.Clear();

        if (doubleRewardButton != null)
        {
            doubleRewardButton.onClick.RemoveListener(WatchAdForDoubleReward);
        }
    }

    // 디버깅 및 접근용 메서드들
    public int GetCoinsCollected() => coinsCollected;
    public int GetActiveCoinCount() => coinObjDic.Count;
    public bool IsRushActive() => rushActive;
    public float GetRemainingTime() => remainingTime;

    /// <summary>
    /// LevelManager가 코인들을 인식할 수 있도록 Dictionary를 반환
    /// </summary>
    public Dictionary<Guid, HiddenObj> GetCoinDictionary() => coinObjDic;

    /// <summary>
    /// LevelManager에서 코인들을 포함해야 하는지 여부
    /// (미리 세팅된 코인 또는 시작시 모든 코인 생성)
    /// </summary>
    public bool ShouldIncludeCoinsInLevelManager() => usePresetCoins || spawnAllCoinsAtStart;
}