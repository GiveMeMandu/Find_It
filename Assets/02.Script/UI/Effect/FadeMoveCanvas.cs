using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using DG.Tweening;
using UnityEngine;

namespace UI.Effect
{
    public class FadeMoveCanvas : MonoBehaviour
    {
        [SerializeField] private bool isMove = true;
        [SerializeField] private float playTime = 3f;
        [SerializeField] private float startDelay = 0f;
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

            if(isMove)
            {
                //* 이동 효과
                r.DOLocalMoveY(originVec.y + 1.25f, playTime).SetEase(Ease.OutExpo).SetDelay(startDelay);
            }

            //* 페이드 효과
            canvasGroup.DOFade(1, 0.5f).SetEase(Ease.InSine).SetDelay(startDelay);
            canvasGroup.DOFade(0, 0.5f).SetEase(Ease.InSine).SetDelay(playTime - 0.5f);
        }

    }
}