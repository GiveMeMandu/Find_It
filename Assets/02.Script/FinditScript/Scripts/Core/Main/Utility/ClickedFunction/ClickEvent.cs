using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DeskCat.FindIt.Scripts.Core.Main.System;
using System.Collections.Generic;

namespace DeskCat.FindIt.Scripts.Core.Main.Utility.ClickedFunction
{
    public class ClickEvent : MonoBehaviour, 
        IPointerDownHandler, 
        IPointerUpHandler,
        IPointerClickHandler,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public bool Enable = true;
        public UnityEvent OnMouseDownEvent;
        public UnityEvent OnMouseUpEvent;
        public UnityEvent OnClickEvent;

        private int _clickCount = 0;
        public int _maxClickCount = -1;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Enable) return;
            
            // OnPointerDown에서는 OnMouseDownEvent만 실행
            OnMouseDownEvent?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Enable) return;
            
            OnMouseUpEvent?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Enable) return;
            
            // 드래그 중이면 클릭 이벤트 실행하지 않음
            if (eventData.dragging) return;
            
            // HiddenObj 우선순위 체크
            if (!CheckHiddenObjectPriority(eventData)) return;
            
            // 최대 클릭 수 체크
            if (_maxClickCount != -1 && _clickCount >= _maxClickCount) return;
            
            // 클릭 이벤트 실행
            OnClickEvent?.Invoke();
            
            _clickCount++;
        }

        // 드래그 방지를 위한 구현들
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            // 드래그 초기화를 차단하고 이벤트 전파 중지
            eventData.useDragThreshold = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // 드래그 시작을 차단하고 이벤트 전파 중지
            eventData.pointerDrag = null;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 드래그 진행을 차단하고 이벤트 전파 중지
            eventData.pointerDrag = null;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // 드래그 종료를 차단하고 이벤트 전파 중지
            eventData.pointerDrag = null;
        }

        public void IsEnable(bool enable)
        {
            Enable = enable;
        }
        
        public void SetMaxClickCount(int maxCount)
        {
            _maxClickCount = maxCount;
        }
        
        public void ResetClickCount()
        {
            _clickCount = 0;
        }
        
        public int GetClickCount()
        {
            return _clickCount;
        }

        /// <summary>
        /// HiddenObj가 있는 경우 ClickEvent보다 우선순위를 낮춥니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        /// <returns>ClickEvent가 실행되어야 하는지 여부</returns>
        private bool CheckHiddenObjectPriority(PointerEventData eventData)
        {
            // Raycast를 통해 모든 히트된 오브젝트들을 가져옴
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            // HiddenObj 컴포넌트를 가진 오브젝트가 있는지 확인
            foreach (var result in raycastResults)
            {
                var hiddenObj = result.gameObject.GetComponent<HiddenObj>();
                if (hiddenObj != null && !hiddenObj.IsFound)
                {
                    // 찾지 않은 HiddenObj가 있으면 ClickEvent는 실행하지 않음
                    return false;
                }
            }

            // HiddenObj가 없거나 모두 찾은 상태면 ClickEvent 실행 허용
            return true;
        }
    }
}