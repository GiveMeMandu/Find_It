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

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Enable) return;
            OnMouseDownEvent?.Invoke();
            OnClickEvent?.Invoke();
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
    }
}