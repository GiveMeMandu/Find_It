using System;
using System.Collections.Generic;
using System.Linq;
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

        [Header("Game End")] 
        public GameObject GameEndUI;
        public Button GameEndBtn;
        public Text GameTimeText;
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
            rabbitObjCount = RabbitObjDic.Count;
            RabbitCountText.text = $"x {rabbitObjCount}";

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
            rabbitObjCount--;
            // todo : rabbit countt updata
            RabbitCountText.text = $"x {rabbitObjCount}";
            DetectGameEnd();
        }

        private void DetectGameEnd()
        {
            if (hiddenObjCount <= 0 && rabbitObjCount <= 0)
            {
                if (IsOverwriteGameEnd)
                {
                    GameEndEvent?.Invoke();
                    return;
                }
                
                DefaultGameEndFunc();
            }
        }

        public void DefaultGameEndFunc()
        {
            EndTime = DateTime.Now;
            var timeUsed = EndTime.Subtract(StartTime);
            GameTimeText.text = timeUsed.ToString(@"hh\:mm\:ss");
            GameEndUI.SetActive(true);
        }
    }
}