using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace InGame
{
    [RequireComponent(typeof(Animator))]
    public class AnimationObj : MonoBehaviour
    {
        private Animator _animator;
        private string curAnim;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void ChangeAnimation(string name, float crossFade = 0, float time = 0)
        {
            if(time > 0) {
                // 내부 비동기 처리
                UniTask.Void(async () =>
                {
                    await UniTask.WaitForSeconds(time - crossFade); // time 만큼 기다림
                    Validate();
                });

            }else Validate();

            void Validate()
            {
                if (curAnim != name)
                {
                    curAnim = name;
                    if(curAnim == "")
                        CheckAnimtion();
                    else
                        _animator.CrossFade(name, crossFade);
                }
            }
        }
        public virtual void CheckAnimtion()
        {

        }
    }
}