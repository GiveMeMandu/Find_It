using System;
using DeskCat.FindIt.Scripts.Core.Main.System;
using UnityEngine;
using UnityEngine.Events;
using Util.CameraSetting;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.DragObj
{
    [Serializable] public class DragAndDropEvent3D : UnityEvent<DragObj> { }
    public class DragObj : MonoBehaviour
    {
        public string DropRegionName = "";
        public bool HideWhenDropToRegion = true;
        public bool EnableCollisionWhenDrag = false;

        public bool IsThisRegionAsTarget = true;
        public bool DragToRegionToFound = false;
        public bool IsReturnToOriginalPosition = false;

        [Header("Freeze Drag Axis")] 
        public bool freezeX;
        public bool freezeY;
        public bool freezeZ;

        [Header("Drag Event")] 
        public DragAndDropEvent3D onBeginDrag;
        public DragAndDropEvent3D onDrag;
        public DragAndDropEvent3D onDragToRegion;
        public DragAndDropEvent3D onEndDrag;
        
        [Header("Drop Event")] 
        public DragAndDropEvent3D onDropRegion;

        private Camera _mainCamera;
        private Vector3 _mOffset;
        private float _mZCoord;

        private float _xPosition;
        private float _yPosition;
        private float _zPosition;

        private HiddenObj _hiddenObj;
        private BoxCollider2D _collider;


        public HiddenObj GetCurrentHiddenObj()
        {
            return _hiddenObj;
        }

        private void Start()
        {
            if (!Camera.main)
            {
                Debug.LogError("Easy Drag And Drop: Main Camera Does Not Exists");
                return;
            }

            _mainCamera = Camera.main;
            
            _hiddenObj = GetComponent<HiddenObj>();
            _collider = GetComponent<BoxCollider2D>();

        }

        private void OnMouseDown()
        {
            var position = gameObject.transform.position;
            _xPosition = position.x;
            _yPosition = position.y;
            _zPosition = position.z;

            _mOffset = position - CalculateObjPosition();

            onBeginDrag?.Invoke(this);
            
            CameraView2D.SetEnablePanAndZoom(false);
            CameraView3D.SetEnableOrbit(false);
        }

        private void OnMouseDrag()
        {
            if (!EnableCollisionWhenDrag)
            {
                _collider.enabled = false;
            }

            transform.position = CalculateObjPosition() + _mOffset;
            FreezePositionOnDrag();
            onDrag?.Invoke(this);
            
            if (CurrentDragInfo.CurrentDropRegion == null) return;
            if (CurrentDragInfo.CurrentDropRegion.RegionName != DropRegionName) return;
            onDragToRegion?.Invoke(this);
            if (DragToRegionToFound)
            {
                _hiddenObj.DragRegionAction?.Invoke();
            }

        }

        private void OnMouseUp()
        {   
            if (!EnableCollisionWhenDrag)
            {
                _collider.enabled = true;
            }

            onEndDrag?.Invoke(this);
            
            CameraView2D.SetEnablePanAndZoom(true);
            CameraView3D.SetEnableOrbit(true);
            
            DropRegionCheck();
            
            if (IsReturnToOriginalPosition)
            {
                transform.position = new Vector3(_xPosition, _yPosition, _zPosition);
            }

        }

        private void DropRegionCheck()
        {
            if (CurrentDragInfo.CurrentDropRegion == null) return;
            if (CurrentDragInfo.CurrentDropRegion.RegionName != DropRegionName) return;
            
            if (HideWhenDropToRegion)
            {
                gameObject.SetActive(false);
            }

            if (IsThisRegionAsTarget)
            {
                _hiddenObj.DragRegionAction?.Invoke();
            }
            
            onDropRegion?.Invoke(this);
            CurrentDragInfo.CurrentDropRegion.DropRegionEvent?.Invoke();
        }

        private Vector3 CalculateObjPosition()
        {
            _mZCoord = _mainCamera.WorldToScreenPoint(gameObject.transform.position).z;
            var mousePoint = Input.mousePosition;
            mousePoint.z = _mZCoord;
            return _mainCamera.ScreenToWorldPoint(mousePoint);
        }

        private void FreezePositionOnDrag()
        {
            if (freezeX)
            {
                var position = new Vector3(_xPosition , transform.position.y, transform.position.z);
                transform.position = position;
            }


            if (freezeY)
            {
                var position = new Vector3(transform.position.x, _yPosition , transform.position.z);
                transform.position = position;
            }

            if (freezeZ)
            {
                var position = new Vector3(transform.position.x, transform.position.y, _zPosition);
                transform.position = position;
            }
        }
    }
}