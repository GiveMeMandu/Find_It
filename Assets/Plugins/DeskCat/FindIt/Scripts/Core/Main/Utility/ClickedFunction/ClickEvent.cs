using UnityEngine;
using UnityEngine.Events;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class ClickEvent : MonoBehaviour
    {
        public UnityEvent OnClickEvent;

        public void OnMouseDown()
        {
            OnClickEvent?.Invoke();
        }

    }
}