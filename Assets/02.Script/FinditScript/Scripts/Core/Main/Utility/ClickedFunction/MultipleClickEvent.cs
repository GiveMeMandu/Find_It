using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class MultipleClickEvent : MonoBehaviour, IPointerClickHandler
    {
        public bool EnableClickLoop = true;
        
        [NonReorderable]
        public UnityEvent[] OnClickEventList;  

        [SerializeField]
        private int CurrentClickCount = 0;

        // 이 오브젝트를 클릭했을 때만 호출됨
        public void OnPointerClick(PointerEventData eventData)
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