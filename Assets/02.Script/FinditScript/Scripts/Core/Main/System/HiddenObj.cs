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

        [Tooltip("Enable debug logs for GetUISprite")]
        public bool debugGetUISprite = false;

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

                // UIChangeHelper 컴포넌트 자동 찾기 (자식까지 검색)
                if (uiChangeHelper == null)
                {
                    uiChangeHelper = GetComponentInChildren<UIChangeHelper>(true);
                }
                cachedUISprite = (uiChangeHelper != null && uiChangeHelper.sprite != null) ? uiChangeHelper.sprite : UISprite;
            }

            // 객체가 비활성화되어 있으면 UIChangeHelper의 최신 스프라이트로 캐시를 갱신한 뒤 반환
            if (!gameObject.activeInHierarchy)
            {
                if (uiChangeHelper == null)
                {
                    uiChangeHelper = GetComponentInChildren<UIChangeHelper>(true);
                }
                var inactiveSprite = (uiChangeHelper != null && uiChangeHelper.sprite != null) ? uiChangeHelper.sprite : cachedUISprite;
                if (inactiveSprite != null)
                {
                    cachedUISprite = inactiveSprite;
                }
                if (debugGetUISprite)
                {
                    string helperName = (uiChangeHelper != null && uiChangeHelper.sprite != null) ? uiChangeHelper.sprite.name : "null";
                    string uiName = UISprite != null ? UISprite.name : "null";
                    string cachedName = cachedUISprite != null ? cachedUISprite.name : "null";
                    Debug.Log($"[HiddenObj.GetUISprite] (INACTIVE) obj={gameObject.name} uiHelper.sprite={helperName} UISprite={uiName} cached={cachedName} returning={cachedName}");
                }
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
            if (debugGetUISprite)
            {
                string helperName = (uiChangeHelper != null && uiChangeHelper.sprite != null) ? uiChangeHelper.sprite.name : "null";
                string uiName = UISprite != null ? UISprite.name : "null";
                string cachedName = cachedUISprite != null ? cachedUISprite.name : "null";
                string returnName = currentSprite != null ? currentSprite.name : "null";
                Debug.Log($"[HiddenObj.GetUISprite] (ACTIVE) obj={gameObject.name} uiHelper.sprite={helperName} UISprite={uiName} cached={cachedName} returning={returnName}");
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
                // 이벤트 및 액션은 SetActive(false) 전에 먼저 호출
                // (부모가 꺼지면 자식 컴포넌트 이벤트도 영향받을 수 있으므로)
                OnFound?.Invoke();
                whenFoundEventHelper?.onFoundEvent?.Invoke();
                TargetClickAction?.Invoke();

                // 컬렉션 매칭 처리: 이름 또는 스프라이트가 일치하는 CollectionSO를 유저 데이터에 추가
                if (Global.CollectionManager != null)
                {
                    Global.CollectionManager.TryCollectFromHiddenObj(this);
                }

                if (EnableBGAnimation)
                {
                    BgAnimationTransform.gameObject.SetActive(true);
                }

                if (HideWhenFound)
                {
                    // BG 애니메이션이 있으면 즉시 부모를 끄지 않음
                    // BgAnimationLerp의 HideHiddenObjAfterDone이 애니메이션 완료 후 부모를 비활성화함
                    // (부모를 바로 끄면 자식인 BgAnimationTransform도 꺼져 코루틴이 중단됨)
                    if (EnableBGAnimation && BgAnimationLerp != null)
                    {
                        BgAnimationLerp.HideHiddenObjAfterDone = true;
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                }
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

                // UIChangeHelper 컴포넌트 자동 찾기 (자식까지 검색)
                if (uiChangeHelper == null)
                {
                    uiChangeHelper = GetComponentInChildren<UIChangeHelper>(true);
                }

                // 인스펙터에서의 혼동을 방지하기 위해, uiChangeHelper의 스프라이트를 우선적으로 UISprite 필드에 동기화합니다
                if (uiChangeHelper != null && uiChangeHelper.sprite != null)
                {
                    UISprite = uiChangeHelper.sprite;
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

            if (TryGetComponent(out BGAnimationHelper bGAnimationHelper))
            {
                if (!bGAnimationHelper.UseBgAnimation)
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