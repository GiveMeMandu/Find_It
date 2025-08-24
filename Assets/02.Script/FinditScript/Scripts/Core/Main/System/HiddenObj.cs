using System;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Main.Utility.Animation;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public class HiddenObj : MonoBehaviour, IPointerClickHandler
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

        private void Awake()
        {
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

        private void OnMouseDown()
        {
            HitHiddenObject();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 클릭 우선순위 체크
            if (!CheckClickPriority(eventData))
                return;
                
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


        private void HitHiddenObject()
        {
            if(IsFound == false) {
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
        /// 클릭 우선순위를 확인합니다. HiddenObj가 최우선 순위를 가집니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        /// <returns>HiddenObj가 클릭되어야 하는지 여부</returns>
        private bool CheckClickPriority(PointerEventData eventData)
        {
            // HiddenObj가 이미 찾아진 경우 클릭 무시
            if (IsFound)
                return false;

            // Raycast를 통해 모든 히트된 오브젝트들을 가져옴
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            if (raycastResults.Count == 0)
                return false;

            // HiddenObj 컴포넌트를 가진 오브젝트들 찾기
            List<GameObject> hiddenObjects = new List<GameObject>();
            foreach (var result in raycastResults)
            {
                var hiddenObj = result.gameObject.GetComponent<HiddenObj>();
                if (hiddenObj != null && !hiddenObj.IsFound)
                {
                    hiddenObjects.Add(result.gameObject);
                }
            }

            // HiddenObj가 없으면 다른 오브젝트들도 클릭 가능
            if (hiddenObjects.Count == 0)
                return false;

            // 여러 HiddenObj가 겹쳐있는 경우, 우선순위 결정
            GameObject topPriorityObject = GetTopPriorityHiddenObject(hiddenObjects);
            
            // 디버그 로그 (필요시 주석 해제)
            // Debug.Log($"[HiddenObj] Click Priority Check - Current: {gameObject.name}, TopPriority: {topPriorityObject.name}, Hidden objects found: {hiddenObjects.Count}");
            
            // 현재 오브젝트가 최우선 순위인지 확인
            return topPriorityObject == gameObject;
        }

        /// <summary>
        /// 여러 HiddenObj 중에서 최우선 순위 오브젝트를 결정합니다.
        /// </summary>
        /// <param name="hiddenObjects">HiddenObj 컴포넌트를 가진 오브젝트들</param>
        /// <returns>최우선 순위 오브젝트</returns>
        private GameObject GetTopPriorityHiddenObject(List<GameObject> hiddenObjects)
        {
            if (hiddenObjects.Count == 1)
                return hiddenObjects[0];

            GameObject topPriority = hiddenObjects[0];
            var topHiddenObj = topPriority.GetComponent<HiddenObj>();
            int highestClickPriority = topHiddenObj.clickPriority;
            float highestZ = topPriority.transform.position.z;
            int highestSortingOrder = GetSortingOrder(topPriority);
            
            // 클릭 우선순위 -> SortingOrder -> Z축 순으로 우선순위 결정
            for (int i = 1; i < hiddenObjects.Count; i++)
            {
                var currentObj = hiddenObjects[i];
                var currentHiddenObj = currentObj.GetComponent<HiddenObj>();
                int currentClickPriority = currentHiddenObj.clickPriority;
                float currentZ = currentObj.transform.position.z;
                int currentSortingOrder = GetSortingOrder(currentObj);
                
                // 1. 클릭 우선순위가 높을수록 우선순위
                if (currentClickPriority > highestClickPriority)
                {
                    topPriority = currentObj;
                    topHiddenObj = currentHiddenObj;
                    highestClickPriority = currentClickPriority;
                    highestZ = currentZ;
                    highestSortingOrder = currentSortingOrder;
                }
                // 2. 클릭 우선순위가 같으면 SortingOrder로 비교
                else if (currentClickPriority == highestClickPriority)
                {
                    if (currentSortingOrder > highestSortingOrder)
                    {
                        topPriority = currentObj;
                        topHiddenObj = currentHiddenObj;
                        highestZ = currentZ;
                        highestSortingOrder = currentSortingOrder;
                    }
                    // 3. SortingOrder도 같으면 Z축으로 비교 (Z값이 클수록 카메라에 가까움)
                    else if (currentSortingOrder == highestSortingOrder && currentZ > highestZ)
                    {
                        topPriority = currentObj;
                        topHiddenObj = currentHiddenObj;
                        highestZ = currentZ;
                    }
                }
            }
            
            return topPriority;
        }

        /// <summary>
        /// 오브젝트의 SortingOrder를 가져옵니다.
        /// </summary>
        /// <param name="obj">확인할 오브젝트</param>
        /// <returns>SortingOrder 값</returns>
        private int GetSortingOrder(GameObject obj)
        {
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                return spriteRenderer.sortingOrder;
            
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                return renderer.sortingOrder;
                
            return 0;
        }

    }
}