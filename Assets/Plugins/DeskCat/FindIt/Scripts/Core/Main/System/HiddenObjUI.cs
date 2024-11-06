using System;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Model;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public class HiddenObjUI : MonoBehaviour
    {
        [LabelText("찾았을 때 배경이 바뀔 이미지")] [SerializeField] private Sprite backGroundToChange;
        public Image backGround;
        public Image targetSprite;
        public Image FoundSprite;
        private bool isEnableTooltips;
        private List<MultiLanguageTextListModel> uiTooltipsListModel;
        private TooltipsType tooltipsType;
        private int clickCount = 0;
        
        public UnityEvent OnUIClickEvent;
        public Action<List<MultiLanguageTextListModel>,TooltipsType, int, Transform> UIClickEvent;
        
        public void Initialize(Sprite sprite)
        {
            targetSprite.sprite = sprite;
            FoundSprite.gameObject.SetActive(false);
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
            if (isEnableTooltips) {
                UIClickEvent?.Invoke(uiTooltipsListModel, tooltipsType, clickCount++, transform);
            }
        }
        public void Found()
        {
            FoundSprite.gameObject.SetActive(true);
            if(backGround != null && backGroundToChange != null) {
                backGround.sprite = backGroundToChange;
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