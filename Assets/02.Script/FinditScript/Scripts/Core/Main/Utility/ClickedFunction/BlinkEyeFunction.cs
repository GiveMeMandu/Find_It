using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class BlinkEyeFunction : MonoBehaviour
    {
        public bool BlinkOnStart = true;
        public bool LoopBlinkEye = true;
        
        public float BlinkOnDuration = 1.5f;
        public float BlinkOffDuration = 0.2f;
        public GameObject TargetBlinkEyeObject;
        
        private void Start()
        {
            if (BlinkOnStart)
            {
                StartCountdown();
            }
        }

        private void StartCountdown()
        {
            StartCoroutine(WaitForSecond());
        }

        private IEnumerator WaitForSecond()
        {
            yield return new WaitForSeconds(BlinkOnDuration);
            TargetBlinkEyeObject.SetActive(true);
            yield return new WaitForSeconds(BlinkOffDuration);
            TargetBlinkEyeObject.SetActive(false);
            
            while (LoopBlinkEye)
            {
                yield return new WaitForSeconds(BlinkOnDuration);
                TargetBlinkEyeObject.SetActive(true);
                yield return new WaitForSeconds(BlinkOffDuration);
                TargetBlinkEyeObject.SetActive(false);
            }
        }

    }
}