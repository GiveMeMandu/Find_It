using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class CoroutineFunction : MonoBehaviour
    {
        public bool PlayCoroutineOnStart = false;
        public bool LoopCoroutine = false;
        
        public float Duration = 2;
        public UnityEvent CoroutineEvent;

        private void Start()
        {
            if (PlayCoroutineOnStart)
            {
                StartCountdown();
            }
        }

        public void StartCountdown()
        {
            StartCoroutine(WaitForSecond());
        }

        private IEnumerator WaitForSecond()
        {
            yield return new WaitForSeconds(Duration);
            CoroutineEvent?.Invoke();
            
            while (LoopCoroutine)
            {
                yield return new WaitForSeconds(Duration);
                CoroutineEvent?.Invoke();
            }
        }

    }
}