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
        private Vector3 _dragOffset;
        private bool _isDragging = false;
        private GameObject _dragTarget;

        public override bool RayCast(Vector2 hitPointNormalized, BookActionDelegate action)
        {
            return RayCast3D(hitPointNormalized, action);
        }

        public bool RayCast3D(Vector2 hitPointNormalized, BookActionDelegate action)
        {
            if (pageViewCamera == null) pageViewCamera = GetComponentInChildren<Camera>();
            if (pageViewCamera == null) return false;

            lastRay = pageViewCamera.ViewportPointToRay(hitPointNormalized);
            
            RaycastHit2D hit2D = Physics2D.Raycast(lastRay.origin, lastRay.direction, maxRayCastDistance, raycastLayerMask);
            if (hit2D.collider != null) return HandleHit(hit2D.collider.gameObject, hit2D.point, action);

            RaycastHit hit;
            if (Physics.Raycast(lastRay, out hit, maxRayCastDistance, raycastLayerMask)) return HandleHit(hit.collider.gameObject, hit.point, action);

            return false;
        }

        public void HandleDrag3D(Vector2 hitPointNormalized, Vector2 deltaNormalized)
        {
            if (pageViewCamera == null) pageViewCamera = GetComponentInChildren<Camera>();
            if (pageViewCamera == null) return;

            Ray ray = pageViewCamera.ViewportPointToRay(hitPointNormalized);

            if (_dragTarget == null)
            {
                RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction, maxRayCastDistance, raycastLayerMask);
                if (hit2D.collider != null) _dragTarget = hit2D.collider.gameObject;
                else
                {
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, maxRayCastDistance, raycastLayerMask)) _dragTarget = hit.collider.gameObject;
                }
            }

            if (_dragTarget != null)
            {
                var dragObj = _dragTarget.GetComponentInParent<DragObj>();
                if (dragObj != null)
                {
                    float dist = Vector3.Distance(dragObj.transform.position, pageViewCamera.transform.position);
                    Vector3 targetWorldPos = ray.GetPoint(dist);
                    
                    if (!_isDragging)
                    {
                        _dragOffset = dragObj.transform.position - targetWorldPos;
                        _isDragging = true;
                    }
                    
                    dragObj.transform.position = targetWorldPos + _dragOffset;
                }
            }
        }

        public void EndDrag3D() 
        { 
            _isDragging = false; 
            _dragTarget = null;
        }

        private void OnDrawGizmos()
        {
            if (!debugDrawRay) return;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(lastRay.origin, lastRay.origin + lastRay.direction * 15f);
        }

        private bool HandleHit(GameObject target, Vector3 hitPoint, BookActionDelegate action)
        {
            // 1. 레거시 OnMouseDown 메시지 전달
            target.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);

            // 2. LeanClickEvent 처리 (클릭 이벤트 발사)
            var leanClick = target.GetComponentInParent<LeanClickEvent>();
            if (leanClick != null && leanClick.Enable)
            {
                leanClick.OnClickEvent?.Invoke();
            }

            // 3. 표준 인터페이스 처리
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
