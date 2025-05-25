using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

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
        [BoxGroup("클릭 이펙트 설정")][LabelText("앵커 기준점 사용 (체크 시 앵커에 따라 위치 조정)")][SerializeField] private bool useAnchorReference = true;

        private Vector3 startPosition;
        private Vector2 anchorReference = new Vector2(0.5f, 0.5f); // 기준 앵커 (중앙)
        private bool isInitialized = false;  // 초기화 여부를 체크하는 플래그
        
        public void SetIsResetOnNext()
        {
            isResetOnNext = true;
        }
        
        /// <summary>
        /// 복귀 상태를 설정합니다
        /// </summary>
        /// <param name="resetedState">이미 복귀된 상태인지 여부</param>
        public void SetResetedState(bool resetedState)
        {
            isResetedThisTime = resetedState;
        }

        /// <summary>
        /// 이펙트를 초기 상태로 리셋합니다
        /// </summary>
        public void ResetToInitialState()
        {
            // 복귀 관련 플래그 초기화
            isResetOnNext = false;
            isResetedThisTime = false;
            
            // 위치를 초기 위치로 즉시 이동
            if (isUIEffect)
            {
                var rectTransform = GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = startPosition;
                    rectTransform.localScale = Vector3.one;
                }
            }
            else
            {
                transform.localPosition = startPosition;
                transform.localScale = Vector3.one;
            }
            
            // 진행 중인 이펙트 중지
            StopVFX();
        }

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
                    Vector3 adjustedPosition = GetAdjustedPosition(targetPosition, rectTransform);
                    rectTransform.anchoredPosition = adjustedPosition;
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
                    var rectTransform = GetComponent<RectTransform>();
                    startPosition = rectTransform.anchoredPosition;
                    if (useAnchorReference)
                    {
                        // 현재 앵커 값 저장
                        anchorReference = new Vector2(
                            (rectTransform.anchorMin.x + rectTransform.anchorMax.x) * 0.5f,
                            (rectTransform.anchorMin.y + rectTransform.anchorMax.y) * 0.5f
                        );
                    }
                }
                else
                {
                    startPosition = transform.localPosition;
                }
                isInitialized = true;
            }
        }

        // 앵커에 따른 위치 조정 계산 함수
        private Vector3 GetAdjustedPosition(Vector3 position, RectTransform rectTransform)
        {
            if (!useAnchorReference || !isUIEffect)
                return position;

            // 현재 앵커 값 계산
            Vector2 currentAnchor = new Vector2(
                (rectTransform.anchorMin.x + rectTransform.anchorMax.x) * 0.5f,
                (rectTransform.anchorMin.y + rectTransform.anchorMax.y) * 0.5f
            );

            // 부모 RectTransform의 크기 가져오기
            RectTransform parentRect = rectTransform.parent as RectTransform;
            if (parentRect == null)
                return position;

            Vector2 parentSize = new Vector2(parentRect.rect.width, parentRect.rect.height);
            
            // 앵커 차이에 따른 위치 조정
            Vector2 anchorDifference = currentAnchor - anchorReference;
            Vector2 adjustment = new Vector2(
                anchorDifference.x * parentSize.x,
                anchorDifference.y * parentSize.y
            );

            return position - new Vector3(adjustment.x, adjustment.y, 0);
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
                Vector3 adjustedTargetPosition = GetAdjustedPosition(targetPosition * (effectAddValue + 1), rectTransform);
                await UniTask.WhenAll(
                    rectTransform.DOAnchorPos(adjustedTargetPosition, 1 * effectSpeed).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token),
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
                Vector3 adjustedStartPosition = GetAdjustedPosition(startPosition * (effectAddValue + 1), rectTransform);
                await UniTask.WhenAll(
                    rectTransform.DOAnchorPos(adjustedStartPosition, 1 * effectSpeed).SetEase(Ease.OutCubic).WithCancellation(destroyCancellation.Token),
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