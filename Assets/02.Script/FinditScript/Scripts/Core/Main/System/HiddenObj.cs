using System;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Main.Utility.Animation;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using Lean.Touch;
using SnowRabbit.Helper;
using Manager;

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

        [Tooltip("WhenFoundEventHelper component for custom found events")]
        public WhenFoundEventHelper whenFoundEventHelper;

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

        private void OnEnable()
        {
            // HiddenObj 터치 처리를 위한 LeanTouch 이벤트 등록
            LeanTouch.OnFingerTap += HandleFingerTap;
        }

        // HiddenObj는 독립적인 클릭 처리
        private void Start()
        {
            // HiddenObj 초기화
        }

        private void OnDisable()
        {
            // HiddenObj 터치 이벤트 해제
            LeanTouch.OnFingerTap -= HandleFingerTap;
        }

        private void OnDestroy()
        {
            // HiddenObj 터치 이벤트 해제
            LeanTouch.OnFingerTap -= HandleFingerTap;
        }

        // Lean Touch 탭 처리
        private void HandleFingerTap(LeanFinger finger)
        {
            if (IsFound) return;

            // InputManager의 isEnabled 상태 확인
            if (!IsInputEnabled()) return;

            // 이 오브젝트 위에 손가락이 있는지 확인
            if (IsFingerOverThis(finger))
            {
                HitHiddenObject();
            }
        }

        /// <summary>
        /// 손가락이 이 GameObject 위에 있는지 확인합니다.
        /// </summary>
        private bool IsFingerOverThis(LeanFinger finger)
        {
            // UI 체크
            if (EventSystem.current != null)
            {
                var ped = new PointerEventData(EventSystem.current)
                {
                    position = finger.ScreenPosition
                };

                var raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(ped, raycastResults);

                foreach (var result in raycastResults)
                {
                    if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                    {
                        return true;
                    }
                }
            }

            // 3D/2D Physics 체크
            var cam = Camera.main ?? Camera.current;
            if (cam != null)
            {
                var ray = cam.ScreenPointToRay(finger.ScreenPosition);

                // 3D 체크
                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.collider != null && (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)))
                    {
                        return true;
                    }
                }

                // 2D 체크
                var wp = cam.ScreenToWorldPoint(new Vector3(finger.ScreenPosition.x, finger.ScreenPosition.y, cam.nearClipPlane));
                var hit2d = Physics2D.OverlapPoint(wp);
                if (hit2d != null && (hit2d.gameObject == gameObject || hit2d.transform.IsChildOf(transform)))
                {
                    return true;
                }
            }

            return false;
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

                // WhenFoundEventHelper 이벤트 호출
                whenFoundEventHelper?.onFoundEvent?.Invoke();

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

            // WhenFoundEventHelper 컴포넌트 자동 찾기
            if (whenFoundEventHelper == null)
            {
                whenFoundEventHelper = GetComponent<WhenFoundEventHelper>();
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
            
            // InputManager의 isEnabled 상태 확인
            if (!IsInputEnabled()) return;
            
            HitHiddenObject();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"OnPointerClick called for {gameObject.name}");
            
            // InputManager의 isEnabled 상태 확인
            if (!IsInputEnabled()) return;
            
            // 이미 찾아진 경우 무시
            if (IsFound)
            {
                Debug.Log($"{gameObject.name} is already found, ignoring click");
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
        /// InputManager의 isEnabled 상태를 확인하는 헬퍼 메서드
        /// </summary>
        private bool IsInputEnabled()
        {
            return Global.InputManager != null && Global.InputManager.isEnabled;
        }
    }
}