using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using DeskCat.FindIt.Scripts.Core.Main.System;
using Util.CameraSetting;
using Cysharp.Threading.Tasks;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Manager
{
    public class InputManager : MonoBehaviour
    {
        public event EventHandler OnPauseAction;
        public event EventHandler<TouchData> OnTouchPressAction;
        public event EventHandler<TouchData> OnTouchPressEndAction;
        public event EventHandler<TouchData> OnTouchMoveAction;
        public event Action OnSumbitAction;

        public PlayerAction playerAction;
        private EventSystem eventSystem;
        public bool isEnabled = true;

        public struct TouchData
        {
            public int TouchId;
            public Vector2 ScreenPosition;
            public Vector2 WorldPosition;
            public TouchPhase Phase;

            public TouchData(int id, Vector2 screenPos, Vector2 worldPos, TouchPhase phase)
            {
                TouchId = id;
                ScreenPosition = screenPos;
                WorldPosition = worldPos;
                Phase = phase;
            }
        }

        private void Awake()
        {
            playerAction = new PlayerAction();
            EnhancedTouchSupport.Enable();
            
            // 마우스 입력 시스템 강제 초기화
            try
            {
                if (Mouse.current != null)
                {
                    InputSystem.EnableDevice(Mouse.current);
                    Debug.Log("Mouse.current 활성화 성공");
                }
                else
                {
                    Debug.LogWarning("Mouse.current가 null입니다. New Input System이 제대로 초기화되지 않았을 수 있습니다.");
                    
                    // 마우스 디바이스를 수동으로 추가 시도
                    var mouse = InputSystem.AddDevice<Mouse>();
                    if (mouse != null)
                    {
                        Debug.Log("마우스 디바이스를 수동으로 추가했습니다.");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"마우스 입력 시스템 초기화 실패: {e.Message}");
            }
            
            // 터치 이벤트 바인딩
            Touch.onFingerDown += OnFingerDown;
            Touch.onFingerUp += OnFingerUp;
            Touch.onFingerMove += OnFingerMove;
            
            playerAction.playerControl.Pause.performed += Pause_Performed;
            playerAction.playerControl.Enable();

            eventSystem = EventSystem.current;
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            if (EnhancedTouchSupport.enabled)
            {
                Touch.onFingerDown -= OnFingerDown;
                Touch.onFingerUp -= OnFingerUp;
                Touch.onFingerMove -= OnFingerMove;
                EnhancedTouchSupport.Disable();
            }

            playerAction.playerControl.Pause.performed -= Pause_Performed;
            playerAction.playerControl.Disable();
            playerAction.Disable();
        }

        private void OnDestroy()
        {
            Touch.onFingerDown -= OnFingerDown;
            Touch.onFingerUp -= OnFingerUp;
            Touch.onFingerMove -= OnFingerMove;
        }

        private void OnFingerDown(Finger finger)
        {
            if (!isEnabled) return;
            
            ProcessFingerDownAsync(finger).Forget();
        }

        private async UniTaskVoid ProcessFingerDownAsync(Finger finger)
        {
            // 한 프레임 대기하여 UI 상태가 업데이트되도록 함
            await UniTask.Yield();

            if (eventSystem.IsPointerOverGameObject(finger.index)) return;
            
            // 터치가 유효한지 확인
            if (!finger.currentTouch.valid) return;

            try 
            {
                Vector2 screenPos = finger.currentTouch.screenPosition;
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                var touchData = new TouchData(finger.index, screenPos, worldPos, finger.currentTouch.phase);
                
                OnTouchPressAction?.Invoke(this, touchData);
            }
            catch (InvalidOperationException)
            {
                Debug.LogWarning("터치 입력이 유효하지 않습니다.");
                return;
            }
        }

        private void OnFingerUp(Finger finger)
        {
            if (!isEnabled) return;

            try
            {
                // 터치가 유효한지 확인
                if (!finger.currentTouch.valid) return;

                Vector2 screenPos = finger.currentTouch.screenPosition;
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                var touchData = new TouchData(finger.index, screenPos, worldPos, finger.currentTouch.phase);
                
                OnTouchPressEndAction?.Invoke(this, touchData);
            }
            catch (InvalidOperationException)
            {
                Debug.LogWarning("터치 종료 입력이 유효하지 않습니다.");
                return;
            }
            catch (Exception e)
            {
                Debug.LogError($"OnFingerUp 오류: {e.Message}");
                return;
            }
        }

        private void OnFingerMove(Finger finger)
        {
            if (!isEnabled) return;

            try
            {
                // 터치가 유효한지 확인
                if (!finger.currentTouch.valid) return;

                Vector2 screenPos = finger.currentTouch.screenPosition;
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                var touchData = new TouchData(finger.index, screenPos, worldPos, finger.currentTouch.phase);
                
                OnTouchMoveAction?.Invoke(this, touchData);
            }
            catch (InvalidOperationException)
            {
                Debug.LogWarning("터치 이동 입력이 유효하지 않습니다.");
                return;
            }
            catch (Exception e)
            {
                Debug.LogError($"OnFingerMove 오류: {e.Message}");
                return;
            }
        }

        private void Pause_Performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            OnPauseAction?.Invoke(this, EventArgs.Empty);
        }

        public Vector2 GetCurTouchScreenPosition()
        {
            if (Touch.activeTouches.Count > 0)
            {
                return Touch.activeTouches[0].screenPosition;
            }
            return Vector2.zero;
        }

        public void DisableAllInput()
        {
            isEnabled = false;
            playerAction.playerControl.Disable();
            playerAction.Disable();
            
            if (eventSystem != null)
            {
                eventSystem.enabled = false;
            }
            
            CameraView2D.SetForceDisabled(true);
        }

        public void EnableAllInput()
        {
            isEnabled = true;
            playerAction.playerControl.Enable();
            playerAction.Enable();
            
            if (eventSystem != null)
            {
                eventSystem.enabled = true;
            }
            
            CameraView2D.SetForceDisabled(false);
        }

        public void DisableGameInputOnly()
        {
            isEnabled = false;
            playerAction.playerControl.Disable();
            playerAction.Disable();
            
            // UI는 활성화 상태로 유지
            if (eventSystem != null)
            {
                eventSystem.enabled = true;
            }
            
            // 카메라 컨트롤을 강제로 비활성화 (UI 드래그 상태와 관계없이)
            CameraView2D.SetForceDisabled(true);
        }

        public void EnableGameInputOnly()
        {
            isEnabled = true;
            playerAction.playerControl.Enable();
            playerAction.Enable();
            
            // UI는 활성화 상태로 유지
            if (eventSystem != null)
            {
                eventSystem.enabled = true;
            }
            
            // 카메라 컨트롤 강제 비활성화 해제 (UI 드래그 상태에 따라 자동 관리)
            CameraView2D.SetForceDisabled(false);
        }

        public bool IsInputEnabled()
        {
            return isEnabled;
        }
    }
}