using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UI
{
    /// <summary>
    /// 길게 누르면(onPointerDown 후 일정 시간 유지) onLongPress 이벤트를 1회 호출하는 UI 컴포넌트.
    /// 진행률(0~1)은 onProgress 로 매 프레임 전달됩니다.
    /// 포인터가 영역 밖으로 나가거나(pointerExit) 올라오면(pointerUp) 취소됩니다.
    /// </summary>
    [AddComponentMenu("SnowRabbit/UI/Long Press Button")]
    public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ICancelHandler
    {
        [Header("Settings")]
        [SerializeField]
        private float requiredHoldTime = 1f; // 길게 눌러야 하는 시간(초)

        [SerializeField]
        private bool invokeWhileTimeScaleZero = false; // Time.timeScale=0 인 경우에도 동작할지 (Realtime 사용)

        [SerializeField]
        private bool interactable = true; // 외부에서 비활성화 가능

        [Tooltip("필요 시간 채우면 자동으로 한번 호출 후 진행 종료. 다시 사용하려면 손을 떼고 재시작.")]
        [SerializeField]
        private bool autoReleaseAfterInvoke = true;

#if ENABLE_INPUT_SYSTEM
        [Header("Input Action (Optional)")]
        [SerializeField] private bool enableInputAction = true; // 새 입력 시스템으로도 조작할지
        [SerializeField] private InputActionReference inputAction; // Press (Button) 타입 권장
#endif
        [Header("Events")]
        [SerializeField]
        private UnityEvent onPressStart = new UnityEvent();

        [SerializeField]
        private UnityEvent<float> onProgress = new UnityEvent<float>(); // 0~1

        [SerializeField]
        private UnityEvent onLongPress = new UnityEvent();

        [SerializeField]
        private UnityEvent onPressCancel = new UnityEvent();

        // 내부 상태
        private bool _isHolding;
        private float _holdStartTime; // Time.time 또는 realtimeSinceStartup 저장
        private float _currentProgress; // 0~1
        private bool _invoked;
#if ENABLE_INPUT_SYSTEM
        private bool _actionSubscribed;
#endif

        public bool Interactable
        {
            get => interactable;
            set
            {
                if (interactable == value) return;
                interactable = value;
                if (!interactable)
                {
                    CancelHold();
                }
            }
        }

        public float RequiredHoldTime
        {
            get => requiredHoldTime;
            set => requiredHoldTime = Mathf.Max(0.01f, value);
        }

        public UnityEvent OnLongPress => onLongPress;
        public UnityEvent<float> OnProgress => onProgress;
        public UnityEvent OnPressStart => onPressStart;
        public UnityEvent OnPressCancel => onPressCancel;
        public float progress => _currentProgress;


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {
            if (!_isHolding)
            {
                onProgress.Invoke(0);
            }
            if (!_isHolding || _invoked && autoReleaseAfterInvoke) return;
            float elapsed = GetTime() - _holdStartTime;
            _currentProgress = requiredHoldTime > 0f ? Mathf.Clamp01(elapsed / requiredHoldTime) : 1f;
            onProgress.Invoke(_currentProgress);
            if (!_invoked && elapsed >= requiredHoldTime)
            {
                _invoked = true;
                onLongPress.Invoke();
                if (autoReleaseAfterInvoke)
                {
                    CancelHold(silentCancelEvent: true);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable || !IsPointerValid(eventData)) return;
            StartHold();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isHolding) return;
            // 아직 발동 전이면 취소, 이미 발동했고 autoReleaseAfterInvoke=false 라면 여기서 정리
            CancelHold(silentCancelEvent: _invoked);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isHolding) return;
            // 영역 벗어나면 취소
            CancelHold();
        }

        public void OnCancel(BaseEventData eventData)
        {
            if (!_isHolding) return;
            CancelHold();
        }

        private void StartHold()
        {
            _isHolding = true;
            _invoked = false;
            _holdStartTime = GetTime();
            _currentProgress = 0f;
            onPressStart.Invoke();
            onProgress.Invoke(0f);
        }

        private void CancelHold(bool silentCancelEvent = false)
        {
            if (!_isHolding) return;
            _isHolding = false;
            if (!silentCancelEvent && !_invoked)
            {
                onPressCancel.Invoke();
            }
            _currentProgress = 0f;
        }

        private bool IsPointerValid(PointerEventData eventData)
        {
            // 추후 멀티터치나 특정 버튼 제약 필요 시 확장
            if (eventData.button != PointerEventData.InputButton.Left) return false;
            return true;
        }

        private float GetTime() => invokeWhileTimeScaleZero ? Time.unscaledTime : Time.time;

        /// <summary>
        /// 외부에서 진행 초기화 (강제 취소)
        /// </summary>
        public void ResetState() => CancelHold();

#if ENABLE_INPUT_SYSTEM
        private void OnEnable()
        {
            TrySubscribeInputAction();
        }

        private void OnDisable()
        {
            UnsubscribeInputAction();
        }

        private void TrySubscribeInputAction()
        {
            if (!_actionSubscribed && enableInputAction && inputAction != null && inputAction.action != null)
            {
                var act = inputAction.action;
                // started: 버튼이 눌리기 시작
                act.started += OnActionStarted;
                // canceled: 버튼이 떼어짐 / 취소
                act.canceled += OnActionCanceled;
                if (!act.enabled) act.Enable();
                _actionSubscribed = true;
            }
        }

        private void UnsubscribeInputAction()
        {
            if (_actionSubscribed && inputAction != null && inputAction.action != null)
            {
                var act = inputAction.action;
                act.started -= OnActionStarted;
                act.canceled -= OnActionCanceled;
            }
            _actionSubscribed = false;
        }

        private void OnActionStarted(InputAction.CallbackContext ctx)
        {
            if (!interactable) return;
            if (_isHolding) return; // 이미 진행 중이면 무시
            StartHold();
        }

        private void OnActionCanceled(InputAction.CallbackContext ctx)
        {
            if (_isHolding)
            {
                CancelHold(silentCancelEvent: _invoked);
            }
        }

        /// <summary>
        /// 런타임 중 InputAction 교체
        /// </summary>
        public void SetInputAction(InputActionReference reference, bool enable = true)
        {
            UnsubscribeInputAction();
            inputAction = reference;
            enableInputAction = enable;
            TrySubscribeInputAction();
        }
#endif
#if UNITY_EDITOR
        private void OnValidate()
        {
            requiredHoldTime = Mathf.Max(0.01f, requiredHoldTime);
#if ENABLE_INPUT_SYSTEM
            if (!enableInputAction)
            {
                UnsubscribeInputAction();
            }
            else
            {
                if (Application.isPlaying) TrySubscribeInputAction();
            }
#endif
        }
#endif
    }
}
