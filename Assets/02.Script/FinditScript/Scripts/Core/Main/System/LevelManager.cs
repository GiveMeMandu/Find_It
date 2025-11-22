using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeskCat.FindIt.Scripts.Core.Model;
using NaughtyAttributes;
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
using UI;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public class HiddenObjGroup
    {
        public List<HiddenObj> Objects { get; set; }
        public int TotalCount => Objects.Count;
        public int FoundCount { get; set; }
        public HiddenObj Representative => Objects[0];
        public Dictionary<HiddenObj, bool> ObjectStates { get; private set; }
        public HiddenObj LastClickedObject { get; set; }
        public string BaseGroupName { get; private set; }
        
        // UI 연결을 위한 참조 추가
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
        public HiddenObj[] TargetObjs;
        public HiddenObj[] RabbitObjs;
        public TextMeshProUGUI RabbitCountText;
        public bool IsRandomItem;
        public int MaxRandomItem;

        public GameObject Canvas;

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

        public List<Transform> StarList = new List<Transform>();

        // 기존 CurrentLevelName, NextLevelName 제거하고 SceneBase에서 자동으로 가져오기
        public bool IsOverwriteGameEnd;
        public UnityEvent GameEndEvent;

        public Dictionary<Guid, HiddenObjGroup> TargetObjDic;
        public Dictionary<Guid, HiddenObj> RabbitObjDic;
        private DateTime StartTime;
        private DateTime EndTime;

        private int rabbitObjCount = 0;
        private int maxRabbitObjCount = 0;

        // 새로운 변수 추가
        private List<HiddenObj> normalHiddenObjs = new List<HiddenObj>();
        
        // HiddenObjUI 관리를 위한 리스트 추가
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

            // 버튼들 null 체크
            if (ToggleBtn != null)
                ToggleBtn.onClick.AddListener(ToggleScrollView);
            if (GameEndBtn != null)
                GameEndBtn.onClick.AddListener(GoToNextLevel);

            StartTime = DateTime.Now;

            if (Canvas != null)
            {
                Canvas.SetActive(true);
            }
            OnFoundObjCountChanged?.Invoke(this, EventArgs.Empty);
            // 모드 초기화: ModeSelector가 있으면 선택된 모드를 초기화하고,
            // 없으면 기존 동작대로 씬의 아무 ModeManager 하나를 초기화합니다.
            modeSelector = FindAnyObjectByType<ModeSelector>();
            if (modeSelector != null)
            {
                modeSelector.InitializeSelectedMode();
            }
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
                        // HiddenObj 컴포넌트가 없다면 추가
                        HiddenObj hiddenObj = null;
                        if (!child.TryGetComponent<HiddenObj>(out hiddenObj))
                        {
                            try
                            {
                                hiddenObj = child.gameObject.AddComponent<HiddenObj>();
                                Debug.Log($"[LevelManager] Successfully added HiddenObj to {child.name}");
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
                        boxCollider.size = new Vector2(boxCollider.size.x * 1.5f, boxCollider.size.y * 1.5f);

                        // 배경 애니메이션 설정

                        // BG Object 생성 및 설정
                        if (DefaultBgAnimation != null)
                        {
                            var bgObj = Instantiate(DefaultBgAnimation, hiddenObj.transform);
                            hiddenObj.BgAnimationTransform = bgObj.transform;
                            hiddenObj.SetBgAnimation(bgObj);
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
            UIScrollType = (UIScrollType == UIScrollType.Vertical) ? UIScrollType.Horizontal : UIScrollType.Vertical;
            ScrollViewTrigger();
        }

        private void ScrollViewTrigger()
        {
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
                
                Debug.Log($"[LevelManager] ScrollView UI 업데이트 완료: {allHiddenObjUIs.Count}개의 HiddenObjUI 생성 및 그룹 연결");
            }
        }

        private void UIClick()
        {
            UIClickEvent?.Invoke();
        }

        private void BuildDictionary()
        {
            TargetObjDic = new Dictionary<Guid, HiddenObjGroup>();

            // TargetObjs null 체크 및 추가
            if (TargetObjs != null && TargetObjs.Length > 0)
            {
                normalHiddenObjs.AddRange(TargetObjs);
            }

            var groupedObjects = normalHiddenObjs
                .Where(obj => obj != null)
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
            if (!TargetObjDic.ContainsKey(guid)) return;

            if (TargetObjDic[guid].Representative.hiddenObjFoundType != HiddenObjFoundType.Drag) return;

            FoundObjAction(guid);
        }

        private void FoundObjAction(Guid guid)
        {
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

                // CurrentScrollView null 체크
                if (CurrentScrollView != null)
                    CurrentScrollView.UpdateScrollView(TargetObjDic, TargetImagePrefab, TargetClick, RegionToggle, UIClick);

                OnFoundObj?.Invoke(this, clickedObj);

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

            DetectGameEnd();
        }

        //* 김일 수정 : 게임 종료 조건 = 숨긴 물건만 찾고 추가 조건은 태스크로 관리
        private async void DetectGameEnd()
        {
            // 실제 남은 오브젝트 수 계산
            int remainingObjects = GetLeftHiddenObjCount();
            int totalObjects = GetTotalHiddenObjCount();
            int foundObjects = totalObjects - remainingObjects;

            // 디버그 로그 추가
            Debug.Log($"[LevelManager] DetectGameEnd - Remaining: {remainingObjects}, Total: {totalObjects}, Found: {foundObjects}");
            Debug.Log($"[LevelManager] ItemSetManager - Found: {ItemSetManager.Instance?.FoundSetsCount}, Total: {ItemSetManager.Instance?.TotalSetsCount}");

            // DARK 모드인 경우 미션(ItemSet) 검사 제외
            bool isDarkMode = modeSelector != null && modeSelector.selectedMode == ModeManager.GameMode.DARK;
            bool itemSetConditionMet = isDarkMode || (ItemSetManager.Instance.FoundSetsCount == ItemSetManager.Instance.TotalSetsCount);

            // 모든 숨겨진 오브젝트를 찾았고, (DARK 모드가 아니라면) ItemSetManager의 모든 세트도 찾았을 때만 게임 종료
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
                Debug.Log($"[LevelManager] Game End condition not met - Remaining objects: {remainingObjects}, ItemSet condition: {itemSetConditionMet} (DARK 모드: {isDarkMode})");
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
            EndTime = DateTime.Now;
            var timeUsed = EndTime.Subtract(StartTime);

            int totalObjects = TargetObjDic.Sum(x => x.Value.TotalCount);
            int foundObjects = TargetObjDic.Sum(x => x.Value.FoundCount);

            if (FoundObjCountText != null)
            {
                FoundObjCountText.text = $"{foundObjects} / {totalObjects}";
            }
            if (CurrentFoundObjCountText != null)
            {
                CurrentFoundObjCountText.text = $"{foundObjects} / {totalObjects}";
            }
            if (FoundObjCountFillImage != null)
            {
                FoundObjCountFillImage.fillAmount = (float)foundObjects / totalObjects;
            }
            if (FoundRabbitCountText != null)
            {
                FoundRabbitCountText.text = $"{rabbitObjCount} / {maxRabbitObjCount}";
            }
            if (StageCompleteText != null)
            {
                string levelName = "CLEAR!";
                if (Global.CurrentScene != null)
                {
                    levelName = SceneHelper.GetFormattedStageName(Global.CurrentScene.SceneName);
                }
                StageCompleteText.text = levelName + " CLEAR!";
            }

            if (GameTimeText != null)
            {
                GameTimeText.text = timeUsed.Hours > 0
                    ? timeUsed.ToString(@"hh\:mm\:ss")
                    : timeUsed.ToString(@"mm\:ss");
            }

            var starCount = 0;

            float foundObjRatio = (float)foundObjects / totalObjects;
            float foundRabbitRatio = (float)rabbitObjCount / maxRabbitObjCount;
            // 구름토끼
            // float totalProgress = (foundObjRatio + foundRabbitRatio) / 2;

            float totalProgress = foundObjRatio; // rabbit 점수 제외, 숨겨진 오브젝트만으로 계산

            if (totalProgress >= 0.9f) starCount = 3;
            else if (totalProgress >= 0.6f) starCount = 2;
            else if (totalProgress >= 0.3f) starCount = 1;

            for (int i = 0; i < starCount; i++)
            {
                StarList[i].gameObject.SetActive(true);
            }

            GameEndUI.SetActive(true);
        }

        // 그룹 상태를 확인하기 위한 public 메서드 추가
        public (bool exists, bool isComplete, string baseGroupName) GetGroupStatus(string groupName)
        {
            var group = TargetObjDic.FirstOrDefault(x => x.Value.BaseGroupName == groupName).Value;
            return group != null
                ? (true, group.FoundCount == group.TotalCount, group.BaseGroupName)
                : (false, false, string.Empty);
        }

        // 그룹 이름으로 HiddenObj 목록을 찾는 메서드
        public List<HiddenObj> GetHiddenObjsByGroupName(string groupName)
        {
            // 씬에서 모든 HiddenObj 컴포넌트를 찾음
            var allHiddenObjs = TargetObjDic.Values.SelectMany(group => group.Objects).ToList();

            // 그룹 이름이 일치하는 HiddenObj들 반환
            return allHiddenObjs
                .Where(obj => InGameObjectNameFilter.GetBaseGroupName(obj.gameObject.name) == groupName)
                .ToList();
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
        [Button("테스트 : 아무 물건 찾기")]
        public void FindAnyHidden()
        {
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

                Debug.Log($"[LevelManager] 테스트로 찾은 오브젝트: {selectedObj.name} (그룹: {group.BaseGroupName})");

                // UI 업데이트
                if (CurrentScrollView != null)
                    CurrentScrollView.UpdateScrollView(TargetObjDic, TargetImagePrefab, TargetClick, RegionToggle, UIClick);

                // 사운드 재생
                if (group.Representative.PlaySoundWhenFound && FoundFx != null)
                    FoundFx.Play();

                // 이벤트 발생
                OnFoundObj?.Invoke(this, selectedObj);
                OnFoundObjCountChanged?.Invoke(this, EventArgs.Empty);

                // 게임 종료 조건 확인
                DetectGameEnd();
            }
        }
    }
}