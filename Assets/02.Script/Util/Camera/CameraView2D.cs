using UnityEngine;
using UnityEngine.InputSystem;

namespace Util.CameraSetting
{
    public class CameraView2D : MMSingleton<CameraView2D>
    {
        public SpriteRenderer backgroundSprite;

        [Header("---Zoom---")] public bool _enableZoom;
        public float zoomMin = 2f;
        public float zoomMax = 5.4f;
        public float zoomPan = 0f;

        [Header("---Pan---")] public bool _enablePan;
        public bool _infinitePan = false;
        public bool _autoPanBoundary = true;
        public float _panMinX, _panMinY;
        public float _panMaxX, _panMaxY;

        private UnityEngine.Camera _camera;
        private Vector3 _dragOrigin;
        private Vector3 _touchStart;
        protected override void Awake()
        {
            base.Awake();
            _camera = UnityEngine.Camera.main;
            // ScaleOverflowCamera();
        }


        private void Update()
        {
            if(backgroundSprite == null) return;
            PanCamera();
            ZoomCamera();
        }


        public static void SetEnablePanAndZoom(bool value)
        {
            if (Instance == null) return;
            Instance._enablePan = value;
            Instance._enableZoom = value;
        }

        private void ScaleOverflowCamera()
        {
            if (_camera == null || !(_camera.pixelWidth > backgroundSprite.sprite.textureRect.width)) return;
            var pixel = (_camera.aspect - 1.7f) / 0.4375f;
            zoomMax -= pixel;
            if (zoomMax <= zoomMin)
            {
                zoomMax = zoomMin;
            }
        }

        private void PanCamera() {
            if (!_enablePan) return;

            Pan();
            MobilePan();
        }
        private void MobilePan() {
            if (Touchscreen.current == null || !Touchscreen.current.touches[0].isInProgress) return;
            
            var touch = Touchscreen.current.touches[0];
            var touchPosition = touch.position.ReadValue();
            
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                _dragOrigin = _camera.ScreenToWorldPoint(touchPosition);
            }
            else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                var currentPosition = _camera.ScreenToWorldPoint(touchPosition);
                var dragDifference = _dragOrigin - currentPosition;
                
                if (_infinitePan)
                {
                    _camera.transform.position += dragDifference;
                }
                else
                {
                    _camera.transform.position = ClampCamera(_camera.transform.position + dragDifference);
                }
            }
        }
        private void Pan()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _dragOrigin = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            }

            if (Mouse.current.leftButton.isPressed)
            {
                var dragDifference = _dragOrigin - _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                if (_infinitePan)
                {
                    _camera.transform.position += dragDifference;
                }
                else
                {
                    _camera.transform.position = ClampCamera(_camera.transform.position + dragDifference);
                }
            }
        }

        private void ZoomCamera()
        {
            if (!_enableZoom) return;
            Zoom(Mouse.current.scroll.ReadValue().y);
            MobileTouchZoom();
            if (!_infinitePan)
            {
                _camera.transform.position = ClampCamera(_camera.transform.position);
            }
        }

        private void MobileTouchZoom()
        {
            if (Touchscreen.current == null || !Touchscreen.current.touches[0].isInProgress || !Touchscreen.current.touches[1].isInProgress) return;

            var touch0 = Touchscreen.current.touches[0];
            var touch1 = Touchscreen.current.touches[1];

            var touch0PrevPos = touch0.position.ReadValue() - touch0.delta.ReadValue();
            var touch1PrevPos = touch1.position.ReadValue() - touch1.delta.ReadValue();

            var prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            var currentMagnitude = (touch0.position.ReadValue() - touch1.position.ReadValue()).magnitude;

            var touchDifference = currentMagnitude - prevMagnitude;
            
            var zoomSpeed = 0.005f;
            Zoom(touchDifference * zoomSpeed);
            
            if (!_infinitePan)
            {
                _camera.transform.position = ClampCamera(_camera.transform.position);
            }
        }

        private void Zoom(float increment)
        {
            _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - increment, zoomMin, zoomMax);
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
            // if (_camera.orthographicSize < zoomMax + zoomPan && _autoPanBoundary)
            // {
            //     var position = backgroundSprite.transform.position;
            //     var bounds = backgroundSprite.bounds;

            //     _panMinX = position.x - bounds.size.x / 2f;
            //     _panMinY = position.y - bounds.size.y / 2f;
            //     _panMaxX = position.x + bounds.size.x / 2f;
            //     _panMaxY = position.y + bounds.size.y / 2f;

            //     var minX = _panMinX + camWidth;
            //     var minY = _panMinY + orthographicSize;
            //     var maxX = _panMaxX - camWidth;
            //     var maxY = _panMaxY - orthographicSize;

            //     var clampX = Mathf.Clamp(targetPosition.x, minX, maxX);
            //     var clampY = Mathf.Clamp(targetPosition.y, minY, maxY);
            //     return new Vector3(clampX, clampY, targetPosition.z);
            // }
            // else
            // {
            //     if (_autoPanBoundary)
            //     {
            //         _panMinX = 0f;
            //         _panMinY = -0.5f;
            //         _panMaxX = 0;
            //         _panMaxY = -0.5f;
            //     }

            //     var minX = _panMinX; // + camWidth;
            //     var minY = _panMinY; //+ orthographicSize;
            //     var maxX = _panMaxX; // - camWidth;
            //     var maxY = _panMaxY; //- orthographicSize;

            //     var clampX = Mathf.Clamp(targetPosition.x, minX, maxX);
            //     var clampY = Mathf.Clamp(targetPosition.y, minY, maxY);
            //     return new Vector3(clampX, clampY, targetPosition.z);
            // }
        }
    }
}