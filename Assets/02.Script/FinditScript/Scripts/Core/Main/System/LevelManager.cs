using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeskCat.FindIt.Scripts.Core.Model;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Data;
using OutGame;
using Manager;
using DeskCat.FindIt.Scripts.Core.Main.Utility.Animation;
using DG.Tweening;
using UI;
using UI.Page;
using System.Numerics;

using Sirenix.OdinInspector;
using Sirenix.Serialization;
namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    [Serializable]
    [InlineProperty]
    [HideReferenceObjectPicker]
    public class HiddenObjGroup
    {
        [ShowInInspector]
        [ListDrawerSettings(Expanded = true)]
        public List<HiddenObj> Objects { get; set; }
        [ShowInInspector]
        public int TotalCount => Objects != null ? Objects.Count : 0;
        [ShowInInspector]
        public int FoundCount { get; set; }
        [ShowInInspector]
        public HiddenObj Representative => (Objects != null && Objects.Count > 0) ? Objects[0] : null;
        [ShowInInspector]
        public Dictionary<HiddenObj, bool> ObjectStates { get; private set; }
        [ShowInInspector]
        public HiddenObj LastClickedObject { get; set; }
        [ShowInInspector]
        public string BaseGroupName { get; private set; }

        // UI 연결을 위한 참조 추가
        [ShowInInspector]
        public HiddenObjUI AssociatedUI { get; set; }

        public HiddenObjGroup(List<HiddenObj> objects, string baseGroupName)
        {
            Objects = objects;
            BaseGroupName = baseGroupName;
            FoundCount = 0;
            ObjectStates = new Dictionary<HiddenObj, bool>();
            foreach (var obj in objects)
            {
                ObjectStates[obj] = false;
            }
        }

        public void MarkObjectAsFound(HiddenObj obj)
        {
            if (ObjectStates.ContainsKey(obj) && !ObjectStates[obj])
            {
                ObjectStates[obj] = true;
                LastClickedObject = obj;
                FoundCount++;
                obj.IsFound = true;
            }
        }

        public bool IsObjectFound(HiddenObj obj)
        {
            return ObjectStates.ContainsKey(obj) && ObjectStates[obj];
        }
    }

    public class LevelManager : MMSingleton<LevelManager>
    {
        //* 김일 추가 : 종료 조건에 등록된 함수들 먼저 실행
        public List<Func<UniTask>> OnEndEvent = new List<Func<UniTask>>();  // 비동기 메서드 참조
        //* 김일 추가 : 옵젝 찾으면 전역에 알릴려고 추가함
        public EventHandler<HiddenObj> OnFoundObj;
        public EventHandler OnFoundObjCountChanged;
        [Header("Hidden Object List")]
        [Tooltip("Normal hidden objects parent transform")]
        public Transform normalHiddenObjGroup; // 일반 숨김 오브젝트들의 부모 Transform
        [Header("Default Background Animation")]
        public GameObject DefaultBgAnimation;
        
        [ShowInInspector]
        [ListDrawerSettings(Expanded = true)]
        public HiddenObj[] TargetObjs;
        
        [ShowInInspector]
        [ListDrawerSettings(Expanded = true)]
        public HiddenObj[] RabbitObjs;
        
        public TextMeshProUGUI RabbitCountText;
        public bool IsRandomItem;
        public int MaxRandomItem;

        public GameObject Canvas;

        [Header("UI Visibility (Hide/Show)")]
        [Tooltip("페이드 인/아웃으로 숨김 처리할 CanvasGroup 목록")]
        public CanvasGroup[] UICanvasGroups;
        [Tooltip("UI 페이드 애니메이션 지속 시간")]
        public float UIFadeDuration = 0.3f;
        private bool _isUIVisible = true;
        public bool IsUIVisible => _isUIVisible;

        [Header("Scroll View Options")]
        public UIScrollType UIScrollType;
        public Button ToggleBtn;
        public GameObject TargetImagePrefab;
        public HiddenScrollView HorizontalScrollView;
        public HiddenScrollView VerticalScrollView;
        private HiddenScrollView CurrentScrollView;
        public UnityEvent UIClickEvent;
        public TextMeshProUGUI FoundObjCountText;
        public Image FoundObjCountFillImage;

        [Header("Sound Effect")]
        public AudioSource FoundFx;
        public AudioSource ItemFx;

        [Header("Game End 내용물")]
        public GameObject GameEndUI;
        public Button GameEndBtn;
        public Text GameTimeText;
        public Text CurrentFoundObjCountText;
        public Text FoundRabbitCountText;
        public TextMeshProUGUI StageCompleteText;
        public Sprite coinSprite;

        public List<Transform> StarList = new List<Transform>();

        // 기존 CurrentLevelName, NextLevelName 제거하고 SceneBase에서 자동으로 가져오기
        public bool IsOverwriteGameEnd;
        public UnityEvent GameEndEvent;

        [LabelText("게임 종료 시 아이템 세트 미션 체크 여부")]
        public bool CheckItemSetCondition = true;

        [ShowInInspector]
        [LabelText("Target Object Groups")]
        [PropertySpace(6)]
        [DictionaryDrawerSettings(KeyLabel = "ID", ValueLabel = "Group", DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public Dictionary<Guid, HiddenObjGroup> TargetObjDic = new Dictionary<Guid, HiddenObjGroup>();
        public Dictionary<Guid, HiddenObj> RabbitObjDic = new Dictionary<Guid, HiddenObj>();
        private DateTime StartTime;
        private DateTime EndTime;
        private BigInteger StartCoinAmount; // 스테이지 시작 시 코인 기록용

        private int rabbitObjCount = 0;
        private int maxRabbitObjCount = 0;

        // 새로운 변수 추가
        private List<HiddenObj> normalHiddenObjs = new List<HiddenObj>();

        [ShowInInspector]
        [ReadOnly]
        [ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        private List<HiddenObjUI> allHiddenObjUIs = new List<HiddenObjUI>();

        // ModeSelector 캐싱
        private ModeSelector modeSelector;

        public static void PlayItemFx(AudioClip clip)
        {
            if (clip == null) clip = Instance.ItemFx.clip;
            Instance.ItemFx.clip = clip;
            Instance.ItemFx.Play();
        }

        /// <summary>
        /// 다음 레벨로 이동하는 메서드
        /// </summary>
        private void GoToNextLevel()
        {
            // SceneBase에서 현재 씬 정보 가져오기
            if (Global.CurrentScene != null)
            {
                SceneName currentScene = Global.CurrentScene.SceneName;
                SceneName? nextScene = SceneHelper.GetNextStageScene(currentScene);

                if (nextScene.HasValue)
                {
                    // 다음 스테이지가 있으면 이동
                    string nextSceneName = nextScene.Value.ToString();
                    SceneManager.LoadScene(nextSceneName);
                }
                else
                {
                    // 다음 스테이지가 없으면 선택 화면으로 이동
                    SceneManager.LoadScene("Select");
                }
            }
            else
            {
                // SceneBase 정보가 없으면 선택 화면으로 이동
                SceneManager.LoadScene("Select");
            }
        }

        private void Start()
        {
            // 시작 시 Hidden 태그를 가진 오브젝트들 수집
            CollectHiddenObjects();
            BuildDictionary();
            ScrollViewTrigger();
            DebugGameState();
            // 버튼들 null 체크
            if (ToggleBtn != null)
                ToggleBtn.onClick.AddListener(ToggleScrollView);
            if (GameEndBtn != null)
                GameEndBtn.onClick.AddListener(GoToNextLevel);

            if (Global.CoinManager != null)
            {
                StartCoinAmount = Global.CoinManager.GetCoinValue();
            }

            StartTime = DateTime.Now;

            // 스테이지 시작 시 컬렉션(스티커) 획득 목록 초기화
            if (Global.CollectionManager != null)
            {
                Global.CollectionManager.ClearEarnedThisStage();
            }

            if (Canvas != null)
            {
                Canvas.SetActive(true);
            }

            // 모드 초기화: ModeSelector가 있으면 선택된 모드를 초기화하고,
            // 없으면 기존 동작대로 씬의 아무 ModeManager 하나를 초기화합니다.
            modeSelector = FindAnyObjectByType<ModeSelector>();
            if (modeSelector != null)
            {
                modeSelector.InitializeSelectedMode();
            }

            // 초기화 완료 후 UI 업데이트 알림 (늦게 구독한 리스너들을 위해)
            Debug.Log($"[LevelManager] Initialization complete. TargetObjDic count: {TargetObjDic?.Count ?? 0}");
            OnFoundObjCountChanged?.Invoke(this, EventArgs.Empty);
        }

        private void CollectHiddenObjects()
        {
            if (normalHiddenObjGroup != null)
            {
                // 그룹 내의 모든 자식들을 검사
                Transform[] children = normalHiddenObjGroup.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    // 자기 자신은 제외
                    if (child == normalHiddenObjGroup) continue;

                    // Hidden 태그를 가진 오브젝트 확인
                    if (child.CompareTag("Hidden"))
                    {
                        // if(!child.gameObject.TryGetComponent<SpriteRenderer>(out var sr))
                        // {
                        //     // Debug.LogWarning($"[LevelManager] Object {child.name} has 'Hidden' tag but no SpriteRenderer component found. Skipping this object.");
                        //     continue; // SpriteRenderer가 없는 오브젝트는 건너뛰기
                        // }
                        // HiddenObj 컴포넌트가 없다면 추가
                        HiddenObj hiddenObj = null;
                        if (!child.TryGetComponent<HiddenObj>(out hiddenObj))
                        {
                            try
                            {
                                hiddenObj = child.gameObject.AddComponent<HiddenObj>();
                                // Debug.Log($"[LevelManager] Successfully added HiddenObj to {child.name}");
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"[LevelManager] Failed to add HiddenObj to {child.name}: {e.Message}\n{e.StackTrace}");
                                continue; // 이 오브젝트는 건너뛰고 다음으로
                            }
                        }

                        // hiddenObj가 null이면 건너뛰기
                        if (hiddenObj == null)
                        {
                            Debug.LogError($"[LevelManager] hiddenObj is null for {child.name}, skipping...");
                            continue;
                        }

                        // hideWhenFound 클래스가 있다면 여기의 설정을 HiddenObj 에 덮어쓰기
                        HideWhenFoundHelper hideWhenFoundHelper = null;
                        if (child.TryGetComponent(out hideWhenFoundHelper))
                        {
                            hiddenObj.HideWhenFound = hideWhenFoundHelper.hideWhenFound;
                        }

                        // UIChangeHelper 컴포넌트가 있다면 HiddenObj에 연결
                        if (hiddenObj.uiChangeHelper == null)
                        {
                            hiddenObj.uiChangeHelper = child.GetComponent<UIChangeHelper>();
                        }

                        // WhenFoundEventHelper 컴포넌트가 있다면 HiddenObj에 연결
                        if (hiddenObj.whenFoundEventHelper == null)
                        {
                            hiddenObj.whenFoundEventHelper = child.GetComponent<WhenFoundEventHelper>();
                        }

                        // BoxCollider2D 추가 또는 리셋
                        if (!child.TryGetComponent<BoxCollider2D>(out var boxCollider))
                        {
                            boxCollider = child.gameObject.AddComponent<BoxCollider2D>();
                        }

                        // 터치 영역을 넓히기 위해 콜라이더 사이즈 조정
                        boxCollider.size = new UnityEngine.Vector2(boxCollider.size.x * 1.5f, boxCollider.size.y * 1.5f);

                        // 배경 애니메이션 설정
                        // BGAnimationHelper가 있으면 해당 설정을 우선 적용
                        BGAnimationHelper bgAnimHelper = child.GetComponent<BGAnimationHelper>();
                        bool useBgAnim = bgAnimHelper == null || bgAnimHelper.UseBgAnimation;
                        GameObject bgAnimPrefab = bgAnimHelper != null && bgAnimHelper.CustomBgAnimationPrefab != null
                            ? bgAnimHelper.CustomBgAnimationPrefab
                            : DefaultBgAnimation;

                        // BG Object 생성 및 설정 (UseBgAnimation이 false면 스킵)
                        if (useBgAnim && bgAnimPrefab != null)
                        {
                            GameObject bgObj = null;
                            if (hiddenObj.BgAnimationTransform == null)
                            {
                                bgObj = Instantiate(bgAnimPrefab, hiddenObj.transform);
                                // Debug.Log($"Added BGAnimation to {hiddenObj.gameObject.name}" +
                                // (bgAnimHelper != null ? " (custom)" : " (default)"));
                                hiddenObj.BgAnimationTransform = bgObj.transform;
                                hiddenObj.SetBgAnimation(bgObj);
                            }
                            else bgObj = hiddenObj.BgAnimationTransform.gameObject;
                            BGScaleLerp bGScaleLerp = bgObj.GetComponent<BGScaleLerp>();
                            if (bGScaleLerp != null)
                                if (hideWhenFoundHelper != null)
                                    bGScaleLerp.HideHiddenObjAfterDone = hideWhenFoundHelper.hideWhenFound;
                        }

                        // Debug.Log($"Added HiddenObj component and BoxCollider2D to {child.name}");
                        normalHiddenObjs.Add(hiddenObj);
                    }
                }
                // Debug.Log($"Found and processed {normalHiddenObjs.Count} hidden objects in group");
            }
            else
            {
                // 그룹이 지정되지 않은 경우 씬 전체에서 태그로 검색
                GameObject[] hiddenObjects = GameObject.FindGameObjectsWithTag("Hidden");
                foreach (GameObject obj in hiddenObjects)
                {
                    if (!obj.TryGetComponent<HiddenObj>(out HiddenObj hiddenObj))
                    {
                        hiddenObj = obj.AddComponent<HiddenObj>();
                        // Debug.Log($"Added HiddenObj component to {obj.name}");
                    }

                    // UIChangeHelper 컴포넌트가 있다면 HiddenObj에 연결
                    if (hiddenObj.uiChangeHelper == null)
                    {
                        hiddenObj.uiChangeHelper = obj.GetComponent<UIChangeHelper>();
                    }

                    // WhenFoundEventHelper 컴포넌트가 있다면 HiddenObj에 연결
                    if (hiddenObj.whenFoundEventHelper == null)
                    {
                        hiddenObj.whenFoundEventHelper = obj.GetComponent<WhenFoundEventHelper>();
                    }

                    normalHiddenObjs.Add(hiddenObj);
                }
                // Debug.Log($"Found and processed {normalHiddenObjs.Count} hidden objects in scene with tag");
            }
        }

        public int GetLeftHiddenObjCount() => TargetObjDic?.Sum(x => x.Value.TotalCount - x.Value.FoundCount) ?? 0;

        public int GetTotalHiddenObjCount() => TargetObjDic?.Sum(x => x.Value.TotalCount) ?? 0;

        public void AddHiddenObject(HiddenObj hiddenObj)
        {
            Debug.Log("time");
            var group = new HiddenObjGroup(new List<HiddenObj> { hiddenObj }, hiddenObj.gameObject.name);
            TargetObjDic.Add(Guid.NewGuid(), group);
            ScrollViewTrigger();
        }

        public void ToggleScrollView()
        {
            if (TargetObjDic == null)
            {
                Debug.LogWarning("[LevelManager] TargetObjDic is null. Cannot toggle scroll view.");
                return;
            }

            UIScrollType = (UIScrollType == UIScrollType.Vertical) ? UIScrollType.Horizontal : UIScrollType.Vertical;
            ScrollViewTrigger();
        }

        private void ScrollViewTrigger()
        {
            // TargetObjDic 초기화 확인
            if (TargetObjDic == null)
            {
                Debug.LogWarning("[LevelManager] TargetObjDic is null. Skipping ScrollViewTrigger.");
                return;
            }

            // ScrollView들이 null인지 체크
            if (HorizontalScrollView == null || VerticalScrollView == null)
            {
                Debug.LogWarning("[LevelManager] HorizontalScrollView or VerticalScrollView is null");
                return;
            }

            CurrentScrollView = UIScrollType == UIScrollType.Horizontal ? HorizontalScrollView : VerticalScrollView;

            // mainPanel null 체크
            if (HorizontalScrollView.mainPanel != null)
                HorizontalScrollView.mainPanel.SetActive(false);
            if (VerticalScrollView.mainPanel != null)
                VerticalScrollView.mainPanel.SetActive(false);

            // CurrentScrollView null 체크
            if (CurrentScrollView != null)
            {
                CurrentScrollView.Initialize();
                var createdUIs = CurrentScrollView.UpdateScrollView(TargetObjDic, TargetImagePrefab, TargetClick, RegionToggle, UIClick);

                // 생성된 UI들을 LevelManager에서 관리
                allHiddenObjUIs.Clear();
                allHiddenObjUIs.AddRange(createdUIs);

                // 그룹과 UI 연결 (Dictionary의 순서와 UI 리스트의 순서가 일치)
                var groupList = TargetObjDic.Values.ToList();
                for (int i = 0; i < Math.Min(groupList.Count, createdUIs.Count); i++)
                {
                    groupList[i].AssociatedUI = createdUIs[i];
                }

                // 시각적 정렬: 이미 모두 찾은(완료된) 그룹들의 UI는 리스트의 마지막으로 보냅니다.
                // contentContainer가 존재할 때만 순서를 변경합니다.
                if (CurrentScrollView.contentContainer != null)
                {
                    // pair list 생성 (group, ui)
                    var pairs = new List<(HiddenObjGroup group, HiddenObjUI ui)>();
                    for (int i = 0; i < Math.Min(groupList.Count, createdUIs.Count); i++)
                    {
                        pairs.Add((groupList[i], createdUIs[i]));
                    }

                    // 완성 여부 기준으로 정렬: 미완성(앞), 완성(뒤). OrderBy는 안정 정렬이므로 기존 순서 보존.
                    var sorted = pairs.OrderBy(p => p.group.FoundCount >= p.group.TotalCount ? 1 : 0).ToList();

                    // sibling index를 재설정하여 contentContainer 내의 시각적 순서를 변경
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        var uiTransform = sorted[i].ui != null ? sorted[i].ui.transform : null;
                        if (uiTransform != null)
                        {
                            uiTransform.SetSiblingIndex(i);
                        }
                    }

                    // LevelManager에서 관리하는 UI 리스트도 새 순서로 갱신
                    allHiddenObjUIs = sorted.Select(p => p.ui).ToList();
                }

                Debug.Log($"[LevelManager] ScrollView UI 업데이트 완료: {allHiddenObjUIs.Count}개의 HiddenObjUI 생성 및 그룹 연결");
            }
        }

        private void UIClick()
        {
            UIClickEvent?.Invoke();
        }

        /// <summary>
        /// CoinRushModeManager의 코인들을 TargetObjDic에 포함
        /// (미리 세팅된 코인 또는 시작시 생성된 코인)
        /// </summary>
        private void IncludeCoinRushCoins()
        {
            // ModeSelector를 통해 현재 모드가 COIN_RUSH인지 확인
            if (modeSelector != null && modeSelector.selectedMode == ModeManager.GameMode.COIN_RUSH)
            {
                // CoinRushModeManager 찾기
                var coinRushManager = FindAnyObjectByType<CoinRushModeManager>();
                if (coinRushManager != null && coinRushManager.ShouldIncludeCoinsInLevelManager())
                {
                    var coinDic = coinRushManager.GetCoinDictionary();
                    if (coinDic != null && coinDic.Count > 0)
                    {
                        Debug.Log($"[LevelManager] Including {coinDic.Count} coins from CoinRushModeManager");

                        // 각 코인을 개별 그룹으로 추가 (TimeChallengeManager 방식)
                        foreach (var kvp in coinDic)
                        {
                            var coinObj = kvp.Value;
                            if (coinObj != null)
                            {
                                // BGAnimation 처리
                                if (DefaultBgAnimation != null)
                                {
                                    // 이미 BGAnimation이 있는지 확인
                                    if (coinObj.BgAnimationTransform == null)
                                    {
                                        GameObject bgObj = Instantiate(DefaultBgAnimation, coinObj.transform);
                                        coinObj.BgAnimationTransform = bgObj.transform;
                                        coinObj.SetBgAnimation(bgObj);

                                        Debug.Log($"[LevelManager] Added BGAnimation to coin: {coinObj.gameObject.name}");
                                    }
                                }

                                var group = new HiddenObjGroup(
                                    new List<HiddenObj> { coinObj },
                                    coinObj.gameObject.name
                                );
                                TargetObjDic.Add(kvp.Key, group);
                            }
                        }

                        Debug.Log($"[LevelManager] Total objects after including coins: {TargetObjDic.Count}");
                    }
                }
            }
        }

        private void BuildDictionary()
        {
            TargetObjDic = new Dictionary<Guid, HiddenObjGroup>();

            // TargetObjs null 체크 및 추가
            if (TargetObjs != null && TargetObjs.Length > 0)
            {
                normalHiddenObjs.AddRange(TargetObjs);
            }

            // CoinRushModeManager의 미리 세팅된 코인들을 포함
            IncludeCoinRushCoins();

            var groupedObjects = normalHiddenObjs
                .Where(obj => obj != null)
                .Distinct()
                .GroupBy(obj => InGameObjectNameFilter.GetBaseGroupName(obj.gameObject.name))
                .ToDictionary(g => g.Key, g => g.ToList());

            // Debug.Log($"Grouped objects: {string.Join(", ", groupedObjects.Keys)}");

            foreach (var group in groupedObjects)
            {
                if (group.Value.Count > 0)
                {
                    var hiddenObjGroup = new HiddenObjGroup(group.Value, group.Key);
                    TargetObjDic.Add(Guid.NewGuid(), hiddenObjGroup);
                    // Debug.Log($"Added {group.Key} to target dictionary with {group.Value.Count} similar objects");

                    // 각 오브젝트에 클릭 이벤트 설정
                    foreach (var obj in group.Value)
                    {
                        var guid = TargetObjDic.First(x => x.Value.Objects.Contains(obj)).Key;
                        obj.TargetClickAction = () =>
                        {
                            var targetGroup = TargetObjDic[guid];
                            targetGroup.LastClickedObject = obj;
                            TargetClick(guid);
                        };
                    }
                }
            }

            RabbitObjDic = new Dictionary<Guid, HiddenObj>();

            // RabbitObjs null 체크
            if (RabbitObjs != null)
            {
                foreach (var rabbit in RabbitObjs)
                {
                    if (rabbit != null)
                    {
                        Guid guid = Guid.NewGuid();
                        RabbitObjDic.Add(guid, rabbit);

                        rabbit.TargetClickAction = () => { TargetClick(guid); };
                    }
                }
            }

            maxRabbitObjCount = RabbitObjDic.Count;
            rabbitObjCount = 0;

            // RabbitCountText null 체크
            if (RabbitCountText != null)
                RabbitCountText.text = $"{rabbitObjCount}/{maxRabbitObjCount}";
            if (!IsRandomItem) return;

            var randomIndex = new List<int>();
            for (var i = 0; i < MaxRandomItem; i++)
            {
                var index = Random.Range(0, TargetObjDic.Count - 1);
                while (randomIndex.Contains(index))
                {
                    index = Random.Range(0, TargetObjDic.Count - 1);
                }
                randomIndex.Add(index);
            }

            var tempDic = new Dictionary<Guid, HiddenObjGroup>();
            foreach (var index in randomIndex)
            {
                var item = TargetObjDic.ElementAt(index);
                tempDic.Add(item.Key, item.Value);
            }

            TargetObjDic = tempDic;
        }

        private void TargetClick(Guid guid)
        {
            if (TargetObjDic == null)
            {
                Debug.LogWarning("[LevelManager] TargetClick called but TargetObjDic is not initialized yet.");
                return;
            }

            if (TargetObjDic.ContainsKey(guid))
            {
                if (TargetObjDic[guid].Representative.hiddenObjFoundType != HiddenObjFoundType.Click) return;

                FoundObjAction(guid);
            }
            else if (RabbitObjDic.ContainsKey(guid))
            {
                if (RabbitObjDic[guid].hiddenObjFoundType != HiddenObjFoundType.Click) return;

                FoundRabbitObjAction(guid);
            }

        }

        private void RegionToggle(Guid guid)
        {
            if (TargetObjDic == null)
            {
                Debug.LogWarning("[LevelManager] RegionToggle called but TargetObjDic is not initialized yet.");
                return;
            }

            if (!TargetObjDic.ContainsKey(guid)) return;

            if (TargetObjDic[guid].Representative.hiddenObjFoundType != HiddenObjFoundType.Drag) return;

            FoundObjAction(guid);
        }

        public void FoundObjAction(Guid guid)
        {
            if (TargetObjDic == null)
            {
                Debug.LogWarning("[LevelManager] FoundObjAction called but TargetObjDic is not initialized yet.");
                return;
            }

            if (!TargetObjDic.ContainsKey(guid))
            {
                Debug.LogWarning($"[LevelManager] FoundObjAction called with unknown guid: {guid}");
                return;
            }

            var group = TargetObjDic[guid];
            var clickedObj = group.LastClickedObject;

            // 실제로 오브젝트를 찾았을 때만 사운드 재생 및 처리
            if (clickedObj != null && !group.IsObjectFound(clickedObj))
            {
                // 오브젝트를 찾았을 때만 사운드 재생
                if (group.Representative.PlaySoundWhenFound && FoundFx != null)
                    FoundFx.Play();

                group.MarkObjectAsFound(clickedObj);

                // WhenFoundEventHelper 이벤트 호출
                if (clickedObj.whenFoundEventHelper != null)
                {
                    clickedObj.whenFoundEventHelper.onFoundEvent?.Invoke();
                }

                // CurrentScrollView null 체크 및 UI 갱신
                if (CurrentScrollView != null)
                {
                    var createdUIs = CurrentScrollView.UpdateScrollView(TargetObjDic, TargetImagePrefab, TargetClick, RegionToggle, UIClick);

                    // LevelManager에서 관리하는 UI 리스트 갱신
                    allHiddenObjUIs.Clear();
                    allHiddenObjUIs.AddRange(createdUIs);

                    // 그룹과 UI 연결 (기존 순서 기준)
                    var groupList = TargetObjDic.Values.ToList();
                    for (int i = 0; i < Math.Min(groupList.Count, createdUIs.Count); i++)
                    {
                        groupList[i].AssociatedUI = createdUIs[i];
                    }

                    // 시각적 정렬: 이미 모두 찾은(완료된) 그룹들의 UI는 리스트의 마지막으로 보냅니다.
                    if (CurrentScrollView.contentContainer != null)
                    {
                        var pairs = new List<(HiddenObjGroup group, HiddenObjUI ui)>();
                        for (int i = 0; i < Math.Min(groupList.Count, createdUIs.Count); i++)
                        {
                            pairs.Add((groupList[i], createdUIs[i]));
                        }

                        var sorted = pairs.OrderBy(p => p.group.FoundCount >= p.group.TotalCount ? 1 : 0).ToList();

                        for (int i = 0; i < sorted.Count; i++)
                        {
                            var uiTransform = sorted[i].ui != null ? sorted[i].ui.transform : null;
                            if (uiTransform != null)
                            {
                                uiTransform.SetSiblingIndex(i);
                            }

                            // 정렬 후에도 그룹-UI 연결을 최신화
                            sorted[i].group.AssociatedUI = sorted[i].ui;
                        }

                        // LevelManager에서 관리하는 UI 리스트도 새 순서로 갱신
                        allHiddenObjUIs = sorted.Select(p => p.ui).ToList();
                    }
                }

                OnFoundObj?.Invoke(this, clickedObj);

                // ChangeDayObject 컴포넌트가 있으면 Found() 호출
                if (clickedObj.TryGetComponent<ChangeDayObject>(out var changeDayObject))
                {
                    changeDayObject.Found();
                }

                // Notify listeners that count changed and update UI
                OnFoundObjCountChanged?.Invoke(this, EventArgs.Empty);
                UpdateFoundObjUI();

                // Debug.Log($"Found {clickedObj.name} from group {group.BaseGroupName} ({group.FoundCount}/{group.TotalCount})");

                DetectGameEnd();
            }
        }
        private void FoundRabbitObjAction(Guid guid)
        {
            if (RabbitObjDic[guid].PlaySoundWhenFound && FoundFx != null)
                FoundFx.Play();

            RabbitObjDic.Remove(guid);
            rabbitObjCount++;

            // RabbitCountText null 체크
            if (RabbitCountText != null)
                RabbitCountText.text = $"{rabbitObjCount}/{maxRabbitObjCount}";

            // Update overall found count UI as well
            OnFoundObjCountChanged?.Invoke(this, EventArgs.Empty);
            UpdateFoundObjUI();

            DetectGameEnd();
        }

        //* 김일 수정 : 게임 종료 조건 = 숨긴 물건만 찾고 추가 조건은 태스크로 관리
        private async void DetectGameEnd()
        {
            // 코인러쉬 모드에서는 LevelManager 기본 종료 조건을 사용하지 않음
            // (CoinRushModeManager에서 코인을 다 찾거나 시간이 다 됐을 때 별도로 종료 처리)
            if (modeSelector != null && modeSelector.selectedMode == ModeManager.GameMode.COIN_RUSH)
            {
                Debug.Log("[LevelManager] DetectGameEnd skipped - CoinRush mode manages its own end condition.");
                return;
            }

            // 실제 남은 오브젝트 수 계산
            int remainingObjects = GetLeftHiddenObjCount();
            int totalObjects = GetTotalHiddenObjCount();
            int foundObjects = totalObjects - remainingObjects;

            // 디버그 로그 추가
            Debug.Log($"[LevelManager] DetectGameEnd - Remaining: {remainingObjects}, Total: {totalObjects}, Found: {foundObjects}");
            Debug.Log($"[LevelManager] ItemSetManager - Found: {ItemSetManager.Instance?.FoundSetsCount}, Total: {ItemSetManager.Instance?.TotalSetsCount}" + "\n 아이템 매니저 객체" + gameObject.name);

            // DARK 모드인 경우 미션(ItemSet) 검사 제외
            bool isDarkMode = modeSelector != null && modeSelector.selectedMode == ModeManager.GameMode.DARK;
            // ItemSet 조건 체크: CheckItemSetCondition이 false면 무조건 통과, true면 조건 체크
            bool itemSetConditionMet = !CheckItemSetCondition
                || isDarkMode
                || ItemSetManager.Instance == null
                || (ItemSetManager.Instance.FoundSetsCount == ItemSetManager.Instance.TotalSetsCount);

            // 모든 숨겨진 오브젝트를 찾았고, ItemSet 조건도 만족하면 게임 종료
            if (remainingObjects <= 0 && itemSetConditionMet)
            {
                Debug.Log($"[LevelManager] Game End condition met! (DARK 모드: {isDarkMode}) Starting end sequence...");

                if (IsOverwriteGameEnd)
                {
                    // UnityEvent의 모든 리스너가 실행 완료될 때까지 대기
                    if (OnEndEvent.Count > 0)
                    {
                        foreach (var func in OnEndEvent)
                        {
                            Debug.Log("[LevelManager] Awaiting OnEndEvent function..." + func.Method.Name);
                            await func();
                        }
                    }

                    GameEndEvent?.Invoke();  // 모든 UnityEvent 호출이 완료된 뒤에 종료 이벤트 호출
                    DefaultGameEndFunc(); // GameEndUI를 표시하기 위해 DefaultGameEndFunc 호출
                    return;
                }
                // UnityEvent의 모든 리스너가 실행 완료될 때까지 대기
                if (OnEndEvent.Count > 0)
                {
                    foreach (var func in OnEndEvent)
                    {
                        Debug.Log("[LevelManager] Awaiting OnEndEvent function..." + func.Method.Name);
                        await func();
                    }
                }
                Debug.Log("[LevelManager] task 다 끝남");
                GameEndEvent?.Invoke();  // 모든 UnityEvent 호출이 완료된 뒤에 종료 이벤트 호출

                DefaultGameEndFunc();
            }
            else
            {
                Debug.Log($"[LevelManager] Game End condition not met - Remaining objects: {remainingObjects}, ItemSet condition: {itemSetConditionMet} (Check ItemSet: {CheckItemSetCondition}, DARK 모드: {isDarkMode})");
            }
        }


        // 비동기 이벤트 리스너를 기다리는 함수
        public async UniTask InvokeAsync(Func<UniTask> eventHandler)
        {
            if (eventHandler != null)
            {
                await eventHandler.Invoke();  // 비동기 이벤트 호출
            }
        }
        public void DefaultGameEndFunc()
        {
            // 게임 종료 시 UI가 숨겨진 상태면 다시 표시
            if (!_isUIVisible)
            {
                ShowUI();
            }

            EndTime = DateTime.Now;
            var timeUsed = EndTime.Subtract(StartTime);

            // 게임 종료 시 코인 데이터 저장
            if (Global.CoinManager != null)
            {
                Global.CoinManager.SaveCoinData();
            }

            // 현재 씬을 clearedStages에 추가
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(currentSceneName))
            {
                Global.UserDataManager.SetStageClear(currentSceneName);
            }

            int totalObjects = TargetObjDic.Sum(x => x.Value.TotalCount);
            int foundObjects = TargetObjDic.Sum(x => x.Value.FoundCount);

            // 별 계산
            var starCount = 0;
            float foundObjRatio = totalObjects > 0 ? (float)foundObjects / totalObjects : 0f;
            float totalProgress = foundObjRatio;

            if (totalProgress >= 0.9f) starCount = 3;
            else if (totalProgress >= 0.6f) starCount = 2;
            else if (totalProgress >= 0.3f) starCount = 1;

            // 스테이지 이름
            string stageName = "CLEAR!";
            if (Global.CurrentScene != null)
            {
                stageName = SceneHelper.GetFormattedStageName(Global.CurrentScene.SceneName);
            }

            // GameEndPage를 열어서 게임 결과 표시
            if (Global.UIManager != null)
            {
                var gameEndPage = Global.UIManager.OpenPage<GameEndPage>();
                if (gameEndPage != null)
                {
                    gameEndPage.SetGameResult(timeUsed, foundObjects, totalObjects,
                        rabbitObjCount, maxRabbitObjCount, stageName, starCount);

                    // 결과 아이템 목록 생성 (스티커 + 코인)
                    var resultItems = new List<UI.ResultItemData>();

                    // 1. 획득한 스티커(컬렉션) 추가 (중복 획득 시 카운트 합산)
                    if (Global.CollectionManager != null)
                    {
                        var earnedStickers = Global.CollectionManager.GetEarnedThisStage();

                        var groupedStickers = earnedStickers
                            .Where(c => c != null)
                            .GroupBy(c => c)
                            .Select(g => new { Collection = g.Key, Count = g.Count() });

                        foreach (var group in groupedStickers)
                        {
                            resultItems.Add(new UI.ResultItemData(
                                group.Collection.collectionImage,
                                I2.Loc.LocalizationManager.GetTranslation(group.Collection.collectionName),
                                group.Count
                            ));
                        }
                    }

                    // 2. 획득한 코인 추가
                    // 기존에 IngameCoinLayer의 SessionCoinsCollected를 이용하던 방식을 
                    // LevelManager에서 시작할 때 기록한 StartCoinAmount와 현재 코인량 비교로 변경
                    BigInteger currentCoin = Global.CoinManager != null ? Global.CoinManager.GetCoinValue() : global::System.Numerics.BigInteger.Zero;
                    BigInteger gainedCoin = currentCoin - StartCoinAmount;

                    if (gainedCoin > global::System.Numerics.BigInteger.Zero)
                    {
                        Sprite resultCoinSprite = this.coinSprite;

                        // 기존 코인 러쉬 매니저에서의 스프라이트 가져오기 (호환성 유지 및 덮어쓰기)
                        var coinRushManager = FindAnyObjectByType<CoinRushModeManager>();
                        if (coinRushManager != null && coinRushManager.coinSprite != null)
                        {
                            resultCoinSprite = coinRushManager.coinSprite;
                        }

                        resultItems.Add(new UI.ResultItemData(
                            resultCoinSprite,
                            "Coin",
                            (int)gainedCoin
                        ));
                    }

                    gameEndPage.SetResultItems(resultItems);
                    Debug.Log($"[LevelManager] GameEnd - 결과 아이템 수: {resultItems.Count}");
                }
            }
        }

        /// <summary>
        /// 남은 오브젝트 수와 관계없이 즉시 게임 종료 시퀀스를 실행합니다.
        /// CoinRushModeManager 등 외부 모드에서 직접 게임을 끝낼 때 호출하세요.
        /// </summary>
        public async void TriggerGameEnd()
        {
            Debug.Log("[LevelManager] TriggerGameEnd called.");

            if (IsOverwriteGameEnd)
            {
                if (OnEndEvent.Count > 0)
                {
                    foreach (var func in OnEndEvent)
                    {
                        Debug.Log("[LevelManager] Awaiting OnEndEvent function..." + func.Method.Name);
                        await func();
                    }
                }
                GameEndEvent?.Invoke();
                DefaultGameEndFunc();
                return;
            }

            if (OnEndEvent.Count > 0)
            {
                foreach (var func in OnEndEvent)
                {
                    Debug.Log("[LevelManager] Awaiting OnEndEvent function..." + func.Method.Name);
                    await func();
                }
            }

            GameEndEvent?.Invoke();
            DefaultGameEndFunc();
        }

        // 그룹 상태를 확인하기 위한 public 메서드 추가
        public (bool exists, bool isComplete, string baseGroupName) GetGroupStatus(string groupName)
        {
            if (TargetObjDic == null)
            {
                Debug.LogWarning("[LevelManager] TargetObjDic is not initialized yet.");
                return (false, false, string.Empty);
            }

            var group = TargetObjDic.FirstOrDefault(x => x.Value.BaseGroupName == groupName).Value;
            return group != null
                ? (true, group.FoundCount == group.TotalCount, group.BaseGroupName)
                : (false, false, string.Empty);
        }

        // 그룹 이름으로 HiddenObj 목록을 찾는 메서드
        public List<HiddenObj> GetHiddenObjsByGroupName(string groupName)
        {
            if (TargetObjDic == null)
            {
                Debug.LogWarning("[LevelManager] GetHiddenObjsByGroupName called but TargetObjDic is not initialized yet.");
                return new List<HiddenObj>();
            }

            // 씬에서 모든 HiddenObj 컴포넌트를 찾음
            var allHiddenObjs = TargetObjDic.Values.SelectMany(group => group.Objects).ToList();

            // 그룹 이름이 일치하는 HiddenObj들 반환
            return allHiddenObjs
                .Where(obj => InGameObjectNameFilter.GetBaseGroupName(obj.gameObject.name) == groupName)
                .ToList();
        }

        // 안전하게 숨겨진 물건 카운트 UI를 갱신합니다.
        private void UpdateFoundObjUI()
        {
            if (TargetObjDic == null) return;

            int totalObjects = TargetObjDic.Sum(x => x.Value.TotalCount);
            int foundObjects = TargetObjDic.Sum(x => x.Value.FoundCount);

            if (FoundObjCountText != null)
                FoundObjCountText.text = $"{foundObjects} / {totalObjects}";

            if (CurrentFoundObjCountText != null)
                CurrentFoundObjCountText.text = $"{foundObjects} / {totalObjects}";

            if (FoundObjCountFillImage != null)
                FoundObjCountFillImage.fillAmount = totalObjects == 0 ? 0f : (float)foundObjects / totalObjects;
        }

        public string GetBaseGroupName(string objName)
        {
            return InGameObjectNameFilter.GetBaseGroupName(objName);
        }

        /// <summary>
        /// TargetImagePrefab으로 생성된 모든 HiddenObjUI 컴포넌트를 반환합니다.
        /// ScrollViewTrigger에서 자동으로 관리되므로 FindObject를 사용하지 않습니다.
        /// </summary>
        public List<HiddenObjUI> GetAllHiddenObjUIs()
        {
            return allHiddenObjUIs;
        }

        #region UI Visibility (Hide / Show with Fade)

        /// <summary>
        /// UI 표시 상태를 토글합니다. (숨김 ↔ 표시)
        /// </summary>
        public void ToggleUIVisibility()
        {
            if (_isUIVisible)
                HideUI();
            else
                ShowUI();
        }

        [Button("Hide UI")]
        public void HideUI()
        {
            if (!_isUIVisible) return;
            _isUIVisible = false;
            FadeUICanvasGroups(0f, UIFadeDuration);
        }

        [Button("Show UI")]
        public void ShowUI()
        {
            if (_isUIVisible) return;
            _isUIVisible = true;
            FadeUICanvasGroups(1f, UIFadeDuration);
        }

        /// <summary>
        /// 모든 UICanvasGroups의 alpha를 targetAlpha로 페이드합니다.
        /// 페이드 완료 시 interactable/blocksRaycasts도 함께 설정합니다.
        /// </summary>
        private void FadeUICanvasGroups(float targetAlpha, float duration)
        {
            if (UICanvasGroups == null || UICanvasGroups.Length == 0) return;

            foreach (var cg in UICanvasGroups)
            {
                if (cg == null) continue;

                // 진행 중인 트윈 정리
                cg.DOKill();

                // 페이드 시작 전에 보이도록 설정 (Show일 때 즉시 raycast 차단 해제)
                if (targetAlpha > 0f)
                {
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }

                cg.DOFade(targetAlpha, duration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        // Hide 완료 시 상호작용 차단
                        if (targetAlpha <= 0f)
                        {
                            cg.interactable = false;
                            cg.blocksRaycasts = false;
                        }
                    });
            }
        }

        #endregion
        // 디버깅을 위한 게임 상태 확인 메서드
        public void DebugGameState()
        {
            int remainingObjects = GetLeftHiddenObjCount();
            int totalObjects = GetTotalHiddenObjCount();
            int foundObjects = totalObjects - remainingObjects;

            Debug.Log($"[LevelManager] === GAME STATE DEBUG ===");
            Debug.Log($"[LevelManager] Total Objects: {totalObjects}");
            Debug.Log($"[LevelManager] Found Objects: {foundObjects}");
            Debug.Log($"[LevelManager] Remaining Objects: {remainingObjects}");
            Debug.Log($"[LevelManager] Rabbit Count: {rabbitObjCount}/{maxRabbitObjCount}");

            if (ItemSetManager.Instance != null)
            {
                Debug.Log($"[LevelManager] ItemSet - Found: {ItemSetManager.Instance.FoundSetsCount}, Total: {ItemSetManager.Instance.TotalSetsCount}");
                Debug.Log("[LevelManager] 아이템 매니저 객체 이름 : " + ItemSetManager.Instance.gameObject.name);
            }
            else
            {
                Debug.Log("[LevelManager] ItemSetManager.Instance is null!");
            }

            // 각 그룹별 상태 출력
            foreach (var kvp in TargetObjDic)
            {
                var group = kvp.Value;
                Debug.Log($"[LevelManager] Group '{group.BaseGroupName}': {group.FoundCount}/{group.TotalCount}");
            }

            Debug.Log($"[LevelManager] =========================");
        }
        [Button("테스트 : 모든 물건 찾기")]
        public void FindAllHidden()
        {
            if (TargetObjDic == null)
            {
                Debug.LogError("[LevelManager] TargetObjDic is not initialized. Please start the game first.");
                return;
            }

            // 모든 그룹을 순회하면서 찾지 않은 물건들을 모두 찾은 상태로 변경
            foreach (var kvp in TargetObjDic)
            {
                var group = kvp.Value;
                var notFoundObjects = group.Objects.Where(obj => !group.IsObjectFound(obj)).ToList();

                foreach (var obj in notFoundObjects)
                {
                    // 물건을 찾은 것으로 표시
                    group.LastClickedObject = obj;
                    group.MarkObjectAsFound(obj);

                    // WhenFoundEventHelper 이벤트 호출
                    if (obj.whenFoundEventHelper != null)
                    {
                        obj.whenFoundEventHelper.onFoundEvent?.Invoke();
                    }

                    if (Global.CollectionManager != null)
                    {
                        Global.CollectionManager.TryCollectFromHiddenObj(obj);
                    }

                    // 사운드 재생
                    if (group.Representative.PlaySoundWhenFound && FoundFx != null)
                        FoundFx.Play();

                    Debug.Log($"[LevelManager] 테스트로 찾은 오브젝트: {obj.name} (그룹: {group.BaseGroupName})");
                }
            }

            // UI 업데이트 및 정렬
            if (CurrentScrollView != null)
            {
                var createdUIs = CurrentScrollView.UpdateScrollView(TargetObjDic, TargetImagePrefab, TargetClick, RegionToggle, UIClick);

                // LevelManager에서 관리하는 UI 리스트 갱신
                allHiddenObjUIs.Clear();
                allHiddenObjUIs.AddRange(createdUIs);

                // 그룹과 UI 연결 (기존 순서 기준)
                var groupList = TargetObjDic.Values.ToList();
                for (int i = 0; i < Math.Min(groupList.Count, createdUIs.Count); i++)
                {
                    groupList[i].AssociatedUI = createdUIs[i];
                }

                // 시각적 정렬: 이미 모두 찾은(완료된) 그룹들의 UI는 리스트의 마지막으로 보냅니다.
                if (CurrentScrollView.contentContainer != null)
                {
                    var pairs = new List<(HiddenObjGroup group, HiddenObjUI ui)>();
                    for (int i = 0; i < Math.Min(groupList.Count, createdUIs.Count); i++)
                    {
                        pairs.Add((groupList[i], createdUIs[i]));
                    }

                    var sorted = pairs.OrderBy(p => p.group.FoundCount >= p.group.TotalCount ? 1 : 0).ToList();

                    for (int i = 0; i < sorted.Count; i++)
                    {
                        var uiTransform = sorted[i].ui != null ? sorted[i].ui.transform : null;
                        if (uiTransform != null)
                        {
                            uiTransform.SetSiblingIndex(i);
                        }

                        // 정렬 후에도 그룹-UI 연결을 최신화
                        sorted[i].group.AssociatedUI = sorted[i].ui;
                    }

                    // LevelManager에서 관리하는 UI 리스트도 새 순서로 갱신
                    allHiddenObjUIs = sorted.Select(p => p.ui).ToList();
                }
            }

            // 이벤트 발생
            if (TargetObjDic != null)
            {
                OnFoundObjCountChanged?.Invoke(this, EventArgs.Empty);
                UpdateFoundObjUI();
            }

            Debug.Log($"[LevelManager] 모든 물건을 찾았습니다!");

            // 게임 종료 조건 확인
            DetectGameEnd();
        }

        [Button("테스트 : 아무 물건 찾기")]
        public void FindAnyHidden()
        {
            if (TargetObjDic == null)
            {
                Debug.LogError("[LevelManager] TargetObjDic is not initialized. Please start the game first.");
                return;
            }

            // 찾지 않은 오브젝트가 있는 그룹들을 찾기
            var availableGroups = TargetObjDic.Where(kvp => kvp.Value.FoundCount < kvp.Value.TotalCount).ToList();

            if (availableGroups.Count == 0)
            {
                Debug.Log("[LevelManager] 모든 오브젝트를 이미 찾았습니다!");
                return;
            }

            // 랜덤하게 그룹 선택
            var randomGroupIndex = Random.Range(0, availableGroups.Count);
            var selectedGroup = availableGroups[randomGroupIndex];
            var group = selectedGroup.Value;

            // 해당 그룹에서 아직 찾지 않은 오브젝트들 찾기
            var notFoundObjects = group.Objects.Where(obj => !group.IsObjectFound(obj)).ToList();

            if (notFoundObjects.Count > 0)
            {
                // 랜덤하게 오브젝트 선택
                var randomObjIndex = Random.Range(0, notFoundObjects.Count);
                var selectedObj = notFoundObjects[randomObjIndex];

                // 해당 오브젝트를 찾은 것으로 처리
                group.LastClickedObject = selectedObj;
                group.MarkObjectAsFound(selectedObj);

                // WhenFoundEventHelper 이벤트 호출
                if (selectedObj.whenFoundEventHelper != null)
                {
                    selectedObj.whenFoundEventHelper.onFoundEvent?.Invoke();
                }

                if (Global.CollectionManager != null)
                {
                    Global.CollectionManager.TryCollectFromHiddenObj(selectedObj);
                }

                Debug.Log($"[LevelManager] 테스트로 찾은 오브젝트: {selectedObj.name} (그룹: {group.BaseGroupName})");

                // UI 업데이트 및 정렬
                if (CurrentScrollView != null)
                {
                    var createdUIs = CurrentScrollView.UpdateScrollView(TargetObjDic, TargetImagePrefab, TargetClick, RegionToggle, UIClick);

                    // LevelManager에서 관리하는 UI 리스트 갱신
                    allHiddenObjUIs.Clear();
                    allHiddenObjUIs.AddRange(createdUIs);

                    // 그룹과 UI 연결 (기존 순서 기준)
                    var groupList = TargetObjDic.Values.ToList();
                    for (int i = 0; i < Math.Min(groupList.Count, createdUIs.Count); i++)
                    {
                        groupList[i].AssociatedUI = createdUIs[i];
                    }

                    // 시각적 정렬: 이미 모두 찾은(완료된) 그룹들의 UI는 리스트의 마지막으로 보냅니다.
                    if (CurrentScrollView.contentContainer != null)
                    {
                        var pairs = new List<(HiddenObjGroup group, HiddenObjUI ui)>();
                        for (int i = 0; i < Math.Min(groupList.Count, createdUIs.Count); i++)
                        {
                            pairs.Add((groupList[i], createdUIs[i]));
                        }

                        var sorted = pairs.OrderBy(p => p.group.FoundCount >= p.group.TotalCount ? 1 : 0).ToList();

                        for (int i = 0; i < sorted.Count; i++)
                        {
                            var uiTransform = sorted[i].ui != null ? sorted[i].ui.transform : null;
                            if (uiTransform != null)
                            {
                                uiTransform.SetSiblingIndex(i);
                            }

                            // 정렬 후에도 그룹-UI 연결을 최신화
                            sorted[i].group.AssociatedUI = sorted[i].ui;
                        }

                        // LevelManager에서 관리하는 UI 리스트도 새 순서로 갱신
                        allHiddenObjUIs = sorted.Select(p => p.ui).ToList();
                    }
                }

                // 사운드 재생
                if (group.Representative.PlaySoundWhenFound && FoundFx != null)
                    FoundFx.Play();

                // 이벤트 발생 (TargetObjDic이 여전히 유효한지 확인 후)
                if (TargetObjDic != null)
                {
                    OnFoundObj?.Invoke(this, selectedObj);
                    OnFoundObjCountChanged?.Invoke(this, EventArgs.Empty);
                    UpdateFoundObjUI();
                }

                // 게임 종료 조건 확인
                DetectGameEnd();
            }
        }
    }
}