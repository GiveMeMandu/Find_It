namespace echo17.EndlessBook.Demo02
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class PageView_3D : PageView
    {
        [Header("Interaction Settings")]
        public PointerEventData.InputButton button = PointerEventData.InputButton.Left;
        
        [Header("Gizmo Debug")]
        public bool debugDrawRay = true;
        private Ray lastRay;

        // 명시적으로 새로운 메서드 정의
        public bool RayCast3D(Vector2 hitPointNormalized, BookActionDelegate action)
        {
            if (pageViewCamera == null)
            {
                pageViewCamera = GetComponentInChildren<Camera>();
            }
            if (pageViewCamera == null) return false;

            lastRay = pageViewCamera.ViewportPointToRay(hitPointNormalized);
            
            // 1. 2D Raycast 시도
            RaycastHit2D hit2D = Physics2D.Raycast(lastRay.origin, lastRay.direction, maxRayCastDistance, raycastLayerMask);
            if (hit2D.collider != null)
            {
                Debug.Log($"[PageView_3D] 2D Hit 성공! 대상: {hit2D.collider.gameObject.name}");
                return HandleHit2D(hit2D, action);
            }

            // 2. 3D Raycast 시도
            RaycastHit hit;
            if (Physics.Raycast(lastRay, out hit, maxRayCastDistance, raycastLayerMask))
            {
                Debug.Log($"[PageView_3D] 3D Hit 성공! 대상: {hit.collider.gameObject.name}");
                return HandleHit(hit, action);
            }

            Debug.Log($"[PageView_3D] 레이 Hit 실패. 좌표: {hitPointNormalized}");
            return false;
        }

        private void OnDrawGizmos()
        {
            if (!debugDrawRay) return;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(lastRay.origin, lastRay.origin + lastRay.direction * 5f);
        }

        private bool HandleHit2D(RaycastHit2D hit, BookActionDelegate action)
        {
            return TriggerObjectInteraction(hit.collider.gameObject, (Vector3)hit.point);
        }

        protected override bool HandleHit(RaycastHit hit, BookActionDelegate action)
        {
            return TriggerObjectInteraction(hit.collider.gameObject, hit.point);
        }

        private bool TriggerObjectInteraction(GameObject target, Vector3 hitPoint)
        {
            bool handled = false;

            var leanClick = target.GetComponentInParent<LeanClickEvent>();
            if (leanClick != null && leanClick.Enable)
            {
                leanClick.OnClickEvent?.Invoke();
                Debug.Log($"[PageView_3D] LeanClickEvent invoked on: {target.name}");
                handled = true;
            }

            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                button = this.button,
                position = pageViewCamera.WorldToScreenPoint(hitPoint),
                pointerCurrentRaycast = new RaycastResult { gameObject = target }
            };

            if (ExecuteEvents.ExecuteHierarchy(target, pointerEventData, ExecuteEvents.pointerClickHandler))
            {
                Debug.Log($"[PageView_3D] IPointerClickHandler executed on: {target.name}");
                handled = true;
            }

            return handled;
        }
    }
}
