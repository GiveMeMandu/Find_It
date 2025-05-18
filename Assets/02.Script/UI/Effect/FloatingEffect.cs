using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Effect
{
    public class FloatingEffect : ChainEffect
    {
        public enum FloatingDirection
        {
            UpDown,
            LeftRight,
            Custom
        }

        [BoxGroup("플로팅 이펙트 설정")][LabelText("떠다니는 방향")][SerializeField] private FloatingDirection floatingDirection = FloatingDirection.UpDown;
        [BoxGroup("플로팅 이펙트 설정")][LabelText("떠다니는 거리")][SerializeField] private float floatingDistance = 1f;
        [BoxGroup("플로팅 이펙트 설정")][LabelText("떠다니는 속도")][SerializeField] private float floatingSpeed = 1f;
        [BoxGroup("플로팅 이펙트 설정")][LabelText("커스텀 방향 벡터")][ShowIf("floatingDirection", FloatingDirection.Custom)][SerializeField] private Vector3 customDirection = Vector3.up;
        [BoxGroup("플로팅 이펙트 설정")][LabelText("Ease 타입")][SerializeField] private Ease easeType = Ease.InOutSine;
        [BoxGroup("플로팅 이펙트 설정")][LabelText("자동 시작")][SerializeField] private bool autoStart = true;

        private Vector3 startPosition;
        private Vector3 moveDirection;
        private bool isFloating = false;
        private Sequence floatingSequence;

        protected override void Start()
        {
            base.Start();
            Initialize();

            if (autoStart)
            {
                StartFloating();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            StopFloating();
        }

        private void Initialize()
        {
            if (isUIEffect)
            {
                startPosition = GetComponent<RectTransform>().anchoredPosition;
            }
            else
            {
                startPosition = transform.localPosition;
            }

            // 이동 방향 설정
            switch (floatingDirection)
            {
                case FloatingDirection.UpDown:
                    moveDirection = Vector3.up;
                    break;
                case FloatingDirection.LeftRight:
                    moveDirection = Vector3.right;
                    break;
                case FloatingDirection.Custom:
                    moveDirection = customDirection.normalized;
                    break;
            }
        }

        [Button("떠다니기 시작")]
        public void StartFloating()
        {
            if (isFloating)
                return;

            isFloating = true;
            StartFloatingTask().Forget();
        }

        [Button("떠다니기 정지")]
        public void StopFloating()
        {
            isFloating = false;
            floatingSequence?.Kill();
        }

        private async UniTaskVoid StartFloatingTask()
        {
            while (isFloating)
            {
                if (isUIEffect)
                {
                    await FloatingUITask();
                }
                else
                {
                    await FloatingGameObjectTask();
                }

                await UniTask.Yield(destroyCancellation.Token);
            }
        }

        private async UniTask FloatingGameObjectTask()
        {
            destroyCancellation.Cancel();
            destroyCancellation = new CancellationTokenSource();

            Vector3 targetPos = startPosition + moveDirection * floatingDistance;

            try
            {
                // 위로 이동
                await transform.DOLocalMove(targetPos, floatingSpeed).SetEase(easeType).WithCancellation(destroyCancellation.Token);
                // 아래로 이동
                await transform.DOLocalMove(startPosition, floatingSpeed).SetEase(easeType).WithCancellation(destroyCancellation.Token);
            }
            catch (System.OperationCanceledException)
            {
                // 작업이 취소됨
            }
        }

        private async UniTask FloatingUITask()
        {
            destroyCancellation.Cancel();
            destroyCancellation = new CancellationTokenSource();

            var rectTransform = GetComponent<RectTransform>();
            Vector3 targetPos = startPosition + moveDirection * floatingDistance;

            try
            {
                // 목표 위치로 이동
                await rectTransform.DOAnchorPos(targetPos, floatingSpeed).SetEase(easeType).WithCancellation(destroyCancellation.Token);
                // 원래 위치로 복귀
                await rectTransform.DOAnchorPos(startPosition, floatingSpeed).SetEase(easeType).WithCancellation(destroyCancellation.Token);
            }
            catch (System.OperationCanceledException)
            {
                // 작업이 취소됨
            }
        }

        protected override async UniTask VFXOnceInGame()
        {
            await FloatingGameObjectTask();
        }

        protected override async UniTask VFXOnceUI()
        {
            await FloatingUITask();
        }
    }
}
