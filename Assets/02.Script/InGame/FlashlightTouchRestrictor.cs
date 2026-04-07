using UnityEngine;
using UnityEngine.Events;
using Util.CameraSetting;
using Lean.Touch;

/// <summary>
/// 손전등 UI를 드래그할 수 있게 하는 컴포넌트
/// LeanDragTranslate와 연동하여 드래그 시 카메라 입력을 차단합니다.
/// </summary>
public class FlashlightTouchRestrictor : MonoBehaviour
{
    [Header("Lean Touch Reference")]
    [Tooltip("참조할 LeanDragTranslate 컴포넌트")]
    public LeanDragTranslate leanDragTranslate;

    [Header("Drag Events")]
    [Tooltip("드래그 시작시 호출될 이벤트")]
    public UnityEvent OnDragStart;
    
    [Tooltip("드래그 종료시 호출될 이벤트")]
    public UnityEvent OnDragEnd;

    private bool wasDragging = false;
    private int framesSinceEnable = 0;

    private void Awake()
    {
        Debug.Log("[FlashlightTouchRestrictor] Awake");
        // Awake -> 사용 가능한 초기화는 최소화합니다.
        // 주요 초기화는 Start/OnEnable에서 수행하여 LeanTouch와의 초기화 순서 문제를 피합니다.
    }

    private void Start()
    {
        Debug.Log("[FlashlightTouchRestrictor] Start");
        // LeanDragTranslate 컴포넌트 자동 찾기
        if (leanDragTranslate == null)
        {
            leanDragTranslate = GetComponent<LeanDragTranslate>();
        }

        // UnityEvent 초기화 (리스너는 OnEnable에서 연결)
        if (OnDragStart == null)
            OnDragStart = new UnityEvent();
        if (OnDragEnd == null)
            OnDragEnd = new UnityEvent();
    }

    private void OnEnable()
    {
        Debug.Log("[FlashlightTouchRestrictor] OnEnable");
        framesSinceEnable = 0;
        if (OnDragStart == null)
            OnDragStart = new UnityEvent();
        if (OnDragEnd == null)
            OnDragEnd = new UnityEvent();

        // 이벤트 리스너 연결 (중복 방지 위해 제거 후 추가)
        OnDragStart.RemoveListener(StartDrag);
        OnDragEnd.RemoveListener(EndDrag);
        OnDragStart.AddListener(StartDrag);
        OnDragEnd.AddListener(EndDrag);
    }

    private void OnDisable()
    {
        Debug.Log("[FlashlightTouchRestrictor] OnDisable");
        // 이벤트 해제 및 카메라 입력 복구
        OnDragStart?.RemoveListener(StartDrag);
        OnDragEnd?.RemoveListener(EndDrag);

        if (CameraView2D.Instance != null)
        {
            CameraView2D.SetUIDragState(false);
        }

        wasDragging = false;
    }

    private void Update()
    {
        // LeanDragTranslate 비활성화
        if (leanDragTranslate != null && leanDragTranslate.enabled)
        {
            leanDragTranslate.enabled = false;
        }

        // 그냥 마우스(또는 터치) 위치를 바로 따라가도록 설정
        Vector2 screenPos = Vector2.zero;
        bool hasInput = false;

        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            screenPos = UnityEngine.InputSystem.Mouse.current.position.value;
            hasInput = true;
        }
        else if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            screenPos = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition;
            hasInput = true;
        }
        else
        {
            screenPos = UnityEngine.Input.mousePosition;
            hasInput = true;
        }

        if (hasInput)
        {
            if (transform.parent is RectTransform parentRect)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? 
                             (canvas.worldCamera != null ? canvas.worldCamera : Camera.main) : null;

                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, screenPos, cam, out Vector3 worldPoint))
                {
                    transform.position = worldPoint;
                }
            }
            else
            {
                // UI가 아닌 경우 월드 공간 변환
                if (Camera.main != null)
                {
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(Camera.main.transform.position.z)));
                    worldPos.z = transform.position.z;
                    transform.position = worldPos;
                }
            }
        }
    }

    /// <summary>
    /// 드래그 시작 - CameraView2D 입력 차단
    /// </summary>
    public void StartDrag()
    {
        Debug.Log("[FlashlightTouchRestrictor] 드래그 시작 - 카메라 입력 차단");
        
        // CameraView2D 싱글톤으로 접근하여 UI 드래그 상태 설정
        if (CameraView2D.Instance != null)
        {
            CameraView2D.SetUIDragState(true);
        }
    }

    /// <summary>
    /// 드래그 종료 - CameraView2D 입력 복구
    /// </summary>
    public void EndDrag()
    {
        Debug.Log("[FlashlightTouchRestrictor] 드래그 종료 - 카메라 입력 복구");
        
        // CameraView2D 싱글톤으로 접근하여 UI 드래그 상태 해제
        if (CameraView2D.Instance != null)
        {
            CameraView2D.SetUIDragState(false);
        }
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위한 이벤트 해제
        OnDragStart?.RemoveListener(StartDrag);
        OnDragEnd?.RemoveListener(EndDrag);
        
        // 혹시 드래그 중에 오브젝트가 파괴되는 경우를 대비하여 카메라 입력 복구
        if (CameraView2D.Instance != null)
        {
            CameraView2D.SetUIDragState(false);
        }
    }
}