using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UI.Effect;
using UnityEngine;

namespace InGame
{
    public class WindObj : ChainEffect
    {
        [LabelText("바람 세기")]
        [SerializeField] private float windForce = 3;
        [LabelText("바람 유지 시간")]
        [SerializeField] private float windTime = 3;
        [LabelText("반복 대기 시간 (초)")]
        [SerializeField] private float pauseBetweenSets = 0f;
        [LabelText("회전 이징")]
        [SerializeField] private Ease rotateEase = Ease.Linear;
        [LabelText("회전 모드")]
        [SerializeField] private RotateMode rotateMode = RotateMode.Fast;
        private Vector3 dir;
        protected override void OnEnable()
        {
            base.OnEnable();
            dir = new Vector3(0, 0, windForce);
            WindMove().Forget();
        }
        private async UniTaskVoid WindMove()
        {
            try
            {
                while (gameObject.activeSelf)
                {
                    await transform.DORotate(dir, windTime, rotateMode).SetEase(rotateEase).WithCancellation(destroyCancellationToken);
                    await transform.DORotate(dir * -1, windTime, rotateMode).SetEase(rotateEase).WithCancellation(destroyCancellationToken);

                    if (pauseBetweenSets > 0f)
                    {
                        await UniTask.Delay(System.TimeSpan.FromSeconds(pauseBetweenSets), cancellationToken: destroyCancellationToken);
                    }
                    else
                    {
                        // keep default behavior (no extra wait) but yield once to avoid tight loop
                        await UniTask.Yield();
                    }

                    // A single set (two rotations + optional pause) finished -> notify
                    if (gameObject.activeSelf)
                    {
                        OnVFXEnd();
                    }
                }
            }
            catch (System.OperationCanceledException)
            {
                // expected when object is destroyed or disabled; swallow to allow cleanup
            }
        }
        [SerializeField] [Button("바람 랜덤 세기")]
        public void RandomWindForce()
        {
            windForce = Random.Range(1, 5);
            windTime = Random.Range(1, 5);
        }
    }

}