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
        [SerializeField] [LabelText("효과 재생 시간")] private float effectTime = 0.2f;

        [LabelText("이펙트 타입")]
        [SerializeField] private Ease easeType = Ease.Linear;
        [SerializeField] private RectTransform r;
        void Start()
        {
            if(r == null) r = transform.GetComponent<RectTransform>();
        }
        void OnEnable()
        {
            if(isPlayEnable) SlideIn();
        }
        public void SlideOut(bool isForceReverse = false, float delay = 0f)
        {
            int isReverseValue = isReverse ? -1 : 1;

            if(isForceReverse) isReverseValue *= -1;

            var _effectTime = effectTime + delay;
            if (isSlideHorizontal)
            {
                r.DOLocalMoveX(0, 0);
                r.DOLocalMoveX(1920 * isReverseValue, _effectTime).SetEase(easeType);
            }
            else
            {
                r.DOLocalMoveY(0, 0f);
                r.DOLocalMoveY(1080 * isReverseValue, _effectTime).SetEase(easeType);
            }
        }

        public void SlideIn(bool isForceReverse = false, float delay = 0f)
        {
            int isReverseValue = isReverse ? -1 : 1;

            if(isForceReverse) isReverseValue *= -1;

            var _effectTime = effectTime + delay;
            if (isSlideHorizontal)
            {
                r.DOLocalMoveX(1920 * isReverseValue, 0);
                r.DOLocalMoveX(0, _effectTime).SetEase(easeType);
            }
            else
            {
                r.DOLocalMoveY(1080 * isReverseValue, 0);
                r.DOLocalMoveY(0, _effectTime).SetEase(easeType);
            }

            if (isRotate)
            {
                r.DORotate(Vector3.zero, 0);
                r.DORotate(new Vector3(0, 0, -2), effectTime).SetEase(Ease.InOutQuad);
                r.DORotate(Vector3.zero, effectTime).SetEase(Ease.InOutQuad).SetDelay(effectTime);
                r.DOScale(1, effectTime).SetEase(Ease.OutBack);
            }
        }
    }
}