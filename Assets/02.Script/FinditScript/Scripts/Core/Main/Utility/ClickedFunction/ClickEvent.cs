using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class ClickEvent : MonoBehaviour, 
        IPointerDownHandler, 
        IPointerUpHandler,
        IPointerClickHandler,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public bool Enable = true;
        public UnityEvent OnMouseDownEvent;
        public UnityEvent OnMouseUpEvent;
        public UnityEvent OnClickEvent;

        private int _clickCount = 0;
        public int _maxClickCount = -1;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Enable) return;
            
            // OnPointerDown에서는 OnMouseDownEvent만 실행
            OnMouseDownEvent?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Enable) return;
            
            OnMouseUpEvent?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Enable) return;
            
            // 드래그 중이면 클릭 이벤트 실행하지 않음
            if (eventData.dragging) return;
            
            // 최대 클릭 수 체크
            if (_maxClickCount != -1 && _clickCount >= _maxClickCount) return;
            
            // 클릭 이벤트 실행
            OnClickEvent?.Invoke();
            
            _clickCount++;
        }

        // 드래그 방지를 위한 구현들
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            // 드래그 초기화를 차단하고 이벤트 전파 중지
            eventData.useDragThreshold = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // 드래그 시작을 차단하고 이벤트 전파 중지
            eventData.pointerDrag = null;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 드래그 진행을 차단하고 이벤트 전파 중지
            eventData.pointerDrag = null;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // 드래그 종료를 차단하고 이벤트 전파 중지
            eventData.pointerDrag = null;
        }

        public void IsEnable(bool enable)
        {
            Enable = enable;
        }
        
        public void SetMaxClickCount(int maxCount)
        {
            _maxClickCount = maxCount;
        }
        
        public void ResetClickCount()
        {
            _clickCount = 0;
        }
        
        public int GetClickCount()
        {
            return _clickCount;
        }
    }
}