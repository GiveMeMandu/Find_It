using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D 횡스크롤 게임용 카메라 패닝 컴포넌트
/// 마우스 또는 게임패드 입력에 따라 카메라를 상하좌우로 부드럽게 이동시킵니다.
/// </summary>
public class CameraRootPanning : MonoBehaviour
{
    [Header("Camera Panning Settings")]
    [Tooltip("좌우 패닝을 위한 커브 (0~1 입력에 대한 오프셋 배율)")]
    [SerializeField] private AnimationCurve cameraPanningHorizontalCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Tooltip("상하 패닝을 위한 커브 (0~1 입력에 대한 오프셋 배율)")]
    [SerializeField] private AnimationCurve cameraPanningVerticalCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Tooltip("패닝 속도 (높을수록 빠르게 반응)")]
    [SerializeField] private float panningSpeed = 10f;
    [Tooltip("카메라 패닝 활성화 여부")]
    [SerializeField] private bool enableCameraPanning = true;

    [Header("Panning Range Settings")]
    [InfoBox("패닝 범위를 제한합니다. 0이면 제한 없음")]
    [Tooltip("X축 최대 패닝 거리")]
    [SerializeField] private float maxHorizontalPanning = 2f;
    [Tooltip("Y축 최대 패닝 거리")]
    [SerializeField] private float maxVerticalPanning = 1.5f;

    [Header("Panning Inversion Settings")]
    [InfoBox("패닝 반전 옵션은 씬에 따라 다르게 설정할 수 있습니다.")]
    [LabelText("좌우 반전")]
    [SerializeField] private bool invertHorizontalPanning = false;
    [LabelText("상하 반전")]
    [SerializeField] private bool invertVerticalPanning = false;

    [Header("Camera Target Settings")]
    [InfoBox("카메라 루트가 지정되지 않으면 메인 카메라를 직접 사용합니다.\n" +
             "카메라 루트를 사용하면 카메라의 부모 오브젝트가 이동됩니다.")]
    [SerializeField] private Transform cameraRoot;
    [Tooltip("메인 카메라 대신 특정 카메라를 사용")]
    [SerializeField] private Camera targetCamera;

    // Baseline / offset support
    [Header("기본 위치 오프셋 설정")]
    [InfoBox("카메라의 원래 위치에서 추가로 오프셋을 줄 수 있습니다.")]
    [SerializeField] private Vector2 panningBaselineOffset = Vector2.zero; // configurable offset from original
    private Vector3 _cameraOriginalLocalPosition = Vector3.zero; // recorded once when camera root found
    private Vector3 _computedBaseline = Vector3.zero; // original + panningBaselineOffset

    private Transform _targetTransform; // 실제로 움직일 Transform

    private void Start()
    {
        InitializeComponents();
    }

    private void Update()
    {
        if (_targetTransform != null && enableCameraPanning)
        {
            UpdateCameraPanning();
        }
    }

    private void InitializeComponents()
    {
        // 카메라 루트가 지정되어 있으면 사용
        if (cameraRoot != null)
        {
            _targetTransform = cameraRoot;
        }
        else
        {
            // 카메라 루트가 없으면 카메라를 직접 사용
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                _targetTransform = targetCamera.transform;
            }
        }

        // 초기 위치 기록 및 baseline 계산
        if (_targetTransform != null)
        {
            _cameraOriginalLocalPosition = _targetTransform.localPosition;
            _computedBaseline = _cameraOriginalLocalPosition + new Vector3(panningBaselineOffset.x, panningBaselineOffset.y, 0);
        }
        else
        {
            Debug.LogWarning("[CameraRootPanning] 카메라 또는 카메라 루트를 찾을 수 없습니다.");
        }
    }

    private void UpdateCameraPanning()
    {
        if (_targetTransform == null) return;

        Vector3 localPosition = _targetTransform.localPosition;
        Vector3 newPosition;

        // Check if camera panning should be enabled
        if (!enableCameraPanning)
        {
            // When panning disabled, smoothly return to the baseline (original + configured offset)
            newPosition = _computedBaseline;
        }
        else
        {
            Vector2 normalizedPosition = GetNormalizedInputPosition();

            // Apply inversion settings
            float horizontalInput = invertHorizontalPanning ? -normalizedPosition.x : normalizedPosition.x;
            float verticalInput = invertVerticalPanning ? -normalizedPosition.y : normalizedPosition.y;

            // Evaluate curves and apply max panning limits
            float horizontalOffset = Mathf.Sign(horizontalInput) * cameraPanningHorizontalCurve.Evaluate(Mathf.Abs(horizontalInput)) * maxHorizontalPanning;
            float verticalOffset = Mathf.Sign(verticalInput) * cameraPanningVerticalCurve.Evaluate(Mathf.Abs(verticalInput)) * maxVerticalPanning;

            // 2D이므로 X, Y축만 사용
            newPosition = new Vector3(
                _computedBaseline.x + horizontalOffset,
                _computedBaseline.y + verticalOffset,
                _computedBaseline.z  // Z축은 baseline 유지
            );
        }

        _targetTransform.localPosition = Vector3.Lerp(localPosition, newPosition, panningSpeed * Time.deltaTime);
    }

    private Vector2 GetNormalizedInputPosition()
    {
        Vector2 normalizedPosition = Vector2.zero;

        // 게임패드 입력 우선 확인
        if (Gamepad.current != null && Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
        {
            normalizedPosition = Gamepad.current.rightStick.ReadValue();
        }
        // 마우스 입력 사용
        else if (Mouse.current != null)
        {
            var mousePosition = Mouse.current.position.ReadValue();
            float w = Screen.width, h = Screen.height;

            // Normalize based on the smaller dimension (usually height in 2D side scroller)
            var minimumSize = Mathf.Min(w, h);
            float hw = w * 0.5f, hh = h * 0.5f;

            var centeredPosition = mousePosition - new Vector2(hw, hh);
            normalizedPosition = centeredPosition / (minimumSize * 0.5f);
        }
        // Fallback to legacy Input system
        else
        {
            var mousePosition = InputCompatibility.MousePosition2D();
            float w = Screen.width, h = Screen.height;
            var minimumSize = Mathf.Min(w, h);
            float hw = w * 0.5f, hh = h * 0.5f;

            var centeredPosition = mousePosition - new Vector2(hw, hh);
            normalizedPosition = centeredPosition / (minimumSize * 0.5f);
        }

        // Clamp to reasonable values
        normalizedPosition.x = Mathf.Clamp(normalizedPosition.x, -1f, 1f);
        normalizedPosition.y = Mathf.Clamp(normalizedPosition.y, -1f, 1f);

        return normalizedPosition;
    }

    #region Public Methods

    /// <summary>
    /// 카메라 패닝 활성화/비활성화
    /// </summary>
    public void SetCameraPanningEnabled(bool enabled)
    {
        enableCameraPanning = enabled;
    }

    /// <summary>
    /// 패닝 속도 설정
    /// </summary>
    public void SetPanningSpeed(float speed)
    {
        panningSpeed = Mathf.Max(0f, speed);
    }

    /// <summary>
    /// 좌우 반전 설정
    /// </summary>
    public void SetHorizontalInversion(bool invert)
    {
        invertHorizontalPanning = invert;
    }

    /// <summary>
    /// 상하 반전 설정
    /// </summary>
    public void SetVerticalInversion(bool invert)
    {
        invertVerticalPanning = invert;
    }

    /// <summary>
    /// 최대 수평 패닝 거리 설정
    /// </summary>
    public void SetMaxHorizontalPanning(float maxDistance)
    {
        maxHorizontalPanning = Mathf.Max(0f, maxDistance);
    }

    /// <summary>
    /// 최대 수직 패닝 거리 설정
    /// </summary>
    public void SetMaxVerticalPanning(float maxDistance)
    {
        maxVerticalPanning = Mathf.Max(0f, maxDistance);
    }

    /// <summary>
    /// 좌우 반전 상태
    /// </summary>
    public bool IsHorizontalInverted => invertHorizontalPanning;

    /// <summary>
    /// 상하 반전 상태
    /// </summary>
    public bool IsVerticalInverted => invertVerticalPanning;

    /// <summary>
    /// Baseline Offset 설정 (줌 등의 외부 시스템에서 사용)
    /// </summary>
    public void SetBaselineOffset(Vector2 offset)
    {
        panningBaselineOffset = offset;
        _computedBaseline = _cameraOriginalLocalPosition + new Vector3(offset.x, offset.y, 0);
    }

    /// <summary>
    /// Baseline Offset의 X값을 동적으로 설정
    /// </summary>
    public void SetBaselineOffsetX(float xOffset)
    {
        panningBaselineOffset.x = xOffset;
        _computedBaseline = _cameraOriginalLocalPosition + new Vector3(panningBaselineOffset.x, panningBaselineOffset.y, 0);
    }

    /// <summary>
    /// Baseline Offset의 Y값을 동적으로 설정
    /// </summary>
    public void SetBaselineOffsetY(float yOffset)
    {
        panningBaselineOffset.y = yOffset;
        _computedBaseline = _cameraOriginalLocalPosition + new Vector3(panningBaselineOffset.x, panningBaselineOffset.y, 0);
    }

    /// <summary>
    /// 현재 Baseline Offset 가져오기
    /// </summary>
    public Vector2 GetBaselineOffset()
    {
        return panningBaselineOffset;
    }

    /// <summary>
    /// 카메라를 즉시 baseline 위치로 리셋
    /// </summary>
    public void ResetToBaseline()
    {
        if (_targetTransform != null)
        {
            _targetTransform.localPosition = _computedBaseline;
        }
    }

    /// <summary>
    /// 현재 위치를 새로운 baseline으로 설정 (카메라 이동 후 사용)
    /// </summary>
    public void UpdateBaselineToCurrentPosition()
    {
        if (_targetTransform != null)
        {
            _cameraOriginalLocalPosition = _targetTransform.localPosition;
            _computedBaseline = _cameraOriginalLocalPosition + new Vector3(panningBaselineOffset.x, panningBaselineOffset.y, 0);
        }
    }

    /// <summary>
    /// 패닝을 일시적으로 비활성화하고 카메라 이동 완료 후 현재 위치를 baseline으로 설정
    /// DOTween 시퀀스와 함께 사용하기 좋습니다
    /// </summary>
    public void DisablePanningForCameraMove()
    {
        enableCameraPanning = false;
    }

    /// <summary>
    /// 패닝을 다시 활성화하고 현재 위치를 새로운 baseline으로 설정
    /// </summary>
    public void EnablePanningAfterCameraMove()
    {
        UpdateBaselineToCurrentPosition();
        enableCameraPanning = true;
    }

    #endregion
}
