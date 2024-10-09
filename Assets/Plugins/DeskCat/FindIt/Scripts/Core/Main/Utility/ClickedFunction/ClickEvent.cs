using UnityEngine;
using UnityEngine.Events;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class ClickEvent : MonoBehaviour
    {
        public bool Enable = true;
        public UnityEvent OnMouseDownEvent;
        public UnityEvent OnMouseUpEvent;
        public UnityEvent OnClickEvent;

        public void OnMouseUp()
        {
            if(Enable)
                OnMouseUpEvent?.Invoke();
        }
        public void OnMouseDown()
        {
            if(Enable) {
                OnMouseDownEvent?.Invoke();
                OnClickEvent?.Invoke();
            }
        }
        public void IsEnable(bool enable) {
            Enable = enable;
        }
    }
}