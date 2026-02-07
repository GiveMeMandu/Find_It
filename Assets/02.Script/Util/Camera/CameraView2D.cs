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
        [SerializeField] private bool _forceDisabledDebug = false; // _forceDisabled 상태 디버깅용

        [Header("---Zoom---")]
        public bool _enableZoom;
        public float zoomMin = 2f;
        public float zoomMax = 5.4f;
        [Header("Zoom Speed")]
        [Tooltip("터치 핀치 줌 속도")]
        public float touchPinchZoomSpeed = 0.005f;
        [Tooltip("마우스 휠 줌 속도")]
        public float mouseWheelZoomSpeed = 2f;
        [Tooltip("비모바일 플랫폼에서 부드러운 줌 보간 속도")]
        public float smoothZoomLerpSpeed = 10f;
        public float zoomPan = 0f;

        [Header("---Pan---")]
        public bool _enablePan;
        [Header("Pan Speed")]
        [Tooltip("PC 마우스 드래그 이동 속도")]
        public float pcCamPanSpeed = 5.0f;
        [Tooltip("모바일 터치 드래그 이동 속도")]
        public float mobilePanSpeed = 1.5f;
        public bool _infinitePan = false;
        public bool _autoPanBoundary = true;
        public float _panMinX, _panMinY;
        public float _panMaxX, _panMaxY;
        
        // 상태 관리 변수들
        private bool _forceDisabled = false; // InputManager에서 강제로 비활성화된 상태
        private bool _uiDragState = false; // UI 드래그 중인 상태

        private UnityEngine.Camera _camera;
        private Vector2 _previousTouchPosition;
        private float _previousTouchDistance;
        private bool _isDragging;
        private const float MIN_PINCH_DISTANCE = 50f;
        
        // 카메라 이동 관련 변수들
        private bool _isMovingCamera = false;
        
        // 부드러운 줌 관련 변수들 (비모바일 플랫폼)
        private float _targetOrthographicSize;
        private Vector2 _zoomFocusScreenPoint;
        private bool _isSmoothZooming = false;
        
        // Input Action 관련 변수들
        private PlayerAction _playerInputActions;
        private InputAction _mouseWheelAction;

        protected override void Awake()
        {
            base.Awake();
            _camera = UnityEngine.Camera.main;
            _targetOrthographicSize = _camera.orthographicSize;
            
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
                // Debug.Log($"[Input System Debug] 초기화 시작");
                // Debug.Log($"[Input System Debug] InputSystem.settings: {InputSystem.settings}");
                // Debug.Log($"[Input System Debug] 현재 활성화된 디바이스 수: {InputSystem.devices.Count}");
                
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
                    // Debug.Log($"Mouse.current 정상 작동: {Mouse.current.name}");
                    // Debug.Log($"Mouse.current.scroll: {Mouse.current.scroll}");
                    // Debug.Log($"Mouse.current.scroll.ReadValue(): {Mouse.current.scroll.ReadValue()}");
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
                        var panDelta = movement * pcCamPanSpeed * Time.deltaTime * _camera.orthographicSize;
                        var newPosition = _camera.transform.position + new Vector3(panDelta.x, panDelta.y, 0);
                        _camera.transform.position = _infinitePan ? newPosition : ClampCamera(newPosition);
                    }
                }
                return;
            }

            // 마우스 드래그로 이동
            if (_enablePan && Mouse.current.leftButton.isPressed)
            {
                var mouseDelta = Mouse.current.delta.ReadValue() * (_camera.orthographicSize / _camera.pixelHeight) * pcCamPanSpeed;
                var newPosition = _camera.transform.position - new Vector3(mouseDelta.x, mouseDelta.y, 0);
                _camera.transform.position = _infinitePan ? newPosition : ClampCamera(newPosition);
            }
        }

        private void HandleMouseWheelInput()
        {
            // 카메라 이동 중이거나 강제 비활성화 상태면 입력 무시
            if (_isMovingCamera || _forceDisabled || !_enableZoom) 
            {
                if (_showDebugInfo && !_enableZoom)
                    Debug.Log($"[Zoom Disabled] _enableZoom={_enableZoom}, _forceDisabled={_forceDisabled}, _isMovingCamera={_isMovingCamera}");
                return;
            }

            bool inputDetected = false;
            float finalScroll = 0f;
            Vector2 finalMousePos = Vector2.zero;

            // Method 1: 기존 New Input System (가장 직접적이고 안정적)
            if (Mouse.current != null)
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

            // Method 2: PlayerAction을 이용한 마우스 휠 입력 (백업)
            if (!inputDetected && _mouseWheelAction != null)
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

            // 실제 줌 실행 (부드러운 줌을 위해 타겟만 설정)
            if (inputDetected)
            {
                var zoomDelta = finalScroll * mouseWheelZoomSpeed;
                _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize - zoomDelta, zoomMin, zoomMax);
                _zoomFocusScreenPoint = finalMousePos;
                _isSmoothZooming = true;
                
                // 디버그 정보 업데이트
                _lastScrollValue = finalScroll;
                
                if (_showDebugInfo)
                    Debug.Log($"[Zoom Execute] targetSize={_targetOrthographicSize}, method={_lastInputMethod}");
            }
        }
        
        /// <summary>
        /// 비모바일 플랫폼에서 부드러운 줌을 처리합니다.
        /// 매 프레임 현재 줌 값을 타겟 값으로 보간합니다.
        /// </summary>
        private void UpdateSmoothZoom()
        {
            if (!_isSmoothZooming || !_enableZoom || _isMovingCamera || _forceDisabled) return;
            
            float currentSize = _camera.orthographicSize;
            
            // 타겟에 충분히 가까우면 스냅
            if (Mathf.Abs(currentSize - _targetOrthographicSize) < 0.001f)
            {
                _camera.orthographicSize = _targetOrthographicSize;
                _isSmoothZooming = false;
                return;
            }
            
            // 줌 포인트 기준 위치 보정을 위해 줌 전 월드 좌표 저장
            var worldPointBeforeZoom = _camera.ScreenToWorldPoint(new Vector3(_zoomFocusScreenPoint.x, _zoomFocusScreenPoint.y, 0));
            
            // 부드러운 보간
            _camera.orthographicSize = Mathf.Lerp(currentSize, _targetOrthographicSize, Time.deltaTime * smoothZoomLerpSpeed);
            
            // 줌 포인트를 기준으로 카메라 위치 조정
            var worldPointAfterZoom = _camera.ScreenToWorldPoint(new Vector3(_zoomFocusScreenPoint.x, _zoomFocusScreenPoint.y, 0));
            var offset = worldPointBeforeZoom - worldPointAfterZoom;
            
            var newPosition = _camera.transform.position + offset;
            _camera.transform.position = _infinitePan ? newPosition : ClampCamera(newPosition);
        }
