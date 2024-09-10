using System;
using System.Collections.Generic;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public class HiddenObjUI : MonoBehaviour, IPointerClickHandler
    {
        public Image targetSprite;
        private bool isEnableTooltips;
        private List<MultiLanguageTextListModel> uiTooltipsListModel;
        private TooltipsType tooltipsType;
        private int clickCount = 0;
        
        public UnityEvent OnUIClickEvent;
        public Action<List<MultiLanguageTextListModel>,TooltipsType, int, Transform> UIClickEvent;
        
        public void Initialize(Sprite sprite)
        {
            targetSprite.sprite = sprite;
        }
        
        public void InitializeTooltips(List<MultiLanguageTextListModel> tooltipsList, TooltipsType type)
        {
            isEnableTooltips = true;
            tooltipsType = type;
            uiTooltipsListModel = tooltipsList;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnUIClickEvent?.Invoke();
            if (isEnableTooltips) {
                UIClickEvent?.Invoke(uiTooltipsListModel, tooltipsType, clickCount++, transform);
            }
        }
    }
} 