using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System.Threading;
using DeskCat.FindIt.Scripts.Core.Main.System;
using System;

namespace InGame
{
    [Serializable]
    public class DayNightObj
    {
        [SerializeField]
        [LabelText("아침 오브젝트들")]
        public SpriteRenderer DayObjs;
        [SerializeField]
        [LabelText("숨길 오브젝트")]
        public SpriteRenderer NightObjs;
    }
    public class NightObj : FoundObj
    {
        public static bool IsGlobalNight = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            IsGlobalNight = false;
        }

        private HiddenObj cachedHiddenObj;
        // Handler reference so we can unsubscribe cleanly
        private Action hiddenObjFoundHandler;
        // Delay (in frames) before subscribing to HiddenObj.OnFound
        [LabelText("OnFound 구독 대기 프레임 수")] public int subscribeDelayFrames = 2;
        private CancellationTokenSource subscribeCts;
        [SerializeField]
        [LabelText("밤이 될 때 호출할 이벤트")]
        public UnityEvent onNightEvent;
        [SerializeField]
        [LabelText("밤낮 오브젝트들")]
        [EnableIf("isHideOnDay")]
        public List<DayNightObj> dayNightObj;
        [SerializeField]
        [LabelText("밤낮 꺼버려서 교체할 옵젝들")]
        [EnableIf("isHideOnDay")]
        public List<DayNightObj> dayNightEnableObj;
        [SerializeField] [LabelText("밤에 어두워질 옵젝들")] [EnableIf("isHideOnDay")] 
        public List<SpriteRenderer> nightDarkObj;
        [SerializeField] [LabelText("밤에 변할 색깔")] [EnableIf("isChangeColor")] 
        public Color nightColor = Color.black;
        [LabelText("시작시 밤 오브젝트 숨길 것인가")] public bool isHideOnDay = false;
        [LabelText("밤에 강제 활성화 여부")] public bool isActiveOnNight = true;
        [LabelText("밤에 바로 비활성화할것인가")] public bool isDisableOnNight = false;
        [LabelText("페이드 효과 제외")] public bool isNoFade = false;
        [LabelText("시작시 아예 끌 것인가")] public bool isDisableOnStart = false;
        [LabelText("밤에 색 변할 것인가")] public bool isChangeColor = false;

        public bool isNight;
        private void Awake()
        {
            // cache HiddenObj on the same GameObject if present
            if (TryGetComponent<HiddenObj>(out var h))
            {
                cachedHiddenObj = h;
                if (cachedHiddenObj.IsFound)
                {
                    // 이미 찾은 숨겨진 물건이면 밤에 강제 활성화하지 않음
                    isActiveOnNight = false;
                }
            }
            if (isHideOnDay)
            {
                foreach (var obj in dayNightObj)
                {
                    obj.NightObjs.gameObject.SetActive(false);
                }
                foreach (var obj in dayNightEnableObj)
                {
                    obj.DayObjs.gameObject.SetActive(true);
                    obj.NightObjs.gameObject.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            subscribeCts?.Cancel();
            subscribeCts?.Dispose();
            subscribeCts = null;

            if (cachedHiddenObj != null && hiddenObjFoundHandler != null)
            {
                cachedHiddenObj.OnFound -= hiddenObjFoundHandler;
            }
        }

        private void OnEnable()
        {
            // Cancel any previous pending subscribe task
            subscribeCts?.Cancel();
            subscribeCts?.Dispose();
            subscribeCts = new CancellationTokenSource();

            // Start delayed subscription (will be cancelled on disable/destroy)
            SubscribeAfterDelayAsync(subscribeDelayFrames, subscribeCts.Token).Forget();

            if (IsGlobalNight)
            {
                OnNight();
            }
        }

        private void OnDisable()
        {
            // Cancel pending subscribe and unsubscribe handler
            subscribeCts?.Cancel();
            subscribeCts?.Dispose();
            subscribeCts = null;

            if (cachedHiddenObj != null && hiddenObjFoundHandler != null)
            {
                cachedHiddenObj.OnFound -= hiddenObjFoundHandler;
                hiddenObjFoundHandler = null;
            }
        }

        private async UniTaskVoid SubscribeAfterDelayAsync(int frames, CancellationToken ct)
        {
            try
            {
                await UniTask.DelayFrame(frames, cancellationToken: ct);

                if (ct.IsCancellationRequested) return;

                if (cachedHiddenObj == null)
                {
                    TryGetComponent<HiddenObj>(out cachedHiddenObj);
                }

                if (cachedHiddenObj != null)
                {
                    hiddenObjFoundHandler = () => { isActiveOnNight = false; };
                    cachedHiddenObj.OnFound += hiddenObjFoundHandler;

                    // Apply immediately if already found
                    if (cachedHiddenObj.IsFound)
                    {
                        isActiveOnNight = false;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
        }

        public void OnNight()
        {
            IsGlobalNight = true;
            foreach (var obj in dayNightObj)
            {
                obj.DayObjs.sprite = obj.NightObjs.sprite;
                obj.DayObjs.flipX = obj.NightObjs.flipX;
                obj.DayObjs.flipY = obj.NightObjs.flipY;
                if (obj.DayObjs != null)
                {
                    obj.DayObjs.DOKill();
                    obj.DayObjs.color = Color.white;

                    if (!isNoFade)
                    {
                        obj.DayObjs.DOFade(0, 0);
                        obj.DayObjs.DOFade(1, 4f).SetEase(Ease.OutQuart);
                    }
                }
            }
            foreach (var obj in dayNightEnableObj)
            {
                obj.DayObjs.gameObject.SetActive(false);
                obj.NightObjs.gameObject.SetActive(true);
                if (obj.NightObjs != null)
                {
                    obj.NightObjs.DOKill();
                    obj.NightObjs.color = Color.white;

                    obj.NightObjs.DOFade(0, 0);
                    if (!isNoFade)
                        obj.NightObjs.DOFade(1, 4f).SetEase(Ease.OutQuart);
                }
            }
            foreach (var obj in nightDarkObj)
            {
                obj.DOColor(new Color(1, 1, 1, 0), 0);
                obj.DOColor(new Color(175f / 255f, 175f / 255f, 175f / 255f), 5f);
            }

            if(isChangeColor) 
            {
                var sr = transform.GetComponent<SpriteRenderer>();
                if(sr != null)
                    sr.color = nightColor;
            }
            isNight = true;

            // Invoke UnityEvent for night (if assigned)
            if (onNightEvent != null)
            {
                onNightEvent.Invoke();
            }
        }

    }
}