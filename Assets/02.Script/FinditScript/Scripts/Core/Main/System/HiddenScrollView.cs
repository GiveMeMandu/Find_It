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

        [Header("Time Challenge")]
        [Tooltip("타임챌린지 모드에서 표시할 시간 패널")]
        public GameObject TimeChallengePanel;
        [Tooltip("찾은 개수 표시 UI (비활성화 대상)")]
        public GameObject CountCircle;

        public DialogToast dialogToast;

        public Animator scrollAnimator;
        public string hideAnimationName;
        public string showAnimationName;

        private bool _isHiding;

        public Action UIClickAction;

        /// <summary>
        /// 타임챌린지 모드 전환: contentContainer와 CountCircle을 숨기고 TimeChallengePanel을 표시합니다.
        /// </summary>
        public void SetTimeChallengeMode(bool enabled)
        {
            if (contentContainer != null)
                contentContainer.gameObject.SetActive(!enabled);

            if (CountCircle != null)
                CountCircle.SetActive(!enabled);

            if (TimeChallengePanel != null)
                TimeChallengePanel.SetActive(enabled);

            Debug.Log($"[HiddenScrollView] TimeChallengeMode: {enabled}");
        }

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

        public List<HiddenObjUI> UpdateScrollView(
            Dictionary<Guid, HiddenObjGroup> objDic,
            GameObject prefab, 
            Action<Guid> targetClick, 
            Action<Guid> regionToggle, 
            Action uiClick)
        {
            // Null 체크 추가
            if (contentContainer == null)
            {
                Debug.LogError("[HiddenScrollView] contentContainer is null!");
                return new List<HiddenObjUI>();
            }

            if (prefab == null)
            {
                Debug.LogError("[HiddenScrollView] prefab is null!");
                return new List<HiddenObjUI>();
            }

            if (objDic == null)
            {
                Debug.LogError("[HiddenScrollView] objDic is null!");
                return new List<HiddenObjUI>();
            }

            foreach (Transform obj in contentContainer.transform)
            {
                Destroy(obj.gameObject);
            }

            var createdUIs = new List<HiddenObjUI>();

            foreach (var pair in objDic)
            {
                var obj = pair.Value.Representative;
                GameObject img = Instantiate(prefab, contentContainer.transform);
                var imgObj = img.GetComponent<HiddenObjUI>();
                // UIChangeHelper의 스프라이트를 우선적으로 사용
                // Debug.Log("<color=green>GetUISprite called for: " + obj.GetUISprite() + " on object: " + obj.name + " is object active in hierarchy: " + obj.gameObject.activeInHierarchy + "</color>");
                imgObj.Initialize(obj.GetUISprite(), obj.GetUIColor());
                // Assign the represented HiddenObj so the UI knows which object it corresponds to
                imgObj.SetRepresentedHiddenObj(obj);
                
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
                
                // 생성된 UI를 리스트에 추가
                createdUIs.Add(imgObj);
            }

            UIClickAction += uiClick;
            return createdUIs;
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