using UnityEngine;
using UnityEngine.EventSystems;
using Util.CameraSetting;

namespace UI.EventTrigger
{
    /// <summary>
    /// Event Trigger에서 카메라 컨트롤을 관리하는 컴포넌트
    /// 기존 SetEnablePanAndZoom 대신 SetUIDragState를 사용하여
    /// InputManager와의 충돌을 방지합니다.
    /// </summary>
    public class CameraControlEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [Header("카메라 컨트롤 설정")]
        [Tooltip("포인터 진입 시 카메라 컨트롤 비활성화")]
        public bool disableOnPointerEnter = true;
        
        [Tooltip("포인터 나갈 때 카메라 컨트롤 활성화")]
        public bool enableOnPointerExit = true;
        
        [Tooltip("드래그 시작 시 카메라 컨트롤 비활성화")]
        public bool disableOnBeginDrag = true;
        
        [Tooltip("드래그 종료 시 카메라 컨트롤 활성화")]
        public bool enableOnEndDrag = true;
        
        [Tooltip("드래그 중 DialogToast 비활성화")]
        public bool disableDialogToastOnDrag = true;
        
        [Header("참조")]
        [Tooltip("드래그 중 비활성화할 DialogToast 오브젝트")]
        public GameObject dialogToast;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (disableOnPointerEnter)
            {
                CameraView2D.SetUIDragState(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (enableOnPointerExit)
            {
                CameraView2D.SetUIDragState(false);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (disableOnBeginDrag)
            {
                CameraView2D.SetUIDragState(true);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (enableOnEndDrag)
            {
                CameraView2D.SetUIDragState(false);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (disableDialogToastOnDrag && dialogToast != null)
            {
                dialogToast.SetActive(false);
            }
        }
    }
}
