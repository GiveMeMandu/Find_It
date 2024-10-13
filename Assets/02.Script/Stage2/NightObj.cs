using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;
using System;

namespace InGame
{
    [Serializable]
    public class DayNightObj{
        [SerializeField] [LabelText("아침 오브젝트들")]
        public SpriteRenderer DayObjs;
        [SerializeField] [LabelText("숨길 오브젝트")]
        public SpriteRenderer NightObjs;
    }
    public class NightObj : FoundObj
    {
        [SerializeField] [LabelText("밤낮 오브젝트들")] [EnableIf("isHideOnDay")]
        public List<DayNightObj> dayNightObj;
        [SerializeField] [LabelText("밤낮 꺼버려서 교체할 옵젝들")] [EnableIf("isHideOnDay")]
        public List<DayNightObj> dayNightEnableObj;
        [SerializeField] [LabelText("밤에 어두워질 옵젝들")] [EnableIf("isHideOnDay")]
        public List<SpriteRenderer> nightDarkObj;
        [LabelText("시작시 밤 오브젝트 숨길 것인가")]
        public bool isHideOnDay = false;


        [LabelText("시작시 아예 끌 것인가")]
        public bool isDisableOnStart = false;

        public bool isNight;
        private void Awake() {
            if(isHideOnDay)
            {
                foreach(var obj in dayNightObj) {
                    obj.NightObjs.gameObject.SetActive(false);   
                }
                foreach(var obj in dayNightEnableObj) {
                    obj.DayObjs.gameObject.SetActive(true);   
                    obj.NightObjs.gameObject.SetActive(false);   
                }
            }
        }
        public void OnNight()
        {
            if(isHideOnDay)
            {
                foreach(var obj in dayNightObj) {
                    obj.DayObjs.sprite = obj.NightObjs.sprite;
                    obj.DayObjs.flipX = obj.NightObjs.flipX;
                    obj.DayObjs.flipY = obj.NightObjs.flipY;
                    if (obj.DayObjs != null)
                    {
                        obj.DayObjs.DOFade(0, 0);
                        obj.DayObjs.DOFade(1, 4f).SetEase(Ease.OutQuart);
                    }
                }
                foreach(var obj in dayNightEnableObj) {
                    obj.DayObjs.gameObject.SetActive(false);   
                    obj.NightObjs.gameObject.SetActive(true);
                    if (obj.NightObjs != null)
                    {
                        obj.NightObjs.DOFade(0, 0);
                        obj.NightObjs.DOFade(1, 4f).SetEase(Ease.OutQuart);
                    }
                }
                foreach(var obj in nightDarkObj) {
                    obj.DOColor(new Color(1,1,1,0), 0);
                    obj.DOColor(new Color(175f/255f,175f/255f,175f/255f), 5f);
                }
            }
            isNight = true;
        }

    }
}