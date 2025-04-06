using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Effect
{
    public class VFXObject : AutoTaskControl
    {
        [BoxGroup("세부 설정")] [LabelText("시작시 재생")] [SerializeField] private bool isPlayOnStart = false;
        [BoxGroup("세부 설정")] [LabelText("활성화시 재생")] [SerializeField] private bool isPlayOnEnable = false;
        [BoxGroup("세부 설정")] [LabelText("재생시 다른 이펙트 멈춤")] [SerializeField] private bool isStopOtherEffect = false;
        [BoxGroup("세부 설정")] [LabelText("재생 시작 전 딜레이 (초)")] [SerializeField] private float startDelay = 0;
        [BoxGroup("세부 설정")] [LabelText("반복 재생시 딜레이 (초)")] [SerializeField] private float delay = 0;
        [BoxGroup("세부 설정")] [LabelText("재생시 무조건 무한루프")] [SerializeField] private bool isLoopForce = false;
        [BoxGroup("세부 설정")] [LabelText("UI 이펙트인가")] [SerializeField] protected bool isUIEffect = false;
        [BoxGroup("세부 설정")] [LabelText("효과 정도 가중치")] [SerializeField] protected float effectAddValue = 0;
        [BoxGroup("세부 설정")] [LabelText("효과 속도, 클수록 느려짐")] [SerializeField] protected float effectSpeed = 1;
        [BoxGroup("세부 설정")] [LabelText("재생중 재생 시도 금지")] [SerializeField] protected bool isPlayLock = false;


        [BoxGroup("이벤트 설정")] public UnityEvent OnEffectStart;
        [BoxGroup("이벤트 설정")] public UnityEvent OnEffectEnd;


        public bool isPlaying = false;
        private bool isPendingPlay = false;  // 대기 중인 재생 요청 플래그

        protected virtual void Start()
        {
            if(isPlayOnStart) PlayVFX();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            if(isPlayOnEnable) PlayVFX();
        }
        public void PlayVFX(bool isLoop = false, int loopCount = 0)
        {
            if (isPlayLock && isPlaying || isPendingPlay) return;
            PlayVFXTask(isLoop, loopCount).Forget();
        }
        public void PlayVFX()
        {
            if (isPlayLock && isPlaying || isPendingPlay) return;
            PlayVFXTask(false, 0).Forget();
        }

        public void PlayVFXAppend()
        {
            if (isPlaying)
            {
                isPendingPlay = true;  // 재생 대기 요청
                return;
            }else{
                PlayVFXTask(false, 0).Forget();
            }
        }

        public void PlayVFXForce()
        {
            PlayVFXTask(false, 0).Forget();
        }

        public void StopVFX()
        {
            StopAllTask();
            isPlaying = false;
        }

        private async UniTaskVoid PlayVFXTask(bool isLoop, int loopCount)
        {
            isPlaying = true;
            if(isStopOtherEffect)
            {
                // 현재 게임오브젝트의 모든 VFXObject 컴포넌트를 가져옵니다
                var otherEffects = GetComponents<VFXObject>();
                foreach(var effect in otherEffects)
                {
                    // 현재 실행중인 이펙트는 제외
                    if(effect != this)
                    {
                        effect.StopVFX();
                    }
                }
            }

            // 재생 시작 전 딜레이 적용
            if(startDelay > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(startDelay), cancellationToken: destroyCancellation.Token);
            }

            int loop = 0;
            if(isLoopForce) isLoop = true;
            do
            {
                await UniTask.Yield(cancellationToken: destroyCancellation.Token);
                OnEffectStart?.Invoke();
                if(isUIEffect) await VFXOnceUI();
                else await VFXOnceInGame();
                loop++;
                
                if(!isLoop) break;
                if(loopCount > 0 && loop >= loopCount) break;
                
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: destroyCancellation.Token);
            } while (true);
            OnVFXEnd();
        }
        
        //* VFX 재생시 재생될 효과들 디테일하게 구현
        protected virtual async UniTask VFXOnceInGame()
        {
            await UniTask.Yield();
        }
        protected virtual async UniTask VFXOnceUI()
        {
            await UniTask.Yield();
        }
        protected virtual void OnVFXEnd() {
            isPlaying = false;
            OnEffectEnd?.Invoke();
            
            // 대기 중인 재생 요청이 있다면 실행
            if (isPendingPlay)
            {
                isPendingPlay = false;
                PlayVFXTask(false, 0).Forget();
            }
        }
    }
}