namespace echo17.EndlessBook.Demo02
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Simple touch pad colliders that handle input on the book pages.
    /// This is a crude component that should probably be replaced with something
    /// more sophisticated for your projects, but is sufficient for this demo.
    /// </summary>
    public class TouchPad : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// New Input System actions mapping
        /// </summary>
        protected BookInputActions _bookInputActions;
#endif

        /// <summary>
        /// Touchpad collider names
        /// </summary>
        protected const string PageLeftColliderName = "Page Left";
        protected const string PageRightColliderName = "Page Right";
        protected const string TableOfContentsColliderName = "TableOfContents Button";

        /// <summary>
        /// The minimum amount the mouse needs to move to be considered a drag event
        /// </summary>
        protected const float DragThreshold = 0.007f;

        /// <summary>
        /// The size of each page collider
        /// </summary>
        protected Rect[] pageRects;

        /// <summary>
        /// Whether we have touched down on the pad
        /// </summary>
        protected bool touchDown;

        /// <summary>
        /// The position if we have touched down
        /// </summary>
        protected Vector2 touchDownPosition;

        /// <summary>
        /// The last drag position used to calculate the increment between frames
        /// </summary>
        protected Vector2 lastDragPosition;

        /// <summary>
        /// 처음에 터치한 페이지를 기억하여, 마우스를 밖에서 뗐을 때 올바른 뷰에 TouchUp을 보내기 위함
        /// </summary>
        protected PageEnum lastTouchedPage;

        /// <summary>
        /// Whether we are dragging
        /// </summary>
        protected bool dragging;

        /// <summary>
        /// One of two pages
        /// </summary>
        public enum PageEnum
        {
            Left,
            Right
        }

        /// <summary>
        /// The demo camera
        /// </summary>
        public Camera mainCamera;

        /// <summary>
        /// The colliders for each page
        /// </summary>
        public Collider[] pageColliders;

        /// <summary>
        /// The upper left "button" used to go back to the table of contents
        /// </summary>
        public Collider tableOfContentsCollider;

        /// <summary>
        /// The mask of the touchpad colliders
        /// </summary>
        public LayerMask pageTouchPadLayerMask;

        /// <summary>
        /// Handler for when a touch down is detected
        /// </summary>
        public Action<PageEnum, Vector2> touchDownDetected;

        /// <summary>
        /// Handler for when a touch up is detected
        /// </summary>
        public Action<PageEnum, Vector2, bool> touchUpDetected;

        /// <summary>
        /// Handler for when a drag is detected
        /// </summary>
        public Action<PageEnum, Vector2, Vector2, Vector2> dragDetected;

        /// <summary>
        /// Handler for when the table of contents "button" is clicked
        /// </summary>
        public Action tableOfContentsDetected;

#if ENABLE_INPUT_SYSTEM
        private void OnEnable()
        {
            // set up the new input system actions
            _bookInputActions = new BookInputActions();
            _bookInputActions.TouchPad.Enable();
        }

        private void OnDisable()
        {
            // disable the new input system actions
            _bookInputActions.TouchPad.Disable();
        }
