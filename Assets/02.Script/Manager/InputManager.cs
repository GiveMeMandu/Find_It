using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Manager
{

    public class InputManager : MonoBehaviour
    {
        public event EventHandler OnPauseAction;
        //* 인게임 터치 처리
        public event EventHandler<Vector2> OnTouchPressAction;
        public event EventHandler<Vector2> OnTouchPressEndAction;
        public event Action OnSumbitAction;

        public PlayerAction playerAction;

        private void Awake()
        {
            playerAction = new PlayerAction();
            playerAction.playerControl.Enable();
            //* 뒤로가기 등 여러 조작
            playerAction.playerControl.Pause.performed += Pause_Performed;
            //* 터치 조작
            playerAction.Touch.Enable();
            playerAction.Touch.TouchPress.performed += TouchPress_Performed;
            playerAction.Touch.TouchPress.canceled += TouchPressEnd_Performed;
        }
        private void OnDisable()
        {
            playerAction.playerControl.Pause.performed -= Pause_Performed;
            playerAction.Touch.TouchPress.performed -= TouchPress_Performed;
            playerAction.Touch.TouchPress.canceled -= TouchPressEnd_Performed;

            playerAction.Touch.Disable();
            playerAction.playerControl.Disable();
            playerAction.Disable();
        }
        private void Pause_Performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            OnPauseAction?.Invoke(this, EventArgs.Empty);
        }
        private void TouchPress_Performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            // 값 유형 모르면 아래 함수 사용
            // context.ReadValueAsObject()
            Vector2 positon = Camera.main.ScreenToWorldPoint(playerAction.Touch.TouchPosition.ReadValue<Vector2>());

            OnTouchPressAction?.Invoke(this, positon);

        }
        private void TouchPressEnd_Performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            Vector2 positon = Camera.main.ScreenToWorldPoint(playerAction.Touch.TouchPosition.ReadValue<Vector2>());
            OnTouchPressEndAction?.Invoke(this, positon);
        }
        public Vector2 GetCurMousePos()
        {
            return playerAction.Touch.TouchPosition.ReadValue<Vector2>();
        }
    }

}