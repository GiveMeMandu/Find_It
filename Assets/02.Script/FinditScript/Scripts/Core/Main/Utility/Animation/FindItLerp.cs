using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.Animation
{
    public class FindItLerp : MonoBehaviour
    {
        public AnimationCurve AnimationCurve = AnimationCurve.Linear(0,0,1,1);
        public float TimeDuration = 1;
        public bool PlayOnStart = true;
        
        [Range(0,1)]
        public float currentTime;

        public bool HideHiddenObjAfterDone = true;
        private float WaitForSecondBeforeHide = 0.3f;
        public UnityEvent AfterDone;

        private void Start()
        {
            if(PlayOnStart)
                StartAnimation();
        }

        public float GetLerpValue(float time)
        {
            return AnimationCurve.Evaluate(time);
        }
        
        public void StartAnimation()
        {
            StartCoroutine(LerpFunction(TimeDuration));
        }

        private IEnumerator LerpFunction(float duration)
        {
            float time = 0;
            while (time < duration)
            {
                currentTime = Mathf.Lerp(0, 1, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            
            yield return new WaitForSeconds(WaitForSecondBeforeHide);
            AfterDone?.Invoke();
            if (HideHiddenObjAfterDone)
            {
                transform.parent.gameObject.SetActive(false);
            }
            else
            {
                transform.gameObject.SetActive(false);
            }
        }
    }
}