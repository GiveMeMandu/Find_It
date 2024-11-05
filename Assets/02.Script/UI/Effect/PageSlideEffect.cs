using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;
using Sirenix.OdinInspector;


namespace UI.Effect
{
    public class PageSlideEffect : MonoBehaviour
    {
        [SerializeField] [LabelText("시작시 실행")] private bool isPlayEnable = false;
        [SerializeField] [LabelText("슬라이드 될 방향이 행인가")] private bool isSlideHorizontal = true;
        [SerializeField] [LabelText("역으로 오는가")] private bool isReverse = true;
        [SerializeField] [LabelText("회전할건가")] private bool isRotate = true;

        [LabelText("이펙트 타입")]
        [SerializeField] private Ease easeType = Ease.Linear;
        private RectTransform r;
        void Start()
        {
            r = transform.GetComponent<RectTransform>();
        }
        void OnEnable()
        {
            SlideIn();
        }
        public void SlideOut(bool isForceReverse = false)
        {
            int isReverseValue = 1;

            if(isForceReverse) isReverseValue = -1;

            if (isSlideHorizontal)
            {
                r.DOLocalMoveX(0, 0);
                r.DOLocalMoveX(1920 * isReverseValue, 0.2f).SetEase(easeType);
            }
            else
            {
                r.DOLocalMoveY(0, 0f);
                r.DOLocalMoveY(1080 * isReverseValue, 0.2f).SetEase(easeType);
            }
        }

        public void SlideIn(bool isForceReverse = false)
        {
            if(!isPlayEnable) return;

            int isReverseValue = isReverse ? -1 : 1;

            if(isForceReverse) isReverseValue = -1;

            if (isSlideHorizontal)
            {
                r.DOLocalMoveX(1920 * isReverseValue, 0);
                r.DOLocalMoveX(0, 0.2f).SetEase(easeType);
            }
            else
            {
                r.DOLocalMoveY(1080 * isReverseValue, 0);
                r.DOLocalMoveY(0, 0.2f).SetEase(easeType);
            }

            if (isRotate)
            {
                r.DORotate(Vector3.zero, 0);
                r.DORotate(new Vector3(0, 0, -2), 0.25f).SetEase(Ease.InOutQuad);
                r.DORotate(Vector3.zero, 0.4f).SetEase(Ease.InOutQuad).SetDelay(0.25f);
                r.DOScale(1, 0.35f).SetEase(Ease.OutBack);
            }
        }
    }
}