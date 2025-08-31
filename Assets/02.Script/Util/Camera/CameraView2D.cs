using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Cysharp.Threading.Tasks;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Util.CameraSetting
{
    public class CameraView2D : MMSingleton<CameraView2D>
    {
        public SpriteRenderer backgroundSprite;

        [Header("---Debug Info---")]
        [SerializeField] private bool _showDebugInfo = true;
        [SerializeField] private bool _mouseCurrentAvailable = false;
        [SerializeField] private float _lastScrollValue = 0f;
        [SerializeField] private string _lastInputMethod = "None";

        [Header("---Zoom---")]
        public bool _enableZoom;
        public float zoomMin = 2f;
        public float zoomMax = 5.4f;
        [Header("Zoom Speed")]
        [Tooltip("터치 핀치 줌 속도")]
        public float touchPinchZoomSpeed = 0.005f;
        [Tooltip("마우스 휠 줌 속도")]
        public float mouseWheelZoomSpeed = 2f;
        public float zoomPan = 0f;

        [Header("---Pan---")]
        public bool _enablePan;
        [Header("Pan Speed")]
        [Tooltip("PC 마우스 드래그 이동 속도")]
        public float pcPanSpeed = 1.0f;
        [Tooltip("모바일 터치 드래그 이동 속도")]
        public float mobilePanSpeed = 1.5f;
        public bool _infinitePan = false;
        public bool _autoPanBoundary = true;
        public float _panMinX, _panMinY;
        public float _panMaxX, _panMaxY;

        private UnityEngine.Camera _camera;
        private Vector2 _previousTouchPosition;
        private float _previousTouchDistance;
        private bool _isDragging;
        private const float MIN_PINCH_DISTANCE = 50f;
        
        // 카메라 이동 관련 변수들
        private bool _isMovingCamera = false;
        
        // Input Action 관련 변수들
        private PlayerAction _playerInputActions;
        private InputAction _mouseWheelAction;

        protected override void Awake()
        {
            base.Awake();
            _camera = UnityEngine.Camera.main;
            
            // PlayerAction 초기화
            _playerInputActions = new PlayerAction();
            _mouseWheelAction = _playerInputActions.playerControl.MouseWheel;
            
            // New Input System 강제 초기화
            InitializeInputSystem();
            
        }

        private void InitializeInputSystem()
        {
            // EnhancedTouchSupport 초기화
            if (!EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
            }
            
            // Mouse 디바이스 강제 초기화
            try
            {
                Debug.Log($"[Input System Debug] 초기화 시작");
                Debug.Log($"[Input System Debug] InputSystem.settings: {InputSystem.settings}");
                Debug.Log($"[Input System Debug] 현재 활성화된 디바이스 수: {InputSystem.devices.Count}");
                
                foreach (var device in InputSystem.devices)
                {
                    Debug.Log($"[Input System Debug] 디바이스: {device.name} (타입: {device.GetType().Name})");
                }
                
                if (Mouse.current == null)
                {
                    Debug.LogWarning("Mouse.current가 null입니다. 마우스 디바이스를 강제로 추가합니다.");
                    
                    // 기존 마우스 디바이스가 있는지 확인
                    var existingMouse = InputSystem.GetDevice<Mouse>();
                    if (existingMouse == null)
                    {
                        // 마우스 디바이스 추가
                        var mouse = InputSystem.AddDevice<Mouse>();
                        Debug.Log($"마우스 디바이스 추가됨: {mouse != null}");
                    }
                    else
                    {
                        Debug.Log($"기존 마우스 디바이스 발견: {existingMouse.name}");
                    }
                }
                else
                {
                    Debug.Log($"Mouse.current 정상 작동: {Mouse.current.name}");
                    Debug.Log($"Mouse.current.scroll: {Mouse.current.scroll}");
                    Debug.Log($"Mouse.current.scroll.ReadValue(): {Mouse.current.scroll.ReadValue()}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Input System 초기화 실패: {e.Message}");
            }
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            
            // PlayerAction 활성화
            if (_playerInputActions != null)
            {
                _playerInputActions.Enable();
            }
        }

        private void OnDisable()
        {
            if (EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Disable();
            }
            
            // PlayerAction 비활성화
            if (_playerInputActions != null)
            {
                _playerInputActions.Disable();
            }
        }
        
        private void OnDestroy()
        {
            // PlayerAction 정리
            if (_playerInputActions != null)
            {
                _playerInputActions.Dispose();
                _playerInputActions = null;
            }
        }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        private void HandleMouseInput()
        {
            if (Mouse.current == null) 
            {
                // Debug.LogWarning("Mouse.current is null in HandleMouseInput!");
                // Legacy Input 사용 불가능하므로 키보드로 대체
                if (_enablePan && Keyboard.current != null)
                {
                    Vector2 movement = Vector2.zero;
                    if (Keyboard.current.wKey.isPressed) movement.y += 1;
                    if (Keyboard.current.sKey.isPressed) movement.y -= 1;
                    if (Keyboard.current.aKey.isPressed) movement.x -= 1;
                    if (Keyboard.current.dKey.isPressed) movement.x += 1;
                    
                    if (movement != Vector2.zero)
                    {
                        var panDelta = movement * pcPanSpeed * Time.deltaTime * _camera.orthographicSize;
                        var newPosition = _camera.transform.position + new Vector3(panDelta.x, panDelta.y, 0);
                        _camera.transform.position = _infinitePan ? newPosition : ClampCamera(newPosition);
                    }
                }
                return;
            }

            // 마우스 드래그로 이동
            if (_enablePan && Mouse.current.leftButton.isPressed)
            {
                var mouseDelta = Mouse.current.delta.ReadValue() * (_camera.orthographicSize / _camera.pixelHeight) * pcPanSpeed;
                var newPosition = _camera.transform.position - new Vector3(mouseDelta.x, mouseDelta.y, 0);
                _camera.transform.position = _infinitePan ? newPosition : ClampCamera(newPosition);
            }
        }

        private void HandleMouseWheelInput()
        {
            if (!_enableZoom) 
            {
                return;
            }

            bool inputDetected = false;
            float finalScroll = 0f;
            Vector2 finalMousePos = Vector2.zero;

            // Method 1: PlayerAction을 이용한 마우스 휠 입력 (가장 안정적)
            if (_mouseWheelAction != null)
            {
                var scrollValue = _mouseWheelAction.ReadValue<Vector2>();
                var scroll = scrollValue.y;
                
                // 0이 아닌 모든 값 감지
                if (scroll != 0f)
                {
                    if (_showDebugInfo)
                        Debug.Log($"[PlayerAction] 마우스 휠 감지: scroll={scroll}");
                    
                    finalScroll = scroll;
                    finalMousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(Screen.width/2, Screen.height/2);
                    inputDetected = true;
                    _lastInputMethod = "PlayerAction";
                }
            }

            // Method 2: 기존 New Input System (백업)
            if (!inputDetected && Mouse.current != null)
            {
                var scrollVector = Mouse.current.scroll.ReadValue();
                var scroll = scrollVector.y;
                
                if (scroll != 0f)
                {
                    if (_showDebugInfo)
                        Debug.Log($"[Mouse.current] 마우스 휠 감지: scroll={scroll}");
                    
                    finalScroll = scroll;
                    finalMousePos = Mouse.current.position.ReadValue();
                    inputDetected = true;
                    _lastInputMethod = "Mouse.current";
                }
            }

            // Method 3: 키보드 백업 (테스트용)
            if (!inputDetected && Keyboard.current != null)
            {
                if (Keyboard.current.numpadPlusKey.wasPressedThisFrame || 
                    Keyboard.current.equalsKey.wasPressedThisFrame ||
                    Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    if (_showDebugInfo)
                        Debug.Log("[Keyboard Input] 키보드로 줌인");
                    
                    finalScroll = 120f; // 일반적인 마우스 휠 값
                    finalMousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(Screen.width/2, Screen.height/2);
                    inputDetected = true;
                    _lastInputMethod = "Keyboard Input";
                }
                else if (Keyboard.current.numpadMinusKey.wasPressedThisFrame || 
                         Keyboard.current.minusKey.wasPressedThisFrame)
                {
                    if (_showDebugInfo)
                        Debug.Log("[Keyboard Input] 키보드로 줌아웃");
                    
                    finalScroll = -120f;
                    finalMousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(Screen.width/2, Screen.height/2);
                    inputDetected = true;
                    _lastInputMethod = "Keyboard Input";
                }
            }

            // 실제 줌 실행
            if (inputDetected)
            {
                var zoomDelta = finalScroll * mouseWheelZoomSpeed;
                HandleZoom(zoomDelta, finalMousePos);
                
                // 디버그 정보 업데이트
                _lastScrollValue = finalScroll;
                
                if (_showDebugInfo)
                    Debug.Log($"[Zoom Execute] delta={zoomDelta}, method={_lastInputMethod}");
            }
        }
#endif

        private void Update()
        {
            if (backgroundSprite == null) return;
            if (!_enablePan && !_enableZoom) return;
            
            // 카메라가 자동 이동 중일 때는 사용자 입력 무시
            if (_isMovingCamera) return;


#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            HandleMouseInput();
#endif

            // 모든 플랫폼에서 터치 입력 처리
            HandleTouchInput();
        }

        private void LateUpdate()
        {
            // 마우스 휠 입력을 LateUpdate에서 별도로 처리
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            HandleMouseWheelInput();
            
            // Unity 에디터에서 추가 입력 처리
#if UNITY_EDITOR
            HandleEditorSpecificInput();
#endif
#endif
        }

#if UNITY_EDITOR
        private void HandleEditorSpecificInput()
        {
            // Unity 에디터에서만 작동하는 추가 입력 처리
            if (!_enableZoom) return;
            
            if (Keyboard.current != null)
            {
                bool ctrlPressed = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
                
                // Ctrl + 마우스 휠 대체: Ctrl + Plus/Minus
                if (ctrlPressed)
                {
                    if (Keyboard.current.equalsKey.wasPressedThisFrame)
                    {
                        Debug.Log("[Editor Shortcut] Ctrl + = 로 줌인");
                        var mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(Screen.width/2, Screen.height/2);
                        HandleZoom(1f * mouseWheelZoomSpeed, mousePos);
                        _lastScrollValue = 1f;
                        _lastInputMethod = "Editor Shortcut";
                    }
                    else if (Keyboard.current.minusKey.wasPressedThisFrame)
                    {
                        Debug.Log("[Editor Shortcut] Ctrl + - 로 줌아웃");
                        var mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(Screen.width/2, Screen.height/2);
                        HandleZoom(-1f * mouseWheelZoomSpeed, mousePos);
                        _lastScrollValue = -1f;
                        _lastInputMethod = "Editor Shortcut";
                    }
                }
            }
        }
#endif

        private void HandleTouchInput()
        {
            // EnhancedTouchSupport가 활성화되어 있는지 확인
            if (!EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
                return;
            }

            var touches = Touch.activeTouches;
            if (touches.Count == 0) return;

            // 안드로이드에서 터치 입력 디버깅
#if UNITY_ANDROID && !UNITY_EDITOR
            if (touches.Count > 0)
            {
                // Debug.Log($"터치 감지: {touches.Count}개, 첫 번째 터치 위치: {touches[0].screenPosition}");
            }
#endif

            if (touches.Count == 1 && _enablePan)
            {
                HandleSingleTouch(touches[0]);
            }
            else if (touches.Count >= 2 && _enableZoom)
            {
                HandlePinchToZoom(touches[0], touches[1]);
            }
        }

        private void HandleSingleTouch(Touch touch)
        {
            try
            {
                switch (touch.phase)
                {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                        _isDragging = true;
                        _previousTouchPosition = touch.screenPosition;
                        // Debug.Log($"터치 시작: {touch.screenPosition}");
                        break;

                    case UnityEngine.InputSystem.TouchPhase.Moved:
                        if (!_isDragging) return;
                        
                        var touchDelta = (Vector2)touch.screenPosition - _previousTouchPosition;
                        var scaledDelta = touchDelta * (_camera.orthographicSize / _camera.pixelHeight) * mobilePanSpeed;
                        
                        var newPosition = _camera.transform.position - new Vector3(scaledDelta.x, scaledDelta.y, 0);
                        _camera.transform.position = _infinitePan ? newPosition : ClampCamera(newPosition);
                        
                        _previousTouchPosition = touch.screenPosition;
                        break;

                    case UnityEngine.InputSystem.TouchPhase.Ended:
                    case UnityEngine.InputSystem.TouchPhase.Canceled:
                        _isDragging = false;
                        // Debug.Log("터치 종료");
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"터치 처리 중 오류: {e.Message}");
                _isDragging = false;
            }
        }

        private void HandlePinchToZoom(Touch touch1, Touch touch2)
        {
            try
            {
                var currentTouchDistance = Vector2.Distance(touch1.screenPosition, touch2.screenPosition);
                
                if (currentTouchDistance < MIN_PINCH_DISTANCE) return;

                if (touch1.phase == UnityEngine.InputSystem.TouchPhase.Began || 
                    touch2.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    _previousTouchDistance = currentTouchDistance;
                    // Debug.Log($"핀치 줌 시작: 거리 {currentTouchDistance}");
                    return;
                }

                var touchDelta = currentTouchDistance - _previousTouchDistance;
                var zoomDelta = touchDelta * touchPinchZoomSpeed;

                // 줌 포인트를 두 터치의 중간점으로 설정
                var zoomCenter = (touch1.screenPosition + touch2.screenPosition) * 0.5f;
                HandleZoom(zoomDelta, zoomCenter);

                _previousTouchDistance = currentTouchDistance;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"핀치 줌 처리 중 오류: {e.Message}");
            }
        }

        private void HandleZoom(float zoomDelta, Vector2 screenZoomCenter)
        {
            if (Mathf.Approximately(zoomDelta, 0f)) return;

            // Debug.Log($"Zooming with delta: {zoomDelta}, current size: {_camera.orthographicSize}");
            
            var worldPointBeforeZoom = _camera.ScreenToWorldPoint(new Vector3(screenZoomCenter.x, screenZoomCenter.y, 0));

            var prevSize = _camera.orthographicSize;
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - zoomDelta, zoomMin, zoomMax);

            if (!Mathf.Approximately(prevSize, _camera.orthographicSize))
            {
                // 줌 포인트를 기준으로 카메라 위치 조정
                var worldPointAfterZoom = _camera.ScreenToWorldPoint(new Vector3(screenZoomCenter.x, screenZoomCenter.y, 0));
                var offset = worldPointBeforeZoom - worldPointAfterZoom;

                var newPosition = _camera.transform.position + offset;
                _camera.transform.position = _infinitePan ? newPosition : ClampCamera(newPosition);
                
                // Debug.Log($"New camera size: {_camera.orthographicSize}");
            }
        }

        public static void SetEnablePanAndZoom(bool value)
        {
            if (Instance == null) return;
            Instance._enablePan = value;
            Instance._enableZoom = value;
        }

        private Vector3 ClampCamera(Vector3 targetPosition)
        {
            var orthographicSize = _camera.orthographicSize;
            var camWidth = orthographicSize * _camera.aspect;

            var position = backgroundSprite.transform.position;
            var bounds = backgroundSprite.bounds;

            var margin = 0.01f;
            _panMinX = position.x - bounds.size.x / 2f + margin;
            _panMinY = position.y - bounds.size.y / 2f + margin;
            _panMaxX = position.x + bounds.size.x / 2f - margin;
            _panMaxY = position.y + bounds.size.y / 2f - margin;

            var minX = _panMinX + camWidth;
            var minY = _panMinY + orthographicSize;
            var maxX = _panMaxX - camWidth;
            var maxY = _panMaxY - orthographicSize;

            if (minX > maxX)
            {
                var center = (_panMinX + _panMaxX) * 0.5f;
                minX = maxX = center;
            }
            if (minY > maxY)
            {
                var center = (_panMinY + _panMaxY) * 0.5f;
                minY = maxY = center;
            }

            var clampX = Mathf.Clamp(targetPosition.x, minX, maxX);
            var clampY = Mathf.Clamp(targetPosition.y, minY, maxY);
            return new Vector3(clampX, clampY, targetPosition.z);
        }
        
        /// <summary>
        /// 지정된 위치로 카메라를 부드럽게 이동시킵니다.
        /// </summary>
        /// <param name="targetPosition">목표 위치</param>
        /// <param name="duration">이동 시간 (초)</param>
        public async UniTask MoveCameraToPositionAsync(Vector3 targetPosition, float duration = 2f)
        {
            if (_isMovingCamera) return; // 이미 이동 중이면 무시
            
            _isMovingCamera = true;
            
            var startPosition = _camera.transform.position;
            var clampedTargetPosition = _infinitePan ? targetPosition : ClampCamera(targetPosition);
            
            // Z 좌표는 유지
            clampedTargetPosition.z = startPosition.z;
            
            float elapsedTime = 0f;
            
            // Debug.Log($"카메라 이동 시작: {startPosition} -> {clampedTargetPosition}");
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                // 부드러운 이동을 위한 이징 함수 (ease-out)
                t = 1f - Mathf.Pow(1f - t, 3f);
                
                var currentPosition = Vector3.Lerp(startPosition, clampedTargetPosition, t);
                _camera.transform.position = currentPosition;
                
                await UniTask.Yield();
            }
            
            // 최종 위치 설정
            _camera.transform.position = clampedTargetPosition;
            _isMovingCamera = false;
            
            // Debug.Log($"카메라 이동 완료: {clampedTargetPosition}");
        }
        
        /// <summary>
        /// 카메라가 현재 이동 중인지 확인합니다.
        /// </summary>
        public bool IsMovingCamera => _isMovingCamera;
    }
}