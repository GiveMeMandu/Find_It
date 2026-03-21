using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UI
{
    /// <summary>
    /// Custom Button class that supports the new Input System.
    /// </summary>
    public class Button2 : Button
    {
#if ENABLE_INPUT_SYSTEM
        [Header("Input Action (Optional)")]
        [SerializeField] private bool enableInputAction = true; // 새 입력 시스템 사용 여부
        [SerializeField] private InputActionReference inputAction; // Press (Button) 타입 권장
        private bool _actionSubscribed;
#endif

        #region InputSystem Subscription
#if ENABLE_INPUT_SYSTEM
        protected override void OnEnable()
        {
            base.OnEnable();
            TrySubscribeInputAction();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnsubscribeInputAction();
        }

        private void TrySubscribeInputAction()
        {
            if (_actionSubscribed) return;
            if (!enableInputAction) return;
            if (inputAction == null || inputAction.action == null) return;
            var act = inputAction.action;
            // started = 버튼 눌림 (다운) -> 시각적 Press 상태로 전환
            act.started += OnActionStarted;
            // performed = 요구 조건 충족 (보통 업) -> 실제 클릭 처리
            act.performed += OnActionPerformed;
            // canceled = 입력 취소 -> Press 상태 복귀
            act.canceled += OnActionCanceled;
            if (!act.enabled) act.Enable();
            _actionSubscribed = true;
        }

        private void UnsubscribeInputAction()
        {
            if (!_actionSubscribed) return;
            if (inputAction != null && inputAction.action != null)
            {
                var act = inputAction.action;
                act.started -= OnActionStarted;
                act.performed -= OnActionPerformed;
                act.canceled -= OnActionCanceled;
            }
            _actionSubscribed = false;
        }

        private void OnActionStarted(InputAction.CallbackContext ctx)
        {
            if (!IsActive() || !IsInteractable()) return;
            // 시각적으로 눌림 상태로 전환
            DoStateTransition(SelectionState.Pressed, false);
        }

        private void OnActionPerformed(InputAction.CallbackContext ctx)
        {
            if (!IsActive() || !IsInteractable()) return;
            // Unity 기본 키/패드 Submit 과 동일하게 처리
            // OnSubmit 은 Press()-> 애니메이션 코루틴 호출 포함
            OnSubmit(new BaseEventData(EventSystem.current));
        }

        private void OnActionCanceled(InputAction.CallbackContext ctx)
        {
            if (!IsActive() || !IsInteractable()) return;
            // 눌렸다 취소된 경우 정상 상태로 복귀
            // 현재 선택되어 있다면 Selected, 아니면 Normal
            var state = currentSelectionState; // protected getter 없음 -> Selection 재평가
            // 강제로 EvaluateAndTransitionToSelectionState 를 쓰고 싶지만 protected 내부
            // 간단히 Normal 로 돌리고, 선택된 경우 OnSelect 다시 트리거 되면 상태 반영
            DoStateTransition(SelectionState.Normal, false);
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
        #endregion

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
#if ENABLE_INPUT_SYSTEM
            if (!enableInputAction)
            {
                UnsubscribeInputAction();
            }
            else if (Application.isPlaying)
            {
                // 플레이 중이면 재구독 시도
                UnsubscribeInputAction();
                TrySubscribeInputAction();
            }
#endif
        }
#endif
    }
}
