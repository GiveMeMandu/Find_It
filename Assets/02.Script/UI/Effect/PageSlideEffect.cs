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
        [LabelText("슬라이드 될 방향이 행인가")]
        [SerializeField] private bool isSlideHorizontal = true;
        [LabelText("역으로 오는가")]
        [SerializeField] private bool isReverse = true;
        [LabelText("회전할건가")]
        [SerializeField] private bool isRotate = true;
        void OnEnable()
        {
            RectTransform r = transform.GetComponent<RectTransform>();

            int isReverseValue = isReverse ? -1 : 1;

            if (isSlideHorizontal)
            {
                r.DOLocalMoveX(1080 * isReverseValue, 0);
                r.DOLocalMoveX(-100, 0.7f).SetEase(Ease.OutExpo);
                r.DOLocalMoveX(0, 0.2f).SetEase(Ease.Linear).SetDelay(0.7f);
            }
            else
            {
                r.DOLocalMoveY(1920 * isReverseValue, 0);
                r.DOLocalMoveY(-4, 0.7f).SetEase(Ease.OutExpo);
                r.DOLocalMoveY(0, 0.2f).SetEase(Ease.Linear).SetDelay(0.7f);
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