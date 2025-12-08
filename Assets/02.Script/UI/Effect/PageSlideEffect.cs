using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using NaughtyAttributes;
using System;
namespace UI.Effect
{
    public class PageScaleEffect : UIDotweenEffect
    {
        [Label("행 방향으로 들어오는가")]
        [SerializeField] private bool isHorizontal = false; // 행(가로)으로 이동할지 여부
        [Label("역방향으로 들어오는가")]
        [SerializeField] private bool isReverse = false;    // 역방향으로 들어올지 여부
        [Label("첫 OnEnable 무시할까")]
        [SerializeField] private bool ignoreFirstOnEnable = false; // 첫 OnEnable 시 애니메이션을 실행하지 않을지
        private bool hasEnabledOnce = false;

        private RectTransform rectTransform;
        private Image image;

        protected override void OnEnable()
        {
            base.OnEnable();
            rectTransform = transform as RectTransform;
            image = GetComponent<Image>();

            if (rectTransform == null)
                return;

            // 첫 OnEnable을 무시하도록 설정된 경우, 최초 진입에서는 애니메이션을 실행하지 않고 플래그만 설정
            if (ignoreFirstOnEnable && !hasEnabledOnce)
            {
                hasEnabledOnce = true;
                return;
            }

            PlayScaleAnimation();
        }

        private void PlayScaleAnimation()
        {
            // 초기 설정
            rectTransform.localScale = Vector3.zero;

            // // 페이드 시퀀스
            // if (image != null)
            // {
            //     var fadeSequence = CreateSequence();
            //     fadeSequence.SetUpdate(true);
            //     fadeSequence
            //         .AppendCallback(() => image.color = new Color(0, 0, 0, 0))
            //         .Append(image.DOColor(new Color(1, 1, 1, 1), 0.2f).SetEase(Ease.InSine));
            // }

            // 이동 시퀀스 (가로/세로 분기)
            var moveSequence = CreateSequence();
            moveSequence.SetUpdate(true);
            int dir = isReverse ? -1 : 1;

            if (isHorizontal)
            {
                // 가로로 들어오는 경우: 화면 밖 X 위치 -> 중간 X -> 최종 0
                moveSequence
                    .AppendCallback(() => rectTransform.localPosition = new Vector3(1080f * dir, rectTransform.localPosition.y, 0))
                    .Append(rectTransform.DOLocalMoveX(4f * dir, 0.7f).SetEase(Ease.OutExpo))
                    .Append(rectTransform.DOLocalMoveX(0f, 0.2f).SetEase(Ease.Linear));
            }
            else
            {
                // 세로로 들어오는 기존 동작과 동일하게 동작하도록 유지
                moveSequence
                    .AppendCallback(() => rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, 1080f * dir, 0))
                    .Append(rectTransform.DOLocalMoveY(4f * dir, 0.7f).SetEase(Ease.OutExpo))
                    .Append(rectTransform.DOLocalMoveY(0f, 0.2f).SetEase(Ease.Linear));
            }

            // 회전 시퀀스 (진입 방향에 따라 회전 방향을 약간 반전)
            var rotateSequence = CreateSequence();
            rotateSequence.SetUpdate(true);
            rotateSequence
                .AppendCallback(() => rectTransform.rotation = Quaternion.identity)
                .Append(rectTransform.DORotate(new Vector3(0, 0, -2f * dir), 0.25f).SetEase(Ease.InOutQuad))
                .Append(rectTransform.DORotate(Vector3.zero, 0.4f).SetEase(Ease.InOutQuad));

            // 스케일 시퀀스 (독립적으로 실행)
            var scaleSequence = CreateSequence();
            scaleSequence.SetUpdate(true);
            scaleSequence
                .Append(rectTransform.DOScale(1, 0.35f).SetEase(Ease.OutBack));
        }
    }
}