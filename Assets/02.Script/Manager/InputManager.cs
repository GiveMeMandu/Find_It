using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Util.CameraSetting;

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

        private EventSystem eventSystem;

        private void Awake()
        {
            playerAction = new PlayerAction();
            
            // 터치 이벤트 바인딩을 먼저 설정
            playerAction.Touch.TouchPress.performed += TouchPress_Performed;
            playerAction.Touch.TouchPress.canceled += TouchPressEnd_Performed;
            playerAction.playerControl.Pause.performed += Pause_Performed;

            // 그 다음 활성화
            playerAction.Touch.Enable();
            playerAction.playerControl.Enable();

            eventSystem = EventSystem.current;
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

        public void DisableAllInput()
        {
            playerAction.Touch.Disable();
            playerAction.playerControl.Disable();
            playerAction.Disable();
            
            // EventSystem 비활성화
            if (eventSystem != null)
            {
                eventSystem.enabled = false;
            }
            
            // CameraView2D 비활성화
            CameraView2D.SetEnablePanAndZoom(false);
        }

        public void EnableAllInput()
        {
            // 활성화 전에 입력 상태 초기화
            playerAction.Touch.TouchPress.Reset();
            
            playerAction.Touch.Enable();
            playerAction.playerControl.Enable();
            playerAction.Enable();
            
            if (eventSystem != null)
            {
                eventSystem.enabled = true;
            }
            
            CameraView2D.SetEnablePanAndZoom(true);
        }
    }

}