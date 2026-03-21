
namespace UI
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using UnityEngine.UI;

    /// <summary>
    /// 선택 불가능한 버튼입니다. 클릭은 가능하지만 선택은 불가능합니다.
    /// </summary>
    public class UnSelectableButton : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent onClick;
        [SerializeField] private InputActionReference inputAction; // Press (Button) 타입 권장

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke();
        }

        void OnEnable()
        {
            if (inputAction != null && inputAction.action != null)
            {
                var act = inputAction.action;
                act.performed += OnActionPerformed;
                if (!act.enabled) act.Enable();
            }
        }

        void OnDisable()
        {
            if (inputAction != null && inputAction.action != null)
            {
                var act = inputAction.action;
                act.performed -= OnActionPerformed;
            }
        }

        private void OnActionPerformed(InputAction.CallbackContext context)
        {
            onClick?.Invoke();
        }
    }
}