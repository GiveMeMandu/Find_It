using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace InGame
{
    public class BigCarrot : MonoBehaviour
    {
        [SerializeField] private AnimationObj bunny1;
        [SerializeField] private AnimationObj bunny2;
        [SerializeField] private AnimationObj mole;
        //* 시작 시 토끼들이 뛰어옴
        public void StartBunniesRun()
        {
            mole.ChangeAnimation("Down");

            bunny1.ChangeAnimation("Walk");
            bunny2.ChangeAnimation("Walk");
        }
        public void EndBunniesRun()
        {
            bunny1.ChangeAnimation("Idle", 0.2f);
            bunny2.ChangeAnimation("Idle", 0.2f);
            UniTask.Void( async () => 
            {
                await UniTask.WaitForSeconds(2);
                BunnyCarrotUp();
            });
        }
        public void BunnyCarrotUp()
        {
            bunny1.ChangeAnimation("CarrotUp", 0.2f);
            bunny2.ChangeAnimation("CarrotUp", 0.2f);
            UniTask.Void( async () => 
            {
                await UniTask.WaitForSeconds(2.5f);
                mole.ChangeAnimation("UPKnockDown");
                UniTask.Void(async () =>
                {
                    await UniTask.WaitForSeconds(2.5f);
                    bunny1.ChangeAnimation("Idle", 0.2f);
                    bunny2.ChangeAnimation("Idle", 0.2f);

                    await UniTask.WaitForSeconds(1f);
                    mole.ChangeAnimation("Angry");
                    await UniTask.WaitForSeconds(2f);
                    bunny1.ChangeAnimation("Jump", 0.2f);
                    
                    mole.ChangeAnimation("KnockDown", 0.2f, 0.5f);
                    await UniTask.WaitForSeconds(1.15f);

                    bunny1.ChangeAnimation("Walk", 0.2f);
                    bunny1.ChangeAnimation("Idle", 0.2f, 1.25f);
                });
            });
        }
    }
}