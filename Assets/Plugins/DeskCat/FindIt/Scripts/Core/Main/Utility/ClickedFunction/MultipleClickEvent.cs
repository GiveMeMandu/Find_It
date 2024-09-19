using UnityEngine;
using UnityEngine.Events;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class MultipleClickEvent : MonoBehaviour
    {
        public bool EnableClickLoop = true;
        
        [NonReorderable]
        public UnityEvent[] OnClickEventList;  

        [SerializeField]
        private int CurrentClickCount = 0;

        public void OnMouseDown()
        {
            Click();
        }

        public void Click()
        {
            if (CurrentClickCount >= OnClickEventList.Length)
            {
                if (EnableClickLoop)
                {
                    CurrentClickCount = 0;
                }
                else
                {
                    return;
                }
            }
            OnClickEventList[CurrentClickCount]?.Invoke();
            CurrentClickCount++;
        }

        public void ResetCount()
        {
            CurrentClickCount = 0;
        }
    }
}