using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace InGame
{
    public class AnimationObj : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private UnityEvent OnAnimationComplete;
        [SerializeField] private UnityEvent<string> OnAnimationCompleteWithName;
        
        private string curAnim;
        private bool isTrackingAnimation = false;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
            if (_animator == null)
            {
                Debug.LogError("Animator component not found on this object or its children.");
            }
        }

        public void ChangeAnimation(string name, float crossFade = 0, float time = 0, bool force = false)
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
                // force가 true면 현재 애니메이션과 동일하더라도 강제로 재생
                if (force || curAnim != name)
                {
                    curAnim = name;
                    if (string.IsNullOrEmpty(curAnim))
                    {
                        CheckAnimtion();
                    }
                    else
                    {
                        if (_animator == null)
                        {
                            Debug.LogWarning("Animator is null. Cannot play animation: " + name);
                            return;
                        }

                        if (force)
                        {
                            // force가 true면 처음부터 재생 (normalizedTime을 0으로 설정)
                            _animator.Play(name, -1, 0f);
                        }
                        else
                        {
                            // 일반적인 경우 CrossFade 사용
                            _animator.CrossFade(name, crossFade);
                        }
                        
                        // 애니메이션 완료 추적 시작
                        StartTrackingAnimation(name);
                    }
                }
            }
        }
        public void RePlayCurAnimation()
        {
            if (_animator == null)
            {
                Debug.LogWarning("Animator is null. Cannot replay current animation.");
                return;
            }
            if (string.IsNullOrEmpty(curAnim))
            {
                Debug.LogWarning("curAnim is empty. Nothing to replay.");
                return;
            }
            _animator.Play(curAnim);
            // 애니메이션 완료 추적 시작
            StartTrackingAnimation(curAnim);
        }
        
        public virtual void CheckAnimtion()
        {

        }
        
        /// <summary>
        /// 애니메이션 완료 추적 시작
        /// </summary>
        private void StartTrackingAnimation(string animationName)
        {
            if (!isTrackingAnimation)
            {
                UniTask.Void(async () => await TrackAnimationCompletion(animationName));
            }
        }
        
        /// <summary>
        /// 애니메이션 완료를 추적하는 비동기 메서드
        /// </summary>
        private async UniTask TrackAnimationCompletion(string animationName)
        {
            if (_animator == null) return;
            
            isTrackingAnimation = true;
            
            // 애니메이션이 시작될 때까지 잠시 대기
            await UniTask.DelayFrame(1);
            
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            
            // 애니메이션이 완료될 때까지 대기
            while (_animator != null && 
                   _animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) &&
                   _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                await UniTask.DelayFrame(1);
            }
            
            isTrackingAnimation = false;
            
            // 애니메이션 완료 이벤트 호출
            OnAnimationComplete?.Invoke();
            OnAnimationCompleteWithName?.Invoke(animationName);
        }
    }
}