using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Effect
{
    public class RingVFX : VFXObject
    {
        [Header("Ring Shake Settings")]
        [SerializeField, Range(0.1f, 2f)] private float shakeDuration = 0.5f;
        [SerializeField, Range(0.1f, 90f)] private float shakeStrength = 45f;
        [SerializeField, Range(1, 20)] private int vibrato = 10;
        [SerializeField] private float randomness = 90f;
        [SerializeField] private bool fadeOut = true;
        
        private Vector3 originalScale;
        private Quaternion originalRotation;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            originalScale = transform.localScale;
            originalRotation = transform.rotation;
        }
        
        protected override async UniTask VFXOnceInGame()
        {
            await base.VFXOnceInGame();
            
            try 
            {
                var sequence = transform.DOShakeRotation(
                    shakeDuration,
                    new Vector3(0, 0, shakeStrength),
                    vibrato,
                    randomness,
                    fadeOut
                );
                
                destroyCancellation.Token.Register(() => {
                    sequence?.Kill();
                    if (this != null && transform != null)
                    {
                        transform.rotation = originalRotation;
                    }
                });
                
                await sequence.WithCancellation(destroyCancellation.Token);
            }
            finally 
            {
                if (this != null && transform != null)
                {
                    transform.rotation = originalRotation;
                }
            }
        }

        protected override void OnDisable()
        {
            transform.DOKill(true);
            transform.rotation = originalRotation;
            StopVFX();
            base.OnDisable();
        }
    }
}
