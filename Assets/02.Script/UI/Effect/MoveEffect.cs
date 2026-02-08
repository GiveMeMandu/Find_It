using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Effect
{
    public class MoveEffect : ChainEffect
    {
        [LabelText("비활성화시 처음 위치로 리셋")] [SerializeField] private bool resetPosOnDisable = false;
        [BoxGroup("클릭 이펙트 설정")][LabelText("이동하고 나서 작아지게 될 것인가")][SerializeField] private bool isSmallOnEnd = false;
        [BoxGroup("클릭 이펙트 설정")]
        [LabelText("작아지는 시점 (1이면 이동 끝나고 나서, 0이면 시작시 바로 작아짐)")]
        [EnableIf("isSmallOnEnd")]
        [SerializeField] private float smallTiming = 1;        [BoxGroup("축별 이동 설정")][LabelText("X축 이동 사용")][SerializeField] private bool useXAxis = true;
        [BoxGroup("축별 이동 설정")][LabelText("Y축 이동 사용")][SerializeField] private bool useYAxis = true;
        [BoxGroup("축별 이동 설정")][LabelText("Z축 이동 사용")][SerializeField] private bool useZAxis = true;        [BoxGroup("클릭 이펙트 설정")][LabelText("랜덤 위치 사용")][SerializeField] private bool useRandomPosition = false;
        [BoxGroup("클릭 이펙트 설정")][LabelText("최소 이동 위치")][ShowIf("useRandomPosition")][SerializeField] private Vector3 minPosition = Vector3.zero;
        [BoxGroup("클릭 이펙트 설정")][LabelText("최대 이동 위치")][ShowIf("useRandomPosition")][SerializeField] private Vector3 maxPosition = Vector3.one * 100f;
        [BoxGroup("클릭 이펙트 설정")][LabelText("대상 이동 위치")][HideIf("useRandomPosition")][SerializeField] private Vector3 targetPosition;
        [BoxGroup("클릭 이펙트 설정")][LabelText("다음 실행시 복귀 효과 재생")][SerializeField] private bool isResetOnNext = false;
        [BoxGroup("클릭 이펙트 설정")][LabelText("이미 복귀 효과가 재생되었는지 체크")][SerializeField] private bool isResetedThisTime = false;
        [BoxGroup("클릭 이펙트 설정")][LabelText("시작 시 복귀 효과에서 시작")][SerializeField] private bool isResetOnStart = false;
        [BoxGroup("클릭 이펙트 설정")][LabelText("앵커 기준점 사용 (체크 시 앵커에 따라 위치 조정)")][SerializeField] private bool useAnchorReference = true;
        [BoxGroup("이펙트 트윈 설정")] public Ease moveEase = Ease.OutCubic;
        [BoxGroup("이펙트 트윈 설정")][LabelText("이동 시간 (초)")][SerializeField] private float moveDuration = 1f;
        [BoxGroup("클릭 이펙트 설정")][LabelText("시작 위치")][ReadOnly][SerializeField]private Vector3 startPosition;
        private Vector2 anchorReference = new Vector2(0.5f, 0.5f); // 기준 앵커 (중앙)
        private bool isInitialized = false;  // 초기화 여부를 체크하는 플래그
        private RectTransform rectTransform;
        private Vector3 originalScale;
        [Button("이펙트 리셋 테스트")]
        public void ResetEffect()
        {
            ResetToInitialState();
        }
        
        /// <summary>
        /// 출발점과 도착점을 동적으로 설정합니다
        /// </summary>
        /// <param name="startPos">출발점 (로컬 좌표)</param>
        /// <param name="targetPos">도착점 (로컬 좌표)</param>
        public void SetMovePositions(Vector3 startPos, Vector3 targetPos)
        {
            startPosition = new Vector3(targetPos.x, startPosition.y, targetPos.z);
            targetPosition = targetPos;
            
            // 현재 위치를 출발점으로 즉시 이동
            if (isUIEffect)
            {
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = startPosition;
                }
            }
            else
            {
                transform.localPosition = startPosition;
            }
            
            // Debug.Log($"MoveEffect {name}: 출발점 {startPos}, 도착점 {targetPos}로 설정됨");
        }
        
        /// <summary>
        /// 월드 좌표를 기준으로 출발점과 도착점을 설정합니다
        /// </summary>
        /// <param name="worldStartPos">출발점 (월드 좌표)</param>
        /// <param name="worldTargetPos">도착점 (월드 좌표)</param>
        public void SetMovePositionsFromWorld(Vector3 worldStartPos, Vector3 worldTargetPos)
        {
            // 월드 좌표를 로컬 좌표로 변환
            Vector3 localStartPos = transform.parent != null ? 
                transform.parent.InverseTransformPoint(worldStartPos) : worldStartPos;
            Vector3 localTargetPos = transform.parent != null ? 
                transform.parent.InverseTransformPoint(worldTargetPos) : worldTargetPos;
            
            SetMovePositions(localStartPos, localTargetPos);
        }
        
        /// <summary>
        /// 강제로 이펙트를 재생합니다 (재생 중이어도 실행)
        /// </summary>
        public override void PlayVFXForce()
        {
            // 기존 이펙트 중지
            StopVFX();
            isPlaying = false;
            
            // 강제 재생
            base.PlayVFXForce();
        }
        
        /// <summary>
        /// Y축만 원래 시작 위치로 리셋합니다 (EnemyWaterThrowAttack 전용)
        /// </summary>
        public void ResetYPositionOnly()
        {
            if (isUIEffect)
            {
                if (rectTransform != null)
                {
                    Vector3 currentPos = rectTransform.anchoredPosition;
                    currentPos.y = startPosition.y;
                    rectTransform.anchoredPosition = currentPos;
                }
            }
            else
            {
                Vector3 currentPos = transform.localPosition;
                currentPos.y = startPosition.y;
                transform.localPosition = currentPos;
            }
        }
        
        public void ResetToInitialState()
        {
            // 복귀 관련 플래그 초기화
            isResetOnNext = false;
            isResetedThisTime = false;
            
            // 위치를 초기 위치로 즉시 이동
            if (isUIEffect)
            {
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = startPosition;
                    rectTransform.localScale = originalScale;
                }
            }
            else
            {
                transform.localPosition = startPosition;
                transform.localScale = originalScale;
            }
            
            // 재생 상태 강제 초기화
            isPlaying = false;
        }

        protected override void Start()
        {
            base.Start();
        }

        // Awake에서 초기화하여 OnEnable에서 PlayVFX가 호출되더라도
        // startPosition과 originalScale이 올바르게 설정되도록 합니다.
        private void Awake()
        {
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
            if (resetPosOnDisable)
            {
                ResetToInitialState();
            }
        }
        private void Initial()
        {
            if (!isInitialized)
            {
                if (isUIEffect)
                {
                    rectTransform = GetComponent<RectTransform>();
                    startPosition = rectTransform.anchoredPosition;
                    if (useAnchorReference)
                    {
                        // 현재 앵커 값 저장
                        anchorReference = new Vector2(
                            (rectTransform.anchorMin.x + rectTransform.anchorMax.x) * 0.5f,
                            (rectTransform.anchorMin.y + rectTransform.anchorMax.y) * 0.5f
                        );
                        
                    }
                    // UI인 경우에도 원래 스케일을 저장합니다.
                    originalScale = rectTransform.localScale;
                }
                else
                {
                    startPosition = transform.localPosition;
                    originalScale = transform.localScale;
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
                // 랜덤 위치 사용 여부에 따라 목표 위치 결정
                Vector3 effectiveTargetPosition = useRandomPosition ? 
                    new Vector3(
                        Random.Range(minPosition.x, maxPosition.x),
                        Random.Range(minPosition.y, maxPosition.y),
                        Random.Range(minPosition.z, maxPosition.z)
                    ) : targetPosition;
                
                // 현재 위치 가져오기
                Vector3 currentPosition = transform.localPosition;
                
                // 축별 이동 적용 (사용하지 않는 축은 현재 위치 유지)
                Vector3 finalTargetPosition = new Vector3(
                    useXAxis ? effectiveTargetPosition.x * (effectAddValue + 1) : currentPosition.x,
                    useYAxis ? effectiveTargetPosition.y * (effectAddValue + 1) : currentPosition.y,
                    useZAxis ? effectiveTargetPosition.z * (effectAddValue + 1) : currentPosition.z
                );
                
                await UniTask.WhenAll(
                    transform.DOLocalMove(finalTargetPosition, moveDuration * effectSpeed).SetEase(moveEase).WithCancellation(destroyCancellation.Token),
                    isSmallOnEnd ? transform.DOScale(0, 0.5f).SetEase(moveEase).SetDelay(moveDuration * effectSpeed * smallTiming).WithCancellation(destroyCancellation.Token) : UniTask.CompletedTask
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
                // 랜덤 위치 사용 여부에 따라 목표 위치 결정
                Vector3 effectiveTargetPosition = useRandomPosition ? 
                    new Vector3(
                        Random.Range(minPosition.x, maxPosition.x),
                        Random.Range(minPosition.y, maxPosition.y),
                        Random.Range(minPosition.z, maxPosition.z)
                    ) : targetPosition;
                
                // 현재 위치 가져오기
                Vector3 currentPosition = rectTransform.anchoredPosition;
                
                // 축별 이동 적용 (사용하지 않는 축은 현재 위치 유지)
                Vector3 finalTargetPosition = new Vector3(
                    useXAxis ? effectiveTargetPosition.x * (effectAddValue + 1) : currentPosition.x,
                    useYAxis ? effectiveTargetPosition.y * (effectAddValue + 1) : currentPosition.y,
                    useZAxis ? effectiveTargetPosition.z * (effectAddValue + 1) : currentPosition.z
                );
                
                Vector3 adjustedTargetPosition = GetAdjustedPosition(finalTargetPosition, rectTransform);
                await UniTask.WhenAll(
                    rectTransform.DOAnchorPos(adjustedTargetPosition, moveDuration * effectSpeed).SetEase(moveEase).WithCancellation(destroyCancellation.Token),
                    isSmallOnEnd ? rectTransform.DOScale(0, 0.5f).SetEase(moveEase).SetDelay(moveDuration * effectSpeed * smallTiming).WithCancellation(destroyCancellation.Token) : UniTask.CompletedTask
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
                    rectTransform.DOAnchorPos(adjustedStartPosition, moveDuration * effectSpeed).SetEase(moveEase).WithCancellation(destroyCancellation.Token),
                    isSmallOnEnd ? rectTransform.DOScale(1, 0.5f).SetEase(moveEase).WithCancellation(destroyCancellation.Token) : UniTask.CompletedTask
                );
            }
            else
            {
                await UniTask.WhenAll(
                    transform.DOLocalMove(startPosition * (effectAddValue + 1), moveDuration * effectSpeed).SetEase(moveEase).WithCancellation(destroyCancellation.Token),
                    isSmallOnEnd ? transform.DOScale(1, 0.5f).SetEase(moveEase).WithCancellation(destroyCancellation.Token) : UniTask.CompletedTask
              );
            }
        }
        [Button("자동 대상 이동 위치 지금 위치로 설정")]
        public void AutoSetTargetPosition()
        {
            this.targetPosition = transform.localPosition;
        }

    }
}