using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class ClickEvent : MonoBehaviour, 
        IPointerDownHandler, 
        IPointerUpHandler
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
            
            // 최대 클릭 수 체크
            if (_maxClickCount != -1 && _clickCount >= _maxClickCount) return;
            
            OnMouseDownEvent?.Invoke();
            OnClickEvent?.Invoke();
            
            _clickCount++;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Enable) return;
            
            OnMouseUpEvent?.Invoke();
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