#endif

        private void Update()
        {
            if (backgroundSprite == null) return;
            
            // 디버그 정보 업데이트
            if (_showDebugInfo)
            {
                _mouseCurrentAvailable = Mouse.current != null;
                _forceDisabledDebug = _forceDisabled; // 인스펙터에서 확인용
            }
            
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
            UpdateSmoothZoom();
            
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
                        _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize - 1f * mouseWheelZoomSpeed, zoomMin, zoomMax);
                        _zoomFocusScreenPoint = mousePos;
                        _isSmoothZooming = true;
                        _lastScrollValue = 1f;
                        _lastInputMethod = "Editor Shortcut";
                    }
                    else if (Keyboard.current.minusKey.wasPressedThisFrame)
                    {
                        Debug.Log("[Editor Shortcut] Ctrl + - 로 줌아웃");
                        var mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(Screen.width/2, Screen.height/2);
                        _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize + 1f * mouseWheelZoomSpeed, zoomMin, zoomMax);
                        _zoomFocusScreenPoint = mousePos;
                        _isSmoothZooming = true;
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
            _targetOrthographicSize = _camera.orthographicSize; // 부드러운 줌 타겟 동기화

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
            
            // InputManager에서 강제로 비활성화된 상태라면 SetEnablePanAndZoom 호출을 무시
            if (Instance._forceDisabled)
            {
                return;
            }
            
            Instance._enablePan = value;
            Instance._enableZoom = value;
        }
        
        /// <summary>
        /// InputManager에서 강제로 카메라 컨트롤을 비활성화/활성화합니다.
        /// 이 상태에서는 UI 드래그 상태와 관계없이 강제로 적용됩니다.
        /// </summary>
        public static void SetForceDisabled(bool value)
        {
            if (Instance == null) 
            {
                Debug.LogWarning("[CameraView2D] SetForceDisabled: Instance is null!");
                return;
            }
            
            if (Instance._showDebugInfo)
            {
                Debug.Log($"[CameraView2D] SetForceDisabled: {Instance._forceDisabled} -> {value}");
            }
            
            Instance._forceDisabled = value;
            
            if (value)
            {
                // 강제 비활성화 시 즉시 카메라 컨트롤 비활성화
                Instance._enablePan = false;
                Instance._enableZoom = false;
            }
            else
            {
                // 강제 비활성화 해제 시 UI 드래그 상태에 따라 결정
                UpdateActualPanAndZoomState();
            }
        }
        
        /// <summary>
        /// UI 드래그 상태를 설정합니다.
        /// InputManager가 강제 비활성화 상태가 아닐 때만 적용됩니다.
        /// </summary>
        public static void SetUIDragState(bool value)
        {
            if (Instance == null) return;
            
            // InputManager에서 강제로 비활성화된 상태라면 UI 드래그 상태 변경을 무시
            if (Instance._forceDisabled)
            {
                return;
            }
            
            Instance._uiDragState = value;
            UpdateActualPanAndZoomState();
        }
        
        /// <summary>
        /// 실제 팬과 줌 상태를 업데이트합니다.
        /// InputManager 강제 비활성화 상태가 우선순위를 가집니다.
        /// </summary>
        private static void UpdateActualPanAndZoomState()
        {
            if (Instance == null) return;
            
            // InputManager에서 강제로 비활성화된 경우 - UI 드래그 상태와 관계없이 항상 비활성화
            if (Instance._forceDisabled)
            {
                Instance._enablePan = false;
                Instance._enableZoom = false;
                return; // 강제 비활성화 상태에서는 UI 드래그 상태를 무시
            }
            
            // InputManager가 강제 비활성화 상태가 아닐 때만 UI 드래그 상태를 고려
            Instance._enablePan = !Instance._uiDragState;
            Instance._enableZoom = !Instance._uiDragState;
        }

        private Vector3 ClampCamera(Vector3 targetPosition)
        {
            var orthographicSize = _camera.orthographicSize;
            var camWidth = orthographicSize * _camera.aspect;

            float minX, minY, maxX, maxY;

            // _autoPanBoundary가 false이면 수동으로 설정된 경계값 사용 (FogModeManager 등에서 설정)
            if (!_autoPanBoundary)
            {
                // 수동으로 설정된 _panMinX, _panMaxX, _panMinY, _panMaxY 값 사용
                minX = _panMinX + camWidth;
                minY = _panMinY + orthographicSize;
                maxX = _panMaxX - camWidth;
                maxY = _panMaxY - orthographicSize;
            }
            else
            {
                // _autoPanBoundary가 true이면 backgroundSprite 기준으로 자동 계산
                var position = backgroundSprite.transform.position;
                var bounds = backgroundSprite.bounds;

                var margin = 0.01f;
                _panMinX = position.x - bounds.size.x / 2f + margin;
                _panMinY = position.y - bounds.size.y / 2f + margin;
                _panMaxX = position.x + bounds.size.x / 2f - margin;
                _panMaxY = position.y + bounds.size.y / 2f - margin;

                minX = _panMinX + camWidth;
                minY = _panMinY + orthographicSize;
                maxX = _panMaxX - camWidth;
                maxY = _panMaxY - orthographicSize;
            }

            // 경계값이 역전된 경우 중앙으로 설정
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
            _targetOrthographicSize = _camera.orthographicSize;
            _isSmoothZooming = false;
            _isMovingCamera = false;
            
            // Debug.Log($"카메라 이동 완료: {clampedTargetPosition}");
        }
        
        /// <summary>
        /// 지정된 Orthographic Size로 카메라를 부드럽게 줌합니다.
        /// </summary>
        /// <param name="targetSize">목표 Orthographic Size</param>
        /// <param name="duration">줌 시간 (초)</param>
        public async UniTask ZoomCameraToSizeAsync(float targetSize, float duration = 1f)
        {
            if (_camera == null) return;
            
            float startSize = _camera.orthographicSize;
            float clampedTargetSize = Mathf.Clamp(targetSize, zoomMin, zoomMax);
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                // 부드러운 줌을 위한 이징 함수 (ease-in-out)
                t = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
                
                _camera.orthographicSize = Mathf.Lerp(startSize, clampedTargetSize, t);
                await UniTask.Yield();
            }
            
            _camera.orthographicSize = clampedTargetSize;
            _targetOrthographicSize = clampedTargetSize;
            _isSmoothZooming = false;
        }
        
        /// <summary>
        /// 지정된 위치로 카메라를 이동하면서 동시에 줌합니다.
        /// </summary>
        public async UniTask MoveCameraAndZoomAsync(Vector3 targetPosition, float targetZoomSize, float duration = 1f)
        {
            if (_isMovingCamera) return;
            
            _isMovingCamera = true;
            
            var startPosition = _camera.transform.position;
            var startSize = _camera.orthographicSize;
            var clampedTargetSize = Mathf.Clamp(targetZoomSize, zoomMin, zoomMax);
            
            // 목표 위치의 Z좌표는 유지
            var clampedTargetPosition = targetPosition;
            clampedTargetPosition.z = startPosition.z;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                
                // 부드러운 이동 + 줌을 위한 이징 함수 (ease-out)
                t = 1f - Mathf.Pow(1f - t, 3f);
                
                _camera.transform.position = Vector3.Lerp(startPosition, clampedTargetPosition, t);
                _camera.orthographicSize = Mathf.Lerp(startSize, clampedTargetSize, t);
                
                await UniTask.Yield();
            }
            
            _camera.transform.position = clampedTargetPosition;
            _camera.orthographicSize = clampedTargetSize;
            _targetOrthographicSize = clampedTargetSize;
            _isSmoothZooming = false;
            _isMovingCamera = false;
        }
        
        /// <summary>
        /// 현재 카메라의 Orthographic Size를 반환합니다.
        /// </summary>
        public float CurrentOrthographicSize => _camera != null ? _camera.orthographicSize : 5.4f;

        /// <summary>
        /// 카메라가 현재 이동 중인지 확인합니다.
        /// </summary>
        public bool IsMovingCamera => _isMovingCamera;
    }
}