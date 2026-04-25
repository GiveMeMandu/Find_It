namespace echo17.EndlessBook.Demo02
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>
    /// RenderTexture 기반으로 화면을 책에 출력하는 경우,
    /// 터치 좌표를 이벤트 시스템(UI Canvas)으로 전달하여 클릭/드래그를 가능하게 해주는 뷰 클래스입니다.
    /// </summary>
    public class PageView_UI : PageView
    {
        [Header("UI Interaction Settings")]
        [Tooltip("UICamera가 렌더링하고 있는, 책 머티리얼에 적용된 렌더 텍스처를 연결해주세요.")]
        public RenderTexture targetRenderTexture;
            
        [Tooltip("이벤트를 강제로 전달할 타겟 캔버스의 GraphicRaycaster를 연결해주세요.")]
        public GraphicRaycaster uiRaycaster;

        [Header("Drag Settings")]
        [Tooltip("드래그 민감도 (기본 1.0. 값이 너무 크면 수치를 낮춰보세요. 예: 0.2~0.5)")]
        public float dragSensitivity = 0.3f;

        private PointerEventData currentEventData;
        private GameObject currentPointerPress;
        private GameObject currentDragObject;
        [Header("Gizmo Debug")]
        [Tooltip("Enable drawing gizmos to debug pointer mapping and render texture bounds.")]
        public bool debugDrawGizmos = true;
        public Color gizmoRTBoundsColor = Color.cyan;
        public Color gizmoPointerDownColor = Color.green;
        public Color gizmoPointerUpColor = Color.red;
        public Color gizmoDragColor = Color.yellow;
        public float gizmoSphereSize = 0.02f;

        // Debug state captured on pointer events
        private Vector2 debugLastPointerDownNormalized;
        private Vector2 debugLastPointerUpNormalized;
        private Vector2 debugLastDragNormalized;
        private Vector2 debugLastDragDeltaNormalized;
        private bool debugHasPointerDown = false;
        private bool debugHasPointerUp = false;
        private bool debugHasDrag = false;
        
        /// <summary>
        /// 현재 UI 요소를 터치(상호작용) 중인지 여부입니다.
        /// </summary>
        public bool isUIInteracting { get; private set; }

        /// <summary>
        /// Canvas의 Event Camera를 현재 UI를 보고 있는 카메라로 덮어씌웁니다.
        /// 하나의 캔버스를 두 카메라가 볼 때 레이캐스트 시점이 꼬이는 것을 방지합니다.
        /// </summary>
        private void SwapEventCamera()
        {
            if (uiRaycaster == null) return;
            Canvas canvas = uiRaycaster.GetComponent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            {
                // PageView(부모 클래스)에서 캐싱한 pageViewCamera를 Event Camera로 사용
                canvas.worldCamera = pageViewCamera;
            }
        }

        /// <summary>
        /// 화면(책)을 터치했을 때 호출하여 UI 요소에 PointerDown 이벤트를 전달합니다.
        /// </summary>
        public void HandlePointerDown(Vector2 hitPointNormalized)
        {
            if (targetRenderTexture == null || EventSystem.current == null || uiRaycaster == null) return;

            SwapEventCamera(); // 레이캐스트를 쏘기 전 캔버스의 카메라를 자신으로 교체

            Vector2 screenPos = NormalizedToScreen(hitPointNormalized);
            currentEventData = new PointerEventData(EventSystem.current)
            {
                position = screenPos,
                button = PointerEventData.InputButton.Left
            };

            List<RaycastResult> results = new List<RaycastResult>();
            // 전역 이벤트 시스템이 아닌 해당 캔버스로만 직접 레이캐스트 발사
            uiRaycaster.Raycast(currentEventData, results);
            
            Debug.Log($"{gameObject.name} [PageView_UI] PointerDown - 터치 좌표(정규화): {hitPointNormalized}, 픽셀해상도 변환 좌표: {screenPos}");
            Debug.Log($"{gameObject.name} [PageView_UI] PointerDown - UI 레이캐스트 맞은 개수: {results.Count}");

            if (results.Count > 0)
            {
                isUIInteracting = true; // UI가 인식됨
                currentEventData.pointerCurrentRaycast = results[0];
                GameObject hitObject = results[0].gameObject;
                Debug.Log($"[PageView_UI] PointerDown - 가장 먼저 맞은 UI 오브젝트: {hitObject.name}");

                // record debug pointer down
                debugLastPointerDownNormalized = hitPointNormalized;
                debugHasPointerDown = true;
                debugHasPointerUp = false;
                debugHasDrag = false;

                // UI PointerDown 이벤트 실행
                currentPointerPress = ExecuteEvents.ExecuteHierarchy(hitObject, currentEventData, ExecuteEvents.pointerDownHandler);
                if (currentPointerPress == null)
                {
                    currentPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hitObject);
                }

                // 스크롤뷰 등을 대비한 초기화 (잠재적 드래그 인식)
                currentEventData.pointerPress = currentPointerPress;
                currentDragObject = ExecuteEvents.ExecuteHierarchy(hitObject, currentEventData, ExecuteEvents.initializePotentialDrag);
            }
            else
            {
                isUIInteracting = false; // UI가 아닌 빈 공간
            }
        }

        /// <summary>
        /// 드래그(스크롤)를 처리합니다.
        /// </summary>
        public void HandleDrag(Vector2 hitPointNormalized, Vector2 deltaNormalized)
        {
            if (currentEventData == null || targetRenderTexture == null) return;

            SwapEventCamera(); // 드래그 중에도 이벤트 카메라 최신화

            Vector2 screenPos = NormalizedToScreen(hitPointNormalized);
            Vector2 deltaScreen = new Vector2(deltaNormalized.x * targetRenderTexture.width, deltaNormalized.y * targetRenderTexture.height) * dragSensitivity;

            // record debug drag
            debugLastDragNormalized = hitPointNormalized;
            debugLastDragDeltaNormalized = deltaNormalized;
            debugHasDrag = true;

            currentEventData.position = screenPos;
            currentEventData.delta = deltaScreen;

            if (!currentEventData.dragging && currentDragObject != null)
            {
                ExecuteEvents.Execute(currentDragObject, currentEventData, ExecuteEvents.beginDragHandler);
                currentEventData.dragging = true;
            }

            if (currentEventData.dragging && currentDragObject != null)
            {
                ExecuteEvents.Execute(currentDragObject, currentEventData, ExecuteEvents.dragHandler);
            }
        }

        /// <summary>
        /// 터치를 뗐을 때 호출하여 UI 요소에 PointerUp 및 Click 이벤트를 전달합니다.
        /// </summary>
        public bool HandlePointerUp(Vector2 hitPointNormalized)
        {
            if (currentEventData == null || targetRenderTexture == null) return false;

            SwapEventCamera(); // 포인터 업 중에도 이벤트 카메라 최신화

            bool wasUIInteracting = isUIInteracting;

            Vector2 screenPos = NormalizedToScreen(hitPointNormalized);
            currentEventData.position = screenPos;

            // record debug pointer up
            debugLastPointerUpNormalized = hitPointNormalized;
            debugHasPointerUp = true;
            // keep pointer down flag as-is for possible comparison in gizmos

            if (currentPointerPress != null)
            {
                // Pointer Up 핸들러
                ExecuteEvents.Execute(currentPointerPress, currentEventData, ExecuteEvents.pointerUpHandler);

                // 누른 대상과 뗀 대상이 같다면 (드래그하지 않고 그 자리에서 뗐다면) Click 판정
                GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentPointerPress);
                if (currentPointerPress == pointerUpHandler)
                {
                    ExecuteEvents.Execute(currentPointerPress, currentEventData, ExecuteEvents.pointerClickHandler);
                }

                // 포인터가 영역을 벗어났음(Exit)을 강제로 알려 Hover/Pressed 잔상 제거
                ExecuteEvents.Execute(currentPointerPress, currentEventData, ExecuteEvents.pointerExitHandler);
            }

            if (currentDragObject != null && currentEventData.dragging)
            {
                ExecuteEvents.Execute(currentDragObject, currentEventData, ExecuteEvents.endDragHandler);
            }

            // 이벤트 종료 처리
            currentEventData = null;
            currentPointerPress = null;
            currentDragObject = null;
            isUIInteracting = false;

            // 터치가 끝날 때 잔여 포커스(선택) 상태를 강제로 해제하여 
            // 버튼이 계속 하이라이트(Selected)되어 있는 현상을 방지합니다.
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            return wasUIInteracting;
        }

        private Vector2 NormalizedToScreen(Vector2 hitPointNormalized)
        {
            Camera cam = pageViewCamera != null ? pageViewCamera : GetComponentInChildren<Camera>();
            float width = targetRenderTexture.width;
            float height = targetRenderTexture.height;
            
            if (cam != null && cam.targetTexture != null)
            {
                width = cam.pixelWidth;
                height = cam.pixelHeight;
            }

            return new Vector2(
                hitPointNormalized.x * width,
                hitPointNormalized.y * height
            );
        }

        // Map a viewport-normalized point to a world point using the page camera and raycasts
        private Vector3 GetWorldPointForViewport(Vector2 normalized, Camera cam)
        {
            if (cam == null) cam = pageViewCamera != null ? pageViewCamera : GetComponentInChildren<Camera>();
            if (cam == null) return transform.position;

            Ray ray = cam.ViewportPointToRay(new Vector3(normalized.x, normalized.y, 0f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxRayCastDistance, raycastLayerMask))
            {
                return hit.point;
            }

            float depth = Mathf.Max(cam.nearClipPlane + 0.1f, 0.5f);
            return cam.ViewportToWorldPoint(new Vector3(normalized.x, normalized.y, depth));
        }

        private void DrawPointAtViewport(Vector2 normalized, Color color)
        {
            Camera cam = pageViewCamera != null ? pageViewCamera : GetComponentInChildren<Camera>();
            if (cam == null) return;
            Vector3 world = GetWorldPointForViewport(normalized, cam);
            Gizmos.color = color;
            Gizmos.DrawSphere(world, gizmoSphereSize);
        }

        private void OnDrawGizmos()
        {
            if (!debugDrawGizmos) return;

            Camera cam = pageViewCamera != null ? pageViewCamera : GetComponentInChildren<Camera>();
            if (cam == null) return;

            // Draw render-texture / camera viewport bounds by raycasting the four viewport corners
            Vector2[] cornersViewport = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) };
            Vector3[] worldCorners = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                Ray r = cam.ViewportPointToRay(new Vector3(cornersViewport[i].x, cornersViewport[i].y, 0f));
                RaycastHit hit;
                if (Physics.Raycast(r, out hit, maxRayCastDistance, raycastLayerMask))
                {
                    worldCorners[i] = hit.point;
                }
                else
                {
                    worldCorners[i] = cam.ViewportToWorldPoint(new Vector3(cornersViewport[i].x, cornersViewport[i].y, cam.nearClipPlane + 0.5f));
                }
            }

            Gizmos.color = gizmoRTBoundsColor;
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(worldCorners[i], worldCorners[(i + 1) % 4]);
            }

            // Draw last pointer positions and drag
            if (debugHasPointerDown) DrawPointAtViewport(debugLastPointerDownNormalized, gizmoPointerDownColor);
            if (debugHasPointerUp) DrawPointAtViewport(debugLastPointerUpNormalized, gizmoPointerUpColor);
            if (debugHasDrag) DrawPointAtViewport(debugLastDragNormalized, gizmoDragColor);

            if (debugHasPointerDown && debugHasDrag)
            {
                Vector3 p1 = GetWorldPointForViewport(debugLastPointerDownNormalized, cam);
                Vector3 p2 = GetWorldPointForViewport(debugLastDragNormalized, cam);
                Gizmos.color = gizmoDragColor;
                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}