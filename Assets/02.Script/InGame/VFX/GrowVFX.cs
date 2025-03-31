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
        [SerializeField, Range(0.1f, 1f)] private float startScale = 0.3f;
        [SerializeField, Range(0.1f, 2f)] private float growthDuration = 0.5f;
        [SerializeField] private Ease growthEase = Ease.OutBack;
        
        private Vector3 originalScale;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            originalScale = transform.localScale;
        }
        
        protected override async UniTask VFXOnceInGame()
        {
            await base.VFXOnceInGame();
            
            try 
            {
                // 시작 크기로 설정
                transform.localScale = originalScale * startScale;
                
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
