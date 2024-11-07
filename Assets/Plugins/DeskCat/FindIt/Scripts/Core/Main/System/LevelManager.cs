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
    public class LevelManager : MonoBehaviour
    {
        //* 김일 추가 : 종료 조건에 등록된 함수들 먼저 실행
        public List<Func<UniTask>> OnEndEvnt = new List<Func<UniTask>>();  // 비동기 메서드 참조
        //* 김일 추가 : 옵젝 찾으면 전역에 알릴려고 추가함
        public EventHandler<int> OnFoundObj;
        [Header("Hidden Object List")] 
        [Tooltip("Place The Hidden Object Into This Array")]
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

        private Dictionary<Guid, HiddenObj> TargetObjDic;
        private Dictionary<Guid, HiddenObj> RabbitObjDic;
        private static LevelManager LevelManagerInstance;
        private DateTime StartTime;
        private DateTime EndTime;

        private int hiddenObjCount = 0;
        private int rabbitObjCount = 0;
        private int maxRabbitObjCount = 0;

        public static void PlayItemFx(AudioClip clip)
        {
            LevelManagerInstance.ItemFx.clip = clip;
            LevelManagerInstance.ItemFx.Play();
        }

        private void Start()
        {
            if (LevelManagerInstance == null)
            {
                LevelManagerInstance = this;
            }

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

        public void AddHiddenObject(HiddenObj hiddenObj)
        {
            Debug.Log("time");
            TargetObjDic.Add(Guid.NewGuid(), hiddenObj);
            ScrollViewTrigger();
        }

        private void ToggleScrollView()
        {
            UIScrollType = (UIScrollType == UIScrollType.Vertical) ? UIScrollType.Vertical : UIScrollType.Horizontal;
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
            TargetObjDic = new Dictionary<Guid, HiddenObj>();
            foreach (var target in TargetObjs)
            {
                if (target != null)
                {
                    TargetObjDic.Add(Guid.NewGuid(), target);
                }
            }
            hiddenObjCount = TargetObjDic.Count;
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

            var tempDic = new Dictionary<Guid, HiddenObj>();
            foreach (var index in randomIndex)
            {
                tempDic.Add(TargetObjDic.ElementAt(index).Key, TargetObjDic.ElementAt(index).Value);
            }

            TargetObjDic = tempDic;

        }

        private void TargetClick(Guid guid)
        {
            if (TargetObjDic.ContainsKey(guid)) {
                if (TargetObjDic[guid].hiddenObjFoundType != HiddenObjFoundType.Click) return;

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

            if (TargetObjDic[guid].hiddenObjFoundType != HiddenObjFoundType.Drag) return;

            FoundObjAction(guid);
        }

        private void FoundObjAction(Guid guid)
        {
            if (TargetObjDic[guid].PlaySoundWhenFound)
                FoundFx.Play();

            // TargetObjDic.Remove(guid);
            CurrentScrollView.UpdateScrollView(TargetObjDic, TargetImagePrefab, TargetClick, RegionToggle, UIClick);
            hiddenObjCount--;
            OnFoundObj?.Invoke(this, hiddenObjCount);
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
            if (hiddenObjCount <= 0)
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

                    GameEndEvent?.Invoke();  // ���든 UnityEvent 호출이 완료된 뒤에 종료 이벤트 호출
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
            
            FoundObjCountText.text = $"{TargetObjDic.Count - hiddenObjCount} / {TargetObjDic.Count}";
            FoundRabbitCountText.text = $"{rabbitObjCount} / {maxRabbitObjCount}";
            StageCompleteText.text = CurrentLevelName + " CLEAR!";

            // 1시간 미만일 경우 mm:ss 형식으로, 1시간 이상일 경우 hh:mm:ss 형식으로 표시
            GameTimeText.text = timeUsed.Hours > 0 
                ? timeUsed.ToString(@"hh\:mm\:ss")
                : timeUsed.ToString(@"mm\:ss");

            var starCount = 0;
            
            // 찾은 오브젝트 비율 (0.0 ~ 1.0)
            float foundObjRatio = (float)(TargetObjDic.Count - hiddenObjCount) / TargetObjDic.Count;
            // 찾은 토끼 비율 (0.0 ~ 1.0)
            float foundRabbitRatio = (float)rabbitObjCount / maxRabbitObjCount;
            
            // 전체 진행률 평균 계산 (0.0 ~ 1.0)
            float totalProgress = (foundObjRatio + foundRabbitRatio) / 2;
            
            // 별 개수 계산 (0~3)
            if (totalProgress >= 0.9f) starCount = 3;
            else if (totalProgress >= 0.6f) starCount = 2;
            else if (totalProgress >= 0.3f) starCount = 1;
            
            for(int i = 0; i < starCount; i++) {
                StarList[i].gameObject.SetActive(true);
            }

            GameEndUI.SetActive(true);
        }
    }
}