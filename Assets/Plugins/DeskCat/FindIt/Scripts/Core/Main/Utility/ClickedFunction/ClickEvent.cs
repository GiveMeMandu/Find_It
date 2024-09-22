using UnityEngine;
using UnityEngine.Events;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class ClickEvent : MonoBehaviour
    {
        public UnityEvent OnMouseDownEvent;
        public UnityEvent OnMouseUpEvent;
        public UnityEvent OnClickEvent;

        public void OnMouseUp()
        {
            OnMouseUpEvent?.Invoke();
        }
        public void OnMouseDown()
        {
            OnMouseDownEvent?.Invoke();
            OnClickEvent?.Invoke();
        }
    }
}