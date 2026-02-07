using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
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
        // Keep references to running sequences so we can kill/reuse them
        private Sequence currentSequence;
        // UnityEvents to notify when enter/exit animations complete
        public UnityEvent onEnterComplete = new UnityEvent();
        public UnityEvent onExitComplete = new UnityEvent();

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

            PlayEnterAnimation();
        }

        // Play entry animation (called on enable or manually)
        public void PlayEnterAnimation()
        {
            // 초기 설정
            rectTransform.localScale = Vector3.zero;
            rectTransform.rotation = Quaternion.identity;

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

            // call UnityEvent when enter animation finishes
            scaleSequence.OnComplete(() => { if (onEnterComplete != null) onEnterComplete.Invoke(); });

            // store reference to last important sequence so we can stop it when exiting
            currentSequence = scaleSequence;
        }

        // Play exit animation (moves the page back off-screen and scales out)
        public void PlayExitAnimation(bool deactivateAfter = false)
        {
            if (rectTransform == null)
                rectTransform = transform as RectTransform;

            // stop any current tweens on this transform
            rectTransform.DOKill();

            int dir = isReverse ? -1 : 1;

            // 이동 시퀀스 (Enter의 역순: 0 -> 중간 -> 화면 밖)
            var moveSequence = CreateSequence();
            moveSequence.SetUpdate(true);

            if (isHorizontal)
            {
                moveSequence
                    .AppendCallback(() => rectTransform.localPosition = new Vector3(0f, rectTransform.localPosition.y, 0))
                    .Append(rectTransform.DOLocalMoveX(4f * dir, 0.2f).SetEase(Ease.Linear))
                    .Append(rectTransform.DOLocalMoveX(1080f * dir, 0.7f).SetEase(Ease.InExpo));
            }
            else
            {
                moveSequence
                    .AppendCallback(() => rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, 0f, 0))
                    .Append(rectTransform.DOLocalMoveY(4f * dir, 0.2f).SetEase(Ease.Linear))
                    .Append(rectTransform.DOLocalMoveY(1080f * dir, 0.7f).SetEase(Ease.InExpo));
            }

            // 회전 시퀀스 (Enter의 역순)
            var rotateSequence = CreateSequence();
            rotateSequence.SetUpdate(true);
            rotateSequence
                .Append(rectTransform.DORotate(new Vector3(0, 0, -2f * dir), 0.4f).SetEase(Ease.InOutQuad))
                .Append(rectTransform.DORotate(Vector3.zero, 0.25f).SetEase(Ease.InOutQuad));

            // 스케일 시퀀스 (독립적으로 실행)
            var scaleSequence = CreateSequence();
            scaleSequence.SetUpdate(true);
            scaleSequence
                .AppendInterval(0.05f) // 약간 늦게 시작
                .Append(rectTransform.DOScale(0f, 0.35f).SetEase(Ease.InBack));

            // 항상 Exit 완료 이벤트 호출, 필요 시 비활성화도 처리
            scaleSequence.OnComplete(() =>
            {
                if (onExitComplete != null) onExitComplete.Invoke();
                if (deactivateAfter) gameObject.SetActive(false);
            });

            currentSequence = scaleSequence;
        }

        // Convenience methods to show/hide from other scripts
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide(bool deactivateAfter = true)
        {
            PlayExitAnimation(deactivateAfter);
        }
    }
}