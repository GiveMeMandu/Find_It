using UnityEngine;

namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public class CameraView3D : MonoBehaviour
    {
        [Header("---Initial Camera Rotation---"), Tooltip("Fill in initial camera rotation")] 
        public float initialX = 0f;
        public float initialY = 0f;
        
        [Header("---Orbit---")] 
        public Transform orbitTarget;
        public bool isOrbit = true;
        public float xMinLimit = 0f;
        public float xMaxLimit = 80f;
        public float yMinLimit = 0f;
        public float yMaxLimit = 80f;
        

        [Header("---Zoom---")] public bool isZoom = true;
        public float distance = 8f;
        public float distanceMin = 3f;
        public float distanceMax = 10f;

        [Header("---Pan---")] public bool isPan = true;
        public bool isPanReturn = true;
        public float panSpeed = 0.5f;

        private float _cameraX;
        private float _cameraY;
        private Vector3 orbitTargetInitialPosition;

        public static CameraView3D instance { get; private set; }


        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
            }
        }

        private void Start()
        {
            var angles = transform.eulerAngles;
            _cameraX = initialY;
            _cameraY = initialX;
            orbitTargetInitialPosition = orbitTarget.transform.position;
        }

        private void LateUpdate()
        {
            if (!orbitTarget) return;

            ZoomCamera();
            PanCamera();
            OrbitCamera();
        }

        public static void SetEnableOrbit(bool value)
        {
            if (instance == null) return;
            instance.isOrbit = value;
        }

        private void PanCamera()
        {
            if (!isPan) return;

            if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                float x = Input.GetAxis("Mouse X");
                float y = Input.GetAxis("Mouse Y");
                Vector3 right = transform.right;
                Vector3 up = transform.forward;

                right.y = 0;
                up.y = 0;

                orbitTarget.position += -right * x * panSpeed + -up * y * panSpeed;
                UpdateCameraPosition();
                UpdateCameraDistance(false);
            }

            if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
            {
                if (isPanReturn)
                {
                    orbitTarget.position = orbitTargetInitialPosition;
                    UpdateCameraPosition();
                    UpdateCameraDistance(false);
                }
            }
        }

        private void ZoomCamera()
        {
            if (!isZoom) return;
            UpdateCameraDistance(false);
        }

        private void OrbitCamera()
        {
            if (!isOrbit) return;

            if (Input.GetMouseButton(0))
            {
                UpdateCameraPosition();
                UpdateCameraDistance(true);
            }
        }

        private void UpdateCameraPosition()
        {
            _cameraX += Input.GetAxis("Mouse X") * distance * 2f;
            _cameraY -= Input.GetAxis("Mouse Y") * 2f;

            _cameraX = ClampAngle(_cameraX, xMinLimit, xMaxLimit);
            _cameraY = ClampAngle(_cameraY, yMinLimit, yMaxLimit);
        }

        private void UpdateCameraDistance(bool isRotate)
        {
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);
            if (Physics.Linecast(orbitTarget.position, transform.position, out var hit))
            {
                distance -= hit.distance;
            }

            var cameraTransform = transform;
            if (isRotate)
            {
                cameraTransform.rotation = Quaternion.Euler(_cameraY, _cameraX, 0);
            }

            cameraTransform.position = cameraTransform.rotation * new Vector3(0.0f, 0.0f, -distance) +
                                       orbitTarget.position;
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
    }
}