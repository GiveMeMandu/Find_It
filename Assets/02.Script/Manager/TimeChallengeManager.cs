using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeskCat.FindIt.Scripts.Core.Main.System;
using DeskCat.FindIt.Scripts.Core.Model;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimeChallengeManager : MMSingleton<TimeChallengeManager>
{
    [Header("Rabbit Objects")]
    public HiddenObj[] RabbitObjs; // 기존 방식 (수동 배치)
    public GameObject RabbitPrefab; // 새로운 방식 (랜덤 생성)

    [Header("Random Rabbit Generation")]
    public bool UseRandomGeneration = true;
    public int RandomRabbitCount = 20;
    public Vector2 LeftTopCorner = new Vector2(-26.32f, 8.18f);     // 좌상단 꼭지점
    public Vector2 RightBottomCorner = new Vector2(12.35979f, -5.788644f); // 우하단 꼭지점
    public float MinDistanceBetweenRabbits = 1.2f;
    public int MaxGenerationAttempts = 200;

    [Header("Random Rabbit Transform")]
    public Vector2 ScaleRange = new Vector2(0.8f, 1.2f);  // 최소, 최대 크기
    public Vector2 RotationRange = new Vector2(-15f, 15f); // 최소, 최대 회전각도
    public bool MirrorRandomly = true;  // X축 반전 여부를 랜덤하게 적용

    [Header("Background Color Matching")]
    public bool MatchBackgroundColor = true; // 배경 색상 매칭 활성화
    public float ColorSamplingRadius = 0.5f; // 색상 샘플링 반경
    [Range(0f, 1f)]
    public float ColorBlendStrength = 0.8f; // 배경 색상 블렌딩 강도 (0=원본, 1=완전 배경색)
    [Range(0f, 2f)]
    public float ColorBrightness = 1.2f; // 토끼 색상 밝기 조정
    [Range(0f, 2f)]
    public float ColorSaturation = 0.7f; // 토끼 색상 채도 조정

    [Header("Default Background Animation")]
    public GameObject DefaultBgAnimation;

    [Header("Timer Settings")]
    public float TimeLimit = 60f; // 제한 시간 (초)

    [Header("Scroll View Options")]
    public UIScrollType UIScrollType;
    public Button ToggleBtn;
    public GameObject TargetImagePrefab;
    public HiddenScrollView HorizontalScrollView;
    public HiddenScrollView VerticalScrollView;
    private HiddenScrollView CurrentScrollView;
    public UnityEvent UIClickEvent;

    [Header("Sound Effects")]
    public AudioSource FoundFx;
    public AudioSource ItemFx;
    public AudioSource TimeUpFx;

    [Header("Game End UI")]
    public GameObject GameEndUI;
    public Button GameEndBtn;
    public Text GameTimeText;
    public Text CurrentFoundObjCountText;
    public Text FoundRabbitCountText;
    public TextMeshProUGUI StageCompleteText;
    public List<Transform> StarList = new List<Transform>();
    
    [Header("Ad Reward UI")]
    public Button WatchAdButton;  // 광고 시청 버튼
    public GameObject AdRewardUI; // 광고 보상 UI

    [Header("Level Settings")]
    public string CurrentLevelName;
    public string NextLevelName;
    public UnityEvent GameClearEvent;
    public UnityEvent GameOverEvent;

    // Private variables
    private Dictionary<Guid, HiddenObj> rabbitObjDic;
    private List<GameObject> generatedRabbits = new List<GameObject>(); // 생성된 토끼들 추적
    private int foundRabbitCount = 0;
    private int totalRabbitCount = 0;
    private bool isGameActive = true;
    private TimeChallengeViewModel viewModel;
    private DateTime startTime;
    private DateTime endTime;
    private bool hasUsedAdReward = false; // 광고 보상 사용 여부

    // Events
    public EventHandler<HiddenObj> OnFoundRabbit;
    public EventHandler OnGameEnd;
    private SpriteRenderer[] allRenderers;

    public static void PlayItemFx(AudioClip clip)
    {
        if (Instance.ItemFx != null)
        {
            if (clip != null) Instance.ItemFx.clip = clip;
            Instance.ItemFx.Play();
        }
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        int layerMask = ~(LayerMask.GetMask("UI") | LayerMask.GetMask("BackGroundExclude"));
        allRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where(renderer => ((1 << renderer.gameObject.layer) & layerMask) != 0)
            .ToArray();


        foundRabbitCount = 0;
        isGameActive = true;
        hasUsedAdReward = false; // 광고 보상 사용 여부 초기화
        startTime = DateTime.Now;

        viewModel = FindAnyObjectByType<TimeChallengeViewModel>();

        BuildRabbitDictionary();
        ScrollViewTrigger();

        if (ToggleBtn != null)
        {
            ToggleBtn.onClick.AddListener(ToggleScrollView);
        }

        if (GameEndBtn != null)
        {
            GameEndBtn.onClick.AddListener(() => { LoadNextLevel(); });
        }
        
        if (WatchAdButton != null)
        {
            WatchAdButton.onClick.AddListener(WatchAdForTimeBonus);
        }
    }

    private void BuildRabbitDictionary()
    {
        rabbitObjDic = new Dictionary<Guid, HiddenObj>();

        if (UseRandomGeneration && RabbitPrefab != null)
        {
            GenerateRandomRabbits();
        }
        else
        {
            if (RabbitObjs != null && RabbitObjs.Length > 0)
            {
                foreach (var rabbit in RabbitObjs)
                {
                    if (rabbit != null)
                    {
                        AddRabbitToDictionary(rabbit);
                    }
                }
            }
        }

        totalRabbitCount = rabbitObjDic.Count;
    }

    private void GenerateRandomRabbits()
    {
        List<Vector2> usedPositions = new List<Vector2>();
        Vector2 minBounds, maxBounds;

        minBounds = new Vector2(
            Mathf.Min(LeftTopCorner.x, RightBottomCorner.x),
            Mathf.Min(LeftTopCorner.y, RightBottomCorner.y)
        );
        maxBounds = new Vector2(
            Mathf.Max(LeftTopCorner.x, RightBottomCorner.x),
            Mathf.Max(LeftTopCorner.y, RightBottomCorner.y)
        );

        float mapWidth = maxBounds.x - minBounds.x;
        float mapHeight = maxBounds.y - minBounds.y;
        float mapArea = mapWidth * mapHeight;

        float rabbitArea = Mathf.PI * (MinDistanceBetweenRabbits * MinDistanceBetweenRabbits);
        float maxPossibleRabbits = mapArea / rabbitArea * 0.6f;

        int actualRabbitCount = Mathf.Min(RandomRabbitCount, Mathf.FloorToInt(maxPossibleRabbits));

        int successCount = 0;
        int failCount = 0;

        for (int i = 0; i < RandomRabbitCount && successCount < actualRabbitCount; i++)
        {
            Vector2 randomPosition = GenerateValidPosition(usedPositions, minBounds, maxBounds);

            if (randomPosition != Vector2.zero)
            {
                GameObject rabbitObj = Instantiate(RabbitPrefab, randomPosition, Quaternion.identity);
                rabbitObj.name = $"Rabbit_{successCount + 1}";

                float randomScale = UnityEngine.Random.Range(ScaleRange.x, ScaleRange.y);
                rabbitObj.transform.localScale = new Vector3(randomScale, randomScale, 1f);

                float randomRotation = UnityEngine.Random.Range(RotationRange.x, RotationRange.y);
                rabbitObj.transform.rotation = Quaternion.Euler(0f, 0f, randomRotation);

                if (MirrorRandomly && UnityEngine.Random.value > 0.5f)
                {
                    Vector3 scale = rabbitObj.transform.localScale;
                    scale.x *= -1;
                    rabbitObj.transform.localScale = scale;
                }

                HiddenObj hiddenObj = rabbitObj.GetComponent<HiddenObj>();
                if (hiddenObj == null)
                {
                    hiddenObj = rabbitObj.AddComponent<HiddenObj>();
                }

                var spriteRenderer = hiddenObj.spriteRenderer;
                if (spriteRenderer != null)
                {
                    spriteRenderer.sortingLayerName = "Object";
                    spriteRenderer.sortingOrder = 100;
                }

                if (!rabbitObj.TryGetComponent<BoxCollider2D>(out var collider))
                {
                    collider = rabbitObj.AddComponent<BoxCollider2D>();
                    collider.isTrigger = true;
                }

                if (DefaultBgAnimation != null)
                {
                    var bgObj = Instantiate(DefaultBgAnimation, hiddenObj.transform);
                    hiddenObj.BgAnimationTransform = bgObj.transform;
                    hiddenObj.SetBgAnimation(bgObj);
                }

                ApplyBackgroundColorToRabbit(hiddenObj, randomPosition);

                AddRabbitToDictionary(hiddenObj);
                usedPositions.Add(randomPosition);

                generatedRabbits.Add(rabbitObj);

                successCount++;
            }
            else
            {
                failCount++;

                if (failCount >= 5 && MinDistanceBetweenRabbits > 0.5f)
                {
                    MinDistanceBetweenRabbits *= 0.8f;
                    failCount = 0;
                }
            }
        }
    }

    private Vector2 GenerateValidPosition(List<Vector2> usedPositions, Vector2 minBounds, Vector2 maxBounds)
    {
        float mapWidth = maxBounds.x - minBounds.x;
        float mapHeight = maxBounds.y - minBounds.y;

        int gridCols = Mathf.Max(3, Mathf.FloorToInt(mapWidth / (MinDistanceBetweenRabbits * 3)));
        int gridRows = Mathf.Max(3, Mathf.FloorToInt(mapHeight / (MinDistanceBetweenRabbits * 3)));

        for (int attempt = 0; attempt < MaxGenerationAttempts; attempt++)
        {
            Vector2 randomPos;

            if (attempt < MaxGenerationAttempts * 0.7f)
            {
                int targetCellIndex = usedPositions.Count % (gridCols * gridRows);
                int cellX = targetCellIndex % gridCols;
                int cellY = targetCellIndex / gridCols;

                float cellWidth = mapWidth / gridCols;
                float cellHeight = mapHeight / gridRows;

                float offsetX = UnityEngine.Random.Range(-cellWidth * 0.4f, cellWidth * 0.4f);
                float offsetY = UnityEngine.Random.Range(-cellHeight * 0.4f, cellHeight * 0.4f);

                randomPos = new Vector2(
                    minBounds.x + (cellX + 0.5f) * cellWidth + offsetX,
                    minBounds.y + (cellY + 0.5f) * cellHeight + offsetY
                );
            }
            else if (attempt < MaxGenerationAttempts * 0.9f)
            {
                Vector2 bestPos = Vector2.zero;
                float maxMinDistance = 0;

                for (int i = 0; i < 20; i++)
                {
                    Vector2 candidatePos = new Vector2(
                        UnityEngine.Random.Range(minBounds.x, maxBounds.x),
                        UnityEngine.Random.Range(minBounds.y, maxBounds.y)
                    );

                    float minDistanceToOthers = float.MaxValue;
                    foreach (var usedPos in usedPositions)
                    {
                        float distance = Vector2.Distance(candidatePos, usedPos);
                        if (distance < minDistanceToOthers)
                        {
                            minDistanceToOthers = distance;
                        }
                    }

                    if (minDistanceToOthers > maxMinDistance)
                    {
                        maxMinDistance = minDistanceToOthers;
                        bestPos = candidatePos;
                    }
                }

                randomPos = bestPos;
            }
            else
            {
                randomPos = new Vector2(
                    UnityEngine.Random.Range(minBounds.x, maxBounds.x),
                    UnityEngine.Random.Range(minBounds.y, maxBounds.y)
                );
            }

            randomPos.x = Mathf.Clamp(randomPos.x, minBounds.x, maxBounds.x);
            randomPos.y = Mathf.Clamp(randomPos.y, minBounds.y, maxBounds.y);

            bool isValidPosition = true;
            float minDistanceSquared = MinDistanceBetweenRabbits * MinDistanceBetweenRabbits;

            foreach (var usedPos in usedPositions)
            {
                float distanceSquared = (randomPos - usedPos).sqrMagnitude;
                if (distanceSquared < minDistanceSquared)
                {
                    isValidPosition = false;
                    break;
                }
            }

            if (isValidPosition)
            {
                return randomPos;
            }
        }

        return Vector2.zero;
    }

    private void ApplyBackgroundColorToRabbit(HiddenObj hiddenObj, Vector3 position)
    {
        if (!MatchBackgroundColor || hiddenObj.spriteRenderer == null)
            return;

        try
        {
            // 콜라이더 대신 직접 렌더러를 검색하여 배경 색상 찾기
            SpriteRenderer backgroundRenderer = null;

            int layerMask = ~(LayerMask.GetMask("UI") | LayerMask.GetMask("BackGroundExclude"));

            // allRenderers 배열을 사용하여 배경 렌더러 찾기
            var validRenderers = new List<SpriteRenderer>();

            foreach (var renderer in allRenderers)
            {
                if (renderer == null ||
                    renderer.gameObject == hiddenObj.gameObject ||
                    !renderer.gameObject.activeInHierarchy ||
                    renderer.sortingLayerName == "UI" ||
                    renderer.sortingLayerName == "Object" ||
                    renderer.GetComponent<HiddenObj>() != null) // HiddenObj가 있는 것은 제외
                    continue;

                // 렌더러가 해당 위치를 포함하는지 확인
                Bounds bounds = renderer.bounds;
                if (bounds.Contains(position))
                {
                    validRenderers.Add(renderer);
                }
            }

            // sortingOrder를 고려하여 가장 위에 있는(높은 order) 배경 렌더러 선택
            if (validRenderers.Count > 0)
            {
                backgroundRenderer = validRenderers
                    .OrderBy(r => r.sortingOrder) // 높은 sortingOrder 우선
                    .ThenBy(r => Vector3.Distance(position, r.transform.position)) // 같은 order면 거리순
                    .First();
            }

            if (backgroundRenderer != null)
            {
                Color backgroundColor = SampleColorFromSprite(backgroundRenderer, position);
                hiddenObj.spriteRenderer.color = backgroundColor;
            }
            else
            {
                Color whiteColor = Color.white;
                whiteColor.a = 0.4f;
                hiddenObj.spriteRenderer.color = whiteColor;
            }
        }
        catch (System.Exception)
        {
            if (hiddenObj.spriteRenderer != null)
            {
                Color whiteColor = Color.white;
                whiteColor.a = 0.4f;
                hiddenObj.spriteRenderer.color = whiteColor;
            }
        }
    }

    private Color SampleColorFromSprite(SpriteRenderer spriteRenderer, Vector3 worldPosition)
    {
        try
        {
            Sprite sprite = spriteRenderer.sprite;
            if (sprite == null || sprite.texture == null)
            {
                Color whiteColor = Color.white;
                whiteColor.a = 0.4f;
                return whiteColor;
            }

            if (!sprite.texture.isReadable)
            {
                Color whiteColor = Color.white;
                whiteColor.a = 0.4f;
                return whiteColor;
            }

            Vector3 localPosition = spriteRenderer.transform.InverseTransformPoint(worldPosition);

            Bounds spriteBounds = sprite.bounds;
            Vector2 spriteSize = spriteBounds.size;

            Vector2 normalizedPosition = new Vector2(
                (localPosition.x / spriteSize.x) + 0.5f,
                (localPosition.y / spriteSize.y) + 0.5f
            );

            Rect spriteRect = sprite.textureRect;
            int pixelX = Mathf.RoundToInt(spriteRect.x + normalizedPosition.x * spriteRect.width);
            int pixelY = Mathf.RoundToInt(spriteRect.y + normalizedPosition.y * spriteRect.height);

            pixelX = Mathf.Clamp(pixelX, (int)spriteRect.x, (int)(spriteRect.x + spriteRect.width - 1));
            pixelY = Mathf.Clamp(pixelY, (int)spriteRect.y, (int)(spriteRect.y + spriteRect.height - 1));

            Color pixelColor = sprite.texture.GetPixel(pixelX, pixelY);

            Color finalColor = pixelColor * spriteRenderer.color;

            // #000000 (완전한 검은색)인 경우 흰색으로 변경하고 알파값 0.4 적용
            if (finalColor.r == 0f && finalColor.g == 0f && finalColor.b == 0f)
            {
                finalColor = Color.white;
                finalColor.a = 0.4f;
                return finalColor;
            }

            finalColor.a = 1.0f;

            return finalColor;
        }
        catch (System.Exception)
        {
            Color whiteColor = Color.white;
            whiteColor.a = 0.4f;
            return whiteColor;
        }
    }

    private void AddRabbitToDictionary(HiddenObj rabbit)
    {
        // UIChangeHelper 컴포넌트가 있다면 HiddenObj에 연결
        if (rabbit.uiChangeHelper == null)
        {
            rabbit.uiChangeHelper = rabbit.GetComponent<UIChangeHelper>();
        }
        
        Guid guid = Guid.NewGuid();
        rabbitObjDic.Add(guid, rabbit);

        rabbit.TargetClickAction = () => { OnRabbitClick(guid); };

        rabbit.IsFound = false;
    }

    private void OnRabbitClick(Guid guid)
    {
        if (!isGameActive) return;

        var rabbit = rabbitObjDic[guid];

        FoundRabbitAction(guid, rabbit);
    }

    private void FoundRabbitAction(Guid guid, HiddenObj rabbit)
    {
        if (rabbit.PlaySoundWhenFound && FoundFx != null)
        {
            FoundFx.Play();
        }

        rabbit.IsFound = true;
        foundRabbitCount++;

        rabbitObjDic.Remove(guid);

        ScrollViewTrigger();

        OnFoundRabbit?.Invoke(this, rabbit);

        CheckGameClear();
    }

    private void CheckGameClear()
    {
        if (foundRabbitCount >= totalRabbitCount)
        {
            GameClear();
        }
    }

    private void GameClear()
    {
        isGameActive = false;

        ShowGameResult(true);

        GameClearEvent?.Invoke();
        OnGameEnd?.Invoke(this, EventArgs.Empty);
    }

    public void TimeUp()
    {
        isGameActive = false;

        if (TimeUpFx != null)
        {
            TimeUpFx.Play();
        }

        ShowGameResult(false);

        GameOverEvent?.Invoke();
        OnGameEnd?.Invoke(this, EventArgs.Empty);
    }

    private void ShowGameResult(bool isSuccess)
    {
        endTime = DateTime.Now;
        var timeUsed = endTime.Subtract(startTime);

        // LevelManager의 DefaultGameEndFunc()와 동일한 UI 업데이트
        if (CurrentFoundObjCountText != null)
        {
            CurrentFoundObjCountText.text = $"{foundRabbitCount} / {totalRabbitCount}";
        }

        if (FoundRabbitCountText != null)
        {
            FoundRabbitCountText.text = $"{foundRabbitCount} / {totalRabbitCount}";
        }

        if (StageCompleteText != null)
        {
            string resultText = isSuccess ? "CLEAR!" : "TIME UP!";
            StageCompleteText.text = CurrentLevelName + " " + resultText;
        }

        if (GameTimeText != null)
        {
            GameTimeText.text = timeUsed.Hours > 0
                ? timeUsed.ToString(@"hh\:mm\:ss")
                : timeUsed.ToString(@"mm\:ss");
        }

        // 별점 계산 (LevelManager와 동일한 로직)
        var starCount = 0;
        float foundRabbitRatio = (float)foundRabbitCount / totalRabbitCount;

        if (isSuccess)
        {
            if (foundRabbitRatio >= 1.0f) starCount = 3;  // 100% 클리어
            else if (foundRabbitRatio >= 0.8f) starCount = 2;  // 80% 이상
            else if (foundRabbitRatio >= 0.6f) starCount = 1;  // 60% 이상
        }
        else
        {
            if (foundRabbitRatio >= 0.7f) starCount = 2;  // 시간 초과지만 70% 이상
            else if (foundRabbitRatio >= 0.4f) starCount = 1;  // 시간 초과지만 40% 이상
        }

        // 별 활성화
        for (int i = 0; i < StarList.Count && i < starCount; i++)
        {
            StarList[i].gameObject.SetActive(true);
        }

        // Game End UI 활성화
        if (GameEndUI != null)
        {
            GameEndUI.SetActive(true);
        }
        
        // 게임 실패 시 광고 시청 버튼 표시
        if (!isSuccess && WatchAdButton != null && !hasUsedAdReward)
        {
            WatchAdButton.gameObject.SetActive(true);
        }
        else if (WatchAdButton != null)
        {
            WatchAdButton.gameObject.SetActive(false);
        }

        // 기존 ViewModel도 호출 (호환성 유지)
        if (viewModel != null)
        {
            viewModel.ShowGameResult(isSuccess);
        }
    }

    public void RestartGame()
    {
        CleanupGeneratedRabbits();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void CleanupGeneratedRabbits()
    {
        foreach (var rabbit in generatedRabbits)
        {
            if (rabbit != null)
            {          
                DestroyImmediate(rabbit);
            }
        }
        generatedRabbits.Clear();
    }

    private void OnDestroy()
    {
        CleanupGeneratedRabbits();
    }

    public void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(NextLevelName))
        {
            SceneManager.LoadScene(NextLevelName);
        }
    }

    public int GetFoundRabbitCount() => foundRabbitCount;
    public int GetTotalRabbitCount() => totalRabbitCount;
    public bool IsGameActive() => isGameActive;
    public float GetProgress() => (float)foundRabbitCount / totalRabbitCount;

    public void ToggleScrollView()
    {
        UIScrollType = (UIScrollType == UIScrollType.Vertical) ? UIScrollType.Horizontal : UIScrollType.Vertical;
        ScrollViewTrigger();
    }

    private void ScrollViewTrigger()
    {
        if (HorizontalScrollView == null || VerticalScrollView == null) return;

        CurrentScrollView = UIScrollType == UIScrollType.Horizontal ? HorizontalScrollView : VerticalScrollView;
        HorizontalScrollView.mainPanel.SetActive(false);
        VerticalScrollView.mainPanel.SetActive(false);
        CurrentScrollView.Initialize();

        var rabbitGroups = ConvertRabbitDictToGroups();
        var createdUIs = CurrentScrollView.UpdateScrollView(rabbitGroups, TargetImagePrefab, RabbitClick, RabbitRegionToggle, UIClick);
        
        // LevelManager에 생성된 UI들을 등록하여 다른 모드 매니저들이 접근할 수 있도록 함
        if (LevelManager.Instance != null)
        {
            var levelManagerUIs = LevelManager.Instance.GetAllHiddenObjUIs();
            foreach (var ui in createdUIs)
            {
                if (!levelManagerUIs.Contains(ui))
                {
                    levelManagerUIs.Add(ui);
                }
            }
        }
    }

    private Dictionary<Guid, HiddenObjGroup> ConvertRabbitDictToGroups()
    {
        var groups = new Dictionary<Guid, HiddenObjGroup>();

        foreach (var kvp in rabbitObjDic)
        {
            var rabbit = kvp.Value;
            var group = new HiddenObjGroup(new List<HiddenObj> { rabbit }, rabbit.gameObject.name);
            groups.Add(kvp.Key, group);
        }

        return groups;
    }

    private void RabbitClick(Guid guid)
    {
        OnRabbitClick(guid);
    }

    private void RabbitRegionToggle(Guid guid)
    {
        if (!rabbitObjDic.ContainsKey(guid)) return;

        var rabbit = rabbitObjDic[guid];
        if (rabbit.hiddenObjFoundType != HiddenObjFoundType.Drag) return;

        OnRabbitClick(guid);
    }

    private void UIClick()
    {
        UIClickEvent?.Invoke();
    }

    [ContextMenu("Generate Test Rabbits")]
    public void GenerateTestRabbits()
    {
        if (Application.isPlaying)
        {
            CleanupGeneratedRabbits();
            rabbitObjDic.Clear();
            foundRabbitCount = 0;
            GenerateRandomRabbits();
            totalRabbitCount = rabbitObjDic.Count;
            ScrollViewTrigger();
        }
    }

    [ContextMenu("Clear Generated Rabbits")]
    public void ClearGeneratedRabbits()
    {
        CleanupGeneratedRabbits();
        rabbitObjDic.Clear();
        foundRabbitCount = 0;
        totalRabbitCount = 0;
        ScrollViewTrigger();
    }

    [ContextMenu("Debug Map Info")]
    public void DebugMapInfo()
    {
        Vector2 minBounds = new Vector2(
            Mathf.Min(LeftTopCorner.x, RightBottomCorner.x),
            Mathf.Min(LeftTopCorner.y, RightBottomCorner.y)
        );
        Vector2 maxBounds = new Vector2(
            Mathf.Max(LeftTopCorner.x, RightBottomCorner.x),
            Mathf.Max(LeftTopCorner.y, RightBottomCorner.y)
        );

        float mapWidth = maxBounds.x - minBounds.x;
        float mapHeight = maxBounds.y - minBounds.y;
        float mapArea = mapWidth * mapHeight;

        float rabbitArea = Mathf.PI * (MinDistanceBetweenRabbits * MinDistanceBetweenRabbits);
        float maxPossibleRabbits = mapArea / rabbitArea * 0.6f;
    }

    [ContextMenu("Apply Background Colors to Existing Rabbits")]
    public void ApplyBackgroundColorsToExistingRabbits()
    {
        if (!Application.isPlaying) return;

        foreach (var rabbit in generatedRabbits)
        {
            if (rabbit != null)
            {
                var hiddenObj = rabbit.GetComponent<HiddenObj>();
                if (hiddenObj != null)
                {
                    ApplyBackgroundColorToRabbit(hiddenObj, rabbit.transform.position);
                }
            }
        }

        foreach (var rabbit in RabbitObjs)
        {
            if (rabbit != null)
            {
                ApplyBackgroundColorToRabbit(rabbit, rabbit.transform.position);
            }
        }
    }

    [ContextMenu("Reset Rabbit Colors")]
    public void ResetRabbitColors()
    {
        if (!Application.isPlaying) return;

        foreach (var rabbit in generatedRabbits)
        {
            if (rabbit != null)
            {
                var hiddenObj = rabbit.GetComponent<HiddenObj>();
                if (hiddenObj != null && hiddenObj.spriteRenderer != null)
                {
                    hiddenObj.spriteRenderer.color = Color.white;
                }
            }
        }

        foreach (var rabbit in RabbitObjs)
        {
            if (rabbit != null && rabbit.spriteRenderer != null)
            {
                rabbit.spriteRenderer.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// 광고 시청으로 30초 시간 추가
    /// </summary>
    public void WatchAdForTimeBonus()
    {
        if (hasUsedAdReward || isGameActive)
        {
            Debug.Log("이미 광고 보상을 사용했거나 게임이 진행 중입니다.");
            return;
        }
        
        // 실제로는 광고 시스템과 연동해야 하지만, 여기서는 바로 시간 추가
        // TODO: 광고 시스템 연동
        Debug.Log("광고 시청 완료! 30초 시간 추가");
        
        // 30초 시간 추가
        if (viewModel != null)
        {
            viewModel.AddTime(30f);
        }
        
        // 게임 재시작
        hasUsedAdReward = true;
        isGameActive = true;
        
        // 게임 종료 UI 비활성화
        if (GameEndUI != null)
        {
            GameEndUI.SetActive(false);
        }
        
        // 광고 시청 버튼 비활성화
        if (WatchAdButton != null)
        {
            WatchAdButton.gameObject.SetActive(false);
        }
        
        Debug.Log("게임 재시작! 추가 30초로 계속 진행하세요.");
    }
    
    /// <summary>
    /// 광고 보상 사용 여부 확인
    /// </summary>
    public bool HasUsedAdReward() => hasUsedAdReward;
    
    /// <summary>
    /// 광고 보상 사용 가능 여부 확인
    /// </summary>
    public bool CanUseAdReward() => !hasUsedAdReward && !isGameActive;
}

