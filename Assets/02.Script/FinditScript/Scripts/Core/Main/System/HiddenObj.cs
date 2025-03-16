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


        private void Awake()
        {
            if (UISprite == null)
            {
                if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                {
                    UISprite = spriteRenderer.sprite;
                }
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

    }
}