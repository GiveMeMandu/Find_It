using System;
using System.Collections;
using System.Collections.Generic;
using BunnyCafe.InGame.VFX;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace UI.Effect
{
    public class ChainEffect : VFXObject
    {
        public enum ChainType {
            OnStart,
            OnEnable,
            OnDisable,
            OnEnd,
        }
        public enum ChainFireType {
            OnClick,
            OnFuntionCall,
        }
        [BoxGroup("체인 이펙트 설정")][LabelText("다른 이펙트를 실행시키는 시기")][SerializeField] private ChainType chainType;
        [BoxGroup("체인 이펙트 설정")][LabelText("해당 방식일 때 다른 이펙트 실행")][SerializeField] private ChainFireType chainFireType;
        [BoxGroup("체인 이펙트 설정")][LabelText("다른 이펙트를 실행시키는 오브젝트")][SerializeField] private List<VFXObject> otherVFXs = new List<VFXObject>();
        [BoxGroup("체인 이펙트 설정")][LabelText("다른 이펙트를 실행시킬 때 딜레이")][SerializeField] private float chainStartDelay = 0;
        [BoxGroup("체인 이펙트 설정")][LabelText("다른 이펙트들 체인 딜레이")][SerializeField] private float chainDelay = 0;

        private bool isClicked = false;
        protected override void Start() {
            base.Start();
            if(chainType == ChainType.OnStart) PlayOtherVFX().Forget();
        }
        protected override void OnEnable() {
            base.OnEnable();
            if(chainType == ChainType.OnEnable) PlayOtherVFX().Forget();
        }
        protected override void OnDisable() {
            base.OnDisable();
            if(chainType == ChainType.OnDisable) PlayOtherVFX().Forget();
        }
        protected override void OnVFXEnd()
        {
            base.OnVFXEnd();
            if(chainFireType == ChainFireType.OnFuntionCall) PlayOtherVFX().Forget();
            if(chainFireType == ChainFireType.OnClick) {
                if(isClicked)
                {
                    PlayOtherVFX().Forget();
                    isClicked = false;
                }
            }
        }
        public void PlayVFXByClick()
        {
            if(isPlaying) return;
            if (isClicked) return;
            isClicked = true;
            PlayVFX();
        }
        private async UniTask PlayOtherVFX()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(chainStartDelay), cancellationToken: destroyCancellation.Token);
            foreach(var otherVFX in otherVFXs) {
                otherVFX.PlayVFXAppend();
                await UniTask.Delay(TimeSpan.FromSeconds(chainDelay), cancellationToken: destroyCancellation.Token);
            }
        }
    }
}