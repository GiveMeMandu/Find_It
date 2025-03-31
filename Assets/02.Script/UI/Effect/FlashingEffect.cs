using System;
using System.Collections;
using System.Collections.Generic;
using Effect;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Effect
{
    public class FlashingEffect : VFXObject
    {
        [BoxGroup("플래시 이펙트 설정")][LabelText("플래시 시간")][SerializeField] private float flashTime = 0.5f;

        private Image image;
        private SpriteRenderer sr;

        protected override void OnEnable() {
            base.OnEnable();
            if(isUIEffect)
            {
                image = GetComponent<Image>();
            }
            else
            {
                sr = GetComponent<SpriteRenderer>();
            }
        }
        protected override async UniTask VFXOnceInGame()
        {
            await sr.DOFade(0f, 0f).WithCancellation(destroyCancellation.Token);
            await UniTask.Delay(TimeSpan.FromSeconds(flashTime), true, cancellationToken:destroyCancellation.Token);
            await sr.DOFade(1, 0f).WithCancellation(destroyCancellation.Token);
        }
        protected override async UniTask VFXOnceUI()
        {
            await image.DOFade(0f, 0f).WithCancellation(destroyCancellation.Token);
            await UniTask.Delay(TimeSpan.FromSeconds(flashTime), true, cancellationToken:destroyCancellation.Token);
            await image.DOFade(1, 0f).WithCancellation(destroyCancellation.Token);
        }
    }
}