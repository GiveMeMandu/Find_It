using System;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Model;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public class HiddenObjUI : MonoBehaviour
    {
        // Public reference to the HiddenObj this UI represents
        public HiddenObj RepresentedHiddenObj;

        [LabelText("찾았을 때 배경이 바뀔 이미지")][SerializeField] private Sprite backGroundToChange;
        [LabelText("실루엣 모드용 이미지")] public Image silhouetteImage;
        public Image backGround;
        public Image targetSprite;
        public Image FoundSprite;
        public Image countSprite;
        public TextMeshProUGUI countText;

        private int foundCount = 0;
        private int totalCount = 0;
        private bool isEnableTooltips;
        private List<MultiLanguageTextListModel> uiTooltipsListModel;
        private TooltipsType tooltipsType;
        private int clickCount = 0;

        public UnityEvent OnUIClickEvent;
        public Action<List<MultiLanguageTextListModel>, TooltipsType, int, Transform> UIClickEvent;

        private bool isSilhouetteMode = false;

        public void Initialize(Sprite sprite)
        {
            targetSprite.sprite = sprite;

            FoundSprite.gameObject.SetActive(false);
        }

        public void SetRepresentedHiddenObj(HiddenObj obj)
        {
            RepresentedHiddenObj = obj;
        }

        public void SetCount(int totalCount, int foundCount)
        {
            this.foundCount = foundCount;
            this.totalCount = totalCount;
            UpdateCountUI();

            // 남은 개수가 0이면 Found 상태로 변경
            if (foundCount >= totalCount)
            {
                Found();
            }
        }

        private void UpdateCountUI()
        {
            if (countText != null)
            {
                countText.text = string.Format("{0}/{1}", foundCount, totalCount);
            }

            if (countSprite != null)
            {
                countSprite.gameObject.SetActive(foundCount < totalCount);
            }
            
            if (isSilhouetteMode)
            {
                SetSilhouetteMode(foundCount < totalCount);
            }
        }

        public void InitializeTooltips(List<MultiLanguageTextListModel> tooltipsList, TooltipsType type)
        {
            isEnableTooltips = true;
            tooltipsType = type;
            uiTooltipsListModel = tooltipsList;
        }

        public void Click()
        {
            OnUIClickEvent?.Invoke();
            if (isEnableTooltips)
            {
                UIClickEvent?.Invoke(uiTooltipsListModel, tooltipsType, clickCount++, transform);
            }
        }

        public void Found()
        {
            if (FoundSprite.gameObject.activeSelf) return; // 이미 Found 상태면 무시

            FoundSprite.gameObject.SetActive(true);
            if (backGround != null && backGroundToChange != null)
            {
                backGround.sprite = backGroundToChange;
            }

            // 실루엣 모드에서 찾았을 때 원본 이미지로 복원
            if (silhouetteImage)
            {
                // SetSilhouetteMode(false);
            }
        }

        /// <summary>
        /// 실루엣 모드 활성화/비활성화
        /// </summary>
        public void SetSilhouetteMode(bool isActive)
        {
            isSilhouetteMode = isActive;
            SilhoutteModeImageSetup();
        }

        private void SilhoutteModeImageSetup()
        {
            if (silhouetteImage != null)
            {
                silhouetteImage.gameObject.SetActive(isSilhouetteMode);
                targetSprite.gameObject.SetActive(!isSilhouetteMode);

                if (isSilhouetteMode && targetSprite.sprite != null)
                {
                    // 원본 스프라이트를 실루엣 이미지에 복사하고 검은색으로 변경
                    silhouetteImage.sprite = targetSprite.sprite;
                }
            }
        }

        //! 김일 : 이딴 유니티 기본 함수 쓰면 안됨. 이건 진짜 인게임 함수인데 에셋 만든사람 왜 이렇게 했을까 하
        // public void OnPointerClick(PointerEventData eventData)
        // {
        //     OnUIClickEvent?.Invoke();
        //     if (isEnableTooltips) {
        //         UIClickEvent?.Invoke(uiTooltipsListModel, tooltipsType, clickCount++, transform);
        //     }
        // }
    }
}