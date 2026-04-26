namespace echo17.EndlessBook.Demo02
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using Lean.Touch;
    using DeskCat.FindIt.Scripts.Core.Main.Utility.DragObj;

    public class PageView_3D : PageView
    {
        [Header("Interaction Settings")]
        public PointerEventData.InputButton button = PointerEventData.InputButton.Left;

        [Header("Drag Settings")]
        [Tooltip("추종 속도. 높을수록 즉각적, 낮을수록 부드럽게 따라옴 (권장: 15~30)")]
        public float dragSmoothSpeed = 20f;

        [Header("Gizmo Debug")]
        public bool debugDrawRay = true;
        private Ray lastRay;

        private DragObj _activeDragObj;
        private Vector3? _dragStartPos;
        private Vector3 _dragTargetPos; // 부드러운 추종을 위한 목표 위치

        public override bool RayCast(Vector2 hitPointNormalized, BookActionDelegate action)
        {
            return RayCast3D(hitPointNormalized, action);
        }

        public bool RayCast3D(Vector2 hitPointNormalized, BookActionDelegate action)
        {
            if (pageViewCamera == null) pageViewCamera = GetComponentInChildren<Camera>();
            if (pageViewCamera == null) return false;

            lastRay = pageViewCamera.ViewportPointToRay(hitPointNormalized);

            // 2D 우선순위 기반 히트 탐지 (LeanClickEvent와 동일한 로직)
            Vector2 worldPoint2D = ViewportToWorld2D(hitPointNormalized);
            Collider2D[] hits2D = Physics2D.OverlapPointAll(worldPoint2D, raycastLayerMask);
            LeanClickEvent topEvent = LeanClickEvent.FindTopPriorityClickEvent(hits2D);
            if (topEvent != null)
                return HandleHit(topEvent.gameObject, topEvent.transform.position, action);

            // 3D 물리 폴백
            if (Physics.Raycast(lastRay, out RaycastHit hit3D, maxRayCastDistance, raycastLayerMask))
                return HandleHit(hit3D.collider.gameObject, hit3D.point, action);

            return false;
        }

        /// <summary>
        /// 드래그 처리: 우선순위 기반으로 DragObj를 찾고, book 카메라의 뷰포트 델타로 목표 위치를 누적한 뒤
        /// Lerp로 부드럽게 추종. OnBeginDrag/OnEndDrag는 이벤트·pan 비활성화를 위해 호출.
        /// </summary>
        public void HandleDrag3D(Vector2 hitPointNormalized, Vector2 deltaNormalized)
        {
            if (pageViewCamera == null) pageViewCamera = GetComponentInChildren<Camera>();
            if (pageViewCamera == null) return;

            // 첫 번째 호출: 우선순위 기반으로 DragObj 탐색 및 드래그 시작
            if (_activeDragObj == null)
            {
                Vector2 worldPoint2D = ViewportToWorld2D(hitPointNormalized);
                Collider2D[] hits2D = Physics2D.OverlapPointAll(worldPoint2D, raycastLayerMask);
                LeanClickEvent topEvent = LeanClickEvent.FindTopPriorityClickEvent(hits2D);

                if (topEvent != null)
                    _activeDragObj = topEvent.GetComponentInParent<DragObj>();

                if (_activeDragObj != null)
                {
                    _dragStartPos = _activeDragObj.transform.position;
                    _dragTargetPos = _activeDragObj.transform.position;
                    _activeDragObj.OnBeginDrag(null);
                }
            }

            if (_activeDragObj == null) return;

            // 콜라이더 비활성화 (필요 시)
            if (!_activeDragObj.EnableCollisionWhenDrag &&
                _activeDragObj.TryGetComponent<Collider2D>(out var col))
                col.enabled = false;

            // 뷰포트 정규화 델타 → book 카메라 월드 공간 델타 (배율 1:1)
            float near = pageViewCamera.nearClipPlane;
            Vector3 worldDelta =
                pageViewCamera.ViewportToWorldPoint(new Vector3(deltaNormalized.x, deltaNormalized.y, near)) -
                pageViewCamera.ViewportToWorldPoint(new Vector3(0f, 0f, near));

            // 목표 위치에 누적
            _dragTargetPos += worldDelta;

            // 축 고정 (목표 위치에 적용)
            if (_dragStartPos.HasValue)
            {
                if (_activeDragObj.freezeX) _dragTargetPos.x = _dragStartPos.Value.x;
                if (_activeDragObj.freezeY) _dragTargetPos.y = _dragStartPos.Value.y;
                if (_activeDragObj.freezeZ) _dragTargetPos.z = _dragStartPos.Value.z;
            }

            // 프레임레이트 독립적 지수 스무딩으로 목표 위치 추종
            float t = 1f - Mathf.Exp(-dragSmoothSpeed * Time.deltaTime);
            _activeDragObj.transform.position = Vector3.Lerp(_activeDragObj.transform.position, _dragTargetPos, t);

            _activeDragObj.onDrag?.Invoke(_activeDragObj);
        }

        /// <summary>
        /// 드래그 종료: 목표 위치로 스냅 후 OnEndDrag 호출 (드롭 영역 체크, 이벤트, 콜라이더 복원)
        /// </summary>
        public void EndDrag3D()
        {
            if (_activeDragObj != null)
            {
                _activeDragObj.transform.position = _dragTargetPos; // 목표 위치로 스냅 후 드롭 판정
                _activeDragObj.OnEndDrag(null);
                _activeDragObj = null;
            }
            _dragStartPos = null;
        }

        /// <summary>
        /// book 카메라 뷰포트 좌표를 2D 월드 좌표로 변환
        /// </summary>
        private Vector2 ViewportToWorld2D(Vector2 viewportPos)
        {
            Vector3 worldPos = pageViewCamera.ViewportToWorldPoint(
                new Vector3(viewportPos.x, viewportPos.y, pageViewCamera.nearClipPlane));
            return new Vector2(worldPos.x, worldPos.y);
        }

        private void OnDrawGizmos()
        {
            if (!debugDrawRay) return;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(lastRay.origin, lastRay.origin + lastRay.direction * 5f);
        }

        private bool HandleHit(GameObject target, Vector3 hitPoint, BookActionDelegate action)
        {
            target.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);

            var leanClick = target.GetComponent<LeanClickEvent>();
            if (leanClick == null) leanClick = target.GetComponentInParent<LeanClickEvent>();
            if (leanClick != null && leanClick.Enable)
            {
                leanClick.OnClickEvent?.Invoke();
                if (Camera.main != null)
                    LeanClickEvent.RaiseGlobalClickSuccess(
                        leanClick.gameObject, Camera.main.WorldToScreenPoint(hitPoint));
            }

            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                button = this.button,
                position = pageViewCamera.WorldToScreenPoint(hitPoint),
                pointerCurrentRaycast = new RaycastResult { gameObject = target }
            };

            return ExecuteEvents.ExecuteHierarchy(target, pointerEventData, ExecuteEvents.pointerClickHandler);
        }
    }
}
