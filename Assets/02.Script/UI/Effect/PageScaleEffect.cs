using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;


namespace UI.Effect
{
    public class PageScaleEffect : MonoBehaviour
    {
        void OnEnable()
        {
            RectTransform r = transform.GetComponent<RectTransform>();
            Image i = transform.GetComponent<Image>();
            if(i != null)
            {
                i.DOColor(new Color(0, 0, 0, 0), 0);
                i.DOColor(new Color(1, 1, 1, 1), 0.2f).SetEase(Ease.InSine);
            }

            r.DOLocalMoveY(-1080, 0);
            r.DOLocalMoveY(4, 0.7f).SetEase(Ease.OutExpo);
            r.DOLocalMoveY(0, 0.2f).SetEase(Ease.Linear).SetDelay(0.7f);
            r.DORotate(Vector3.zero, 0);
            r.DORotate(new Vector3(0, 0, -2), 0.25f).SetEase(Ease.InOutQuad);
            r.DORotate(Vector3.zero, 0.4f).SetEase(Ease.InOutQuad).SetDelay(0.25f);
            r.DOScale(1, 0.35f).SetEase(Ease.OutBack);
        }
    }
}