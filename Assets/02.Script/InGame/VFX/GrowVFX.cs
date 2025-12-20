using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Effect
{
    public class GrowVFX : VFXObject
    {
        [Header("Growth Settings")]
        [SerializeField, Range(0f, 1f)] private float startScale = 0.3f;
        [SerializeField] private bool useCustomStartScale = false;
        [SerializeField, Range(0f, 2f)] private float customStartScale = 0.3f;
        [SerializeField, Range(0.1f, 2f)] private float growthDuration = 0.5f;
        [SerializeField, Range(0f, 10f)] private float growthDelay = 0f;
        [SerializeField] private Ease growthEase = Ease.OutBack;
        
        private Vector3 originalScale;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            originalScale = transform.localScale;
            if (useCustomStartScale)
            {
                transform.localScale = originalScale * customStartScale;
            }
        }
        
        protected override async UniTask VFXOnceInGame()
        {
            await base.VFXOnceInGame();
            
            try 
            {
                // 시작 크기로 설정 (custom 선택 시 입력한 값 사용)
                var appliedStart = useCustomStartScale ? customStartScale : startScale;
                transform.localScale = originalScale * appliedStart;

                // Optional delay before growth (cancellable)
                if (growthDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(growthDelay), cancellationToken: destroyCancellation.Token);
                }

                // 원래 크기로 성장
                await transform.DOScale(originalScale, growthDuration)
                    .SetEase(growthEase)
                    .WithCancellation(destroyCancellation.Token);
            }
            finally 
            {
                if (this != null && transform != null)
                {
                    transform.localScale = originalScale;
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            transform.DOKill();
            transform.localScale = originalScale;
        }
    }
}
