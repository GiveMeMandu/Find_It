using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace InGame
{
    public class Treasure : AutoTaskControl
    {
        [SerializeField] private Transform openedTreasure;
        private Animator animator;
        private NightObj nightObj;
        private void Start()
        {
            animator = GetComponent<Animator>();
            nightObj = GetComponent<NightObj>();
        }
        public void AfterFind()
        {
            if(!nightObj.isNight) animator.Play("AfterFind");
            else animator.Play("AfterFindNight");
        }
        public void Opended()
        {
            Floating().Forget();
        }
        private async UniTaskVoid Floating()
        {
            while (true)
            {
                await openedTreasure.DOLocalMoveY(0.312f, 1).WithCancellation(destroyCancellationToken);
                await openedTreasure.DOLocalMoveY(0.376f, 1).WithCancellation(destroyCancellationToken);
            }
        }
    }
}