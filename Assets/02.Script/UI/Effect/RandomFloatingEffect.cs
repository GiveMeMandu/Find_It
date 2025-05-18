using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Effect
{
    public class RandomFloatingEffect : ChainEffect
    {
        public enum FloatingMode
        {
            Random2D,    // X, Y 평면에서 랜덤 이동
            Random3D,    // X, Y, Z 공간에서 랜덤 이동
            BoundedArea  // 지정된 범위 내에서 랜덤 이동
        }

        [BoxGroup("랜덤 플로팅 설정")][LabelText("플로팅 모드")][SerializeField] private FloatingMode floatingMode = FloatingMode.Random2D;
        [BoxGroup("랜덤 플로팅 설정")][LabelText("최소 이동 거리")][SerializeField] private float minDistance = 0.5f;
        [BoxGroup("랜덤 플로팅 설정")][LabelText("최대 이동 거리")][SerializeField] private float maxDistance = 1.5f;
        [BoxGroup("랜덤 플로팅 설정")][LabelText("최소 이동 시간")][SerializeField] private float minDuration = 0.8f;
        [BoxGroup("랜덤 플로팅 설정")][LabelText("최대 이동 시간")][SerializeField] private float maxDuration = 1.5f;
        [BoxGroup("랜덤 플로팅 설정")][LabelText("이동 사이 대기 시간")][SerializeField] private float pauseDuration = 0.2f;
        [BoxGroup("랜덤 플로팅 설정")][LabelText("이동 제한 범위")][ShowIf("floatingMode", FloatingMode.BoundedArea)][SerializeField] private Vector3 boundingArea = new Vector3(3f, 3f, 0f);
        [BoxGroup("랜덤 플로팅 설정")][LabelText("Ease 타입")][SerializeField] private Ease easeType = Ease.InOutSine;
        [BoxGroup("랜덤 플로팅 설정")][LabelText("자동 시작")][SerializeField] private bool autoStart = true;
        [BoxGroup("랜덤 플로팅 설정")][LabelText("원점으로 돌아가기")][SerializeField] private bool returnToOrigin = false;
        [BoxGroup("랜덤 플로팅 설정")][LabelText("원점 복귀 확률 (%)")][ShowIf("returnToOrigin")][Range(0, 100)][SerializeField] private int returnChance = 20;

        private Vector3 startPosition;
        private Vector3 currentTargetPosition;
        private bool isFloating = false;
        private bool isInitialized = false;

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
            if (isInitialized) return;

            if (isUIEffect)
            {
                startPosition = GetComponent<RectTransform>().anchoredPosition;
            }
            else
            {
                startPosition = transform.localPosition;
            }

            currentTargetPosition = startPosition;
            isInitialized = true;
        }

        [Button("떠다니기 시작")]
        public void StartFloating()
        {
            if (isFloating) return;

            isFloating = true;
            StartFloatingTask().Forget();
        }

        [Button("떠다니기 정지")]
        public void StopFloating()
        {
            isFloating = false;
        }

        private async UniTaskVoid StartFloatingTask()
        {
            while (isFloating)
            {
                if (isUIEffect)
                {
                    await RandomFloatingUITask();
                }
                else
                {
                    await RandomFloatingGameObjectTask();
                }

                try
                {
                    await UniTask.Delay(System.TimeSpan.FromSeconds(pauseDuration), cancellationToken: destroyCancellation.Token);
                }
                catch (System.OperationCanceledException)
                {
                    break;
                }
            }
        }

        private Vector3 GenerateRandomTargetPosition()
        {
            // 랜덤한 방향 벡터 생성
            Vector3 randomDirection;
            float distance = Random.Range(minDistance, maxDistance);

            switch (floatingMode)
            {
                case FloatingMode.Random2D:
                    // X, Y 평면에서 랜덤한 방향
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    randomDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f).normalized;
                    break;

                case FloatingMode.Random3D:
                    // X, Y, Z 공간에서 랜덤한 방향
                    randomDirection = Random.onUnitSphere;
                    break;

                case FloatingMode.BoundedArea:
                    // 지정된 범위 내의 랜덤한 지점
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-boundingArea.x, boundingArea.x) * 0.5f,
                        Random.Range(-boundingArea.y, boundingArea.y) * 0.5f,
                        Random.Range(-boundingArea.z, boundingArea.z) * 0.5f
                    );
                    return startPosition + randomOffset;

                default:
                    randomDirection = Random.insideUnitSphere;
                    break;
            }

            // 원래 위치로 돌아갈 확률 체크
            if (returnToOrigin && Random.Range(0, 100) < returnChance)
            {
                return startPosition;
            }

            return currentTargetPosition + randomDirection * distance;
        }

        private async UniTask RandomFloatingGameObjectTask()
        {
            destroyCancellation.Cancel();
            destroyCancellation = new CancellationTokenSource();

            Vector3 newTargetPosition = GenerateRandomTargetPosition();
            float duration = Random.Range(minDuration, maxDuration);

            try
            {
                // 새 위치로 이동
                await transform.DOLocalMove(newTargetPosition, duration)
                    .SetEase(easeType)
                    .WithCancellation(destroyCancellation.Token);

                // 현재 타겟 위치 업데이트
                currentTargetPosition = newTargetPosition;
            }
            catch (System.OperationCanceledException)
            {
                // 작업이 취소됨
            }
        }

        private async UniTask RandomFloatingUITask()
        {
            destroyCancellation.Cancel();
            destroyCancellation = new CancellationTokenSource();

            var rectTransform = GetComponent<RectTransform>();
            Vector3 newTargetPosition = GenerateRandomTargetPosition();
            float duration = Random.Range(minDuration, maxDuration);

            try
            {
                // 새 위치로 이동
                await rectTransform.DOAnchorPos(newTargetPosition, duration)
                    .SetEase(easeType)
                    .WithCancellation(destroyCancellation.Token);

                // 현재 타겟 위치 업데이트
                currentTargetPosition = newTargetPosition;
            }
            catch (System.OperationCanceledException)
            {
                // 작업이 취소됨
            }
        }

        protected override async UniTask VFXOnceInGame()
        {
            await RandomFloatingGameObjectTask();
        }

        protected override async UniTask VFXOnceUI()
        {
            await RandomFloatingUITask();
        }
    }
}
