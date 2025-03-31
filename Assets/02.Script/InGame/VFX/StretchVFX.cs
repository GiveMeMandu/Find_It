using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Effect
{
    public class StretchVFX : VFXObject
    {
        [Header("Stretch Settings")]
        [SerializeField, Range(1f, 3f)] private float horizontalStretchMultiplier = 1.5f;
        [SerializeField, Range(1f, 3f)] private float verticalStretchMultiplier = 1.5f;
        [SerializeField, Range(0.01f, 1f)] private float stretchDuration = 0.1f;
        [SerializeField, Range(0f, 1f)] private float yAxisDelay = 0f;
        
        private Vector3 originalScale;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            originalScale = transform.localScale;
        }
        
        protected override async UniTask VFXOnceInGame()
        {
            await base.VFXOnceInGame();
            
            transform.localScale = originalScale;
            
            try 
            {
                // x축 늘어나기
                await transform.DOScaleX(originalScale.x * horizontalStretchMultiplier, stretchDuration)
                    .WithCancellation(destroyCancellation.Token);
                    
                // y축 지연 적용
                if (yAxisDelay > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(yAxisDelay), 
                        cancellationToken: destroyCancellation.Token);
                }
                    
                // y축 늘어나기
                await transform.DOScaleY(originalScale.y * verticalStretchMultiplier, stretchDuration)
                    .WithCancellation(destroyCancellation.Token);
                    
                // x, y축 동시에 원래 크기로
                await UniTask.WhenAll(
                    transform.DOScaleX(originalScale.x, stretchDuration)
                        .WithCancellation(destroyCancellation.Token),
                    transform.DOScaleY(originalScale.y, stretchDuration)
                        .WithCancellation(destroyCancellation.Token)
                );
            }
            finally 
            {
                if (this != null && transform != null) // 오브젝트가 아직 존재하는지 확인
                {
                    transform.localScale = originalScale;
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            transform.DOKill(); // 모든 DOTween 애니메이션 중지
            transform.localScale = originalScale;
        }
    }
}
