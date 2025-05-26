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
            Dictionary<Guid, HiddenObjGroup> objDic,
            GameObject prefab, 
            Action<Guid> targetClick, 
            Action<Guid> regionToggle, 
            Action uiClick)
        {
            foreach (Transform obj in contentContainer.transform)
            {
                Destroy(obj.gameObject);
            }

            foreach (var pair in objDic)
            {
                var obj = pair.Value.Representative;
                GameObject img = Instantiate(prefab, contentContainer.transform);
                var imgObj = img.GetComponent<HiddenObjUI>();
                imgObj.Initialize(obj.UISprite);
                
                // 남은 개수 설정 (전체 개수 - 찾은 개수)
                imgObj.SetCount(pair.Value.TotalCount, pair.Value.FoundCount);

                if (obj.EnableTooltip)
                {
                    if(obj.IsFound) {
                        imgObj.Found();
                    }
                    else {
                        imgObj.FoundSprite.gameObject.SetActive(false);
                        imgObj.InitializeTooltips(obj.Tooltips, obj.TooltipsType);
                        imgObj.UIClickEvent = DisplayTooltips;
                    }
                }

                // 클릭 이벤트 설정
                var guid = pair.Key;
                imgObj.OnUIClickEvent.AddListener(() => targetClick(guid));
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