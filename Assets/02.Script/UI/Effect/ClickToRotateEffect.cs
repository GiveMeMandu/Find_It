using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Effect
{
    public class ClickToRotateEffect : ChainEffect
    {
        [BoxGroup("회전 이펙트 설정")][LabelText("회전하고 나서 스케일을 0으로 만들 것인가")][SerializeField] private bool isSmallOnEnd = false;
        [BoxGroup("회전 이펙트 설정")]
        [LabelText("작아지는 시점 (1이면 회전 끝나고 나서, 0이면 시작시 바로 작아짐)")]
        [EnableIf("isSmallOnEnd")]
        [SerializeField] private float smallTiming = 1;
        [BoxGroup("회전 이펙트 설정")][LabelText("대상 회전값")][SerializeField] private Vector3 targetRotation;
        [BoxGroup("회전 이펙트 설정")][LabelText("다음 실행시 복귀 효과 재생")][SerializeField] private bool isResetOnNext = false;
        [BoxGroup("회전 이펙트 설정")][LabelText("이미 복귀 효과가 재생되었는지 체크")][SerializeField] private bool isResetedThisTime = false;
        [BoxGroup("회전 이펙트 설정")][LabelText("시작 시 복귀 효과에서 시작")][SerializeField] private bool isResetOnStart = false;

        private Vector3 startRotation;
        private bool isInitialized = false;  // 초기화 여부를 체크하는 플래그
        
        protected override void Start()
        {
            base.Start();
            Initial();
            
            // 시작 시 복귀 위치로 회전
            if (isResetOnStart)
            {
                if (isUIEffect)
                {
                    var rectTransform = GetComponent<RectTransform>();
                    rectTransform.localRotation = Quaternion.Euler(targetRotation);
                    if (isSmallOnEnd)
                    {
                        rectTransform.localScale = Vector3.zero;
                    }
                }
                else
                {
                    transform.localRotation = Quaternion.Euler(targetRotation);
                    if (isSmallOnEnd)
                    {
                        transform.localScale = Vector3.zero;
                    }
                }
            }
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
        }
        
        private void Initial()
        {
            if (!isInitialized)
            {
                if (isUIEffect)
                {
                    startRotation = GetComponent<RectTransform>().localRotation.eulerAngles;
                }
                else
                {
                    startRotation = transform.localRotation.eulerAngles;
                }
                isInitialized = true;
            }
        }
        
        protected override async UniTask VFXOnceInGame()
        {
            // 이전 작업 취소
            destroyCancellation.Cancel();
            destroyCancellation = new CancellationTokenSource();

            if (isResetOnNext && !isResetedThisTime)
            {
                isResetedThisTime = true;
                await ResetRotationTask();
            }
            else
            {
                isResetedThisTime = false;
                await UniTask.WhenAll(
                    transform.DOLocalRotate(targetRotation * (effectAddValue + 1), 1 * effectSpeed, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token),
                    isSmallOnEnd ? transform.DOScale(0, 0.5f).SetEase(Ease.OutCubic).SetDelay(1 * effectSpeed * smallTiming).WithCancellation(destroyCancellation.Token) : UniTask.CompletedTask
                );
            }
        }
        
        protected override async UniTask VFXOnceUI()
        {
            destroyCancellation.Cancel();
            destroyCancellation = new CancellationTokenSource();

            var rectTransform = GetComponent<RectTransform>();
            if (isResetOnNext && !isResetedThisTime)
            {
                isResetedThisTime = true;
                await ResetRotationTask();
            }
            else
            {
                await UniTask.WhenAll(
                    rectTransform.DOLocalRotate(targetRotation * (effectAddValue + 1), 1 * effectSpeed, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token),
                    isSmallOnEnd ? rectTransform.DOScale(0, 0.5f).SetEase(Ease.OutCubic).SetDelay(1 * effectSpeed * smallTiming).WithCancellation(destroyCancellation.Token) : UniTask.CompletedTask
                );
                isResetedThisTime = false;
            }
        }
        
        private async UniTask ResetRotationTask()
        {
            if (isUIEffect)
            {
                var rectTransform = GetComponent<RectTransform>();
                await UniTask.WhenAll(
                    rectTransform.DOLocalRotate(startRotation * (effectAddValue + 1), 1 * effectSpeed, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token),
                    isSmallOnEnd ? rectTransform.DOScale(1, 0.5f).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token) : UniTask.CompletedTask
                );
            }
            else
            {
                await UniTask.WhenAll(
                    transform.DOLocalRotate(startRotation * (effectAddValue + 1), 1 * effectSpeed, RotateMode.FastBeyond360).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token),
                    isSmallOnEnd ? transform.DOScale(1, 0.5f).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token) : UniTask.CompletedTask
                );
            }
        }
    }
}