#endif

        void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            Debug.Log("TouchPad: Using new Input System");
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            Debug.Log("TouchPad: Using old Input System");
#endif

            // set up collider rects

            pageRects = new Rect[2];
            for (var i = 0; i < 2; i++)
            {
                pageRects[i] = new Rect(pageColliders[i].bounds.min.x, pageColliders[i].bounds.min.z, pageColliders[i].bounds.size.x, pageColliders[i].bounds.size.z);
            }
        }

        void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            // Old Input System

            if (Input.GetMouseButtonDown(0))
            {
                // left mouse button pressed
                DetectTouchDown(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(0))
            {
                // left mouse button un-pressed
                DetectTouchUp(Input.mousePosition);
            }
            else if (touchDown && Input.GetMouseButton(0))
            {
                // dragging
                DetectDrag(Input.mousePosition);
            }
#endif

#if ENABLE_INPUT_SYSTEM
            // New Input System

            if (_bookInputActions.TouchPad.Press.WasPressedThisFrame())
            {
                // left mouse button pressed
                DetectTouchDown(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            }
            else if (_bookInputActions.TouchPad.Press.WasReleasedThisFrame())
            {
                // left mouse button un-pressed
                DetectTouchUp(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            }
            else if (touchDown && _bookInputActions.TouchPad.Press.IsPressed())
            {
                // dragging
                DetectDrag(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            }        
#endif
        }

        /// <summary>
        /// Turn a page collider on or off.
        /// Useful if we are in a state of the book that cannot handle one of the colliders,
        /// like ClosedFront cannot handle a left page interaction.
        /// </summary>
        /// <param name="page">The page collider to toggle</param>
        /// <param name="on">Whether to toggle on</param>
        public virtual void Toggle(PageEnum page, bool on)
        {
            // activate or deactive the collider
            pageColliders[(int)page].gameObject.SetActive(on);
        }

        /// <summary>
        /// Turn the table of contents collider on or off.
        /// Useful for when we are turning pages: we should turn it off,
        /// turning it back on when the turn has completed.
        /// </summary>
        /// <param name="on">Whether to toggle on</param>
        public virtual void ToggleTableOfContents(bool on)
        {
            // activate or deactive the collider
            tableOfContentsCollider.gameObject.SetActive(on);
        }

        /// <summary>
        /// Determine if a touch down occurred
        /// </summary>
        /// <param name="position">Position of mouse</param>
        protected virtual void DetectTouchDown(Vector2 position)
        {
            Vector2 hitPosition;
            Vector2 hitPositionNormalized;
            PageEnum page;
            bool tableOfContents;

            // get the hit point if we can
            if (GetHitPoint(position, out hitPosition, out hitPositionNormalized, out page, out tableOfContents))
            {
                // touched down and stopped dragging
                touchDown = true;
                dragging = false;

                if (tableOfContents)
                {
                    // table of contents "button" clicked
                    tableOfContentsDetected();
                }
                else
                {
                    // page hit
                    touchDownPosition = hitPosition;
                    lastDragPosition = hitPosition;
                    lastTouchedPage = page; // 터치한 페이지 기억

                    if (touchDownDetected != null)
                    {
                        // handle page touched
                        touchDownDetected(page, hitPositionNormalized);
                    }
                }
            }
        }

        /// <summary>
        /// Determine if a drag occurred
        /// </summary>
        /// <param name="position">Position of mouse</param>
        protected virtual void DetectDrag(Vector2 position)
        {
            // exit if we don't have a handler for this
            if (dragDetected == null) return;

            Vector2 hitPosition;
            Vector2 hitPositionNormalized;
            PageEnum page;
            bool tableOfContents;

            bool hit = GetHitPoint(position, out hitPosition, out hitPositionNormalized, out page, out tableOfContents);

            // 드래그 중이면 시작 페이지로 고정 (다른 페이지나 collider 밖으로 이동해도 드래그 유지)
            if (dragging)
            {
                page = lastTouchedPage;
                if (!hit)
                {
                    // collider 밖: 시작 페이지 평면에 투영해 위치 추정
                    hit = ProjectOntoPagePlane(position, lastTouchedPage, out hitPosition, out hitPositionNormalized);
                }
            }

            if (!hit) return;

            // get the offset from the last drag position
            var offset = hitPosition - lastDragPosition;

            // if the offset is more than the drag minimum
            if (offset.magnitude >= DragThreshold)
            {
                dragging = true;
                dragDetected(page, touchDownPosition, hitPosition, offset);
                lastDragPosition = hitPosition;
            }
        }

        /// <summary>
        /// 페이지 collider의 표면 평면에 마우스 레이를 투영해 hitPosition을 구한다.
        /// collider 밖으로 드래그가 벗어났을 때 드래그가 끊기지 않도록 하기 위해 사용.
        /// </summary>
        protected virtual bool ProjectOntoPagePlane(Vector2 mousePosition, PageEnum page,
            out Vector2 hitPosition, out Vector2 hitPositionNormalized)
        {
            hitPosition = Vector2.zero;
            hitPositionNormalized = Vector2.zero;

            Collider col = pageColliders[(int)page];
            Plane pagePlane = new Plane(col.transform.up, col.bounds.center);
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);

            float enter;
            if (!pagePlane.Raycast(ray, out enter)) return false;

            Vector3 worldPoint = ray.GetPoint(enter);
            hitPosition = new Vector2(worldPoint.x, worldPoint.z);

            Bounds bounds = col.bounds;
            hitPositionNormalized = new Vector2(
                (worldPoint.x - bounds.min.x) / bounds.size.x,
                (worldPoint.z - bounds.min.z) / bounds.size.z);

            return true;
        }

        /// <summary>
        /// Determine if a touch up event occurred
        /// </summary>
        /// <param name="position">Mouse position</param>
        protected virtual void DetectTouchUp(Vector2 position)
        {
            // exit if there is no handler
            if (touchUpDetected == null) return;

            Vector2 hitPosition;
            Vector2 hitPositionNormalized;
            PageEnum page;
            bool tableOfContents;

            // no longer touching. 무조건 터치 상태 해제
            touchDown = false;

            // 콜라이더 밖에서 마우스/터치를 뗐더라도 마우스 업 이벤트를 보내기 위함
            bool hit = GetHitPoint(position, out hitPosition, out hitPositionNormalized, out page, out tableOfContents);
            
            // 만약 콜라이더를 벗어난 상태에서 뗐다면, 처음 눌렀던 페이지에 마우스 업 이벤트를 발생시킵니다.
            if (!hit)
            {
                page = lastTouchedPage;
            }

            touchUpDetected(page, hitPositionNormalized, dragging);
        }

        /// <summary>
        /// Gets the hit point of the page collider
        /// </summary>
        /// <param name="mousePosition">The position of the mouse</param>
        /// <param name="hitPosition">The absolute hit point on the page collider</param>
        /// <param name="hitPositionNormalized">The hit point normalized between 0 and 1 on both axis of the page collider</param>
        /// <param name="page">Which page was hit</param>
        /// <param name="tableOfContents">Whether the table of contents "button" was hit</param>
        /// <returns></returns>
        protected virtual bool GetHitPoint(Vector3 mousePosition, out Vector2 hitPosition, out Vector2 hitPositionNormalized, out PageEnum page, out bool tableOfContents)
        {
            hitPosition = Vector2.zero;
            hitPositionNormalized = Vector2.zero;
            page = PageEnum.Left;
            tableOfContents = false;

            // get a ray from the screen to the page colliders
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            // cast the ray against the collider mask
            if (Physics.Raycast(ray, out hit, 1000, pageTouchPadLayerMask))
            {
                // hit

                // determine which page was hit
                page = hit.collider.gameObject.name == PageLeftColliderName ? PageEnum.Left : PageEnum.Right;

                // determine if the table of contents "button" was hit
                tableOfContents = hit.collider.gameObject.name == TableOfContentsColliderName;

                // set the hit position using the x and z axis
                hitPosition = new Vector2(hit.point.x, hit.point.z);

                // normalize the hit position against the current bounds
                // (Awake 시점의 고정된 pageRects 대신 실시간 bounds를 사용하여 페이지가 펼쳐지거나 움직였을 때의 위치 변화를 완벽하게 반영합니다)
                Bounds currentBounds = hit.collider.bounds;
                hitPositionNormalized = new Vector2((hit.point.x - currentBounds.min.x) / currentBounds.size.x,
                                                    (hit.point.z - currentBounds.min.z) / currentBounds.size.z);

                return true;
            }

            return false;
        }
    }
}