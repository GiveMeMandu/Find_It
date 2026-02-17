using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;
using NaughtyAttributes;

namespace UI.Effect
{
    public class PageSlideEffect : UIDotweenEffect
    {
        [Label("슬라이드 될 방향이 행인가")]
        [SerializeField] private bool isSlideHorizontal = true;
        [Label("역으로 오는가")]
        [SerializeField] private bool isReverse = false;
        [Label("첫 OnEnable 무시할까")]
        [SerializeField] private bool ignoreFirstOnEnable = false;
        private bool hasEnabledOnce = false;
        [Label("회전할건가")]
        [SerializeField] private bool isRotate = false;
        [Label("회전 정도")] [ShowIf("isRotate")]
        [SerializeField] private float rotationAngle = 2f;

        [Space(10)]
        [Label("첫 번째 슬라이드 시간")]
        [SerializeField] private float firstSlideDuration = 0.5f;
        [Label("두 번째 슬라이드 시간")]
        [SerializeField] private float secondSlideDuration = 0.2f;
        [Label("두 번째 슬라이드 딜레이")]
        [SerializeField] private float secondSlideDelay = 0.1f;

        [Label("도착 지점")]
        [SerializeField] private float arrivePosition = 0f;

        private RectTransform rectTransform;

        protected override void OnEnable()
        {
            base.OnEnable();
            rectTransform = transform as RectTransform;
            if (rectTransform == null)
                return;

            // 최초 OnEnable을 무시하도록 설정된 경우, 첫 호출에서는 애니메이션을 실행하지 않고 플래그만 설정
            if (ignoreFirstOnEnable && !hasEnabledOnce)
            {
                hasEnabledOnce = true;
                return;
            }

            PlaySlideAnimation();
            if (isRotate) PlayRotateAnimation();
        }

        public void PlaySlideAnimation()
        {
            var slideSequence = CreateSequence();
            int isReverseValue = isReverse ? -1 : 1;

            if (isSlideHorizontal)
            {
                rectTransform.localPosition = new Vector3(1080f * isReverseValue, rectTransform.localPosition.y, 0);
                
                slideSequence
                    .Append(rectTransform.DOLocalMoveX(arrivePosition - 100, firstSlideDuration).SetEase(Ease.OutExpo))
                    .AppendInterval(secondSlideDelay)
                    .Append(rectTransform.DOLocalMoveX(arrivePosition, secondSlideDuration).SetEase(Ease.Linear));
            }
            else
            {
                rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, 1920f * isReverseValue, 0);
                
                slideSequence
                    .Append(rectTransform.DOLocalMoveY(arrivePosition - 4, firstSlideDuration).SetEase(Ease.OutExpo))
                    .AppendInterval(secondSlideDelay)
                    .Append(rectTransform.DOLocalMoveY(arrivePosition, secondSlideDuration).SetEase(Ease.Linear));
            }
        }

        /// <summary>
        /// 화면 밖에서 안으로 슬라이드
        /// </summary>
        /// <param name="horizontal">true면 가로, false면 세로</param>
        /// <param name="duration">슬라이드 시간</param>
        public void SlideIn(bool horizontal, float duration)
        {
            if (rectTransform == null) rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            var sequence = CreateSequence();
            int reverseValue = isReverse ? -1 : 1;

            if (horizontal)
            {
                rectTransform.localPosition = new Vector3(1080f * reverseValue, rectTransform.localPosition.y, 0);
                sequence.Append(rectTransform.DOLocalMoveX(arrivePosition, duration).SetEase(Ease.OutExpo));
            }
            else
            {
                rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, 1920f * reverseValue, 0);
                sequence.Append(rectTransform.DOLocalMoveY(arrivePosition, duration).SetEase(Ease.OutExpo));
            }
        }

        /// <summary>
        /// 현재 위치에서 화면 밖으로 슬라이드
        /// </summary>
        /// <param name="horizontal">true면 가로, false면 세로</param>
        /// <param name="duration">슬라이드 시간</param>
        public void SlideOut(bool horizontal, float duration)
        {
            if (rectTransform == null) rectTransform = transform as RectTransform;
            if (rectTransform == null) return;

            var sequence = CreateSequence();
            int reverseValue = isReverse ? -1 : 1;

            if (horizontal)
            {
                sequence.Append(rectTransform.DOLocalMoveX(-1080f * reverseValue, duration).SetEase(Ease.InExpo));
            }
            else
            {
                sequence.Append(rectTransform.DOLocalMoveY(-1920f * reverseValue, duration).SetEase(Ease.InExpo));
            }
        }

        public void PlayRotateAnimation()
        {
            var rotateSequence = CreateSequence();
            rotateSequence.AppendCallback(() => rectTransform.DORotate(Vector3.zero, 0))
                .Append(rectTransform.DORotate(new Vector3(0, 0, -rotationAngle), 0.25f).SetEase(Ease.InOutQuad))
                .AppendInterval(0.25f)
                .Append(rectTransform.DORotate(Vector3.zero, 0.4f).SetEase(Ease.InOutQuad))
                .Join(rectTransform.DOScale(1, 0.35f).SetEase(Ease.OutBack));
        }
    }
}