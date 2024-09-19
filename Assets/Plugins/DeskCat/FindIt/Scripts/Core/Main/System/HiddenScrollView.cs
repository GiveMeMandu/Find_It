using System;
using System.Collections.Generic;
using System.Linq;
using DeskCat.FindIt.Scripts.Core.Model;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public class HiddenScrollView : MonoBehaviour
    {
        public GameObject mainPanel;
        public Transform contentContainer;

        public DialogToast dialogToast;

        public Animator scrollAnimator;
        public string hideAnimationName;
        public string showAnimationName;

        private bool _isHiding;

        public Action UIClickAction;

        public void Initialize()
        {
            mainPanel.SetActive(true);
        }

        private void ToggleAnimation()
        {
            var animName = _isHiding ? showAnimationName : hideAnimationName;
            scrollAnimator.Play(animName);
            _isHiding = !_isHiding;
        }

        public void UpdateScrollView(
            Dictionary<Guid, HiddenObj> targetObjDic,
            GameObject targetImagePrefab,
            Action<Guid> targetClick,
            Action<Guid> regionToggle,
            Action uiClick)
        {
            foreach (Transform obj in contentContainer.transform)
            {
                Destroy(obj.gameObject);
            }

            foreach (var target in targetObjDic)
            {
                GameObject img = Instantiate(targetImagePrefab, contentContainer.transform);
                var imgObj = img.GetComponent<HiddenObjUI>();
                imgObj.Initialize(target.Value.UISprite);

                if (target.Value.EnableTooltip)
                {
                    imgObj.InitializeTooltips(target.Value.Tooltips, target.Value.TooltipsType);
                    imgObj.UIClickEvent = DisplayTooltips;
                }

                target.Value.TargetClickAction = () => { targetClick(target.Key); };
                target.Value.DragRegionAction = () => { regionToggle(target.Key); };
            }

            UIClickAction += uiClick;
        }

        private void DisplayTooltips(List<MultiLanguageTextListModel> multiLanguageTextList, TooltipsType tooltipsType,
            int clickCount, Transform uiTransform)
        {
            UIClickAction?.Invoke();
            
            var currentLanguage = GlobalSetting.CurrentLanguage;
            var targetTooltipsList = multiLanguageTextList.FirstOrDefault(x => x.LanguageKey == currentLanguage)?.Value;

            if (targetTooltipsList == null) {
                return;
            }
            
            switch(tooltipsType)
            {
                case TooltipsType.Single:
                    dialogToast.Initialize(targetTooltipsList[0], uiTransform);
                    
                    break;
                case TooltipsType.Random:
                    dialogToast.Initialize(targetTooltipsList[Random.Range(0, targetTooltipsList.Count)], uiTransform);

                    break;
                case TooltipsType.Incremental:
                    dialogToast.Initialize(targetTooltipsList[clickCount%targetTooltipsList.Count], uiTransform);
                    break;
            }
        }
    }
}