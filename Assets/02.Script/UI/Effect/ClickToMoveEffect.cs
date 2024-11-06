using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BunnyCafe.InGame.VFX;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace UI.Effect
{
    public class ClickToMoveEffect : ChainEffect
    {
        [BoxGroup("클릭 이펙트 설정")][LabelText("이동하고 나서 작아지게 될 것인가")][SerializeField] private bool isSmallOnEnd = false;
        [BoxGroup("클릭 이펙트 설정")]
        [LabelText("작아지는 시점 (1이면 이동 끝나고 나서, 0이면 시작시 바로 작아짐)")]
        [EnableIf("isSmallOnEnd")]
        [SerializeField] private float smallTiming = 1;
        [BoxGroup("클릭 이펙트 설정")][LabelText("대상 이동 위치")][SerializeField] private Vector3 targetPosition;
        [BoxGroup("클릭 이펙트 설정")][LabelText("다음 실행시 복귀 효과 재생")][SerializeField] private bool isResetOnNext = false;
        [BoxGroup("클릭 이펙트 설정")][LabelText("이미 복귀 효과가 재생되었는지 체크")][SerializeField] private bool isResetedThisTime = false;
        [BoxGroup("클릭 이펙트 설정")][LabelText("시작 시 복귀 효과에서 시작")][SerializeField] private bool isResetOnStart = false;

        private Vector3 startPosition;
        private bool isInitialized = false;  // 초기화 여부를 체크하는 플래그
        protected override void Start()
        {
            base.Start();
            Initial();
            
            // 시작 시 복귀 위치로 이동
            if (isResetOnStart)
            {
                if (isUIEffect)
                {
                    var rectTransform = GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = targetPosition;
                    if (isSmallOnEnd)
                    {
                        rectTransform.localScale = Vector3.zero;
                    }
                }
                else
                {
                    transform.localPosition = targetPosition;
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
                    startPosition = GetComponent<RectTransform>().anchoredPosition;
                }
                else
                {
                    startPosition = transform.localPosition;
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
                await ResetPositionTask();
            }
            else
            {
                isResetedThisTime = false;
                await UniTask.WhenAll(
                    transform.DOLocalMove(targetPosition * (effectAddValue + 1), 1 * effectSpeed).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token),
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
                await ResetPositionTask();
            }
            else
            {
                await UniTask.WhenAll(
                    rectTransform.DOAnchorPos(targetPosition * (effectAddValue + 1), 1 * effectSpeed).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token),
                    isSmallOnEnd ? rectTransform.DOScale(0, 0.5f).SetEase(Ease.OutCubic).SetDelay(1 * effectSpeed * smallTiming).WithCancellation(destroyCancellation.Token) : UniTask.CompletedTask
                );
                isResetedThisTime = false;
            }
        }
        private async UniTask ResetPositionTask()
        {
            if (isUIEffect)
            {
                var rectTransform = GetComponent<RectTransform>();
                await UniTask.WhenAll(
                    rectTransform.DOAnchorPos(startPosition * (effectAddValue + 1), 1 * effectSpeed).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token),
                    isSmallOnEnd ? rectTransform.DOScale(1, 0.5f).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token) : UniTask.CompletedTask
                );
            }
            else
            {
                await UniTask.WhenAll(
                    transform.DOLocalMove(startPosition * (effectAddValue + 1), 1 * effectSpeed).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token),
                    isSmallOnEnd ? transform.DOScale(1, 0.5f).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token) : UniTask.CompletedTask
              );
            }
        }
    }
}