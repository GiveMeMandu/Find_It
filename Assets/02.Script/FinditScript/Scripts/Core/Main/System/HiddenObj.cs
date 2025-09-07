using System;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Main.Utility.Animation;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using Lean.Touch;
using SnowRabbit.Helper;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public class HiddenObj : LeanClickEvent
    {
        [Tooltip("Decide This Object Is Click To Found Or Drag To Specified Region")]
        public HiddenObjFoundType hiddenObjFoundType = HiddenObjFoundType.Click;

        [Tooltip("Sprite To Display In UI Panel, Leave Empty To Use The Current Game Object Sprite")]
        public Sprite UISprite;

        [Tooltip("UIChangeHelper component for custom UI sprite")]
        public UIChangeHelper uiChangeHelper;

        [Tooltip("Game Object Will Be Invisible When Start Scene")]
        public bool HideOnStart;

        [Tooltip("Tooltips or Hint Display When Click On The UI Object")]
        public bool EnableTooltip;
        public TooltipsType TooltipsType;

        [NonReorderable] 
        public List<MultiLanguageTextListModel> Tooltips;

        [Tooltip("Use Background Animation When Clicked")]
        public bool EnableBGAnimation;
        public GameObject BGAnimationPrefab;
        public Transform BgAnimationTransform;
        public BGScaleLerp BgAnimationLerp;
        public SpriteRenderer BgAnimationSpriteRenderer;

        [Tooltip("Hide The Object When Found")]
        public bool HideWhenFound = true;

        [Tooltip("Play Sound Effect When Found")]
        public bool PlaySoundWhenFound = true;

        [Tooltip("Click Priority (Higher value = Higher priority)")]
        public int clickPriority = 0;

        public AudioClip AudioWhenClick;

        [Tooltip("Action When Target Is Clicked")]
        public Action TargetClickAction;

        [Tooltip("Action When Target Is Drag To The Specified Region")]
        public Action DragRegionAction;

        [HideInInspector] public bool IsFound;
        
        public bool baseInfoBool = true;
        public bool tooltipsBool = true;
        public bool bgAnimBool = true;
        public bool actionFoldoutBool = true;

        public SpriteRenderer spriteRenderer;

        /// <summary>
        /// UI에 표시할 스프라이트를 반환합니다. UIChangeHelper가 있으면 그 스프라이트를, 없으면 UISprite를 반환합니다.
        /// </summary>
        public Sprite GetUISprite()
        {
            if (uiChangeHelper != null && uiChangeHelper.sprite != null)
            {
                return uiChangeHelper.sprite;
            }
            return UISprite;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            Debug.Log($"HiddenObj {gameObject.name} OnEnable - LeanClickEvent inherited");
        }

        // HiddenObj는 LeanClickEvent의 OnClickEvent를 설정하여 최우선 처리
        private void Start()
        {
            // HiddenObj는 우선순위 검사를 비활성화 (무조건 클릭 허용)
            Enable = true; // LeanClickEvent 활성화
            
            // HiddenObj 전용 클릭 이벤트 설정
            if (OnClickEvent == null)
            {
                OnClickEvent = new UnityEngine.Events.UnityEvent();
            }
            
            // 기존 리스너 제거 후 HiddenObj 클릭 처리를 연결
            OnClickEvent.RemoveAllListeners();
            OnClickEvent.AddListener(HitHiddenObject);
            
            Debug.Log($"HiddenObj {gameObject.name} Start - Click event setup complete");
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public void HitHiddenObject()
        {
            Debug.Log($"HitHiddenObject called for {gameObject.name}, IsFound: {IsFound}");
            
            if(IsFound == false) {
                Debug.Log($"Setting {gameObject.name} as found!");
                IsFound = true;
                if (AudioWhenClick != null)
                {
                    LevelManager.PlayItemFx(AudioWhenClick);
                }
                if (EnableBGAnimation)
                {
                    BgAnimationTransform.gameObject.SetActive(true);
                }

                if (HideWhenFound)
                {
                    gameObject.SetActive(false);
                }

                TargetClickAction?.Invoke();
            }
            else
            {
                Debug.Log($"{gameObject.name} is already found, ignoring click");
            }
        }

        private void Awake()
        {
            // 에디터에서 레이어 자동 설정
#if UNITY_EDITOR
            if (gameObject.layer != LayerManager.HiddenObjectLayer)
            {
                gameObject.layer = LayerManager.HiddenObjectLayer;
                UnityEditor.EditorUtility.SetDirty(gameObject);
            }
#endif

            if (UISprite == null)
            {
                if (TryGetComponent(out spriteRenderer))
                {
                    UISprite = spriteRenderer.sprite;
                }
            }

            // UIChangeHelper 컴포넌트 자동 찾기
            if (uiChangeHelper == null)
            {
                uiChangeHelper = GetComponent<UIChangeHelper>();
            }

            if (HideOnStart)
            {
                gameObject.SetActive(false);
            }
            if (BgAnimationSpriteRenderer == null && BgAnimationTransform != null)
            {
                BgAnimationSpriteRenderer = BgAnimationTransform.GetComponentInChildren<SpriteRenderer>();

                BgAnimationLerp = BgAnimationTransform.GetComponent<BGScaleLerp>();
            }
        }

        // Unity 에디터에서 컴포넌트가 추가되거나 Reset 버튼을 눌렀을 때 호출됩니다
        private void Reset()
        {
#if UNITY_EDITOR
            // HiddenObj 컴포넌트가 추가될 때 자동으로 레이어를 HiddenObjectLayer로 설정
            gameObject.layer = LayerManager.HiddenObjectLayer;
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
        }

        private void OnMouseDown()
        {
            Debug.Log($"OnMouseDown called for {gameObject.name}");
            HitHiddenObject();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"OnPointerClick called for {gameObject.name}");
            
            // 클릭 우선순위 체크
            if (!CheckClickPriority(eventData))
            {
                Debug.Log($"CheckClickPriority failed for {gameObject.name}");
                return;
            }
                
            Debug.Log($"OnPointerClick processing {gameObject.name}");
            HitHiddenObject();
        }

        // You Can Use This Function To Toggle Item Visibility
        public void ToggleItem()
        {
            if (IsFound)
            {
                return;
            }

            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void SetBgAnimation(GameObject bgAnimationPrefab)
        {
            HideWhenFound = false;
            EnableBGAnimation = true;
            BGAnimationPrefab = bgAnimationPrefab;
            BgAnimationTransform = bgAnimationPrefab.transform;
            BgAnimationSpriteRenderer = bgAnimationPrefab.GetComponentInChildren<SpriteRenderer>();
            BgAnimationLerp = bgAnimationPrefab.GetComponent<BGScaleLerp>();
        }

        /// <summary>
        /// 클릭 가능한지 확인합니다. HiddenObj는 찾아지지 않은 상태에서만 클릭 가능합니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        /// <returns>HiddenObj가 클릭되어야 하는지 여부</returns>
        private bool CheckClickPriority(PointerEventData eventData)
        {
            // HiddenObj가 이미 찾아진 경우에만 클릭 무시
            // 찾아지지 않은 상태라면 무조건 클릭 허용
            return !IsFound;
        }
    }
}