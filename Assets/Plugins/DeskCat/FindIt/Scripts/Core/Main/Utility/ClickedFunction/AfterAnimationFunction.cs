using UnityEngine;
using UnityEngine.Events;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class AfterAnimationFunction : MonoBehaviour
    {
        public UnityEvent AfterAnimationEvent;
 
        public void StartAfterAnimationEvent()
        {
            AfterAnimationEvent?.Invoke();
        }

    }
}