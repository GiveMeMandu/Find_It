using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InGame
{
    public class WindObj : AutoTaskControl
    {
        [LabelText("바람 세기")]
        [SerializeField] private float windForce = 3;
        [LabelText("바람 유지 시간")]
        [SerializeField] private float windTime = 3;
        private Vector3 dir;
        protected override void OnEnable()
        {
            base.OnEnable();
            dir = new Vector3(0, 0, windForce);
            WindMove().Forget();
        }
        private async UniTaskVoid WindMove()
        {
            while(gameObject.activeSelf)
            {
                await transform.DORotate(dir, windTime).WithCancellation(destroyCancellationToken);
                await transform.DORotate(dir * -1, windTime).WithCancellation(destroyCancellationToken);
            }
        }
    }

}