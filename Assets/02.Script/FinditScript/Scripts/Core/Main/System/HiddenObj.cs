using System;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Main.Utility.Animation;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using Lean.Touch;
using Helper;
using Manager;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public class HiddenObj : MonoBehaviour
    {
        private LeanClickEvent leanClickEvent;
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

        // Event invoked when this hidden object is found (useful for subscribers like NightObj)
        public event Action OnFound;

        [Tooltip("Action When Target Is Drag To The Specified Region")]
        public Action DragRegionAction;

        [Tooltip("WhenFoundEventHelper component for custom found events")]
        public WhenFoundEventHelper whenFoundEventHelper;

        [HideInInspector] public bool IsFound;

        public bool baseInfoBool = true;
        public bool tooltipsBool = true;
        public bool bgAnimBool = true;
        public bool actionFoldoutBool = true;

        public SpriteRenderer spriteRenderer;

        // Cached UI sprite so GetUISprite works even if the GameObject is inactive
        private Sprite cachedUISprite;

        /// <summary>
        /// UI에 표시할 스프라이트를 반환합니다. UIChangeHelper가 있으면 그 스프라이트를, 없으면 UISprite를 반환합니다.
        /// </summary>
        public Sprite GetUISprite()
        {
            // 캐시가 null이면 강제로 다시 캐시 시도
            if (cachedUISprite == null)
            {
                if (UISprite == null)
                {
                    if (TryGetComponent(out spriteRenderer) && spriteRenderer.sprite != null)
                    {
                        UISprite = spriteRenderer.sprite;
                    }
                    else
                    {
                        // Try to find a SpriteRenderer in children (some prefabs use child renderers)
                        var childSr = GetComponentInChildren<SpriteRenderer>();
                        if (childSr != null && childSr.sprite != null)
                        {
                            spriteRenderer = childSr;
                            UISprite = childSr.sprite;
                        }
                    }
                }

                // UIChangeHelper 컴포넌트 자동 찾기
                if (uiChangeHelper == null)
                {
                    uiChangeHelper = GetComponent<UIChangeHelper>();
                }
                cachedUISprite = (uiChangeHelper != null && uiChangeHelper.sprite != null) ? uiChangeHelper.sprite : UISprite;
            }

            // 객체가 비활성화되어 있으면 캐시된 값만 반환
            if (!gameObject.activeInHierarchy)
            {
                return cachedUISprite;
            }

            // 객체가 활성화되어 있으면 실시간으로 체크하고 캐시 업데이트
            Sprite currentSprite = (uiChangeHelper != null && uiChangeHelper.sprite != null)
                ? uiChangeHelper.sprite
                : UISprite;

            // 캐시 업데이트
            if (currentSprite != null)
            {
                cachedUISprite = currentSprite;
            }

            return currentSprite;
        }

        /// <summary>
        /// UI에 표시할 색깔을 반환합니다. SpriteRenderer가 있으면 그 색깔을, 없으면 흰색을 반환합니다.
        /// </summary>
        public Color GetUIColor()
        {
            if (spriteRenderer != null)
            {
                return spriteRenderer.color;
            }
            
            // spriteRenderer가 없으면 자식에서 찾기
            var childSr = GetComponentInChildren<SpriteRenderer>();
            if (childSr != null)
            {
                spriteRenderer = childSr;
                return childSr.color;
            }
            
            // 기본값으로 흰색 반환
            return Color.white;
        }

        // Call to refresh the cached UI sprite if components change at runtime
        public void RefreshCachedUISprite()
        {
            cachedUISprite = (uiChangeHelper != null && uiChangeHelper.sprite != null) ? uiChangeHelper.sprite : UISprite;
        }

        public void HitHiddenObject()
        {
            Debug.Log($"HitHiddenObject called for {gameObject.name}, IsFound: {IsFound}");

            // 이미 찾아진 경우 무시
            if (IsFound)
            {
                // Debug.Log($"{gameObject.name} is already found, ignoring click");
                return;
            }

            if (IsFound == false)
            {
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

                // Raise the public found event for subscribers
                OnFound?.Invoke();

                // WhenFoundEventHelper 이벤트 호출
                whenFoundEventHelper?.onFoundEvent?.Invoke();

                TargetClickAction?.Invoke();
            }
        }

        private void Awake()
        {
            try
            {
                // 에디터에서 레이어 자동 설정
#if UNITY_EDITOR
                if (gameObject.layer != LayerManager.HiddenObjectLayer)
                {
                    gameObject.layer = LayerManager.HiddenObjectLayer;
                    UnityEditor.EditorUtility.SetDirty(gameObject);
                }
#endif

                // LeanClickEvent 컴포넌트 확인 및 추가
                if (!TryGetComponent<LeanClickEvent>(out leanClickEvent))
                {
                    leanClickEvent = gameObject.AddComponent<LeanClickEvent>();
                    // Debug.Log($"[HiddenObj.Awake] {gameObject.name}: Added LeanClickEvent component");
                }

                if (UISprite == null)
                {
                    if (TryGetComponent(out spriteRenderer) && spriteRenderer.sprite != null)
                    {
                        UISprite = spriteRenderer.sprite;
                    }
                    else
                    {
                        // Try to find a SpriteRenderer in children (some prefabs use child renderers)
                        var childSr = GetComponentInChildren<SpriteRenderer>();
                        if (childSr != null && childSr.sprite != null)
                        {
                            spriteRenderer = childSr;
                            UISprite = childSr.sprite;
                        }
                    }
                }

                // UIChangeHelper 컴포넌트 자동 찾기
                if (uiChangeHelper == null)
                {
                    uiChangeHelper = GetComponent<UIChangeHelper>();
                }

                // WhenFoundEventHelper 컴포넌트 자동 찾기
                if (whenFoundEventHelper == null)
                {
                    whenFoundEventHelper = GetComponent<WhenFoundEventHelper>();
                }

                if (BgAnimationSpriteRenderer == null && BgAnimationTransform != null)
                {
                    BgAnimationSpriteRenderer = BgAnimationTransform.GetComponentInChildren<SpriteRenderer>();

                    BgAnimationLerp = BgAnimationTransform.GetComponent<BGScaleLerp>();
                }

                // Cache the UI sprite value BEFORE potentially hiding the object
                // (avoids relying on active state for sprite access)
                cachedUISprite = (uiChangeHelper != null && uiChangeHelper.sprite != null) ? uiChangeHelper.sprite : UISprite;

                // HideOnStart는 모든 초기화 후에 마지막으로 실행
                if (HideOnStart)
                {
                    gameObject.SetActive(false);
                }

                if (cachedUISprite == null)
                {
                    Debug.LogWarning($"[HiddenObj.Awake] No UI sprite cached for {gameObject.name}. uiChangeHelper: {(uiChangeHelper != null)}, UISprite set: {(UISprite != null)}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[HiddenObj.Awake] Exception in {gameObject.name}: {e.Message}\n{e.StackTrace}");
            }
        }

        private void Start()
        {
            // LeanClickEvent의 OnClickEvent에 HitHiddenObject 연결
            if (leanClickEvent != null && leanClickEvent.OnClickEvent != null)
            {
                leanClickEvent.OnClickEvent.AddListener(HitHiddenObject);
                // Debug.Log($"[HiddenObj.Start] {gameObject.name}: OnClickEvent listener added successfully");
            }
            else
            {
                Debug.LogWarning($"[HiddenObj.Start] {gameObject.name}: leanClickEvent or OnClickEvent is NULL!");
            }

            if(TryGetComponent(out BGAnimationHelper bGAnimationHelper))
            {
                if(!bGAnimationHelper.UseBgAnimation)
                {
                    EnableBGAnimation = false;
                    if (BgAnimationTransform != null)
                    {
                        BgAnimationTransform.gameObject.SetActive(false);
                    }
                }
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
    }
}