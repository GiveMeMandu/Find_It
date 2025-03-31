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
        public List<Func<UniTask>> OnEndEvnt = new List<Func<UniTask>>();  // 비동기 메서드 참조
        //* 김일 추가 : 옵젝 찾으면 전역에 알릴려고 추가함
        public EventHandler<HiddenObj> OnFoundObj;
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

        [Header("Sound Effect")] 
        public AudioSource FoundFx;
        public AudioSource ItemFx;

        [Header("Game End 내용물")] 
        public GameObject GameEndUI;
        public Button GameEndBtn;
        public Text GameTimeText;
        public Text FoundObjCountText;
        public Text FoundRabbitCountText;
        public TextMeshProUGUI StageCompleteText;

        public List<Transform> StarList = new List<Transform>();
    
        public string CurrentLevelName;
        public string NextLevelName;
        public bool IsOverwriteGameEnd;
        public UnityEvent GameEndEvent;

        private Dictionary<Guid, HiddenObjGroup> TargetObjDic;
        private Dictionary<Guid, HiddenObj> RabbitObjDic;
        private DateTime StartTime;
        private DateTime EndTime;

        private int hiddenObjCount = 0;
        private int rabbitObjCount = 0;
        private int maxRabbitObjCount = 0;

        // 새로운 변수 추가
        private List<HiddenObj> normalHiddenObjs = new List<HiddenObj>();

        public static void PlayItemFx(AudioClip clip)
        {
            if(clip == null) clip = Instance.ItemFx.clip;
            Instance.ItemFx.clip = clip;
            Instance.ItemFx.Play();
        }

        private void Start()
        {
            // 시작 시 Hidden 태그를 가진 오브젝트들 수집
            CollectHiddenObjects();
            BuildDictionary();
            ScrollViewTrigger();
            ToggleBtn.onClick.AddListener(ToggleScrollView);
            GameEndBtn.onClick.AddListener(() => { SceneManager.LoadScene(NextLevelName); });
            StartTime = DateTime.Now;

            if (Canvas != null)
            {
                Canvas.SetActive(true);
            }
        }

        private void CollectHiddenObjects()
        {
            if (normalHiddenObjGroup != null)
            {
                // 그룹 내의 모든 자식들을 검사
                Transform[] children = normalHiddenObjGroup.GetComponentsInChildren<Transform>();
                foreach (Transform child in children)
                {
                    // 자기 자신은 제외
                    if (child == normalHiddenObjGroup) continue;
                    
                    // Hidden 태그를 가진 오브젝트 확인
                    if (child.CompareTag("Hidden"))
                    {
                        // HiddenObj 컴포넌트가 없다면 추가
                        if (!child.TryGetComponent<HiddenObj>(out HiddenObj hiddenObj))
                        {
                            hiddenObj = child.gameObject.AddComponent<HiddenObj>();
                            
                            // BoxCollider2D 추가 또는 리셋
                            if (!child.TryGetComponent<BoxCollider2D>(out var boxCollider))
                            {
                                boxCollider = child.gameObject.AddComponent<BoxCollider2D>();
                            }
                            
                            // 배경 애니메이션 설정
                            hiddenObj.HideWhenFound = false;
                            hiddenObj.EnableBGAnimation = true;
                            hiddenObj.BGAnimationPrefab = DefaultBgAnimation;
                            
                            // BG Object 생성 및 설정
                            if (DefaultBgAnimation != null)
                            {
                                var bgObj = Instantiate(DefaultBgAnimation, hiddenObj.transform);
                                hiddenObj.BgAnimationTransform = bgObj.transform;
                            }
                            
                            Debug.Log($"Added HiddenObj component and BoxCollider2D to {child.name}");
                        }
                        normalHiddenObjs.Add(hiddenObj);
                    }
                }
                Debug.Log($"Found and processed {normalHiddenObjs.Count} hidden objects in group");
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
                        Debug.Log($"Added HiddenObj component to {obj.name}");
                    }
                    normalHiddenObjs.Add(hiddenObj);
                }
                Debug.Log($"Found and processed {normalHiddenObjs.Count} hidden objects in scene with tag");
            }
        }

        public int GetLeftHiddenObjCount() => TargetObjDic.Sum(x => x.Value.TotalCount - x.Value.FoundCount);

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
            CurrentScrollView = UIScrollType == UIScrollType.Horizontal ? HorizontalScrollView : VerticalScrollView;
            HorizontalScrollView.mainPanel.SetActive(false);
            VerticalScrollView.mainPanel.SetActive(false);
            CurrentScrollView.Initialize();
            CurrentScrollView.UpdateScrollView(TargetObjDic, TargetImagePrefab, TargetClick, RegionToggle, UIClick);
        }

        private void UIClick()
        {
            UIClickEvent?.Invoke();
        }

        private void BuildDictionary()
        {
            TargetObjDic = new Dictionary<Guid, HiddenObjGroup>();
            
            if (TargetObjs != null && TargetObjs.Length > 0)
            {
                normalHiddenObjs.AddRange(TargetObjs);
            }
            
            var groupedObjects = normalHiddenObjs
                .Where(obj => obj != null)
                .GroupBy(obj => InGameObjectNameFilter.GetBaseGroupName(obj.gameObject.name))
                .ToDictionary(g => g.Key, g => g.ToList());

            Debug.Log($"Grouped objects: {string.Join(", ", groupedObjects.Keys)}");

            foreach (var group in groupedObjects)
            {
                if (group.Value.Count > 0)
                {
                    var hiddenObjGroup = new HiddenObjGroup(group.Value, group.Key);
                    TargetObjDic.Add(Guid.NewGuid(), hiddenObjGroup);
                    Debug.Log($"Added {group.Key} to target dictionary with {group.Value.Count} similar objects");
                    
                    // 각 오브젝트에 클릭 이벤트 설정
                    foreach (var obj in group.Value)
                    {
                        var guid = TargetObjDic.First(x => x.Value.Objects.Contains(obj)).Key;
                        obj.TargetClickAction = () => { 
                            var targetGroup = TargetObjDic[guid];
                            targetGroup.LastClickedObject = obj;
                            TargetClick(guid); 
                        };
                    }
                }
            }
            
            hiddenObjCount = TargetObjDic.Sum(x => x.Value.TotalCount);
            
            RabbitObjDic = new Dictionary<Guid, HiddenObj>();
            foreach (var rabbit in RabbitObjs)
            {
                if (rabbit != null)
                {
                    Guid guid = Guid.NewGuid();
                    RabbitObjDic.Add(guid, rabbit);
                    
                    rabbit.TargetClickAction = () => { TargetClick(guid); };
                }
            }
            maxRabbitObjCount = RabbitObjDic.Count;
            rabbitObjCount = 0;
            RabbitCountText.text = $"{rabbitObjCount}/{maxRabbitObjCount}";
            if (!IsRandomItem) return;
            
            var randomIndex = new List<int>();
            for (var i = 0; i < MaxRandomItem; i++)
            {
                var index = Random.Range(0, TargetObjDic.Count-1);
                while (randomIndex.Contains(index))
                {
                    index = Random.Range(0, TargetObjDic.Count-1);
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
            if (TargetObjDic.ContainsKey(guid)) {
                if (TargetObjDic[guid].Representative.hiddenObjFoundType != HiddenObjFoundType.Click) return;

                FoundObjAction(guid);
            }
            else if(RabbitObjDic.ContainsKey(guid)) {
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
            if (group.Representative.PlaySoundWhenFound)
                FoundFx.Play();

            var clickedObj = group.LastClickedObject;
            if (clickedObj != null && !group.IsObjectFound(clickedObj))
            {
                group.MarkObjectAsFound(clickedObj);
                CurrentScrollView.UpdateScrollView(TargetObjDic, TargetImagePrefab, TargetClick, RegionToggle, UIClick);
                hiddenObjCount--;
                OnFoundObj?.Invoke(this, clickedObj);
                
                Debug.Log($"Found {clickedObj.name} from group {group.BaseGroupName} ({group.FoundCount}/{group.TotalCount})");
            }

            DetectGameEnd();
        }
        private void FoundRabbitObjAction(Guid guid)
        {
            if (RabbitObjDic[guid].PlaySoundWhenFound)
                FoundFx.Play();

            RabbitObjDic.Remove(guid);
            rabbitObjCount++;
            RabbitCountText.text = $"{rabbitObjCount}/{maxRabbitObjCount}";
            DetectGameEnd();
        }

        //* 김일 수정 : 게임 종료 조건 = 숨긴 물건만 찾고 추가 조건은 태스크로 관리
        private async void DetectGameEnd()
        {
            // 모든 숨겨진 오브젝트를 찾았고, ItemSetManager의 모든 세트도 찾았을 때만 게임 종료
            if (hiddenObjCount <= 0 && ItemSetManager.Instance.FoundSetsCount == ItemSetManager.Instance.TotalSetsCount)
            {
                if (IsOverwriteGameEnd)
                {
                    // UnityEvent의 모든 리스너가 실행 완료될 때까지 대기
                    if (OnEndEvnt.Count > 0)
                    {
                        foreach (var func in OnEndEvnt)
                        {
                            await func(); 
                        }
                    }

                    GameEndEvent?.Invoke();  // 모든 UnityEvent 호출이 완료된 뒤에 종료 이벤트 호출
                    return;
                }
                // UnityEvent의 모든 리스너가 실행 완료될 때까지 대기
                if (OnEndEvnt.Count > 0)
                {
                    foreach (var func in OnEndEvnt)
                    {
                        await func();
                    }
                }

                DefaultGameEndFunc();
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
            
            FoundObjCountText.text = $"{foundObjects} / {totalObjects}";
            FoundRabbitCountText.text = $"{rabbitObjCount} / {maxRabbitObjCount}";
            StageCompleteText.text = CurrentLevelName + " CLEAR!";

            GameTimeText.text = timeUsed.Hours > 0 
                ? timeUsed.ToString(@"hh\:mm\:ss")
                : timeUsed.ToString(@"mm\:ss");

            var starCount = 0;
            
            float foundObjRatio = (float)foundObjects / totalObjects;
            float foundRabbitRatio = (float)rabbitObjCount / maxRabbitObjCount;
            
            float totalProgress = (foundObjRatio + foundRabbitRatio) / 2;
            
            if (totalProgress >= 0.9f) starCount = 3;
            else if (totalProgress >= 0.6f) starCount = 2;
            else if (totalProgress >= 0.3f) starCount = 1;
            
            for(int i = 0; i < starCount; i++) {
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
    }
}