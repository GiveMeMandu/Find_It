using System.Collections;
using System.Collections.Generic;
using BunnyCafe.InGame.VFX;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Effect
{
    public class StretchEffect : VFXObject
    {
        protected override async UniTask VFXOnceInGame()
        {
            await transform.DOScaleX(1.1f + effectAddValue, 0.1f * effectSpeed).SetEase(Ease.OutSine).WithCancellation(destroyCancellation.Token);
            await UniTask.WhenAll(
                transform.DOScaleX(1f, 0.1f * effectSpeed).SetEase(Ease.OutSine).WithCancellation(destroyCancellation.Token),
                transform.DOScaleY(1.1f + effectAddValue, 0.1f * effectSpeed).SetEase(Ease.OutSine).WithCancellation(destroyCancellation.Token)
            );
            await transform.DOScaleY(1f, 0.15f * effectSpeed).SetEase(Ease.OutSine).WithCancellation(destroyCancellation.Token);
        }
        protected override async UniTask VFXOnceUI()
        {
            var rectTransform = GetComponent<RectTransform>();
            await rectTransform.DOScaleX(1.1f + effectAddValue, 0.1f * effectSpeed).SetEase(Ease.OutSine).WithCancellation(destroyCancellation.Token);
            await UniTask.WhenAll(
                rectTransform.DOScaleX(1f, 0.1f).SetEase(Ease.OutSine).WithCancellation(destroyCancellation.Token),
                rectTransform.DOScaleY(1.1f + effectAddValue, 0.1f * effectSpeed).SetEase(Ease.OutSine).WithCancellation(destroyCancellation.Token)
            );
            await rectTransform.DOScaleY(1f, 0.15f * effectSpeed).SetEase(Ease.OutSine).WithCancellation(destroyCancellation.Token);
        }
    }
}