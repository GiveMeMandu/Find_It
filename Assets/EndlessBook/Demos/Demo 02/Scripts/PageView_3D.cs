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

        [Header("Gizmo Debug")]
        public bool debugDrawRay = true;
        private Ray lastRay;

        private DragObj _activeDragObj;
        private Vector3 _dragStartPos;        // 드래그 시작 시 오브젝트 월드 위치
        private Vector2 _dragInitialViewport; // 드래그 시작 시 뷰포트 좌표

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
        /// 드래그 처리.
        /// 절대 오프셋 방식: 드래그 시작 뷰포트 좌표를 기준으로 매 프레임 위치를 계산하므로
        /// 누적 오차(드리프트)가 없고, Lerp 없이 직접 설정해 입력에 즉각 반응한다.
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
                    _dragInitialViewport = hitPointNormalized;
                    _activeDragObj.OnBeginDrag(null);
                }
            }

            if (_activeDragObj == null) return;

            // 콜라이더 비활성화 (필요 시)
            if (!_activeDragObj.EnableCollisionWhenDrag &&
                _activeDragObj.TryGetComponent<Collider2D>(out var col))
                col.enabled = false;

            // 드래그 시작점 기준 절대 오프셋으로 목표 위치 계산 → 누적 오차 없음
            float near = pageViewCamera.nearClipPlane;
            Vector2 viewportDelta = hitPointNormalized - _dragInitialViewport;
            Vector3 worldOffset =
                pageViewCamera.ViewportToWorldPoint(new Vector3(viewportDelta.x, viewportDelta.y, near)) -
                pageViewCamera.ViewportToWorldPoint(new Vector3(0f, 0f, near));

            Vector3 targetPos = _dragStartPos + worldOffset;

            // 축 고정 (시작 위치 기준)
            if (_activeDragObj.freezeX) targetPos.x = _dragStartPos.x;
            if (_activeDragObj.freezeY) targetPos.y = _dragStartPos.y;
            if (_activeDragObj.freezeZ) targetPos.z = _dragStartPos.z;

            _activeDragObj.transform.position = targetPos;
            _activeDragObj.onDrag?.Invoke(_activeDragObj);
        }

        /// <summary>
        /// 드래그 종료: OnEndDrag 호출 (드롭 영역 체크, 이벤트, 콜라이더 복원, pan 재활성화)
        /// </summary>
        public void EndDrag3D()
        {
            if (_activeDragObj != null)
            {
                _activeDragObj.OnEndDrag(null);
                _activeDragObj = null;
            }
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
