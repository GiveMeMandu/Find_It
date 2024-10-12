using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class OnFinishAnimation : StateMachineBehaviour
    {
        [Header("바꿀 애니메이션")]
        [SerializeField] private string animation;
        [Header("페이드 정도")]
        [SerializeField] private float crossfade;
        [Header("지연 시간")]
        [SerializeField] private float delay = 0;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var length = stateInfo.length;
            if(delay != 0) length = delay;
            animator.GetComponent<AnimationObj>().ChangeAnimation(animation, crossfade, length);
        }
    }
}
