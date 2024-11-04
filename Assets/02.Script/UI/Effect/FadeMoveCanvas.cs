using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using DG.Tweening;
using UnityEngine;

namespace UI.Effect
{
    public class FadeMoveCanvas : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        private Vector2 originVec;
        private RectTransform r;
        void CheckInitial()
        {
            if(canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>();
            r = canvasGroup.transform.GetComponent<RectTransform>();
            originVec = r.localPosition;
        }
        void OnEnable()
        {
            if(r == null) CheckInitial();
            r.localPosition = originVec;
            canvasGroup.alpha = 0;

            //* 이동 효과
            r.DOLocalMoveY(originVec.y + 1.25f, 1f).SetEase(Ease.OutExpo);

            //* 페이드 효과
            canvasGroup.DOFade(1, 0.5f).SetEase(Ease.InSine);
            canvasGroup.DOFade(0, 0.5f).SetEase(Ease.InSine).SetDelay(1.5f);
        }

    }
}